using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.Web.Components;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace JJMasterData.Web.TagHelpers;

public class LinkButtonTagHelper : TagHelper
{
    private readonly HtmlComponentFactory _htmlComponentFactory;

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

    public LinkButtonTagHelper(HtmlComponentFactory htmlComponentFactory)
    {
        _htmlComponentFactory = htmlComponentFactory;
    }
    
    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        var link = _htmlComponentFactory.LinkButton.Create();
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