#nullable enable

using JJMasterData.Core.UI.Html;
using JJMasterData.Core.DataDictionary.Models;

namespace JJMasterData.Core.UI.Components;

public class JJMessageToast : HtmlComponent
{
    public string Title { get; set; } = null!;
    public string? TitleMuted { get; set; }
    public string? Message { get; set; }
    public JJIcon? Icon { get; set; }
    public BootstrapColor TitleColor { get; set; }
    public bool ShowAsOpened { get; set; } = true;
    public bool ShowCloseButton { get; set; } = true;
    
    internal JJMessageToast()
    {
        TitleColor = BootstrapColor.Default;
        Name = "toast-alert";
    }

    internal override HtmlBuilder BuildHtml()
    {
        var htmlToast = new HtmlBuilder(HtmlTag.Div)
            .WithCssClass("position-fixed bottom-0 end-0 p-3")
            .WithAttribute("style", "z-index: 5")
            .AppendDiv(toast =>
            {
                toast.WithId(Name)
                    .WithCssClass("toast fade")
                    .WithAttribute("role", "alert")
                    .WithAttribute("aria-live", "assertive")
                    .WithAttribute("aria-atomic", "true")
                    .AppendDiv(header =>
                    {
                        header.WithCssClass("toast-header")
                            .Append(HtmlTag.Strong)
                            .WithCssClass($"me-auto text-{TitleColor.ToColorString()}")
                            .Append(HtmlTag.Strong, s =>
                            {
                                s.WithCssClass($"me-auto text-{TitleColor.ToColorString()}")
                                    .Append(Icon?.GetHtmlBuilder()?.WithCssClass("fs-7 me-1"))
                                    .AppendText(Title);
                            })
                            .AppendIf(TitleMuted != null, HtmlTag.Small, small =>
                            {
                                small.WithCssClass("text-muted")
                                    .AppendText(TitleMuted!);
                            })
                            .Append(HtmlTag.Button, b =>
                            {
                                b.WithCssClass("btn ms-2 p-0")
                                    .WithAttribute("type", "button")
                                    .WithAttribute("data-bs-dismiss", "toast")
                                    .WithAttribute("aria-label", "Close")
                                    .AppendSpan(uil => uil.WithCssClass("uil uil-times fs-7"));
                            });
                        if (ShowCloseButton)
                        {
                            header.Append(HtmlTag.Button, button =>
                            {
                                button.WithAttribute("type", "button");
                                button.WithCssClass("btn-close");
                                button.WithAttribute("data-bs-dismiss", "toast");
                                button.WithAttribute("aria-label", "close");
                            }); }
                    });
                toast.AppendIf(Message != null, HtmlTag.Div, body =>
                {
                    body.WithCssClass("toast-body")
                        .AppendText(Message!);
                });
            });

        var html = new HtmlBuilder();
        html.Append(htmlToast);
        html.AppendIf(ShowAsOpened, HtmlTag.Script, script =>
        {
            script.WithAttribute("type", "text/javascript")
                .WithAttribute("lang", "javascript")
                .AppendText($"MessageToastHelper.showWhenDOMLoaded('{Name}');");
        });

        return html;
    }
}