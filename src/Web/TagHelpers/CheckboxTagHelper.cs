using JJMasterData.Commons.Exceptions;
using JJMasterData.Core.UI.Components;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace JJMasterData.Web.TagHelpers;

public class CheckboxTagHelper : TagHelper
{
    private IControlFactory<JJCheckBox> CheckboxFactory { get; set; }
    
    [HtmlAttributeName("configure")]
    public Action<JJCheckBox>? Configure { get; set; }

    [HtmlAttributeName("name")] 
    public string? Name { get; set; } 
    
    [HtmlAttributeName("for")] 
    public ModelExpression? For { get; set; } 

    [HtmlAttributeName("value")] 
    public bool IsChecked { get; set; }
    
    [HtmlAttributeName("switch")] 
    public bool IsSwitch { get; set; }
    
    [HtmlAttributeName("switch-size")] 
    public CheckBoxSwitchSize? SwitchSize { get; set; }
    
    [HtmlAttributeName("label")] 
    public string? Label { get; set; }
    
    public CheckboxTagHelper(IControlFactory<JJCheckBox> checkboxFactory)
    {
        CheckboxFactory = checkboxFactory;
    }

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        var checkBox = CheckboxFactory.Create();
        checkBox.Name = Name ?? For?.Name ?? throw new JJMasterDataException("Either for or name attributes are required.");

        if (For is not null)
        {
            checkBox.IsChecked = For?.Model is true;
        }
        else
        {
            checkBox.IsChecked = IsChecked;
        }

        checkBox.IsSwitch = IsSwitch;
        checkBox.SwitchSize = SwitchSize;
        checkBox.Text = Label;
        
        Configure?.Invoke(checkBox);
        
        output.TagMode = TagMode.StartTagAndEndTag;

        var htmlBuilder = await checkBox.GetHtmlBuilderAsync();
        
        output.Content.SetHtmlContent(htmlBuilder.ToString());
    }
}