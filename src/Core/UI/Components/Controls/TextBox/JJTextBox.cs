using System.Globalization;
using System.Threading.Tasks;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.Http.Abstractions;
using JJMasterData.Core.UI.Html;

namespace JJMasterData.Core.UI.Components;

public class JJTextBox : ControlBase
{
    public InputType InputType { get; set; }

    public int NumberOfDecimalPlaces { get; set; }

    public CultureInfo CultureInfo { get; set; } = CultureInfo.CurrentCulture;
    
    public double? MinValue
    {
        get
        {
            if (double.TryParse(GetAttr(FormElementField.MinValueAttribute), out var minVal))
                return minVal;
            
            return null;
        }
        set => SetAttr(FormElementField.MinValueAttribute, value);
    }

    public double? MaxValue
    {
        get
        {
            if (double.TryParse(GetAttr(FormElementField.MaxValueAttribute), out var minVal))
                return minVal;
            
            return null;
        }
        set => SetAttr(FormElementField.MaxValueAttribute, value);
    }

    public JJTextBox(IFormValues formValues) : base(formValues)
    {
        InputType = InputType.Text;
        Visible = true;
        Enabled = true;
    }
    
    protected override ValueTask<ComponentResult> BuildResultAsync()
    {
        var html = GetHtmlBuilder();

        var result = new RenderedComponentResult(html);

        return new ValueTask<ComponentResult>(result);
    }

    public virtual HtmlBuilder GetHtmlBuilder()
    {
        string inputType = InputType.ToString().ToLower();
        if (NumberOfDecimalPlaces > 0)
        {
            inputType = "text";
            CssClass += " jj-numeric";
        }

        var html = new HtmlBuilder(HtmlTag.Input)
            .WithNameAndId(Name)
            .WithAttributes(Attributes)
            .WithAttributeIfNotEmpty("placeholder", PlaceHolder)
            .WithAttribute("type", inputType)
            .WithCssClass("form-control")
            .WithCssClass(CssClass)
            .WithToolTip(Tooltip)
            .WithAttributeIf(MaxLength > 0, "maxlength", MaxLength.ToString())
            .WithAttributeIf(NumberOfDecimalPlaces == 0 && InputType == InputType.Number, "onkeypress",
                "return jjutil.justNumber(event);")
            .WithAttributeIf(NumberOfDecimalPlaces > 0 && InputType is InputType.Number or InputType.Currency, "jj-decimal-places",
                NumberOfDecimalPlaces.ToString())
            
            .WithAttributeIf(NumberOfDecimalPlaces > 0 && InputType == InputType.Number, "jj-decimal-separator",CultureInfo.NumberFormat.NumberDecimalSeparator)
            .WithAttributeIf(NumberOfDecimalPlaces > 0 && InputType == InputType.Number, "jj-group-separator",CultureInfo.NumberFormat.NumberGroupSeparator)
            
            .WithAttributeIf(NumberOfDecimalPlaces > 0 && InputType == InputType.Currency, "jj-decimal-separator",CultureInfo.NumberFormat.CurrencyDecimalSeparator)
            .WithAttributeIf(NumberOfDecimalPlaces > 0 && InputType == InputType.Currency, "jj-group-separator",CultureInfo.NumberFormat.CurrencyGroupSeparator)
            
            .WithCssClassIf(NumberOfDecimalPlaces > 0 && InputType  is InputType.Number or InputType.Currency, "jj-numeric")
            .WithAttributeIfNotEmpty("value", Text)
            .WithAttributeIf(ReadOnly, "readonly", "readonly")
            .WithAttributeIf(!Enabled, "disabled", "disabled");
        return html;
    }
}