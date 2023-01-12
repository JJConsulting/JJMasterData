using JJMasterData.Core.WebComponents;
using JJMasterData.Core.WebComponents.Factories;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace JJMasterData.Web.TagHelpers;

public class JJUploadAreaTagHelper : TagHelper
{
    private UploadAreaFactory Factory { get; }

    [HtmlAttributeName("name")] 
    public string? Name { get; set; }
    
    [HtmlAttributeName("configure")]
    public Action<JJUploadArea>? Configure { get; set; }

    public JJUploadAreaTagHelper(UploadAreaFactory factory)
    {
        Factory = factory;
    }
    
    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        var upload = Factory.CreateUploadArea();

        if (!string.IsNullOrEmpty(Name))
        {
            upload.Name = Name;
        }

        Configure?.Invoke(upload);

        output.TagMode = TagMode.StartTagAndEndTag;
        output.Content.SetHtmlContent(upload.GetHtml());
        return Task.CompletedTask;
    }
}