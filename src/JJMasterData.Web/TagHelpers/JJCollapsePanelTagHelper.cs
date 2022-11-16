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
    
    [HtmlAttributeName("partial-path")]
    public string Partial { get; set; }

    [HtmlAttributeName("partial-model")]
    public dynamic? Model { get; set; }
    
    [HtmlAttributeName("icon")]
    
    public string? Icon { get; set; }

    private RazorPartialRendererService RendererService { get; }
    
    public JJCollapsePanelTagHelper(RazorPartialRendererService rendererService)
    {
        RendererService = rendererService;
    }
    
    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        var panel = new JJCollapsePanel
        {
            Title = Title,
            HtmlContent = await RendererService.ToStringAsync(Partial,Model),
            TitleIcon = new JJIcon(Icon)
        };
        
        output.Content.SetHtmlContent(panel.GetHtml());
    }
}
