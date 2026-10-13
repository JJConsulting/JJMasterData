using JJConsulting.FontAwesome;
using JJConsulting.Html.Bootstrap.Abstractions;
using JJConsulting.Html.Bootstrap.Extensions;
using JJConsulting.Html.Bootstrap.Models;
using JJConsulting.Html.Bootstrap.Utils;
using JJConsulting.Html.Extensions;
using Microsoft.AspNetCore.Html;

namespace JJConsulting.Html.Bootstrap.Components;

public class JJCard : HtmlComponent
{
    public string? Title { get; set; }

    public string? SubTitle { get; set; }

    public string? Tooltip { get; set; }

    public PanelLayout Layout { get; set; }

    public BootstrapColor Color { get; set; } = BootstrapColor.Default;

    public IHtmlContent? Content { get; set; }

    private bool HasTitle => !string.IsNullOrEmpty(Title) || !string.IsNullOrEmpty(SubTitle);

    public FontAwesomeIcon? Icon { get; set; }


    protected override HtmlBuilder BuildHtml()
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
            .WithCssClass(BootstrapHelper.GetPanel(Color.ToColorString()));

        html.AppendIf(!string.IsNullOrEmpty(Title), HtmlTag.Div, header =>
        {
            header.WithCssClass(BootstrapHelper.GetPanelHeading(Color.ToColorString()));
            if (Icon is not null)
            {
                var icon = new JJIcon(Icon.Value);
                icon.CssClass += $" {BootstrapHelper.MarginRight}-1";
                header.AppendComponent(icon);
            }

            header.AppendText(Title);

            if (Tooltip is not null)
            {
                var icon = new JJIcon(FontAwesomeIcon.QuestionCircle);
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
                d.Append(BootstrapUtils.GetBlockquote(null, SubTitle));
            }

            d.Append(Content);
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
            html.Append(BootstrapUtils.GetBlockquote(Title, SubTitle));
        }

        html.Append(Content);

        return html;
    }

    private HtmlBuilder GetHtmlWell()
    {
        var html = new HtmlBuilder(HtmlTag.Div)
            .WithAttributes(Attributes)
            .WithNameAndId(Name)
            .WithCssClass(CssClass);

        html.WithCssClass(BootstrapHelper.Version == 3 ? "well" : "card card-body");

        if (HasTitle)
        {
            html.Append(BootstrapUtils.GetBlockquote(Title, SubTitle));
        }

        html.Append(Content);

        return html;
    }
}