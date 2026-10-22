using JJConsulting.FontAwesome;
using JJConsulting.Html.Bootstrap.Components;
using JJConsulting.Html.Bootstrap.Models;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace JJConsulting.Html.Bootstrap.TagHelpers;

public class MessageToastTagHelper : TagHelper
{
    [HtmlAttributeName("name")]
    public required string Name { get; set; }

    [HtmlAttributeName("title")]
    public required string Title { get; set; }

    [HtmlAttributeName("title-muted")] 
    public string? TitleMuted { get; set; }

    [HtmlAttributeName("title-color")]
    public BootstrapColor Color { get; set; }

    [HtmlAttributeName("icon")]
    public FontAwesomeIcon? Icon { get; set; }

    [HtmlAttributeName("message")]
    public string? Message { get; set; }

    [HtmlAttributeName("show-as-opened")] public bool ShowAsOpened { get; set; }

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        var toast = new JJMessageToast
        {
            Name = Name,
            Color = Color,
            Message = Message
        };

        if (!string.IsNullOrEmpty(Title))
        {
            toast.Title = Title;
        }

        if (!string.IsNullOrEmpty(TitleMuted))
        {
            toast.TitleMuted = TitleMuted;
        }

        if (Icon is not null)
        {
            toast.Icon = new JJIcon(Icon.Value);
        }

        toast.ShowAsOpened = ShowAsOpened;

        output.TagMode = TagMode.StartTagAndEndTag;
        output.Content.SetHtmlContent(toast);
    }
}