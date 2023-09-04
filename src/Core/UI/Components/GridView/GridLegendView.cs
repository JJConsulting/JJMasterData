using System.Linq;
using System.Threading.Tasks;
using JJMasterData.Commons.Localization;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.UI.Components;
using JJMasterData.Core.Web.Html;
using Microsoft.Extensions.Localization;

namespace JJMasterData.Core.Web.Components;

internal class GridLegendView 
{
    private IControlFactory<JJComboBox> ComboBoxFactory { get; }
    private IStringLocalizer<JJMasterDataResources> StringLocalizer { get; }
    public bool ShowAsModal { get; set; }
    
    public required FormElement FormElement { get; init; }

    #region "Constructors"

    public GridLegendView(IControlFactory<JJComboBox> comboBoxFactory, IStringLocalizer<JJMasterDataResources> stringLocalizer)
    {
        ComboBoxFactory = comboBoxFactory;
        StringLocalizer = stringLocalizer;
        ShowAsModal = false;
    }

    #endregion
    
    public async Task<HtmlBuilder> GetHtmlBuilderAsync()
    {
        if (ShowAsModal)
        {
            return await GetModalHtmlBuilder(FormElement);
        }

        var field = GetLegendField(FormElement);

        return  await GetLegendHtmlBuilder(field);
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

            var values = await cbo.GetValuesAsync().ToListAsync();
            
            if (values is { Count: > 0 })
            {
                foreach (var item in values)
                {
                    div.Append(HtmlTag.Div, div =>
                    {
                        div.WithAttribute("style", "height:40px");

                        div.AppendComponent(new JJIcon(item.Icon, item.ImageColor, item.Description)
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

    private async Task<HtmlBuilder> GetModalHtmlBuilder(FormElement formElement)
    {
        var field = GetLegendField(formElement);

        var form = new HtmlBuilder(HtmlTag.Div)
            .WithCssClass("form-horizontal")
            .WithAttribute("role", "form")
            .Append(await GetLegendHtmlBuilder(field));
        
        var dialog = new JJModalDialog
        {
            Name = FormElement.Name +"legend_dialog",
            Title = StringLocalizer["Information"],
            HtmlBuilderContent = form
        };
        
        return dialog.BuildHtml();
    }
    
    private FormElementField GetLegendField(FormElement formElement)
    {
        return formElement.Fields.FirstOrDefault(f 
            => f.Component == FormComponent.ComboBox && (f.DataItem?.ShowImageLegend ?? false));
    }

}
