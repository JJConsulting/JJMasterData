using JJConsulting.FontAwesome;
using JJConsulting.Html.Bootstrap.Components;
using JJConsulting.Html.Bootstrap.Models;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace JJConsulting.Html.Bootstrap.TagHelpers;

public class CardTagHelper : TagHelper
{
    [HtmlAttributeName("name")]
    public string? Name { get; set; }

    [HtmlAttributeName("title")]
    public string? Title { get; set; }

    [HtmlAttributeName("icon")]
    public FontAwesomeIcon Icon { get; set; }
    
    [HtmlAttributeName("color")]
    public BootstrapColor Color { get; set; }

    [HtmlAttributeName("layout")]
    public PanelLayout? Layout { get; set; }

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        var card = new JJCard
        {
            Name = Name ?? Title?.ToLower().Replace(" ", "_")!,
            Title = Title,
            Color = Color,
            Layout = Layout ?? PanelLayout.Panel
        };

        if (Icon != default)
        {
            card.Icon = Icon;
        }

        var content = await output.GetChildContentAsync();
        card.Content = content;

        output.TagMode = TagMode.StartTagAndEndTag;
        output.Content.SetHtmlContent(card);
    }
}