using JJMasterData.Core.WebComponents;
using JJMasterData.Core.WebComponents.Factories;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace JJMasterData.Web.TagHelpers;

public class JJDataPanelTagHelper : TagHelper
{
    private DataPanelFactory Factory { get; }

    [HtmlAttributeName("name")] 
    public string? Name { get; set; }
    
    [HtmlAttributeName("element-name")] 
    public string? ElementName { get; set; }
    
    [HtmlAttributeName("configure")] 
    public Action<JJDataPanel>? Configure { get; set; }

    public JJDataPanelTagHelper(DataPanelFactory factory)
    {
        Factory = factory;
    }
    
    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        var form = Factory.CreateDataPanel(ElementName);

        if (!string.IsNullOrEmpty(Name))
        {
            form.Name = Name;
        }

        Configure?.Invoke(form);
        
        output.TagMode = TagMode.StartTagAndEndTag;
        output.Content.SetHtmlContent(form.GetHtml());
        return Task.CompletedTask;
    }
}