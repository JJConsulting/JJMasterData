using JJConsulting.FontAwesome;
using JJConsulting.Html.Bootstrap.Abstractions;
using JJConsulting.Html.Bootstrap.Extensions;
using JJConsulting.Html.Bootstrap.Models;
using JJConsulting.Html.Bootstrap.Utils;
using JJConsulting.Html.Extensions;

namespace JJConsulting.Html.Bootstrap.Components;

public sealed class JJTitle() : HtmlComponent
{
    public string? Title { get; set; }
    public string? SubTitle { get; set; }
    public HeadingSize Size { get; set; } = HeadingSize.H1;
    public FontAwesomeIcon? Icon { get; set; }
    public List<TitleAction>? Actions { get; set; }

    private HtmlTag Tag => Size switch
    {
        HeadingSize.H1 => HtmlTag.H1,
        HeadingSize.H2 => HtmlTag.H2,
        HeadingSize.H3 => HtmlTag.H3,
        HeadingSize.H4 => HtmlTag.H4,
        HeadingSize.H5 => HtmlTag.H5,
        HeadingSize.H6 => HtmlTag.H6,
        _ => throw new ArgumentOutOfRangeException()
    };

    public JJTitle(string title, string subtitle) : this()
    {
        Title = title;
        SubTitle = subtitle;
    }

    public JJTitle(string title, string subTitle, FontAwesomeIcon? icon) : this(title, subTitle)
    {
        Icon = icon;
    }

    protected override HtmlBuilder BuildHtml()
    {
        var div = new HtmlBuilder(HtmlTag.Div)
            .WithNameAndId(Name)
            .WithAttributes(Attributes)
            .WithCssClass(CssClass)
            .WithCssClass(BootstrapHelper.PageHeader)
            .WithCssClass("d-flex justify-content-between");

        if (!string.IsNullOrEmpty(Title))
        {
            div.Append(Tag, tag =>
            {
                if (Icon.HasValue)
                {
                    tag.AppendSpan(span =>
                    {
                        span.Append(new JJIcon(Icon.Value).GetHtmlBuilder());
                        span.WithCssClass("me-1");
                    });
                }

                tag.Append(new HtmlBuilder(Title!, encode: false)).WithCssClass("me-1");
                if (!string.IsNullOrEmpty(SubTitle))
                {
                    tag.Append(HtmlTag.Small, small =>
                    {
                        small.WithCssClass("sub-title");
                        small.Append(new HtmlBuilder(SubTitle!, encode: false));
                    });
                }
            });
        }
        else
        {
            div.Append(new HtmlBuilder(HtmlTag.Div));
        }

        if (Actions == null)
            return div;

        div.AppendDiv(div =>
        {
            foreach (var action in Actions)
            {
                div.AppendA(a =>
                {
                    a.WithCssClass("btn btn-secondary");
                    a.WithHref(action.Url);

                    if (action.Icon.HasValue)
                        a.AppendComponent(new JJIcon(action.Icon!.Value));

                    if (!string.IsNullOrEmpty(action.Text))
                        a.AppendText(" " + action.Text!);

                    a.WithToolTip(action.Tooltip);
                });
            }
        });

        return div;
    }
}