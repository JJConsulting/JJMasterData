using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.Web.Components;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace JJMasterData.Web.TagHelpers;

public class JJLinkButtonTagHelper : TagHelper
{
    [HtmlAttributeName("icon")]
    public IconType Icon { get; set; }
    
    [HtmlAttributeName("url-action")]
    public string? UrlAction { get; set; }
    
    [HtmlAttributeName("enabled")]
    public bool? Enabled { get; set; }
    
    [HtmlAttributeName("text")]
    public string? Text { get; set; }
    
    [HtmlAttributeName("tooltip")]
    public string? Tooltip { get; set; }
    
    [HtmlAttributeName("type")]
    public LinkButtonType? Type { get; set; }

    [HtmlAttributeName("css-class")]
    public string CssClass { get; set; }
    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        var link = new JJLinkButton
        {
            Text = Text,
            IconClass = Icon.GetCssClass(),
            UrlAction = UrlAction,
            Enabled = Enabled ?? true,
            Type = Type ?? LinkButtonType.Button,
            ToolTip = Tooltip,
            CssClass = CssClass
        };

        output.TagMode = TagMode.StartTagAndEndTag;
        output.Content.SetHtmlContent(link.GetHtml());

        return Task.CompletedTask;
    }
}