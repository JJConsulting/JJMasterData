using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.UI.Components;
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
    
    [HtmlAttributeName("text")]
    public string? Text { get; set; }
    
    [HtmlAttributeName("tooltip")]
    public string? Tooltip { get; set; }
    
    [HtmlAttributeName("type")]
    public LinkButtonType? Type { get; set; }

    [HtmlAttributeName("css-class")]
    public string? CssClass { get; set; }

    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        var link = htmlComponentFactory.LinkButton.Create();
        link.Text = Text;
        link.IconClass = Icon.GetCssClass();
        link.UrlAction = UrlAction;
        link.OnClientClick = OnClientClick;
        link.Enabled = Enabled ?? true;
        link.Type = Type ?? LinkButtonType.Button;
        link.Tooltip = Tooltip;
        link.CssClass = CssClass;

        output.TagMode = TagMode.StartTagAndEndTag;
        output.Content.SetHtmlContent(link.GetHtml());

        return Task.CompletedTask;
    }
}