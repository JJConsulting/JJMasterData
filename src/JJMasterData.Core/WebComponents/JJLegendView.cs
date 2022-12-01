using JJMasterData.Commons.Language;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.Html;
using System.Linq;

namespace JJMasterData.Core.WebComponents;

public class JJLegendView : JJBaseView
{
    /// <summary>
    /// Pre-defined Form settings.
    /// </summary>
    public FormElement FormElement { get; set; }

    public bool ShowAsModal { get; set; }

    #region "Constructors"

    public JJLegendView(FormElement formElement)
    {
        FormElement = formElement;
        Name = "iconLegend";
        ShowAsModal = false;
    }

    #endregion
    
    internal override HtmlBuilder RenderHtml()
    {
        if (ShowAsModal)
        {
            return GetHtmlModal();
        }

        var field = GetFieldLegend();
        return GetHtmlLegend(field);
    }

    private HtmlBuilder GetHtmlLegend(FormElementField field)
    {
        var div = new HtmlBuilder(HtmlTag.Div);

        if (field != null)
        {
            var cbo = new JJComboBox()
            {
                Name = field.Name,
                DataItem = field.DataItem
            };
            
            var values = cbo.GetValues();
            
            if (values is { Count: > 0 })
            {
                foreach (var item in values)
                {
                    div.AppendElement(HtmlTag.Div, div =>
                    {
                        div.WithAttribute("style", "height:40px");

                        div.AppendElement(new JJIcon(item.Icon, item.ImageColor, item.Description)
                        {
                            CssClass = "fa-fw fa-2x"
                        });
                        div.AppendText("&nbsp;&nbsp;");
                        div.AppendText(Translate.Key(item.Description));
                        div.AppendElement(HtmlTag.Br);
                    });
                }
            }
        }
        else
        {
            div.AppendElement(HtmlTag.Br);
            div.AppendText(Translate.Key("There is no caption to be displayed"));
        }

        return div;
    }

    private HtmlBuilder GetHtmlModal()
    {
        var field = GetFieldLegend();

        var form = new HtmlBuilder(HtmlTag.Div)
            .WithCssClass("form-horizontal")
            .WithAttribute("role", "form")
            .AppendElement(GetHtmlLegend(field));
        
        var dialog = new JJModalDialog
        {
            Name = Name,
            Title = Translate.Key("Information"),
            HtmlBuilderContent = form
        };
        
        return dialog.RenderHtml();
    }
    
    private FormElementField GetFieldLegend()
    {
        return FormElement.FormFields.FirstOrDefault(f 
            => f.Component == FormComponent.ComboBox && f.DataItem.ShowImageLegend);
    }

}
