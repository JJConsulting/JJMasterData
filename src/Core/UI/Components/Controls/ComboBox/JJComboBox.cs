#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using JJMasterData.Commons.Localization;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.DataManager.Expressions;
using JJMasterData.Core.DataManager.Models;
using JJMasterData.Core.DataManager.Services;
using JJMasterData.Core.Http.Abstractions;
using JJMasterData.Core.UI.Html;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace JJMasterData.Core.UI.Components;

public class JJComboBox : ControlBase
{
    private string? _selectedValue;

    private ExpressionsService ExpressionsService { get; }
    private IStringLocalizer<MasterDataResources> StringLocalizer { get; }
    private DataItemService DataItemService { get; }
    internal ILogger<JJComboBox> Logger { get; }
    internal FormStateData FormStateData { get; set; }

    /// <summary>
    /// If the filter is MULTVALUES_EQUALS, enable multiselect.
    /// </summary>
    public bool MultiSelect { get; set; }

    public FormElementDataItem DataItem { get; set; }

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

    public JJComboBox(
        IFormValues formValues,
        DataItemService dataItemService,
        ExpressionsService expressionsService,
        IStringLocalizer<MasterDataResources> stringLocalizer,
        ILogger<JJComboBox> logger) : base(formValues)
    {
        DataItemService = dataItemService;
        Logger = logger;
        ExpressionsService = expressionsService;
        StringLocalizer = stringLocalizer;
        Enabled = true;
        MultiSelect = false;
        DataItem = new FormElementDataItem();
        var defaultValues = new Dictionary<string, object?>();
        FormStateData = new FormStateData(defaultValues, PageState.List);
    }
    
    protected override async Task<ComponentResult> BuildResultAsync()
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
    
    private HtmlBuilder GetSelectHtml(IEnumerable<DataItemValue> values)
    {
        var select = new HtmlBuilder(HtmlTag.Select)
            .WithCssClass(CssClass)
            .WithCssClass("form-control ")
            .WithCssClass((MultiSelect || DataItem.ShowIcon) ? "selectpicker" : "form-select")
            .WithNameAndId(Name)
            .WithAttributeIf(MultiSelect, "multiple")
            .WithAttributeIf(MultiSelect, "title", StringLocalizer["All"])
            .WithAttributeIf(MultiSelect && FormStateData.PageState == PageState.Filter, "data-live-search", "true")
            .WithAttributeIf(MultiSelect, "multiselect", "multiselect")
            .WithAttributeIf(!Enabled, "disabled", "disabled")
            .WithAttribute("data-style", "form-control")
            .WithAttributes(Attributes)
            .AppendRange(GetOptions(values));

        return select;
    }

    private IEnumerable<HtmlBuilder> GetOptions(IEnumerable<DataItemValue> values)
    {
        if (DataItem == null)
            throw new ArgumentException("[DataItem] properties not defined for combo", Name);

        var firstOption = new HtmlBuilder(HtmlTag.Option)
            .WithValue(string.Empty)
            .AppendTextIf(DataItem.FirstOption == FirstOptionMode.All, StringLocalizer["(All)"])
            .AppendTextIf(DataItem.FirstOption == FirstOptionMode.Choose, StringLocalizer["(Choose)"]);

        if (DataItem.FirstOption != FirstOptionMode.None)
            yield return firstOption;

        foreach (var value in values)
        {
            var label = IsManualValues() ? StringLocalizer[value.Description] : value.Description;

            var isSelected = !MultiSelect && SelectedValue != null && SelectedValue.Equals(value.Id);

            if (MultiSelect && SelectedValue != null)
            {
                isSelected = SelectedValue.Split(',').Contains(value.Id);
            }
            
            var content = new HtmlBuilder(HtmlTag.Div);
            content.AppendComponentIf(DataItem.ShowIcon,new JJIcon(value.Icon,value.IconColor));
            content.Append(HtmlTag.Span, span =>
            {
                span.AppendText(label);
                span.WithCssClassIf(DataItem.ShowIcon,$"{BootstrapHelper.MarginLeft}-1");
            });

            var option = new HtmlBuilder(HtmlTag.Option)
                .WithValue(value.Id)
                .WithAttributeIf(isSelected, "selected")
                .WithAttributeIf(DataItem.ShowIcon, "data-content", HttpUtility.HtmlAttributeEncode(content.ToString()))
                .AppendTextIf(!DataItem.ShowIcon, label);

            yield return option;
        }

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

        foreach (var item in list.Where(item => SelectedValue.Equals(item.Id)))
        {
            selectedText = item.Description;

            if (IsManualValues())
                selectedText = StringLocalizer[selectedText];

            break;
        }

        return selectedText;
    }

    public Task<List<DataItemValue>> GetValuesAsync()
    {
        return DataItemService.GetValuesAsync(DataItem,FormStateData);
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

        var label = IsManualValues() ? StringLocalizer[item.Description] : item.Description;

        if (DataItem.ShowIcon)
        {
            var div = new HtmlBuilder(HtmlTag.Div);

            var icon = new JJIcon(item.Icon, item.IconColor, item.Description)
            {
                CssClass = "fa-lg fa-fw"
            }.BuildHtml();

            div.Append(icon);

            if (DataItem.ReplaceTextOnGrid)
            {
                div.AppendText("&nbsp;");
                div.AppendText(label);
            }
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
        var values = await DataItemService.GetValuesAsync(DataItem,FormStateData,null,searchId);
        return values.FirstOrDefault(v=> v.Id==searchId);
    }
    

    private bool IsManualValues()
    {
        if (DataItem.Items == null)
            return false;

        return DataItem.Items.Count > 0;
    }
}