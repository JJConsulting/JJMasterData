using JJMasterData.Core.UI.Components;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace JJMasterData.Web.TagHelpers;

public class BreadcrumbTagHelper(HtmlComponentFactory htmlComponentFactory) : TagHelper
{
    [HtmlAttributeName("items")]
    public List<BreadcrumbItem> Items { get; set; } = null!;
    
    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        var breadcrumb = htmlComponentFactory.Breadcrumb.Create(Items);
        
        output.SuppressOutput();
        output.Content.SetHtmlContent(breadcrumb.GetHtml());
        
        base.Process(context, output);
    }
}