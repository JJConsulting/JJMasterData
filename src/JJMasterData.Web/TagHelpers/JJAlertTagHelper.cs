using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.Web.Components;

namespace JJMasterData.Web.TagHelpers;

using Microsoft.AspNetCore.Razor.TagHelpers;


public class JJAlertTagHelper : TagHelper
{
    [HtmlAttributeName("title")]
    public string? Title { get; set; }
    
    [HtmlAttributeName("message")]
    public string? Message { get; set; }
    
    [HtmlAttributeName("messages")]
    public List<string>? Messages { get; set; }

    [HtmlAttributeName("color")]
    public PanelColor Color { get; set; }
    
    [HtmlAttributeName("icon")]
    public IconType Icon { get; set; }
    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        var alert = new JJAlert
        {
            Color = Color,
            Icon = Icon,
            Title = Title
        };

        if (Messages != null)
            alert.Messages = Messages;

        if(!string.IsNullOrEmpty(Message))
            alert.Messages.Add(Message);

        var content = (await output.GetChildContentAsync()).GetContent();

        if (!string.IsNullOrEmpty(content))
            alert.Messages.Add(content);

       
        output.TagMode = TagMode.StartTagAndEndTag;
        output.Content.SetHtmlContent(alert.GetHtml());
        
    }
}