using System.Linq;
using System.Threading.Tasks;
using JJMasterData.Commons.Localization;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.UI.Html;
using Microsoft.Extensions.Localization;

namespace JJMasterData.Core.UI.Components;

internal class GridLegendView(IControlFactory<JJComboBox> comboBoxFactory, IStringLocalizer<MasterDataResources> stringLocalizer)
{
    private IControlFactory<JJComboBox> ComboBoxFactory { get; } = comboBoxFactory;
    private IStringLocalizer<MasterDataResources> StringLocalizer { get; } = stringLocalizer;
    public bool ShowAsModal { get; set; } = false;

    public required string Name { get; init; }
    public required FormElement FormElement { get; init; }

    public Task<HtmlBuilder> GetHtmlBuilderAsync()
    {
        if (ShowAsModal)
        {
            return GetModalHtmlBuilder();
        }

        var field = GetLegendField();

        return  GetLegendHtmlBuilder(field);
    }

    private async Task<HtmlBuilder> GetLegendHtmlBuilder(FormElementField field)
    {
        var div = new HtmlBuilder(HtmlTag.Div);

        if (field != null)
        {
            var cbo = ComboBoxFactory.Create();
            cbo.Name = field.Name;
            
            if (field.DataItem != null)
                cbo.DataItem = field.DataItem;

            var values = await cbo.GetValuesAsync();
            
            if (values is { Count: > 0 })
            {
                foreach (var item in values)
                {
                    div.Append(HtmlTag.Div, div =>
                    {
                        div.WithAttribute("style", "height:40px");

                        div.AppendComponent(new JJIcon(item.Icon, item.IconColor, item.Description)
                        {
                            CssClass = "fa-fw fa-2x"
                        });
                        div.AppendText("&nbsp;&nbsp;");
                        div.AppendText(StringLocalizer[item.Description]);
                        div.Append(HtmlTag.Br);
                    });
                }
            }
        }
        else
        {
            div.Append(HtmlTag.Br);
            div.AppendText(StringLocalizer["There is no caption to be displayed"]);
        }

        return div;
    }

    private async Task<HtmlBuilder> GetModalHtmlBuilder()
    {
        var field = GetLegendField();

        var form = new HtmlBuilder(HtmlTag.Div)
            .WithCssClass("form-horizontal")
            .WithAttribute("role", "form")
            .Append(await GetLegendHtmlBuilder(field));
        
        var dialog = new JJModalDialog
        {
            Name = $"{Name}-legend-modal",
            Title = StringLocalizer["Information"],
            HtmlBuilderContent = form
        };
        
        return dialog.BuildHtml();
    }
    
    private FormElementField GetLegendField()
    {
        return FormElement.Fields.FirstOrDefault(f 
            => f.Component == FormComponent.ComboBox && (f.DataItem?.ShowIcon ?? false));
    }

}
