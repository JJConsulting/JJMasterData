using System.Threading.Tasks;
using JJConsulting.Html;
using JJConsulting.Html.Extensions;
using JJMasterData.Core.Html;
using JJMasterData.Core.Http.Abstractions;

using Microsoft.Extensions.Localization;

namespace JJMasterData.Core.UI.Components;

public class JJTextArea : ControlBase, IFloatingLabelControl
{
    
    public string FloatingLabel { get; set; }
    public bool UseFloatingLabel { get; set; }
    
    private IStringLocalizer<MasterDataResources> StringLocalizer { get; }
    public int Rows { get; set; }

    public JJTextArea(IFormValues formValues,IStringLocalizer<MasterDataResources> stringLocalizer) : base(formValues)
    {
        StringLocalizer = stringLocalizer;
        Attributes.Add("class", "form-control");
        Rows = 5;
    }

    protected override ValueTask<ComponentResult> BuildResultAsync()
    {
        var html = GetHtmlBuilder();

        return new ValueTask<ComponentResult>(new RenderedComponentResult(html));
    }

    public HtmlBuilder GetHtmlBuilder()
    {
        var hasMaxLength = MaxLength > 0;
        var textArea = new HtmlBuilder(HtmlTag.TextArea)
            .WithAttributes(Attributes)
            .WithNameAndId(Name)
            .WithCssClass(CssClass)
            .WithToolTip(Tooltip)
            .WithAttributeIf(!string.IsNullOrWhiteSpace(PlaceHolder), "placeholder", PlaceHolder)
            .WithAttribute("cols", "20")
            .WithAttribute("rows", Rows.ToString())
            .WithAttributeIf(hasMaxLength,"maximum-limit-of-characters-label", StringLocalizer["Maximum limit of {0} characters!"])
            .WithAttributeIf(hasMaxLength,"characters-remaining-label", StringLocalizer["({0} characters remaining)"])
            .WithAttributeIf(hasMaxLength, "maxlength", MaxLength.ToString())
            .WithAttributeIf(ReadOnly, "readonly", "readonly")
            .WithAttributeIf(!Enabled, "disabled", "disabled")
            .AppendText(Text);

        if (UseFloatingLabel)
        {
            textArea.WithAttribute("placeholder");

            return new HtmlBuilder(HtmlTag.Div)
                .WithCssClass("form-floating")
                .Append(textArea)
                .AppendLabel(label =>
                {
                    label.AppendText(FloatingLabel);
                    label.WithAttribute("for",Name);
                });
        }
        
        return textArea;
    }
}