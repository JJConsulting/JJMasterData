using JJMasterData.Commons.Localization;
using JJMasterData.Core.UI.Components.Controls;
using JJMasterData.Core.Web.Html;
using JJMasterData.Core.Web.Http.Abstractions;
using Microsoft.Extensions.Localization;

namespace JJMasterData.Core.Web.Components;

public class JJTextArea : HtmlControl
{
    private IStringLocalizer<JJMasterDataResources> StringLocalizer { get; }
    public int Rows { get; set; }

    public JJTextArea(IHttpContext httpContext, IStringLocalizer<JJMasterDataResources> stringLocalizer) : base(httpContext)
    {
        StringLocalizer = stringLocalizer;
        Attributes.Add("class", "form-control");
        Rows = 5;
    }

    internal override HtmlBuilder BuildHtml()
    {
        var html = new HtmlBuilder(HtmlTag.TextArea)
            .WithAttributes(Attributes)
            .WithNameAndId(Name)
            .WithCssClass(CssClass)
            .WithToolTip(ToolTip)
            .WithAttributeIf(!string.IsNullOrWhiteSpace(PlaceHolder), "placeholder", PlaceHolder)
            .WithAttribute("cols", "20")
            .WithAttribute("rows", Rows)
            .WithAttribute("maximum-limit-of-characters-label", StringLocalizer["Maximum limit of {0} characters!"])
            .WithAttribute("characters-remaining-label", StringLocalizer["({0} characters remaining)"])
            .WithAttributeIf(MaxLength > 0, "maxlength", MaxLength.ToString())
            .WithAttributeIf(ReadOnly, "readonly", "readonly")
            .WithAttributeIf(!Enabled, "disabled", "disabled")
            .AppendText(Text);

        return html;
    }
}