#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using JJConsulting.Html;
using JJConsulting.Html.Bootstrap.Components;
using JJConsulting.Html.Bootstrap.Extensions;
using JJConsulting.Html.Extensions;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.DataManager.Models;
using JJMasterData.Core.DataManager.Services;
using JJMasterData.Core.Html;
using JJMasterData.Core.Http.Abstractions;

using Microsoft.Extensions.Localization;

namespace JJMasterData.Core.UI.Components;

public class JJComboBox(
    IFormValues formValues,
    DataItemService dataItemService,
    IStringLocalizer<MasterDataResources> stringLocalizer)
    : ControlBase(formValues), IDataItemControl, IFloatingLabelControl
{
    private string? _selectedValue;

    public Guid? ConnectionId { get; set; }

    public FormStateData FormStateData { get; set; } = new(new Dictionary<string, object?>(), PageState.List);

    public string? Id { get; set; }

    /// <summary>
    /// If the filter is MULTVALUES_EQUALS, enable multiselect.
    /// </summary>
    public bool MultiSelect { get; set; } = false;

    public FormElementDataItem DataItem { get; set; } = new();

    public string? SelectedValue
    {
        get
        {
            if (_selectedValue == null && FormValues.ContainsFormValues())
            {
                _selectedValue = FormValues[Name];
            }

            return _selectedValue;
        }
        set => _selectedValue = value;
    }

    public bool EnableLocalization { get; set; } = true;

    public string? FloatingLabel { get; set; }
    public bool UseFloatingLabel { get; set; }


    protected override async ValueTask<ComponentResult> BuildResultAsync()
    {
        if (DataItem == null)
            throw new ArgumentException($"FormElementDataItem property is null for JJComboBox {Name}");

        var values = await GetValuesAsync();

        if (ReadOnly && Enabled)
        {
            var combobox = new HtmlBuilder(HtmlTag.Div);
            combobox.AppendRange(GetReadOnlyInputs(values));
            return new RenderedComponentResult(combobox);
        }

        return new RenderedComponentResult(GetSelectHtml(values));
    }

    private HtmlBuilder GetSelectHtml(List<DataItemValue> values)
    {
        var select = new HtmlBuilder(HtmlTag.Select)
            .WithCssClass(CssClass)
            .WithCssClass("form-control")
            .WithCssClass(MultiSelect || DataItem.ShowIcon ? "selectpicker" : "form-select")
            .WithName(Name)
            .WithId(Id ?? Name)
            .WithAttributeIf(MultiSelect, "multiple")
            .WithAttributeIf(MultiSelect, "title", stringLocalizer["All"])
            .WithAttributeIf(MultiSelect && FormStateData.PageState == PageState.Filter, "data-live-search", "true")
            .WithAttributeIf(MultiSelect, "multiselect", "multiselect")
            .WithAttributeIf(!Enabled, "disabled")
            .WithAttributeIf(BootstrapHelper.Version is 3, "data-style", "form-control")
            .WithAttributeIf(BootstrapHelper.Version is 5, "data-style-base", "form-select form-dropdown")
            .WithAttributes(Attributes)
            .AppendRange(GetOptions(values));

        if (UseFloatingLabel)
        {
            return new HtmlBuilder(HtmlTag.Div).WithCssClass("form-floating")
                .Append(select)
                .AppendLabel(label =>
                {
                    label.AppendText(FloatingLabel);
                    label.WithAttribute("for", Name);
                });
        }

        //Workaround for when nothing is selected at multiselect.
        if (MultiSelect)
        {
            select.AppendInput(input => input.WithName(Name).WithAttribute("hidden"));
        }

        return select;
    }

    private IEnumerable<HtmlBuilder> GetOptions(List<DataItemValue> values)
    {
        if (DataItem == null)
            throw new ArgumentException(@"[DataItem] properties not defined for combo", Name);

        var firstOption = new HtmlBuilder(HtmlTag.Option)
            .WithValue(string.Empty)
            .AppendTextIf(DataItem.FirstOption == FirstOptionMode.All, stringLocalizer["(All)"])
            .AppendTextIf(DataItem.FirstOption == FirstOptionMode.Choose, stringLocalizer["(Choose)"]);

        if (DataItem.FirstOption != FirstOptionMode.None)
            yield return firstOption;

        var groupedValues = values
            .Where(v => v.Group != null)
            .GroupBy(v => v.Group);

        foreach (var group in groupedValues)
        {
            var optgroup = new HtmlBuilder(HtmlTag.OptGroup)
                .WithAttribute("label", group.Key ?? string.Empty);

            foreach (var value in group)
            {
                optgroup.Append(CreateOption(value));
            }

            yield return optgroup;
        }

        foreach (var value in values)
        {
            if (value.Group == null)
            {
                yield return CreateOption(value);
            }
        }
    }

    private HtmlBuilder CreateOption(DataItemValue value)
    {
        var label = IsManualValues() && EnableLocalization ? stringLocalizer[value.Description!] : value.Description;

        bool isSelected;

        if (MultiSelect && SelectedValue != null)
        {
            isSelected = SelectedValue.Split(',').Contains(value.Id, StringComparer.InvariantCultureIgnoreCase);
        }
        else
        {
            isSelected = SelectedValue?.Equals(value.Id) is true;
        }

        var content = new HtmlBuilder();
        if (DataItem.ShowIcon)
            content.AppendComponent(new JJIcon(value.Icon, value.IconColor));
        
        var span = new HtmlBuilder(HtmlTag.Span);
        span.AppendText(label);
        
        if(DataItem.ShowIcon)
            span.WithCssClass($"{BootstrapHelper.MarginLeft}-1");
        
        content.Append(span);

        var option = new HtmlBuilder(HtmlTag.Option)
            .WithValue(value.Id);
            
        if (isSelected)
            option.WithAttribute("selected");
            
        if (DataItem.ShowIcon)
            option.WithAttribute("data-content", content.ToString());
            
        if (!DataItem.ShowIcon)
            option.AppendText(label);

        return option;
    }

    private IEnumerable<HtmlBuilder> GetReadOnlyInputs(IEnumerable<DataItemValue> values)
    {
        if (SelectedValue != null)
        {
            var hiddenInput = new HtmlBuilder(HtmlTag.Input)
                .WithAttribute("type", "hidden")
                .WithNameAndId(Name)
                .WithValue(SelectedValue);

            yield return hiddenInput;
        }

        var selectedText = GetSelectedText(values);

        var readonlyInput = new HtmlBuilder(HtmlTag.Input)
            .WithNameAndId($"cboview_{Name}")
            .WithCssClass("form-control form-select")
            .WithCssClass(CssClass)
            .WithValue(selectedText)
            .WithAttributes(Attributes)
            .WithAttribute("readonly", "readonly");

        yield return readonlyInput;
    }
    
    private string GetSelectedText(IEnumerable<DataItemValue> list)
    {
        string selectedText = string.Empty;

        if (SelectedValue == null)
            return selectedText;

        foreach (var item in list)
        {
            if (SelectedValue.Equals(item.Id))
            {
                selectedText = item.Description;

                if (IsManualValues()) 
                    selectedText = stringLocalizer[selectedText];

                break;
            }
        }

        return selectedText;
    }

    public Task<List<DataItemValue>> GetValuesAsync()
    {
        return dataItemService.GetValuesAsync(DataItem, new DataQuery(FormStateData, ConnectionId)
        {
            SearchId = FormStateData.PageState == PageState.View && !MultiSelect ? SelectedValue : null
        });
    }

    /// <summary>
    /// Recovers the description from the selected value;
    /// </summary>
    public async Task<string?> GetDescriptionAsync()
    {
        string description;
        var item = await GetValueAsync(SelectedValue);
        if (item == null)
            return null;

        var label = IsManualValues() ? stringLocalizer[item.Description] : item.Description;

        if (DataItem.ShowIcon)
        {
            var div = new HtmlBuilder(HtmlTag.Div);

            var icon = new JJIcon(item.Icon, item.IconColor, item.Description)
            {
                CssClass = "fa-lg fa-fw"
            }.GetHtmlBuilder();

            div.Append(icon);

            div.AppendText(" ");
            div.AppendText(label);

            description = div.ToString();
        }
        else
        {
            description = label;
        }

        return description;
    }
    
    public async Task<DataItemValue?> GetValueAsync(string? searchId)
    {
        var dataQuery = new DataQuery(FormStateData, ConnectionId)
        {
            SearchId = searchId
        };
        var values = await dataItemService.GetValuesAsync(DataItem, dataQuery);
        return values.Find(v => string.Equals(v.Id, searchId, StringComparison.InvariantCultureIgnoreCase));
    }

    private bool IsManualValues()
    {
        if (DataItem.Items == null)
            return false;

        return DataItem.Items.Count > 0;
    }
}