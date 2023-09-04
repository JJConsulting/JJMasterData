using JJMasterData.Core.UI.Components;
using JJMasterData.Core.Web;
using JJMasterData.Core.Web.Components;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace JJMasterData.Web.TagHelpers;

public class JJTextGroupTagHelper : TagHelper
{
    private IControlFactory<JJTextGroup> TextGroupFactory { get; set; }

    [HtmlAttributeName("configure")] 
    public Action<JJTextGroup>? Configure { get; set; }
    
    public JJTextGroupTagHelper(IControlFactory<JJTextGroup> textGroupFactory)
    {
        TextGroupFactory = textGroupFactory;
    }
    
    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        var textGroup = TextGroupFactory.Create();
        
        Configure?.Invoke(textGroup);
        
        output.TagMode = TagMode.StartTagAndEndTag;

        var htmlBuilder = await textGroup.GetHtmlBuilderAsync();
        
        output.Content.SetHtmlContent(htmlBuilder.ToString());
    }
}