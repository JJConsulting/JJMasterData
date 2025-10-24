using JJMasterData.Commons.Exceptions;
using JJMasterData.Core.UI.Components;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Localization;

namespace JJMasterData.Web.TagHelpers;

public sealed class CheckboxTagHelper(
    IStringLocalizer<MasterDataResources> stringLocalizer,
    IControlFactory<JJCheckBox> checkboxFactory,
    IHtmlHelper htmlHelper) : TagHelper
{
    [HtmlAttributeName("configure")]
    public Action<JJCheckBox>? Configure { get; set; }

    [HtmlAttributeName("name")]
    public string? Name { get; set; }

    [HtmlAttributeName("for")]
    public ModelExpression? For { get; set; }

    [HtmlAttributeName("value")]
    public bool IsChecked { get; set; }

    [HtmlAttributeName("layout")]
    public CheckboxLayout Layout { get; set; }

    [HtmlAttributeName("label")]
    public string? Label { get; set; }

    [HtmlAttributeName("onchange")]
    public string? OnChange { get; set; }

    [HtmlAttributeName("tooltip")]
    public string? Tooltip { get; set; }

    [HtmlAttributeName("show-label")]
    public bool ShowLabel { get; set; } = true;

    [ViewContext]
    [HtmlAttributeNotBound]
    public ViewContext ViewContext { get; set; } = null!;

    public bool Enabled { get; set; } = true;

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        Contextualize(ViewContext);
        
        var checkBox = checkboxFactory.Create();
        checkBox.Name = Name ?? htmlHelper.Name(For!.Name) ??
            throw new JJMasterDataException("Either for or name attributes are required.");
        
        checkBox.Enabled = Enabled;
        var displayName = Label ?? For?.ModelExplorer.Metadata.GetDisplayName();
        if (For is not null && !IsChecked)
        {
            checkBox.IsChecked = For?.Model is true;
        }
        else
        {
            checkBox.IsChecked = IsChecked;
        }

        if (OnChange is not null)
            checkBox.Attributes["onchange"] = OnChange;

        checkBox.Layout = Layout;

        if (ShowLabel)
            checkBox.Text = stringLocalizer[displayName ?? string.Empty];

        checkBox.Tooltip = Tooltip;
        Configure?.Invoke(checkBox);
        
        output.TagName = null;
        output.TagMode = TagMode.StartTagAndEndTag;

        var htmlBuilder = checkBox.GetHtmlBuilder();
        output.Content.SetHtmlContent(htmlBuilder.ToString());
    }

    private void Contextualize(ViewContext viewContext)
    {
        if (htmlHelper is IViewContextAware aware)
        {
            aware.Contextualize(viewContext);
        }
    }
}