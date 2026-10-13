using System.ComponentModel;
using JetBrains.Annotations;
using JJConsulting.FontAwesome;
using JJConsulting.Html.Bootstrap.Components;
using JJConsulting.Html.Bootstrap.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace JJConsulting.Html.Bootstrap.TagHelpers;

public class LinkButtonTagHelper(IUrlHelper urlHelper) : TagHelper
{
    [AspMvcController]
    [HtmlAttributeName("asp-controller")]
    public string? Controller { get; set; }

    [AspMvcAction]
    [HtmlAttributeName("asp-action")]
    public string? Action { get; set; }

    [HtmlAttributeName("asp-all-route-data", DictionaryAttributePrefix = "asp-route-")]
    public Dictionary<string, string> RouteValues { get; set; } = new();

    [HtmlAttributeName("icon")]
    public FontAwesomeIcon Icon { get; set; }

    [LanguageInjection("Javascript")]
    [HtmlAttributeName("on-client-click")]
    // ReSharper disable once InconsistentNaming
    public string? OnClientClick { get; set; }

    [HtmlAttributeName("enabled")] public bool? Enabled { get; set; }

    [HtmlAttributeName("color")] public BootstrapColor Color { get; set; }

    [HtmlAttributeName("text")]
    [Localizable(false)]
    public string? Text { get; set; }

    [HtmlAttributeName("tooltip")]
    [LocalizationRequired]
    public string? Tooltip { get; set; }

    [HtmlAttributeName("type")] public LinkButtonType? Type { get; set; }

    [HtmlAttributeName("css-class")] public string? CssClass { get; set; }

    [HtmlAttributeName("show-as-button")] public bool ShowAsButton { get; set; } = true;

    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        var link = new JJLinkButton();

        if (!string.IsNullOrEmpty(Action))
            link.UrlAction = urlHelper.Action(Action, Controller, RouteValues);

        link.Type = Type ?? LinkButtonType.Link;
        link.Text = Text;
        link.Color = Color;
        link.IconClass = Icon.CssClass;
        link.OnClientClick = OnClientClick;
        link.ShowAsButton = ShowAsButton;
        link.Enabled = Enabled ?? true;
        link.Tooltip = Tooltip;
        link.CssClass = CssClass;

        output.SuppressOutput();
        output.Content.SetHtmlContent(link);

        return Task.CompletedTask;
    }
}