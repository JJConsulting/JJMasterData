using JJConsulting.FontAwesome;
using JJConsulting.Html.Bootstrap.Components;
using JJConsulting.Html.Bootstrap.Models;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace JJConsulting.Html.Bootstrap.TagHelpers;

[HtmlTargetElement("jj-title")]
public class TitleTagHelper : TagHelper
{
    [HtmlAttributeName("title")]
    public string? Title { get; set; }

    [HtmlAttributeName("subtitle")]
    public string? SubTitle { get; set; }

    [HtmlAttributeName("size")] 
    public HeadingSize? Size { get; set; }

    [HtmlAttributeName("icon")]
    public FontAwesomeIcon? Icon { get; set; }

    [HtmlAttributeName("actions")]
    public List<TitleAction>? Actions { get; set; }

    [HtmlAttributeName("css-class")]
    public string? CssClass { get; set; }

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        var title = new JJTitle(Title ?? string.Empty, SubTitle ?? string.Empty, Icon)
        {
            Actions = Actions
        };

        if (Size is not null)
        {
            title.Size = Size.Value;
        }

        title.CssClass = CssClass;

        output.SuppressOutput();
        output.Content.SetHtmlContent(title);
    }
}