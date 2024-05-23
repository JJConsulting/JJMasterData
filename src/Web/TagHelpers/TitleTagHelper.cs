using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.UI.Components;

namespace JJMasterData.Web.TagHelpers;

using Microsoft.AspNetCore.Razor.TagHelpers;

[HtmlTargetElement("jj-title")]
public class TitleTagHelper(HtmlComponentFactory htmlComponentFactory) : TagHelper
{
    [HtmlAttributeName("title")]
    public string? Title { get; set; }

    [HtmlAttributeName("subtitle")]
    public string? SubTitle { get; set; }

    [HtmlAttributeName("size")]
    public HeadingSize? Size { get; set; }

    [HtmlAttributeName("icon")]
    public IconType? Icon { get; set; }
    
    [HtmlAttributeName("actions")]
    public List<TitleAction>? Actions {get; set; }
    
    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        var title = htmlComponentFactory.Title.Create(Title ?? string.Empty, SubTitle ?? string.Empty, Icon);

        title.Actions = Actions;
        
        if (Size is not null)
        {
            title.Size = Size.Value;
        }

        output.SuppressOutput();
        output.Content.SetHtmlContent(title.GetHtml());
    }
}
