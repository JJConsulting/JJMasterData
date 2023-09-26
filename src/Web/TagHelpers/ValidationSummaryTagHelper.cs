using JJMasterData.Core.Web.Components;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace JJMasterData.Web.TagHelpers;

public class ValidationSummaryTagHelper : TagHelper
{
    private readonly ValidationSummaryFactory _validationSummaryFactory;

    [HtmlAttributeName("errors")] 
    public IEnumerable<string>? Errors { get; set; }

    public ValidationSummaryTagHelper(ValidationSummaryFactory validationSummaryFactory)
    {
        _validationSummaryFactory = validationSummaryFactory;
    }
    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        var validationSummary = _validationSummaryFactory.Create();
        if (Errors != null)
            validationSummary.Errors.AddRange(Errors);
        
        output.TagMode = TagMode.StartTagAndEndTag;
        output.Content.SetHtmlContent(validationSummary.GetHtml());
        return Task.CompletedTask;
    }
}