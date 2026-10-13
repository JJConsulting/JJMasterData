using JJConsulting.FontAwesome;
using JJConsulting.Html.Bootstrap.Abstractions;
using JJConsulting.Html.Bootstrap.Extensions;
using JJConsulting.Html.Bootstrap.Models;
using JJConsulting.Html.Bootstrap.Utils;
using JJConsulting.Html.Extensions;
using Microsoft.AspNetCore.Html;

namespace JJConsulting.Html.Bootstrap.Components;

public class JJAlert : HtmlComponent
{
    public BootstrapColor Color { get; set; }
    public FontAwesomeIcon? Icon { get; set; }

    [LocalizationRequired] public string? Title { get; set; }

    public HeadingSize TitleSize { get; set; } = HeadingSize.H5;

    private HtmlTag TitleTag => TitleSize switch
    {
        HeadingSize.H1 => HtmlTag.H1,
        HeadingSize.H2 => HtmlTag.H2,
        HeadingSize.H3 => HtmlTag.H3,
        HeadingSize.H4 => HtmlTag.H4,
        HeadingSize.H5 => HtmlTag.H5,
        HeadingSize.H6 => HtmlTag.H6,
        _ => throw new ArgumentOutOfRangeException()
    };

    public List<string> Messages { get; } = [];
    public IHtmlContent? Content { get; set; }

    public bool ShowCloseButton { get; set; }

    public bool ShowIcon { get; set; } = true;

    protected override HtmlBuilder BuildHtml()
    {
        var html = new HtmlBuilder(HtmlTag.Div)
            .WithNameAndId(Name)
            .WithAttributes(Attributes)
            .WithCssClass(CssClass)
            .WithCssClass("alert mt-3")
            .WithCssClassIf(BootstrapHelper.Version > 3, "alert-dismissible")
            .WithCssClass(GetClassType())
            .WithAttribute("role", "alert");

        if (ShowCloseButton)
            html.Append(BootstrapUtils.GetCloseButton("alert"));

        var hasTitle = !string.IsNullOrEmpty(Title);

        if (hasTitle)
        {
            html.Append(TitleTag, title =>
            {
                title.WithCssClass("alert-heading");
                if (ShowIcon)
                {
                    AppendAlertIcon(title);
                }

                title.AppendText(Title);
            });
        }

        html.AppendDiv(div =>
        {
            div.WithCssClass("alert-content");

            if (!hasTitle && ShowIcon)
                AppendAlertIcon(div);

            if (Content is not null)
                div.Append(Content);

            if (Messages.Count > 1)
            {
                div.Append(HtmlTag.Ul, ul =>
                {
                    ul.WithCssClass("m-0");
                    foreach (var message in Messages)
                    {
                        ul.Append(HtmlTag.Li, li =>
                        {
                            //TODO:
                            li.Append(new HtmlBuilder(message, encode: false));
                        });
                    }
                });
            }
            else if (Messages.Count == 1)
            {
                div.Append(new HtmlBuilder(Messages[0], encode:false));
            }
        });

        return html;
    }

    private void AppendAlertIcon(HtmlBuilder div)
    {
        if (Icon == null)
            return;

        var icon = new JJIcon(Icon.Value);
        icon.CssClass += $"{BootstrapHelper.MarginRight}-{1}";
        div.AppendComponent(icon);
    }

    private string GetClassType()
    {
        if (Color == BootstrapColor.Default)
            return BootstrapHelper.Version == 3 ? "well" : "alert-secondary";

        return $"alert-{Color.ToColorString()}";
    }
}