#nullable enable
using System;
using System.Collections.Generic;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.UI.Html;

namespace JJMasterData.Core.UI.Components;

public class JJTitle : HtmlComponent
{
    public string? Title { get; set; }
    public string? SubTitle { get; set; }
    public HeadingSize Size { get; set; }
    public IconType? Icon { get; set; }

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

    public JJTitle()
    {
        Size = HeadingSize.H1;
    }

    public JJTitle(string title, string subtitle)
    {
        Title = title;
        SubTitle = subtitle;
        Size = HeadingSize.H1;
    }

    internal override HtmlBuilder BuildHtml()
    {
        if (string.IsNullOrEmpty(Title) && string.IsNullOrEmpty(SubTitle))
            return new HtmlBuilder();

        var div = new HtmlBuilder(HtmlTag.Div)
            .WithNameAndId(Name)
            .WithAttributes(Attributes)
            .WithCssClass(CssClass)
            .WithCssClass(BootstrapHelper.PageHeader)
            .WithCssClass("d-flex justify-content-between")
            .Append(Tag, tag =>
            {
                tag.AppendIf(Icon.HasValue,HtmlTag.Span,span =>
                {
                    span.AppendComponent(new JJIcon(Icon!.Value));
                });
                tag.AppendText(Title);
                tag.Append(HtmlTag.Small, small =>
                {
                    small.WithCssClass("sub-title");
                    small.AppendText($" {SubTitle}");
                });
            });

        if (Actions == null)
            return div;

        div.AppendDiv(div =>
        {
            foreach (var action in Actions)
            {
                div.Append(HtmlTag.A, a =>
                {
                    a.WithCssClass("btn btn-secondary");
                    a.WithHref(action.Url);
                    a.AppendComponentIf(action.Icon.HasValue, () => new JJIcon(action.Icon!.Value));
                    a.AppendTextIf(!string.IsNullOrEmpty(action.Text), "&nbsp;" + action.Text!);
                    a.WithToolTip(action.Tooltip);
                });
            }
        });


        return div;
    }


    internal HtmlBuilder GetHtmlBlockquote()
    {
        var row = new HtmlBuilder(HtmlTag.Div)
            .WithCssClass("row")
            .Append(HtmlTag.Blockquote, block =>
            {
                block.WithCssClass("blockquote mb-1");
                block.AppendIf(!string.IsNullOrEmpty(Title), HtmlTag.P, p => { p.AppendText(Title); });
                block.AppendIf(!string.IsNullOrEmpty(SubTitle), HtmlTag.Footer, p =>
                {
                    p.WithCssClass("blockquote-footer");
                    p.WithCssClassIf(string.IsNullOrEmpty(Title), "mt-1");
                    p.AppendText(SubTitle);
                });
            });

        return row;
    }
}