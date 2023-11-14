using JJMasterData.Core.UI.Components;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace JJMasterData.Web.TagHelpers;

public class ValidationSummaryTagHelper(ValidationSummaryFactory validationSummaryFactory) : TagHelper
{
    [HtmlAttributeName("errors")] 
    public IEnumerable<string>? Errors { get; set; }

    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        var validationSummary = validationSummaryFactory.Create();
        if (Errors != null)
            validationSummary.Errors.AddRange(Errors);
        
        output.TagMode = TagMode.StartTagAndEndTag;
        output.Content.SetHtmlContent(validationSummary.GetHtml());
        return Task.CompletedTask;
    }
}