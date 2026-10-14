using JJConsulting.Html.Bootstrap.Abstractions;
using JJConsulting.Html.Extensions;

namespace JJConsulting.Html.Bootstrap.Components;

public sealed class JJToolbar : HtmlComponent
{
    public List<HtmlBuilder?> Items { get; set; } = [];

    protected override HtmlBuilder BuildHtml()
    {
        var html = HtmlBuilder.Div()
            .WithNameAndId(Name)
            .WithAttributes(Attributes)
            .WithCssClass(CssClass)
            .AppendDiv(row =>
            {
                row.WithCssClass("row");
                row.Append(GetActionHtml());
            });

        return html;
    }

    private HtmlBuilder GetActionHtml()
    {
        var div = HtmlBuilder.Div().WithCssClass("col-sm-12");

        foreach (var htmlBuilder in Items)
        {
            if (htmlBuilder == null)
                continue;

            htmlBuilder.WithCssClass("me-1");

            div.Append(htmlBuilder);
        }

        return div;
    }
}
