using JetBrains.Annotations;
using JJMasterData.Commons.Exceptions;
using JJMasterData.Core.UI.Components;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace JJMasterData.Web.TagHelpers;

public class CheckboxTagHelper(IControlFactory<JJCheckBox> checkboxFactory, IHtmlHelper htmlHelper) : TagHelper 
{
    private IControlFactory<JJCheckBox> CheckboxFactory { get; set; } = checkboxFactory;

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
    
    [HtmlAttributeName("onchange")] 
    public string? OnChange { get; set; }

    [ViewContext] [HtmlAttributeNotBound] 
    public ViewContext ViewContext { get; set; } = null!;
    
    public bool Enabled { get; set; } = true;

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        Contextualize(ViewContext);
        var checkBox = CheckboxFactory.Create();
        checkBox.Name = Name ?? htmlHelper.Name(For!.Name) ?? throw new JJMasterDataException("Either for or name attributes are required.");
        checkBox.Enabled = Enabled;
        if (For is not null)
        {
            checkBox.IsChecked = For?.Model is true;
        }
        else
        {
            checkBox.IsChecked = IsChecked;
        }

        if (OnChange is not null)
            checkBox.Attributes["onchange"] = OnChange;
        
        checkBox.IsSwitch = IsSwitch;
        checkBox.SwitchSize = SwitchSize ?? CheckBoxSwitchSize.Medium;
        checkBox.Text = Label;
        
        Configure?.Invoke(checkBox);
        
        output.TagMode = TagMode.StartTagAndEndTag;

        var htmlBuilder = checkBox.GetHtmlBuilder();
        
        output.Content.SetHtmlContent(htmlBuilder.ToString());
    }

    public void Contextualize(ViewContext viewContext)
    {
        if (htmlHelper is IViewContextAware aware) {
            aware.Contextualize(viewContext);
        }
    }
}