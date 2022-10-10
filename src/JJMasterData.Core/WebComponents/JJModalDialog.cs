using JJMasterData.Commons.Language;
using JJMasterData.Core.Html;
using System.Collections.Generic;

namespace JJMasterData.Core.WebComponents;
public class JJModalDialog : JJBaseView
{
    public string Title { get; set; }

    public string HtmlContent { get; set; }
    
    public HtmlElement HtmlElementContent { get; set; }
    
    public List<JJLinkButton> Buttons { get; set; }

    public MessageSize Size { get; set; }

    public JJModalDialog()
    {
        Name = "jjmodal";
        Size = MessageSize.Large;
        Buttons = new List<JJLinkButton>();
    }

    internal override HtmlElement GetHtmlElement()
    {
        string title = Translate.Key(Title);

        var html = new HtmlElement(HtmlTag.Div)
            .WithAttributes(Attributes)
            .WithNameAndId(Name)
            .WithCssClass("modal")
            .WithCssClass(CssClass)
            .WithAttribute("role", "dialog")
            .WithAttribute("aria-hidden", "true")
            .WithAttribute("aria-labelledby", $"{Name}-label")
            .AppendElement(HtmlTag.Div, div =>
            {
                div.WithCssClass(GetSizeClass());
                div.AppendElement(HtmlTag.Div, content =>
                {
                    content.WithCssClass("modal-content");
                    content.AppendElement(GetHtmlHeader());
                    
                    content.AppendElement(HtmlTag.Div, body =>
                    {
                        body.WithCssClass("modal-body")
                            .AppendTextIf(!string.IsNullOrEmpty(HtmlContent), HtmlContent)
                            .AppendElement(HtmlElementContent);
                    });
                    content.AppendElementIf(Buttons.Count > 0, HtmlTag.Div, footer =>
                    {
                        footer.WithCssClass("modal-footer");
                        foreach (var btn in Buttons)
                        {
                            footer.AppendElement(btn);
                        }
                    });

                });
            });

        return html;
    }

    private HtmlElement GetHtmlHeader()
    {
        var btn = JJAlert.GetCloseButton("modal");
        var header = new HtmlElement(HtmlTag.Div)
            .WithCssClass("modal-header")
            .AppendElementIf(BootstrapHelper.Version == 3, btn)
            .AppendElement(HtmlTag.H4, h4 =>
            {
                h4.WithCssClass("modal-title")
                  .AppendText(Translate.Key(Title));
            })
            .AppendElementIf(BootstrapHelper.Version > 3, btn);

        return header;
    }

    private string GetSizeClass() =>
        Size switch
        {
            MessageSize.Small => "modal-dialog modal-sm",
            MessageSize.Default => "modal-dialog modal-md",
            _ => "modal-dialog modal-lg",
        };
}
