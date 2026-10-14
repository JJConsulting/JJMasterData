using JJConsulting.Html.Bootstrap.Abstractions;
using JJConsulting.Html.Bootstrap.Models;
using JJConsulting.Html.Extensions;

namespace JJConsulting.Html.Bootstrap.Components;

public class JJMessageToast : HtmlComponent
{
    public string Title { get; set; } = null!;
    public string? TitleMuted { get; set; }
    public string? Message { get; set; }
    public JJIcon? Icon { get; set; }
    public BootstrapColor Color { get; set; }
    public bool ShowAsOpened { get; set; } = true;
    public bool ShowCloseButton { get; set; } = true;
    
    public JJMessageToast()
    {
        Color = BootstrapColor.Default;
        Name = "toast-alert";
    }

    protected override HtmlBuilder BuildHtml()
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
                            .WithCssClass($"me-auto text-{Color.ToColorString()}")
                            .Append(HtmlTag.Strong, s =>
                            {
                                s.WithCssClass($"me-auto text-{Color.ToColorString()}")
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
                        .AppendStyle("white-space: pre-line;")
                        .AppendText(Message);
                });
            });

        var html = new HtmlBuilder();
        html.Append(htmlToast);
        html.AppendIf(ShowAsOpened, HtmlTag.Script, script =>
        {
            script.WithAttribute("type", "text/javascript")
                .WithAttribute("lang", "javascript")
                .Append(new HtmlBuilder($"MessageToastHelper.showWhenDOMLoaded('{Name}');", encode:false));
        });

        return html;
    }
}