using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.Html;
using System;

namespace JJMasterData.Core.WebComponents;

public class JJTitle : JJBaseView
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

    public JJTitle(string title, string subtitle)
    {
        Title = title;
        SubTitle = subtitle;
        Size = HeadingSize.H1;
    }

    public JJTitle(FormElement form)
    {
        if (form == null)
            throw new ArgumentNullException(nameof(form));

        Title = form.Title;
        SubTitle = form.SubTitle;
        Size = HeadingSize.H1;
    }

    internal override HtmlElement RenderHtmlElement()
    {
        return new HtmlElement(HtmlTag.Div)
            .WithNameAndId(Name)
            .WithAttributes(Attributes)
            .WithCssClass(CssClass)
            .WithCssClass(BootstrapHelper.PageHeader)
            .AppendElement(Tag, e =>
            {
                e.AppendText(Title);
                e.AppendElement(HtmlTag.Small, e =>
                {
                    e.AppendText(" " + SubTitle);
                });
            });
    }
}