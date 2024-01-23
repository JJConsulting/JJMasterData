using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.UI.Components;

namespace JJMasterData.Web.TagHelpers;

using Microsoft.AspNetCore.Razor.TagHelpers;

public class IconTagHelper : TagHelper
{
    [HtmlAttributeName("tooltip")]
    public string? Tooltip { get; set; }
    
    [HtmlAttributeName("color")]
    public string? Color { get; set; }

    [HtmlAttributeName("icon")]
    public IconType Icon { get; set; }

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        var icon = new JJIcon(Icon);

        if (Color != null)
        {
            icon.Color = Color;
        }

        if (Tooltip != null)
        {
            icon.Tooltip = Tooltip;
        }
        
        output.TagMode = TagMode.StartTagAndEndTag;
        output.Content.SetHtmlContent(icon.GetHtml());
    }
}
