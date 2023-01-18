using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.Web.Components;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace JJMasterData.Web.TagHelpers;

public class JJFormViewTagHelper : TagHelper
{
    [HtmlAttributeName("name")] 
    public string? Name { get; set; }
    
    [HtmlAttributeName("element-name")] 
    public string? ElementName { get; set; }
    
    [HtmlAttributeName("configure")] 
    public Action<JJFormView>? Configure { get; set; }
    
    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        var form = new JJFormView(ElementName ?? throw new ArgumentNullException(nameof(ElementName)));

        if (!string.IsNullOrEmpty(Name))
        {
            form.Name = Name;
        }

        Configure?.Invoke(form);
        
        output.TagMode = TagMode.StartTagAndEndTag;
        output.Content.SetHtmlContent(form.GetHtml());
        return Task.CompletedTask;
    }
}