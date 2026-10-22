using JJConsulting.FontAwesome;
using JJConsulting.Html.Bootstrap.Components;
using JJConsulting.Html.Bootstrap.Models;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace JJConsulting.Html.Bootstrap.TagHelpers;

public class AlertTagHelper : TagHelper
{
    [HtmlAttributeName("title")] public string? Title { get; set; }

    [HtmlAttributeName("title-size")] public HeadingSize TitleSize { get; set; } = HeadingSize.H5;

    [HtmlAttributeName("message")] public string? Message { get; set; }

    [HtmlAttributeName("messages")] public List<string>? Messages { get; set; }

    [HtmlAttributeName("color")] public BootstrapColor Color { get; set; }

    [HtmlAttributeName("icon")] public FontAwesomeIcon? Icon { get; set; }

    [HtmlAttributeName("show-close-button")]
    public bool ShowCloseButton { get; set; }

    [HtmlAttributeName("css-class")] public string? CssClass { get; set; }

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        var alert = new JJAlert
        {
            Color = Color,
            CssClass = CssClass,
            Title = Title,
            TitleSize = TitleSize,
            ShowCloseButton = ShowCloseButton
        };

        if (Icon is not null)
            alert.Icon = Icon.Value;

        if (Messages != null)
            alert.Messages.AddRange(Messages);

        if (!string.IsNullOrEmpty(Message))
            alert.Messages.Add(Message);

        var content = await output.GetChildContentAsync();

        if (!content.IsEmptyOrWhiteSpace)
            alert.Content = content;

        output.TagMode = TagMode.StartTagAndEndTag;
        output.Content.SetHtmlContent(alert);
    }
}