using JJMasterData.Core.WebComponents;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace JJMasterData.Web.TagHelpers;

public class JJUploadAreaTagHelper : TagHelper
{
    [HtmlAttributeName("name")] 
    public string? Name { get; set; }
    
    [HtmlAttributeName("configure")]
    public Action<JJUploadArea>? Configure { get; set; }
    
    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        var upload = new JJUploadArea
        {
            Name = Name
        };

        Configure?.Invoke(upload);

        output.TagMode = TagMode.StartTagAndEndTag;
        output.Content.SetHtmlContent(upload.GetHtml());
        return Task.CompletedTask;
    }
}