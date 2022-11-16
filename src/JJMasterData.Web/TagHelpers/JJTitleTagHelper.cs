using JJMasterData.Core.WebComponents;

namespace JJMasterData.Web.TagHelpers;

using Microsoft.AspNetCore.Razor.TagHelpers;

public class JJTitleTagHelper : TagHelper
{
    [HtmlAttributeName("title")]
    public string Title { get; set; }
    
    [HtmlAttributeName("subtitle")]
    public string? SubTitle { get; set; }
    
    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        var title = new JJTitle(Title, SubTitle);
        
        output.Content.SetHtmlContent(title.GetHtml());
        
        return Task.CompletedTask;
    }
}
