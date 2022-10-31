using JJMasterData.Commons.Language;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.Html;

namespace JJMasterData.Core.WebComponents;

/// <summary>
/// This is a simple content container
/// </summary>
public class JJCard : JJBaseView
{
    public string Title { get; set; }

    public string SubTitle { get; set; }

    public bool ShowAsWell { get; set; }

    public PanelColor Color { get; set; }

    public HtmlBuilder HtmlBuilderContent { get; set; }

    private bool HasTitle => !string.IsNullOrEmpty(Title) | !string.IsNullOrEmpty(SubTitle);

    public JJCard()
    {
        Color = PanelColor.Default;
    }

    internal override HtmlBuilder RenderHtml()
    {
        HtmlBuilder html;
        if (ShowAsWell)
            html = GetHtmlWell();
        else
            html = GetHtmlPanel();

        if (BootstrapHelper.Version > 3)
        {
            return new HtmlBuilder(HtmlTag.Div)
                .WithCssClass("mb-3")
                .AppendElement(html);
        }
            
        return html;
    }

    private HtmlBuilder GetHtmlPanel()
    {
        var html = new HtmlBuilder(HtmlTag.Div)
            .WithAttributes(Attributes)
            .WithNameAndId(Name)
            .WithCssClass(CssClass)
            .WithCssClass(BootstrapHelper.GetPanel(Color.ToString().ToLower()));

        html.AppendElementIf(!string.IsNullOrEmpty(Title), HtmlTag.Div, header =>
        {
            header.WithCssClass(BootstrapHelper.GetPanelHeading(Color.ToString().ToLower()));
            header.AppendText(Translate.Key(Title));
        });

        html.AppendElement(HtmlTag.Div, d =>
        {
            d.WithCssClass(BootstrapHelper.PanelBody);
            if (!string.IsNullOrEmpty(SubTitle))
            {
                var title = new JJTitle(null, SubTitle);
                d.AppendElement(title.GetHtmlBlockquote());
            }
            d.AppendElement(HtmlBuilderContent);
        });

        return html;
    }

    private HtmlBuilder GetHtmlWell()
    {
        var html = new HtmlBuilder(HtmlTag.Div)
            .WithAttributes(Attributes)
            .WithNameAndId(Name)
            .WithCssClass(CssClass);

        if (BootstrapHelper.Version == 3)
            html.WithCssClass("well");
        else
            html.WithCssClass("card card-body bg-light");

        if (HasTitle)
        {
            var title = new JJTitle(Title, SubTitle);
            html.AppendElement(title.GetHtmlBlockquote());
        }

        html.AppendElement(HtmlBuilderContent);

        return html;
    }

}