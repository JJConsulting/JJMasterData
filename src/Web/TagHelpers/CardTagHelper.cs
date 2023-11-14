

using JetBrains.Annotations;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.UI.Components;
using JJMasterData.Core.UI.Html;
using JJMasterData.Web.Services;

using Microsoft.AspNetCore.Razor.TagHelpers;

namespace JJMasterData.Web.TagHelpers;
public class CardTagHelper(RazorPartialRendererService rendererService, IComponentFactory<JJCard> cardFactory)
    : TagHelper
{
    
    [HtmlAttributeName("name")]
    public string? Name { get; set; }
    
    [HtmlAttributeName("title")]
    public string? Title { get; set; }
    
    [HtmlAttributeName("partial")]
    [AspMvcPartialView]
    public string? Partial { get; set; }

    [HtmlAttributeName("model")]
    public dynamic? Model { get; set; }
    
    [HtmlAttributeName("icon")]
    public IconType Icon { get; set; }
    
    [HtmlAttributeName("configure")]
    public Action<JJCard>? Configure { get; set; }

    [HtmlAttributeName("color")]
    public PanelColor Color { get; set; }
    
    [HtmlAttributeName("layout")]
    public PanelLayout? Layout { get; set; }
    private RazorPartialRendererService RendererService { get; } = rendererService;
    private IComponentFactory<JJCard> CardFactory { get; } = cardFactory;

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        var card = CardFactory.Create();
        card.Name = Name ?? Title?.ToLower().Replace(" ", "_");
        card.Title = Title;
        card.Color = Color;
        card.Layout = Layout ?? PanelLayout.Panel;
        
        if (Icon != default)
        {
            card.Icon = Icon;
        }

        if (Partial == null)
        {
            var content = (await output.GetChildContentAsync()).GetContent();
            card.HtmlBuilderContent = new HtmlBuilder(content);
        }
        else
        {
            card.HtmlBuilderContent = new HtmlBuilder(await RendererService.ToStringAsync(Partial, Model));
        }
        
        Configure?.Invoke(card);
        
        output.TagMode = TagMode.StartTagAndEndTag;
        output.Content.SetHtmlContent(card.GetHtml());
    }
}
