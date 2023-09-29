using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.UI.Components;
using JJMasterData.Core.Web.Components;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace JJMasterData.Web.TagHelpers;

[HtmlTargetElement("combobox")]
public class ComboBoxTagHelper : TagHelper
{
    private IControlFactory<JJComboBox> ComboBoxFactory { get; set; }
    
    [HtmlAttributeName("configure")]
    public Action<JJComboBox>? Configure { get; set; }

    [HtmlAttributeName("name")] 
    public string Name { get; set; } = null!;

    [HtmlAttributeName("value")] 
    public object? Value { get; set; }
    
    [HtmlAttributeName("form-element-data-item")]
    public FormElementDataItem? DataItem { get; set; }
    
    
    public ComboBoxTagHelper(IControlFactory<JJComboBox> comboBoxFactory)
    {
        ComboBoxFactory = comboBoxFactory;
    }

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        var comboBox = ComboBoxFactory.Create();
        comboBox.Name = Name;
        comboBox.SelectedValue = Value?.ToString();
        if (DataItem != null) 
            comboBox.DataItem = DataItem;
        
        Configure?.Invoke(comboBox);
        
        output.TagMode = TagMode.StartTagAndEndTag;

        var htmlBuilder = await comboBox.GetHtmlBuilderAsync();
        
        output.Content.SetHtmlContent(htmlBuilder.ToString());
    }
}