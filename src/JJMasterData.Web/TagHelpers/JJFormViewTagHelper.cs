using System.Data;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.Web.Components;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace JJMasterData.Web.TagHelpers;

public class JJFormViewTagHelper : TagHelper
{
    
    [HtmlAttributeName("element-name")] 
    public string? ElementName { get; set; }

    [HtmlAttributeName("form-element")]
    public FormElement? FormElement { get; set; }

    [HtmlAttributeName("configure")] 
    public Action<JJFormView>? Configure { get; set; }
    
    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {

        JJFormView formView;

        if(ElementName is not null)
        {
            formView = new JJFormView(ElementName);
        }
        else if(FormElement is not null)
        {
            formView = new JJFormView(FormElement);
        }
        else
        {
            throw new InvalidOperationException("Please set ElementName or FormElement at your JJFormView TagHelper.");
        }

        Configure?.Invoke(formView);
        
        output.TagMode = TagMode.StartTagAndEndTag;
        output.Content.SetHtmlContent(formView.GetHtml());
        return Task.CompletedTask;
    }
}