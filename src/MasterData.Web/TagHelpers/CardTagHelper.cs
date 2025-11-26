
using JJConsulting.FontAwesome;
using JJConsulting.Html;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.UI.Components;


using Microsoft.AspNetCore.Razor.TagHelpers;

namespace JJMasterData.Web.TagHelpers;
public class CardTagHelper(IComponentFactory<JJCard> cardFactory)
    : TagHelper
{
    
    [HtmlAttributeName("name")]
    public string? Name { get; set; }
    
    [HtmlAttributeName("title")]
    public string? Title { get; set; }

    [HtmlAttributeName("model")]
    public object? Model { get; set; }
    
    [HtmlAttributeName("icon")]
    public FontAwesomeIcon Icon { get; set; }
    
    [HtmlAttributeName("configure")]
    public Action<JJCard>? Configure { get; set; }

    [HtmlAttributeName("color")]
    public BootstrapColor Color { get; set; }
    
    [HtmlAttributeName("layout")]
    public PanelLayout? Layout { get; set; }
    private IComponentFactory<JJCard> CardFactory { get; } = cardFactory;

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        var card = CardFactory.Create();
        card.Name = Name ?? Title?.ToLower().Replace(" ", "_")!;
        card.Title = Title;
        card.Color = Color;
        card.Layout = Layout ?? PanelLayout.Panel;
        
        if (Icon != default)
        {
            card.Icon = Icon;
        }

      
        var content = (await output.GetChildContentAsync()).GetContent();
        card.HtmlBuilderContent = new HtmlBuilder(content, encode:false);
        
        
        Configure?.Invoke(card);
        
        output.TagMode = TagMode.StartTagAndEndTag;
        output.Content.SetHtmlContent(card.GetHtml());
    }
}
