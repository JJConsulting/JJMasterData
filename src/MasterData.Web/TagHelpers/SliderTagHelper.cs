using JJMasterData.Core.UI.Components;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace JJMasterData.Web.TagHelpers;

public class SliderTagHelper(IControlFactory<JJSlider> sliderFactory) : TagHelper
{

    [HtmlAttributeName("configure")] 
    public Action<JJSlider>? Configure { get; set; }

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        var slider = sliderFactory.Create();
        
        Configure?.Invoke(slider);
        
        output.TagMode = TagMode.StartTagAndEndTag;

        var htmlBuilder = await slider.GetHtmlBuilderAsync();
        
        output.Content.SetHtmlContent(htmlBuilder.ToString());
    }
}