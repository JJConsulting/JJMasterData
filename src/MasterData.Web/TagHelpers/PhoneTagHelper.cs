using JJMasterData.Core.UI.Components.Phone;
using JJMasterData.Core.UI.Components;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace JJMasterData.Web.TagHelpers;

[HtmlTargetElement("jj-phone", TagStructure = TagStructure.WithoutEndTag)]
public sealed class PhoneTagHelper(
    IControlFactory<JJPhoneGroup> phoneFactory,
    IHtmlHelper htmlHelper) : TagHelper
{
    [HtmlAttributeName("asp-for")]
    public ModelExpression? For { get; set; }

    [HtmlAttributeName("name")]
    public string? Name { get; set; }

    [HtmlAttributeName("value")]
    public string? Value { get; set; }

    [HtmlAttributeName("id")]
    public string? Id { get; set; }

    [HtmlAttributeName("readonly")]
    public bool ReadOnly { get; set; }

    [ViewContext]
    [HtmlAttributeNotBound]
    public ViewContext ViewContext { get; set; } = null!;

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        if (htmlHelper is IViewContextAware aware)
            aware.Contextualize(ViewContext);

        var phone = phoneFactory.Create();
        phone.Name = Name ?? (For is null ? null : htmlHelper.Name(For.Name) ?? For.Name)!;
        phone.Text = Value ?? For?.Model?.ToString() ?? string.Empty;
        phone.Id = Id ?? string.Empty;
        phone.ReadOnly = ReadOnly;

        output.TagName = null;
        output.Content.SetHtmlContent(phone.GetHtmlBuilder());
    }
}
