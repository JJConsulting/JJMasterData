using JJMasterData.Core.UI.Components;
using JJMasterData.Core.UI.Html;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace JJMasterData.Web.TagHelpers;

public class OffcanvasTagHelper(HtmlComponentFactory htmlComponentFactory) : TagHelper
{
    [HtmlAttributeName("name")]
    public required string Name { get; set; }
    
    public OffcanvasPosition Position { get; set; }
    
    public string? Title { get; set; } 

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        var offcanvas = htmlComponentFactory.Offcanvas.Create();

        offcanvas.Name = Name;
        offcanvas.Title = Title;
        offcanvas.Position = Position;
        
        var content = (await output.GetChildContentAsync()).GetContent();

        offcanvas.Body = new HtmlBuilder(content);
        
        output.TagMode = TagMode.StartTagAndEndTag;
        output.Content.SetHtmlContent(offcanvas.GetHtml());
    }
}