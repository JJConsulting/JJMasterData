using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.WebComponents;
using JJMasterData.Web.Services;
using Microsoft.AspNetCore.Components.RenderTree;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace JJMasterData.Web.TagHelpers;

using Microsoft.AspNetCore.Razor.TagHelpers;

public class JJCollapsePanelTagHelper : TagHelper
{
    
    [HtmlAttributeName("title")]
    public string Title { get; set; }
    
    [HtmlAttributeName("partial")]
    public string Partial { get; set; }

    [HtmlAttributeName("model")]
    public dynamic? Model { get; set; }
    
    [HtmlAttributeName("icon")]
    
    public IconType Icon { get; set; }

    private RazorPartialRendererService RendererService { get; }
    
    public JJCollapsePanelTagHelper(RazorPartialRendererService rendererService)
    {
        RendererService = rendererService;
    }
    
    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        var panel = new JJCollapsePanel
        {
            Name = Title.ToLower().Replace(" ", "_"),
            Title = Title,
            HtmlContent = await RendererService.ToStringAsync(Partial,Model),
            TitleIcon = new JJIcon(Icon)
        };
        output.TagMode = TagMode.StartTagAndEndTag;
        output.Content.SetHtmlContent(panel.GetHtml());
        
    }
}
