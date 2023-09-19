using JJMasterData.Core.UI.Components;
using JJMasterData.Core.Web;
using JJMasterData.Core.Web.Components;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace JJMasterData.Web.TagHelpers;

public class SliderTagHelper : TagHelper
{
    private IControlFactory<JJSlider> SliderFactory { get; set; }

    [HtmlAttributeName("configure")] 
    public Action<JJSlider>? Configure { get; set; }
    
    public SliderTagHelper(IControlFactory<JJSlider> sliderFactory)
    {
        SliderFactory = sliderFactory;
    }
    
    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        var slider = SliderFactory.Create();
        
        Configure?.Invoke(slider);
        
        output.TagMode = TagMode.StartTagAndEndTag;

        var htmlBuilder = await slider.GetHtmlBuilderAsync();
        
        output.Content.SetHtmlContent(htmlBuilder.ToString());
    }
}