using JJConsulting.Html;
using JJMasterData.Core.UI.Components;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace JJMasterData.Web.TagHelpers;

public sealed class OffcanvasTagHelper(HtmlComponentFactory htmlComponentFactory) : TagHelper
{
    [HtmlAttributeName("name")]
    public required string Name { get; set; }
    
    public OffcanvasSize Size { get; set; }

    public OffcanvasPosition Position { get; set; }
    
    public string? Title { get; set; } 

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        var offcanvas = htmlComponentFactory.Offcanvas.Create();

        offcanvas.Name = Name;
        offcanvas.Title = Title;
        offcanvas.Position = Position;
        offcanvas.Size = Size;

        var content = (await output.GetChildContentAsync()).GetContent();

        offcanvas.Body = new HtmlBuilder(content, encode:false);
        
        output.TagName = null;
        output.Content.SetHtmlContent(offcanvas.GetHtml());
    }
}