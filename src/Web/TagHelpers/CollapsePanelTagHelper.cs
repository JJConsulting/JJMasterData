using JetBrains.Annotations;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.UI.Components;
using JJMasterData.Web.Services;

namespace JJMasterData.Web.TagHelpers;

using Microsoft.AspNetCore.Razor.TagHelpers;

public class CollapsePanelTagHelper(RazorPartialRendererService rendererService,
        IComponentFactory<JJCollapsePanel> collapsePanelFactory)
    : TagHelper
{
    
    [HtmlAttributeName("title")]
    [LocalizationRequired]
    public string? Title { get; set; }
    
    [HtmlAttributeName("partial")]
    [AspMvcPartialView]
    public string? Partial { get; set; }

    [HtmlAttributeName("model")]
    public object? Model { get; set; }
    
    [HtmlAttributeName("icon")]
    public IconType Icon { get; set; }
    
    [HtmlAttributeName("expanded-by-default")]
    public bool ExpandedByDefault { get; set; }
    
    [HtmlAttributeName("configure")]
    public Action<JJCollapsePanel>? Configure { get; set; }

    [HtmlAttributeName("color")]
    public PanelColor Color { get; set; }
    
    private RazorPartialRendererService RendererService { get; } = rendererService;
    private IComponentFactory<JJCollapsePanel> CollapsePanelFactory { get; } = collapsePanelFactory;

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        AssertAttributes();

        var panel = CollapsePanelFactory.Create();
        panel.Name = Title!.ToLower().Replace(" ", "_");
        panel.Title = Title;
        panel.ExpandedByDefault = ExpandedByDefault;
        panel.Color = Color;

        if (Icon != default)
        {
            panel.TitleIcon = new JJIcon(Icon);
        }

        if (Partial == null)
        {
            var content = (await output.GetChildContentAsync()).GetContent();
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
