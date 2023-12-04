using Microsoft.AspNetCore.Razor.TagHelpers;

namespace JJMasterData.Web.TagHelpers;


public class TooltipTagHelper : TagHelper
{
    [HtmlAttributeName("title")] public string Title { get; set; } = null!;

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        output.Attributes.SetAttribute("class","fa fa-question-circle help-description");
        output.Attributes.SetAttribute("data-bs-toggle", "tooltip");
        output.Attributes.SetAttribute("title", Title);
        output.TagName = "span";
        output.TagMode = TagMode.StartTagAndEndTag;
    }
}