using JJMasterData.Core.WebComponents;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace JJMasterData.Web.TagHelpers;

public class JJFormViewTagHelper : TagHelper
{
    
    [HtmlAttributeName("element-name")]
    public string ElementName { get; set; }
    
    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        var form = new JJFormView(ElementName)
        {
            Name = context.AllAttributes["name"].Value?.ToString() ?? ElementName
        };
        output.TagMode = TagMode.StartTagAndEndTag;
        output.Content.SetHtmlContent(form.GetHtml());
        return Task.CompletedTask;
    }
}