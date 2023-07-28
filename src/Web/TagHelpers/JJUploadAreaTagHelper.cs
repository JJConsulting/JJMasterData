using JJMasterData.Core.UI.Components;
using JJMasterData.Core.Web.Components;
using JJMasterData.Core.Web.Factories;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace JJMasterData.Web.TagHelpers;

public class JJUploadAreaTagHelper : TagHelper
{
    private IComponentFactory<JJUploadArea> UploadAreaFactory { get; }

    [HtmlAttributeName("name")] 
    public string? Name { get; set; }
    
    [HtmlAttributeName("configure")]
    public Action<JJUploadArea>? Configure { get; set; }

    public JJUploadAreaTagHelper(IComponentFactory<JJUploadArea> uploadAreaFactory)
    {
        UploadAreaFactory = uploadAreaFactory;
    }
    
    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        var upload = UploadAreaFactory.Create();

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