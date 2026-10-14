using JJConsulting.Html.Bootstrap.Components;
using JJConsulting.Html.Bootstrap.Models;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace JJConsulting.Html.Bootstrap.TagHelpers;

public sealed class OffcanvasTagHelper : TagHelper
{
    [HtmlAttributeName("name")] public required string Name { get; set; }

    public OffcanvasSize Size { get; set; }

    public OffcanvasPosition Position { get; set; }

    public string? Title { get; set; }

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        var offcanvas = new JJOffcanvas
        {
            Name = Name,
            Title = Title,
            Position = Position,
            Size = Size
        };

        var content = (await output.GetChildContentAsync()).GetContent();

        offcanvas.Body = new HtmlBuilder(content, encode: false);

        output.TagName = null;
        output.Content.SetHtmlContent(offcanvas);
    }
}