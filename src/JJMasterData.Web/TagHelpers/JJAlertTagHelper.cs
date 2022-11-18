using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.WebComponents;

namespace JJMasterData.Web.TagHelpers;

using Microsoft.AspNetCore.Razor.TagHelpers;

public class JJAlertTagHelper : TagHelper
{
    [HtmlAttributeName("title")]
    public string Title { get; set; }
    
    [HtmlAttributeName("messages")]
    public List<string> Messages { get; set; }

    [HtmlAttributeName("color")]
    public PanelColor Color { get; set; }
    
    [HtmlAttributeName("icon")]
    public IconType Icon { get; set; }
    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        var title = new JJAlert
        {
            Color = Color,
            Icon = Icon,
            Title = Title,
            Messages = Messages
        };
        output.TagMode = TagMode.StartTagAndEndTag;
        output.Content.SetHtmlContent(title.GetHtml());
        
        return Task.CompletedTask;
    }
}