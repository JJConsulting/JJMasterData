using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.WebComponents;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace JJMasterData.Web.TagHelpers;

public class JJFormViewTagHelper : TagHelper
{
    [HtmlAttributeName("name")] 
    public string? Name { get; set; }
    
    [HtmlAttributeName("element-name")] 
    public string? ElementName { get; set; }
    
    [HtmlAttributeName("configure-form-element")] 
    public Action<FormElement>? ConfigureFormElement { get; set; }
    
    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        var form = new JJFormView(ElementName ?? throw new ArgumentNullException(nameof(ElementName)))
        {
            Name = Name
        };

        ConfigureFormElement?.Invoke(form.FormElement);

        output.TagMode = TagMode.StartTagAndEndTag;
        output.Content.SetHtmlContent(form.GetHtml());
        return Task.CompletedTask;
    }
}