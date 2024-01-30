using JJMasterData.Commons.Localization;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.Http.Abstractions;
using JJMasterData.Core.UI.Components;
using JJMasterData.Core.UI.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Localization;

namespace JJMasterData.Web.TagHelpers;

public class IconPickerTagHelper(IControlFactory<JJIconPicker> iconPickerFactory, IMasterDataUrlHelper urlHelper, IStringLocalizer<MasterDataResources> stringLocalizer) : TagHelper
{
    
    [HtmlAttributeName("for")] 
    public ModelExpression? For { get; set; }

    [HtmlAttributeName("name")] 
    public string? Name { get; set; }

    [HtmlAttributeName("value")] 
    public IconType? Value { get; set; }

    private IMasterDataUrlHelper UrlHelper { get; } = urlHelper;
    private IControlFactory<JJIconPicker> IconPickerFactory { get; } = iconPickerFactory;
    private IStringLocalizer<MasterDataResources> StringLocalizer { get; } = stringLocalizer;

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        var name = For?.Name ?? Name ?? throw new ArgumentException("For or Name properties are required.");

        IconType? modelValue = null;

        if (For is { Model: not null })
        {
            modelValue = (IconType)For.Model;
        }
        else if (Value is not null)
        {
            modelValue = Value;
        }
        var iconPicker = IconPickerFactory.Create();
        iconPicker.Name = name;
        if (modelValue != null) 
            iconPicker.SelectedIcon = modelValue.Value;

        output.TagMode = TagMode.StartTagAndEndTag;
        
        output.Content.SetHtmlContent((await iconPicker.GetHtmlBuilderAsync()).ToString());
    }
}