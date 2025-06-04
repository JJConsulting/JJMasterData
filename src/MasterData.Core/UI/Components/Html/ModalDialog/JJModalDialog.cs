using System.Collections.Generic;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.UI.Html;

namespace JJMasterData.Core.UI.Components;

public class JJModalDialog : HtmlComponent
{
    public string Title { get; set; }

    public string HtmlContent { get; set; }
    
    public HtmlBuilder HtmlBuilderContent { get; set; }
    
    public List<JJLinkButton> Buttons { get; set; }

    public ModalSize Size { get; set; }

    public bool IsCentered { get; set; }
    public bool ShowAsOpened { get; set; }
    public JJModalDialog()
    {
        Name = "jjmodal";
        Size = ModalSize.Small;
        Buttons = [];
    }

    internal override HtmlBuilder BuildHtml()
    {
        var html = new HtmlBuilder(HtmlTag.Div)
            .WithAttributes(Attributes)
            .WithId(Name)
            .WithCssClass("modal fade")
            .WithCssClass(CssClass)
            .WithAttribute("role", "dialog")
            .WithAttribute("aria-hidden", "true")
            .WithAttribute("aria-labelledby", $"{Name}-label")
            .Append(HtmlTag.Div, div =>
            {
                div.WithCssClass($"modal-dialog {Size.GetCssClass()}");
                div.WithCssClassIf(IsCentered, "modal-dialog-centered");
                div.Append(HtmlTag.Div, content =>
                {
                    content.WithCssClass("modal-content");
                    content.Append(GetHtmlHeader());
                    
                    content.Append(HtmlTag.Div, body =>
                    {
                        body.WithCssClass("modal-body")
                            .AppendTextIf(!string.IsNullOrEmpty(HtmlContent), HtmlContent!)
                            .Append(HtmlBuilderContent);
                    });
                    content.AppendIf(Buttons.Count > 0, HtmlTag.Div, footer =>
                    {
                        footer.WithCssClass("modal-footer");
                        foreach (var btn in Buttons)
                        {
                            footer.AppendComponent(btn);
                        }
                    });

                });
            });

        if (ShowAsOpened)
            html.AppendScript(BootstrapHelper.GetModalScript(Name));
       
        
        return html;
    }

    private HtmlBuilder GetHtmlHeader()
    {
        var btn = JJAlert.GetCloseButton("modal");
        var header = new HtmlBuilder(HtmlTag.Div)
            .WithCssClass("modal-header")
            .AppendIf(BootstrapHelper.Version == 3, ()=>btn)
            .Append(HtmlTag.H4, h4 =>
            {
                h4.WithCssClass("modal-title")
                  .AppendText(Title);
            })
            .AppendIf(BootstrapHelper.Version > 3,()=> btn);

        return header;
    }
}
