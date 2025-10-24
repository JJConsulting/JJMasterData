using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.UI.Components;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Localization;

namespace JJMasterData.Web.TagHelpers;

public class ModalDialogTagHelper(HtmlComponentFactory htmlComponentFactory, IStringLocalizer<MasterDataResources> stringLocalizer) : TagHelper
{
    [HtmlAttributeName("name")]
    public required string Name { get; set; }
    
    [HtmlAttributeName("title")]
    public string? Title { get; set; }
    
    [HtmlAttributeName("size")]
    public ModalSize? Size { get; set; }

    [HtmlAttributeName("buttons")]
    public List<JJLinkButton> Buttons { get; set; } = [];
    
    [HtmlAttributeName("show-close-button")] 
    public bool ShowCloseButton { get; set; }

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        var modal = htmlComponentFactory.ModalDialog.Create();

        modal.Name = Name;
        modal.Title = Title;
        
        if (Size is not null)
            modal.Size = Size.Value;
        
        var content = (await output.GetChildContentAsync()).GetContent();

        modal.HtmlContent = content;

        if (ShowCloseButton)
            Buttons.Add(GetCloseButton());

        modal.Buttons = Buttons;
        
        output.TagMode = TagMode.StartTagAndEndTag;
        output.Content.SetHtmlContent(modal.GetHtml());
    }

    private JJLinkButton GetCloseButton()
    {
        var closeButton = htmlComponentFactory.LinkButton.Create();
        closeButton.Attributes["data-bs-dismiss"] = "modal";
        closeButton.ShowAsButton = true;
        closeButton.Text = stringLocalizer["Close"];
        return closeButton;
    }
}
