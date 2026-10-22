using JJConsulting.FontAwesome;
using JJConsulting.Html.Bootstrap.Abstractions;
using JJConsulting.Html.Bootstrap.Extensions;
using JJConsulting.Html.Bootstrap.Models;
using JJConsulting.Html.Bootstrap.Utils;
using JJConsulting.Html.Extensions;
using Microsoft.AspNetCore.Html;

namespace JJConsulting.Html.Bootstrap.Components;

public class JJCard : HtmlComponent
{
    public string? Title { get; set; }

    public string? SubTitle { get; set; }

    public string? Tooltip { get; set; }

    public BootstrapColor Color { get; set; } = BootstrapColor.Default;

    public IHtmlContent? Content { get; set; }

    private bool HasTitle => !string.IsNullOrEmpty(Title) || !string.IsNullOrEmpty(SubTitle);

    public FontAwesomeIcon? Icon { get; set; }
    
    protected override HtmlBuilder BuildHtml()
    {
        var html = new HtmlBuilder(HtmlTag.Div)
            .WithAttributes(Attributes)
            .WithNameAndId(Name)
            .WithCssClass(CssClass);

        html.WithCssClass("card");
        if (Color != BootstrapColor.Default)
            html.WithCssClass($"border-{Color.ToColorString()}");

        html.AppendDiv(body =>
        {
            body.WithCssClass("card-body");

            if (HasTitle)
            {
                body.Append(HtmlTag.H5, title =>
                {
                    title.WithCssClass("card-title");
                    if (Color != BootstrapColor.Default)
                        title.WithCssClass($"text-{Color.ToColorString()}");

                    if (Icon is not null)
                    {
                        var icon = new JJIcon(Icon.Value);
                        icon.CssClass += " me-2";
                        title.AppendComponent(icon);
                    }

                    title.AppendText(Title);

                    if (Tooltip is not null)
                    {
                        var tooltipIcon = new JJIcon(FontAwesomeIcon.QuestionCircle);
                        tooltipIcon.CssClass += " help-description ms-2";
                        tooltipIcon.Attributes["title"] = Tooltip;
                        tooltipIcon.Attributes[BootstrapHelper.DataToggle] = "tooltip";
                        title.AppendComponent(tooltipIcon);
                    }
                });

                if (!string.IsNullOrEmpty(SubTitle))
                {
                    body.Append(HtmlTag.H6, subtitle =>
                    {
                        subtitle.WithCssClass("card-subtitle mb-2 text-body-secondary");
                        subtitle.AppendText(SubTitle);
                    });
                }
            }

            body.AppendDiv(content =>
            {
                content.WithCssClass("card-text");
                content.Append(Content);
            });
        });
        return html;
    }
}
