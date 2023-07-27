using JJMasterData.Commons.Localization;
using JJMasterData.Core.Web.Html;
using JJMasterData.Core.Web.Http.Abstractions;

namespace JJMasterData.Core.Web.Components;

public class JJTextBox : JJBaseControl
{
    public InputType InputType { get; set; }

    public int NumberOfDecimalPlaces { get; set; }

    public double? MinValue { get; set; }

    public double? MaxValue { get; set; }

    public JJTextBox(IHttpContext httpContext) : base(httpContext)
    {
        InputType = InputType.Text;
        Visible = true;
        Enabled = true;
    }

    internal override HtmlBuilder RenderHtml()
    {
        string inputType = InputType.ToString().ToLower();
        if (NumberOfDecimalPlaces > 0)
        {
            inputType = "text";
            CssClass += " jjdecimal";
        }
        
        var html = new HtmlBuilder(HtmlTag.Input)
            .WithNameAndId(Name)
            .WithAttributes(Attributes)
            .WithAttributeIf(!string.IsNullOrWhiteSpace(PlaceHolder), "placeholder", PlaceHolder)
            .WithAttribute("type", inputType)
            .WithCssClass("form-control")
            .WithCssClass(CssClass)
            .WithToolTip(ToolTip)
            .WithAttributeIf(MaxLength > 0, "maxlength", MaxLength.ToString())
            .WithAttributeIf(NumberOfDecimalPlaces == 0 && InputType == InputType.Number, "onkeypress", "return jjutil.justNumber(event);")
            .WithAttributeIf(NumberOfDecimalPlaces > 0 && InputType == InputType.Number, "jjdecimalplaces", NumberOfDecimalPlaces.ToString())
            .WithCssClassIf(NumberOfDecimalPlaces > 0 && InputType == InputType.Number, "jjdecimal")
            .WithAttributeIf(MinValue != null, "min", MinValue?.ToString())
            .WithAttributeIf(MaxValue != null, "max", MaxValue?.ToString())
            .WithAttributeIf(!string.IsNullOrEmpty(Text), "value", Text)
            .WithAttributeIf(ReadOnly, "readonly", "readonly")
            .WithAttributeIf(!Enabled, "disabled", "disabled");

        return html;
    }

}
