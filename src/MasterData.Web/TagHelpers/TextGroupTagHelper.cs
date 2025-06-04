using JJMasterData.Core.UI.Components;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace JJMasterData.Web.TagHelpers;

public class TextGroupTagHelper(IControlFactory<JJTextGroup> textGroupFactory) : TagHelper
{

    [HtmlAttributeName("configure")] 
    public Action<JJTextGroup>? Configure { get; set; }

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        var textGroup = textGroupFactory.Create();
        
        Configure?.Invoke(textGroup);
        
        output.TagMode = TagMode.StartTagAndEndTag;

        var htmlBuilder = await textGroup.GetHtmlBuilderAsync();
        
        output.Content.SetHtmlContent(htmlBuilder.ToString());
    }
}