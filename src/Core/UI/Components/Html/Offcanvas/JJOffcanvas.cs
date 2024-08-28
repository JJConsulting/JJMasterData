#nullable enable
using JJMasterData.Core.UI.Html;

namespace JJMasterData.Core.UI.Components;

public class JJOffcanvas : HtmlComponent
{
    public OffcanvasPosition Position { get; set; }
    public string? Title { get; set; }
    public HtmlBuilder? Body { get; set; }

    internal override HtmlBuilder BuildHtml()
    {
        var offcanvas = new HtmlBuilder(HtmlTag.Div)
            .WithCssClass($"offcanvas {Position.GetCssClass()}")
            .WithAttribute("tabindex", "-1")
            .WithId(Name)
            .AppendDiv(div =>
                {
                    div.WithCssClass("offcanvas-header")
                        .AppendIf(!string.IsNullOrEmpty(Title),HtmlTag.H5, h5 =>
                        {
                            h5.AppendText(Title!).WithCssClass("offcanvas-title");
                        })
                        .Append(HtmlTag.Button, button =>
                        {
                            button.WithAttribute("type", "button")
                                .WithCssClass("btn-close")
                                .WithAttribute("data-bs-dismiss", "offcanvas")
                                .WithAttribute("aria-label", "Close");
                        });
                }
            )
            .AppendDiv(div =>
            {
                div.WithId(Name + "-body")
                    .WithCssClass("offcanvas-body")
                    .Append(Body);
            });

        return offcanvas;
    }
}