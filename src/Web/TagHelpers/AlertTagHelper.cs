using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.UI.Components;
using JJMasterData.Core.Web.Components;

namespace JJMasterData.Web.TagHelpers;

using Microsoft.AspNetCore.Razor.TagHelpers;


public class AlertTagHelper : TagHelper
{
    private readonly HtmlComponentFactory _htmlComponentFactory;

    [HtmlAttributeName("title")]
    public string? Title { get; set; }
    
    [HtmlAttributeName("message")]
    public string? Message { get; set; }
    
    [HtmlAttributeName("messages")]
    public List<string>? Messages { get; set; }

    [HtmlAttributeName("color")]
    public PanelColor Color { get; set; }
    
    [HtmlAttributeName("icon")]
    public IconType? Icon { get; set; }

    [HtmlAttributeName("show-close-button")]
    public bool ShowCloseButton { get; set; }

    [HtmlAttributeName("css-class")]
    public string? CssClass { get; set; }

    public AlertTagHelper(HtmlComponentFactory htmlComponentFactory)
    {
        _htmlComponentFactory = htmlComponentFactory;
    }
    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        var alert = _htmlComponentFactory.Alert.Create();
        alert.Color = Color;
        alert.CssClass = CssClass;
        alert.Title = Title;
        alert.ShowCloseButton = ShowCloseButton;

        if (Icon is not null)
            alert.Icon = Icon.Value;

        if (Messages != null)
            alert.Messages = Messages;

        if(!string.IsNullOrEmpty(Message))
            alert.Messages.Add(Message);

        var content = (await output.GetChildContentAsync()).GetContent();

        if (!string.IsNullOrEmpty(content))
            alert.Messages.Add(content);

        output.TagMode = TagMode.StartTagAndEndTag;
        output.Content.SetHtmlContent(alert.GetHtml());
    }
}