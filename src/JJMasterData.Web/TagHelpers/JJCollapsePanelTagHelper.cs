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
    public string? Title { get; set; }
    
    [HtmlAttributeName("partial")]
    public string? Partial { get; set; }

    [HtmlAttributeName("model")]
    public dynamic? Model { get; set; }
    
    [HtmlAttributeName("icon")]
    public IconType Icon { get; set; }
    
    [HtmlAttributeName("expanded-by-default")]
    public bool ExpandedByDefault { get; set; }
    
    [HtmlAttributeName("configure")]
    public Action<JJCollapsePanel>? Configure { get; set; }

    private RazorPartialRendererService RendererService { get; }
    
    public JJCollapsePanelTagHelper(RazorPartialRendererService rendererService)
    {
        RendererService = rendererService;
    }
    
    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        AssertAttributes();
        
        var panel = new JJCollapsePanel
        {
            Name = Title!.ToLower().Replace(" ", "_"),
            Title = Title,
            ExpandedByDefault = ExpandedByDefault,
            TitleIcon = new JJIcon(Icon)
        };

        if (Partial == null)
        {
            var content = output.GetChildContentAsync().Result.GetContent();
            panel.HtmlContent = content;
        }
        else
        {
            panel.HtmlContent = await RendererService.ToStringAsync(Partial, Model);
        }
        
        Configure?.Invoke(panel);
        
        output.TagMode = TagMode.StartTagAndEndTag;
        output.Content.SetHtmlContent(panel.GetHtml());
    }

    private void AssertAttributes()
    {
        if (Title == null)
            throw new ArgumentNullException(nameof(Title));
    }
}
