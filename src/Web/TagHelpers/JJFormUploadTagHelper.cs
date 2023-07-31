using JJMasterData.Core.UI.Components;
using JJMasterData.Core.Web.Components;
using JJMasterData.Core.Web.Factories;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace JJMasterData.Web.TagHelpers;

public class JJFormUploadTagHelper : TagHelper
{
    private IComponentFactory<JJUploadView> FormUploadFactory { get; }

    [HtmlAttributeName("name")]
    public string? Name { get; set; }

    [HtmlAttributeName("configure")]
    public Action<JJUploadView>? Configure { get; set; }
    
    public JJFormUploadTagHelper(IComponentFactory<JJUploadView> formUploadFactory)
    {
        FormUploadFactory = formUploadFactory;
    }
    
    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        var upload = FormUploadFactory.Create();

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