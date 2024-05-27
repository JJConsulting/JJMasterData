using JJMasterData.Core.UI.Components;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace JJMasterData.Web.TagHelpers;

public class BreadcrumbTagHelper(HtmlComponentFactory htmlComponentFactory) : TagHelper
{
    [HtmlAttributeName("items")]
    public List<BreadcrumbItem> Items { get; set; } = null!;
    
    [HtmlAttributeName("css-class")]
    public string? CssClass { get; set; } 
    
    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        var breadcrumb = htmlComponentFactory.Breadcrumb.Create(Items);
        
        if(CssClass is not null)
            breadcrumb.CssClass = CssClass;
        output.SuppressOutput();
        output.Content.SetHtmlContent(breadcrumb.GetHtml());
        
        base.Process(context, output);
    }
}