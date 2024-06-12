using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.UI.Components;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace JJMasterData.Web.TagHelpers;

public class IconPickerTagHelper(
    IHtmlHelper htmlHelper,
    IControlFactory<JJIconPicker> iconPickerFactory) : TagHelper
{
    
    [HtmlAttributeName("for")] 
    public ModelExpression? For { get; set; }

    [HtmlAttributeName("id")] 
    public string? Id { get; set; }
    
    [HtmlAttributeName("name")] 
    public string? Name { get; set; }

    [HtmlAttributeName("value")] 
    public IconType? Value { get; set; }

    [HtmlAttributeName("enabled")]
    public bool Enabled { get; set; } = true;
    
    [ViewContext] 
    [HtmlAttributeNotBound] 
    public ViewContext ViewContext { get; set; } = null!;

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        Contextualize(ViewContext);
        
        var name =  Name ?? htmlHelper.Name(For?.Name) ?? throw new ArgumentException("For or Name properties are required.");

        var id = Id ?? htmlHelper.Id(For?.Name) ?? name;
        
        IconType? modelValue = null;

        if (For is { Model: not null })
        {
            modelValue = (IconType)For.Model;
        }
        else if (Value is not null)
        {
            modelValue = Value;
        }
        var iconPicker = iconPickerFactory.Create();
        iconPicker.Id = id;
        iconPicker.Name = name;
        iconPicker.Enabled = Enabled;
        
        if (modelValue != null) 
            iconPicker.SelectedIcon = modelValue.Value;

        output.TagMode = TagMode.StartTagAndEndTag;
        
        output.Content.SetHtmlContent((await iconPicker.GetHtmlBuilderAsync()).ToString());
    }
    
    public void Contextualize(ViewContext viewContext)
    {
        if (htmlHelper is IViewContextAware aware) {
            aware.Contextualize(viewContext);
        }
    }
}