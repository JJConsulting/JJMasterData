using JJConsulting.Html.Bootstrap.Components;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace JJConsulting.Html.Bootstrap.TagHelpers;

public class ValidationSummaryTagHelper : TagHelper
{
    [HtmlAttributeName("errors")]
    public IEnumerable<string>? Errors { get; set; }

    [ViewContext]
    [HtmlAttributeNotBound]
    public ViewContext ViewContext { get; set; } = null!;

    [HtmlAttributeName("title")]
    public string? Title { get; set; }

    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        var validationSummary = new JJValidationSummary();

        if(!string.IsNullOrEmpty(Title))
            validationSummary.Title = Title;

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
            output.Content.SetHtmlContent(validationSummary);
        }

        return Task.CompletedTask;
    }
}