using JJConsulting.Html.Bootstrap.Extensions;
using JJConsulting.Html.Extensions;

namespace JJConsulting.Html.Bootstrap.Utils;

internal static class BootstrapUtils
{
    public static HtmlBuilder GetCloseButton(string dismissValue)
    {
        var btn = new HtmlBuilder(HtmlTag.Button)
            .WithAttribute("type", "button")
            .WithAttribute("aria-label", "Close")
            .WithDataAttribute("dismiss", dismissValue)
            .WithCssClass(BootstrapHelper.Close)
            .AppendIf(BootstrapHelper.Version == 3, HtmlTag.Span, span =>
            {
                span.WithAttribute("aria-hidden", "true");
                span.AppendText("&times;");
            });

        return btn;
    }

    public static HtmlBuilder GetBlockquote(string? title, string? subTitle)
    {
        var row = new HtmlBuilder(HtmlTag.Div)
            .WithCssClass("row")
            .AppendBlockquote(block=>
            {
                block.WithCssClass("blockquote mb-1");
                if (!string.IsNullOrEmpty(title))
                {
                    block.AppendP(p =>
                    {
                        p.Append(new HtmlBuilder(title!, encode:false));
                    });
                }
                if (!string.IsNullOrEmpty(subTitle))
                {
                    block.AppendFooter( f =>
                    {
                        f.WithCssClass("blockquote-footer");
                        f.WithCssClassIf(string.IsNullOrEmpty(title), "mt-1");
                        f.Append(new HtmlBuilder(subTitle!, encode:false));
                    });
                }
            });

        return row;
    }
}