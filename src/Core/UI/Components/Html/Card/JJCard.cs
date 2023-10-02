using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.UI.Html;

namespace JJMasterData.Core.UI.Components;

/// <summary>
/// This is a simple content container
/// </summary>
public class JJCard : HtmlComponent
{
    public string Title { get; set; }

    public string SubTitle { get; set; }

    public PanelLayout Layout { get; set; }

    public PanelColor Color { get; set; } = PanelColor.Default;

    public HtmlBuilder HtmlBuilderContent { get; set; }

    private bool HasTitle => !string.IsNullOrEmpty(Title) | !string.IsNullOrEmpty(SubTitle);

    internal JJCard()
    {
        
    }
    
    internal override HtmlBuilder BuildHtml()
    {
        var html = Layout switch
        {
            PanelLayout.Well => GetHtmlWell(),
            PanelLayout.NoDecoration => GetHtmlNoDecoration(),
            _ => GetHtmlPanel()
        };

        if (BootstrapHelper.Version > 3)
        {
            return new HtmlBuilder(HtmlTag.Div)
                .Append(html);
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

        html.AppendIf(!string.IsNullOrEmpty(Title), HtmlTag.Div, header =>
        {
            header.WithCssClass(BootstrapHelper.GetPanelHeading(Color.ToString().ToLower()));
            header.AppendText(Title);
        });

        html.Append(HtmlTag.Div, d =>
        {
            d.WithCssClass(BootstrapHelper.PanelBody);
            if (!string.IsNullOrEmpty(SubTitle))
            {
                var title = new JJTitle();
                title.SubTitle = SubTitle;
                d.Append(title.GetHtmlBlockquote());
            }
            d.Append(HtmlBuilderContent);
        });

        return html;
    }

    private HtmlBuilder GetHtmlNoDecoration()
    {
        var html = new HtmlBuilder(HtmlTag.Div)
            .WithAttributes(Attributes)
            .WithNameAndId(Name)
            .WithCssClass(CssClass);

        if (HasTitle)
        {
            var title = new JJTitle
            {
                Title = Title,
                SubTitle = SubTitle
            };
            html.Append(title.GetHtmlBlockquote());
        }

        html.Append(HtmlBuilderContent);

        return html;
    }

    private HtmlBuilder GetHtmlWell()
    {
        var html = new HtmlBuilder(HtmlTag.Div)
            .WithAttributes(Attributes)
            .WithNameAndId(Name)
            .WithCssClass(CssClass);


        html.WithCssClass(BootstrapHelper.Version == 3 ? "well" : "card card-body bg-light");

        if (HasTitle)
        {
            var title = new JJTitle
            {
                Title = Title,
                SubTitle = SubTitle
            };
            html.Append(title.GetHtmlBlockquote());
        }

        html.Append(HtmlBuilderContent);

        return html;
    }

}