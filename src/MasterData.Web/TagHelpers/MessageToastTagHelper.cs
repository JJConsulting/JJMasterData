using JJConsulting.FontAwesome;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.UI.Components;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace JJMasterData.Web.TagHelpers;

public class MessageToastTagHelper(HtmlComponentFactory htmlComponentFactory) : TagHelper
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

    [HtmlAttributeName("show-as-opened")]
    public bool ShowAsOpened { get; set; }
    
    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        var toast = htmlComponentFactory.MessageToast.Create(Message, Color);

        toast.Name = Name;
        
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
        output.Content.SetHtmlContent(toast.GetHtml());
    }
}