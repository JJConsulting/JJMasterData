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

public class IconPickerTagHelper(IControlFactory<JJComboBox> comboBoxFactory, IMasterDataUrlHelper urlHelper, IStringLocalizer<MasterDataResources> stringLocalizer) : TagHelper
{
    
    [HtmlAttributeName("for")] 
    public ModelExpression? For { get; set; }

    [HtmlAttributeName("name")] 
    public string? Name { get; set; }

    [HtmlAttributeName("value")] 
    public IconType? Value { get; set; }

    private IMasterDataUrlHelper UrlHelper { get; } = urlHelper;
    private IControlFactory<JJComboBox> ComboBoxFactory { get; } = comboBoxFactory;
    private IStringLocalizer<MasterDataResources> StringLocalizer { get; } = stringLocalizer;

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        var name = For?.Name ?? Name ?? throw new ArgumentException("For or Name properties are required.");

        int? modelValue = null;

        if (For is { Model: not null })
        {
            modelValue = (int)For.Model;
        }
        else if (Value is not null)
        {
            modelValue = (int)Value;
        }
        var comboBox = ComboBoxFactory.Create();
        comboBox.Name = name;
        comboBox.SelectedValue = modelValue.ToString();
        comboBox.DataItem = new FormElementDataItem
        {
            DataItemType = DataItemType.Manual,
            Items = new List<DataItemValue>(),
            FirstOption = FirstOptionMode.Choose,
            ShowIcon = true
        };

        foreach (var icon in Enum.GetValues<IconType>())
        {
            comboBox.DataItem.Items.Add(new DataItemValue
            {
                Id = ((int)icon).ToString(),
                Description = icon.ToString(),
                Icon = icon
            });
        }

        comboBox.Attributes["data-live-search"] = "true";
        comboBox.Attributes["data-virtual-scroll"] = "true";
        comboBox.Attributes["data-size"] = "false";
        comboBox.Attributes["data-sanitize"] = "false";

        var div = new HtmlBuilder(HtmlTag.Div);
        div.WithCssClass("input-group");
        await div.AppendControlAsync(comboBox);
        div.AppendDiv(div =>
        {
            var tooltip = StringLocalizer["Search Icon"];
            div.WithCssClass("btn btn-default");
            div.WithToolTip(tooltip);
            div.AppendComponent(new JJIcon(IconType.Search));
            var url = UrlHelper.Action("Index", "Icons", new { inputId = name });
            div.WithAttribute("onclick", $"iconsModal.showUrl('{url}', '{tooltip}', '{(int)ModalSize.ExtraLarge}')");
        });
        
        output.TagMode = TagMode.StartTagAndEndTag;
        
        output.Content.SetHtmlContent(div.ToString());
    }
}