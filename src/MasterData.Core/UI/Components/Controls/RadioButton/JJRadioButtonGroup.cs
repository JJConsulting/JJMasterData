#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using JJConsulting.Html;
using JJConsulting.Html.Bootstrap.Components;
using JJConsulting.Html.Extensions;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.DataManager.Models;
using JJMasterData.Core.DataManager.Services;
using JJMasterData.Core.Http.Abstractions;


namespace JJMasterData.Core.UI.Components;

public class JJRadioButtonGroup(
    DataItemService dataItemService,
    IFormValues formValues) : ControlBase(formValues), IDataItemControl
{
    public string? SelectedValue
    {
        get
        {
            if (field == null && FormValues.ContainsFormValues())
            {
                field = FormValues[Name];
            }

            return field;
        }
        set;
    }

    public FormElementDataItem DataItem { get; set; } = null!;
    public Guid? ConnectionId { get; set; }
    public FormStateData FormStateData { get; set; } = null!;
    
    protected internal override async ValueTask<HtmlBuilder> GetHtmlBuilderAsync()
    {
        var values = await GetValuesAsync();

        var fieldSet = new HtmlBuilder(HtmlTag.Fieldset);

        foreach (var value in values)
        {
            AppendRadioButton(fieldSet, value);
        }
        
        fieldSet.WithCssClass(CssClass);
        return fieldSet;
    }

    public Task<List<DataItemValue>> GetValuesAsync()
    {
        return dataItemService.GetValuesAsync(DataItem, new DataQuery(FormStateData, ConnectionId));
    }

    private void AppendRadioButton(HtmlBuilder html, DataItemValue item)
    {
        var radio = new JJRadioButton(FormValues)
        {
            Id = item.Id,
            Name = Name,
            IsChecked = item.Id == SelectedValue,
            Enabled = Enabled,
            Text = item.Id,
            Attributes = Attributes,
            Layout = DataItem.RadioLayout!.Value
        };

        if (DataItem.ShowIcon)
        {
            radio.LabelHtml.Append(new JJIcon(item.Icon, item.IconColor).GetHtmlBuilder());
        }
        
        radio.LabelHtml.AppendText(item.Description!);
        
        html.Append(radio.GetHtmlBuilder());
    }
}