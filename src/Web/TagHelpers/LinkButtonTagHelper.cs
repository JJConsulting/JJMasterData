using System.ComponentModel;
using JetBrains.Annotations;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.UI.Components;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Razor.TagHelpers;


namespace JJMasterData.Web.TagHelpers;

public class LinkButtonTagHelper(HtmlComponentFactory htmlComponentFactory) : TagHelper
{
    [HtmlAttributeName("icon")]
    public IconType Icon { get; set; }
    
    [HtmlAttributeName("url-action")]
    public string? UrlAction { get; set; }
    
    [HtmlAttributeName("on-client-click")]
    public string? OnClientClick { get; set; }
    
    [HtmlAttributeName("enabled")]
    public bool? Enabled { get; set; }
    
    [HtmlAttributeName("color")]
    public PanelColor Color { get; set; }
    
    [HtmlAttributeName("text")]
    [Localizable(false)]
    public string? Text { get; set; }
    
    [HtmlAttributeName("tooltip")]
    [LocalizationRequired]
    public string? Tooltip { get; set; }
    
    [HtmlAttributeName("type")]
    public LinkButtonType? Type { get; set; }

    [HtmlAttributeName("css-class")]
    public string? CssClass { get; set; }

    [HtmlAttributeName("show-as-button")] 
    public bool ShowAsButton { get; set; } = true;

    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        var link = htmlComponentFactory.LinkButton.Create();
        link.Text = Text;
        link.Color = Color;
        link.IconClass = Icon.GetCssClass();
        link.UrlAction = UrlAction;
        link.OnClientClick = OnClientClick;
        link.ShowAsButton = ShowAsButton;
        link.Enabled = Enabled ?? true;
        link.Type = Type ?? LinkButtonType.Button;
        link.Tooltip = Tooltip;
        link.CssClass = CssClass;
        
        output.SuppressOutput();
        output.Content.SetHtmlContent(link.GetHtml());

        return Task.CompletedTask;
    }
}