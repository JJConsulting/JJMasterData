using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.WebComponents;
using JJMasterData.Core.WebComponents.Factories;
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
    
    [HtmlAttributeName("color")]
    public PanelColor Color { get; set; }
    
    [HtmlAttributeName("configure")]
    public Action<JJCollapsePanel>? Configure { get; set; }

    private CollapsePanelFactory CollapsePanelFactory { get; }
    private RazorPartialRendererService RendererService { get; }
    
    public JJCollapsePanelTagHelper(CollapsePanelFactory collapsePanelFactory, RazorPartialRendererService rendererService)
    {
        CollapsePanelFactory = collapsePanelFactory;
        RendererService = rendererService;
    }
    
    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        AssertAttributes();

        var panel = CollapsePanelFactory.CreateCollapsePanel();
        panel.Name = Title!.ToLower().Replace(" ", "_");
        panel.Title = Title;
        panel.Color = Color;
        panel.ExpandedByDefault = ExpandedByDefault;
        panel.TitleIcon = new JJIcon(Icon);
        
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
