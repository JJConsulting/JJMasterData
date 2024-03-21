using JJMasterData.Core.DataDictionary;
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

    public string Tooltip { get; set; }
    
    public PanelLayout Layout { get; set; }

    public BootstrapColor Color { get; set; } = BootstrapColor.Default;

    public HtmlBuilder HtmlBuilderContent { get; set; }

    private bool HasTitle => !string.IsNullOrEmpty(Title) | !string.IsNullOrEmpty(SubTitle);
    
    public IconType? Icon { get; set; }

    internal JJCard()
    {
        HtmlBuilderContent = new HtmlBuilder();
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
            if (Icon is not null)
            {
                var icon = new JJIcon(Icon.Value);
                icon.CssClass += $" {BootstrapHelper.MarginRight}-1";
                header.AppendComponent(icon);
            }
            header.AppendText(Title);

            if (Tooltip is not null)
            {
                var icon = new JJIcon(IconType.QuestionCircle);
                icon.CssClass += " help-description";
                icon.Attributes["title"] = Tooltip;
                icon.Attributes[BootstrapHelper.DataToggle] = "tooltip";
                header.AppendComponent(icon);
            }
            
        });

        html.Append(HtmlTag.Div, d =>
        {
            d.WithCssClass(BootstrapHelper.PanelBody);
            if (!string.IsNullOrEmpty(SubTitle))
            {
                var title = new JJTitle
                {
                    SubTitle = SubTitle
                };
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

        if (Icon is not null)
        {
            var icon = new JJIcon(Icon.Value);
            icon.CssClass += $" {BootstrapHelper.MarginRight}-1";
            html.AppendComponent(icon);
        }
        
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


        html.WithCssClass(BootstrapHelper.Version == 3 ? "well" : "card card-body");

        
        if (Icon is not null)
        {
            var icon = new JJIcon(Icon.Value);
            icon.CssClass += $" {BootstrapHelper.MarginRight}-1";
            html.AppendComponent(icon);
        }
        
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