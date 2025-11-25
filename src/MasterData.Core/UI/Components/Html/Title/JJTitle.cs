#nullable enable
using System;
using System.Collections.Generic;
using JJConsulting.Html;
using JJConsulting.Html.Extensions;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.Html;


namespace JJMasterData.Core.UI.Components;

public sealed class JJTitle : HtmlComponent
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
                    tag.Append(HtmlTag.Span, Icon.Value, static (icon,span) =>
                    {
                        span.AppendComponent(new JJIcon(icon));
                    });
                }
                tag.AppendText(Title);
                if (!string.IsNullOrEmpty(SubTitle))
                {
                    tag.Append(HtmlTag.Small, SubTitle, static (subTitle,small) =>
                    {
                        small.WithCssClass("sub-title");
                        small.AppendText($" {subTitle}");
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

        div.AppendDiv(Actions, static (actions, div) =>
        {
            foreach (var action in actions)
            {
                div.Append(HtmlTag.A, action, static (action,a) =>
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
    
    internal HtmlBuilder GetHtmlBlockquote()
    {
        var row = new HtmlBuilder(HtmlTag.Div)
            .WithCssClass("row")
            .Append(HtmlTag.Blockquote,this, static (state, block) =>
            {
                block.WithCssClass("blockquote mb-1");
                if (!string.IsNullOrEmpty(state.Title))
                {
                    block.Append(HtmlTag.P, state.Title, static (title, p) =>
                    {
                        p.AppendText(title);
                    });
                }
                if (!string.IsNullOrEmpty(state.SubTitle))
                {
                    block.Append(HtmlTag.Footer, state, static (state,p) =>
                    {
                        p.WithCssClass("blockquote-footer");
                        p.WithCssClassIf(string.IsNullOrEmpty(state.Title), "mt-1");
                        p.AppendText(state.SubTitle);
                    });
                }
            });

        return row;
    }
}