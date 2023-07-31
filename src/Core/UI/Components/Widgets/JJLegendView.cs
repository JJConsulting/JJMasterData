using System.Linq;
using JJMasterData.Commons.Localization;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.Web.Factories;
using JJMasterData.Core.Web.Html;
using JJMasterData.Core.Web.Http.Abstractions;
using Microsoft.Extensions.Localization;

namespace JJMasterData.Core.Web.Components;

public class JJLegendView : JJBaseView
{
    private IControlFactory<JJComboBox> ComboBoxFactory { get; }
    private IStringLocalizer<JJMasterDataResources> StringLocalizer { get; }
    public bool ShowAsModal { get; set; }
    
    public required FormElement FormElement { get; init; }

    #region "Constructors"

    public JJLegendView(IControlFactory<JJComboBox> comboBoxFactory, IStringLocalizer<JJMasterDataResources> stringLocalizer)
    {
        ComboBoxFactory = comboBoxFactory;
        StringLocalizer = stringLocalizer;
        Name = "iconLegend";
        ShowAsModal = false;
    }

    #endregion
    
    internal override HtmlBuilder RenderHtml()
    {
        if (ShowAsModal)
        {
            return GetHtmlModal(FormElement);
        }

        var field = GetFieldLegend(FormElement);
        return GetHtmlLegend(field);
    }

    private HtmlBuilder GetHtmlLegend(FormElementField field)
    {
        var div = new HtmlBuilder(HtmlTag.Div);

        if (field != null)
        {
            var cbo = ComboBoxFactory.Create();
            cbo.Name = field.Name;
            cbo.DataItem = field.DataItem;

            var values = cbo.GetValues();
            
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

    private HtmlBuilder GetHtmlModal(FormElement formElement)
    {
        var field = GetFieldLegend(formElement);

        var form = new HtmlBuilder(HtmlTag.Div)
            .WithCssClass("form-horizontal")
            .WithAttribute("role", "form")
            .Append(GetHtmlLegend(field));
        
        var dialog = new JJModalDialog
        {
            Name = Name,
            Title = StringLocalizer["Information"],
            HtmlBuilderContent = form
        };
        
        return dialog.RenderHtml();
    }
    
    private FormElementField GetFieldLegend(FormElement formElement)
    {
        return formElement.Fields.FirstOrDefault(f 
            => f.Component == FormComponent.ComboBox && f.DataItem.ShowImageLegend);
    }

}
