using JJMasterData.Core.WebComponents;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace JJMasterData.Web.TagHelpers;

public class JJUploadFieldTagHelper : TagHelper
{
    [HtmlAttributeName("name")] 
    public string? Name { get; set; }
    
    [HtmlAttributeName("configure-upload-field")] 
    public Action<JJUploadFile>? ConfigureUploadField { get; set; }
    
    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        var upload = new JJUploadFile
        {
            Name = Name
        };

        ConfigureUploadField?.Invoke(upload);

        output.TagMode = TagMode.StartTagAndEndTag;
        output.Content.SetHtmlContent(upload.GetHtml());
        return Task.CompletedTask;
    }
}