using JJMasterData.Core.UI.Components;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace JJMasterData.Web.TagHelpers;

public class ValidationSummaryTagHelper(ValidationSummaryFactory validationSummaryFactory) : TagHelper
{
    [HtmlAttributeName("errors")] 
    public IEnumerable<string>? Errors { get; set; }

    [ViewContext] 
    [HtmlAttributeNotBound] 
    public ViewContext ViewContext { get; set; } = null!;
    
    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        var validationSummary = validationSummaryFactory.Create();
        var isValid = true;
        
        if (Errors != null)
        {
            validationSummary.Errors.AddRange(Errors);
            isValid = false;
        }
        else if(!ViewContext.ModelState.IsValid)
        {
            var errors = ViewContext.ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage);
            validationSummary.Errors.AddRange(errors);
            isValid = false;  
        }

        if (isValid)
            output.SuppressOutput();
        else
        {
            output.TagMode = TagMode.StartTagAndEndTag;
            output.Content.SetHtmlContent(validationSummary.GetHtml());
        }

        return Task.CompletedTask;
    }
}