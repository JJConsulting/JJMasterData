using JJMasterData.Core.Web.Components;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace JJMasterData.Web.TagHelpers;

public class JJValidationSummaryTagHelper : TagHelper
{
    [HtmlAttributeName("errors")] 
    public IEnumerable<string>? Errors { get; set; }

    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        var validationSummary = new JJValidationSummary
        {
            Errors = Errors?.ToList()
        };
        output.TagMode = TagMode.StartTagAndEndTag;
        output.Content.SetHtmlContent(validationSummary.GetHtml());
        return Task.CompletedTask;
    }
}