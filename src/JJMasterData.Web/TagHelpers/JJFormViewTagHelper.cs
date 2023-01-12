using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.WebComponents;
using JJMasterData.Core.WebComponents.Factories;
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

    public FormViewFactory FormViewFactory { get; }
    
    public JJFormViewTagHelper(FormViewFactory formViewFactory)
    {
        FormViewFactory = formViewFactory;
    }
    
    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        var form = FormViewFactory.CreateFormView(ElementName);

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