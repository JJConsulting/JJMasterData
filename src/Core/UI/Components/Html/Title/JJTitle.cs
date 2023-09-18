using System;
using JJMasterData.Core.UI.Components;
using JJMasterData.Core.Web.Html;

namespace JJMasterData.Core.Web.Components;

public class JJTitle : HtmlComponent
{
    public string Title { get; set; }
    public string SubTitle { get; set; }
    public HeadingSize Size { get; set; }

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

    internal JJTitle()
    {
        Size = HeadingSize.H1;
    }

    internal override HtmlBuilder BuildHtml()
    {
        return new HtmlBuilder(HtmlTag.Div)
            .WithNameAndId(Name)
            .WithAttributes(Attributes)
            .WithCssClass(CssClass)
            .WithCssClass(BootstrapHelper.PageHeader)
            .Append(Tag, e =>
            {
                e.AppendText(Title);
                e.Append(HtmlTag.Small, small =>
                {
                    small.WithCssClass("sub-title");
                    small.AppendText($" {SubTitle}");
                });
            });
    }


    internal HtmlBuilder GetHtmlBlockquote()
    {
        var row = new HtmlBuilder(HtmlTag.Div)
            .WithCssClass("row")
            .Append(HtmlTag.Blockquote, block =>
            {
                block.WithCssClass("blockquote mb-1");
                block.AppendIf(!string.IsNullOrEmpty(Title), HtmlTag.P, p =>
                {
                    p.AppendText(Title);
                });
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