#nullable enable
using System.Collections.Generic;
using System.Threading.Tasks;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.DataManager.Models;
using JJMasterData.Core.DataManager.Services;
using JJMasterData.Core.Http.Abstractions;
using JJMasterData.Core.UI.Html;

namespace JJMasterData.Core.UI.Components;

public class JJRadioButtonGroup(
    DataItemService dataItemService,
    IFormValues formValues) : ControlBase(formValues)
{
    public FormElementDataItem DataItem { get; set; } = null!;
    private DataItemService DataItemService { get; } = dataItemService;
    private string? _selectedValue;
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

    public FormStateData FormStateData { get; set; } = null!;

    protected override async Task<ComponentResult> BuildResultAsync()
    {
        var values = await GetValuesAsync();

        var fieldSet = new HtmlBuilder(HtmlTag.FieldSet);

        foreach (var value in values)
        {
            AppendRadioButton(fieldSet, value);
        }
        

        return new RenderedComponentResult(fieldSet);
    }
    
    public Task<List<DataItemValue>> GetValuesAsync()
    {
        return DataItemService.GetValuesAsync(DataItem,FormStateData);
    }

    public HtmlBuilder AppendRadioButton(HtmlBuilder html, DataItemValue item)
    {
        var radio = new JJRadioButton(FormValues)
        {
            Name = Name,
            IsChecked = item.Id == SelectedValue,
            Enabled = Enabled,
            Text = item.Id,
            Layout = DataItem.RadioLayout!.Value
        };

        if (DataItem.ShowIcon)
        {
            radio.LabelHtml.Append(new JJIcon(item.Icon, item.IconColor).GetHtmlBuilder());
        }
        
        radio.LabelHtml.AppendText(item.Description!);
        
        html.Append(radio.GetHtmlBuilder());
        return html;
    }
}