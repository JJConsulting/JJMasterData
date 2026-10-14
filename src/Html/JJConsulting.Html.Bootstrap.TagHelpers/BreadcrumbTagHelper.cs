using JJConsulting.Html.Bootstrap.Components;
using JJConsulting.Html.Bootstrap.Models;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace JJConsulting.Html.Bootstrap.TagHelpers;

public class BreadcrumbTagHelper : TagHelper
{
    [HtmlAttributeName("items")]
    public List<BreadcrumbItem> Items { get; set; } = null!;

    [HtmlAttributeName("css-class")]
    public string? CssClass { get; set; }

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        var breadcrumb = new JJBreadcrumb(Items);

        if (CssClass is not null)
            breadcrumb.CssClass = CssClass;

        output.SuppressOutput();
        output.Content.SetHtmlContent(breadcrumb);
    }
}