using System.Threading.Tasks;
using JJMasterData.Commons.Localization;
using JJMasterData.Core.Http.Abstractions;
using JJMasterData.Core.UI.Html;
using Microsoft.Extensions.Localization;

namespace JJMasterData.Core.UI.Components;

public class JJTextArea : ControlBase
{
    private IStringLocalizer<MasterDataResources> StringLocalizer { get; }
    public int Rows { get; set; }

    public JJTextArea(IFormValues formValues,IStringLocalizer<MasterDataResources> stringLocalizer) : base(formValues)
    {
        StringLocalizer = stringLocalizer;
        Attributes.Add("class", "form-control");
        Rows = 5;
    }

    protected override async Task<ComponentResult> BuildResultAsync()
    {
        var html = GetHtmlBuilder();

        return await Task.FromResult(new RenderedComponentResult(html));
    }

    public HtmlBuilder GetHtmlBuilder()
    {
        var html = new HtmlBuilder(HtmlTag.TextArea)
            .WithAttributes(Attributes)
            .WithNameAndId(Name)
            .WithCssClass(CssClass)
            .WithToolTip(Tooltip)
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