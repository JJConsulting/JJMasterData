using JJConsulting.Html.Bootstrap.Abstractions;
using JJConsulting.Html.Bootstrap.Models;
using JJConsulting.Html.Extensions;

namespace JJConsulting.Html.Bootstrap.Components;

public class JJOffcanvas : HtmlComponent
{
    public OffcanvasPosition Position { get; set; }
    public string? Title { get; set; }

    public OffcanvasSize Size { get; set; }

    public HtmlBuilder? Body { get; set; }

    protected override HtmlBuilder BuildHtml()
    {
        var offcanvas = HtmlBuilder.Div()
            .WithCssClass($"offcanvas {Position.GetCssClass()}")
            .WithCssClassIf(Size == OffcanvasSize.Wide, "offcanvas-wide")
            .WithAttribute("tabindex", "-1")
            .WithId(Name)
            .AppendDiv(div =>
                {
                    div.WithCssClass("offcanvas-header")
                        .AppendIf(!string.IsNullOrEmpty(Title), HtmlTag.H5,
                            h5 => { h5.AppendText(Title).WithCssClass("offcanvas-title"); })
                        .AppendButton(button =>
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