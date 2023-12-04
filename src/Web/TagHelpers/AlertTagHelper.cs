using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.UI.Components;
using JJMasterData.Core.UI.Html;

namespace JJMasterData.Web.TagHelpers;

using Microsoft.AspNetCore.Razor.TagHelpers;


public class AlertTagHelper(HtmlComponentFactory htmlComponentFactory) : TagHelper
{
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

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        var alert = htmlComponentFactory.Alert.Create();
        alert.Color = Color;
        alert.CssClass = CssClass;
        alert.Title = Title;
        alert.ShowCloseButton = ShowCloseButton;

        if (Icon is not null)
            alert.Icon = Icon.Value;

        if (Messages != null)
            alert.Messages.AddRange(Messages);
        
        if(!string.IsNullOrEmpty(Message))
            alert.Messages.Add(Message);

        var content = (await output.GetChildContentAsync()).GetContent();

        if (!string.IsNullOrEmpty(content))
            alert.InnerHtml = new HtmlBuilder(content);

        output.TagMode = TagMode.StartTagAndEndTag;
        output.Content.SetHtmlContent(alert.GetHtml());
    }
}