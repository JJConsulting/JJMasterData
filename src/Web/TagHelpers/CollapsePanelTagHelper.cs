using JetBrains.Annotations;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.UI.Components;

namespace JJMasterData.Web.TagHelpers;

using Microsoft.AspNetCore.Razor.TagHelpers;

public class CollapsePanelTagHelper(IComponentFactory<JJCollapsePanel> collapsePanelFactory)
    : TagHelper
{
    [HtmlAttributeName("title")]
    [LocalizationRequired]
    public string? Title { get; set; }
    
    [HtmlAttributeName("icon")]
    public IconType Icon { get; set; }
    
    [HtmlAttributeName("expanded-by-default")]
    public bool ExpandedByDefault { get; set; }
    
    [HtmlAttributeName("configure")]
    public Action<JJCollapsePanel>? Configure { get; set; }

    [HtmlAttributeName("color")]
    public BootstrapColor Color { get; set; }

    [HtmlAttributeName("visible")] 
    public bool Visible { get; set; } = true;
    
    private IComponentFactory<JJCollapsePanel> CollapsePanelFactory { get; } = collapsePanelFactory;

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        AssertAttributes();

        var panel = CollapsePanelFactory.Create();
        panel.Name = Title!.ToLowerInvariant().Replace(" ", "_");
        panel.Title = Title;
        panel.ExpandedByDefault = ExpandedByDefault;
        panel.Color = Color;
        panel.Visible = Visible;

        if (Icon != default)
        {
            panel.TitleIcon = new JJIcon(Icon);
        }

     
        var content = (await output.GetChildContentAsync()).GetContent();
        panel.HtmlContent = content;
  
        
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
