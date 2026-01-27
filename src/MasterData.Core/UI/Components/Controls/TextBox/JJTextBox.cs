using System.Globalization;
using System.Threading.Tasks;
using JJConsulting.Html;
using JJConsulting.Html.Bootstrap.Extensions;
using JJConsulting.Html.Extensions;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.Extensions;
using JJMasterData.Core.Http.Abstractions;

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
            if (double.TryParse(GetAttribute(FormElementField.MinValueAttribute), out var minVal))
                return minVal;
            
            return null;
        }
        set => SetAttribute(FormElementField.MinValueAttribute, value.ToString());
    }

    public double? MaxValue
    {
        get
        {
            if (double.TryParse(GetAttribute(FormElementField.MaxValueAttribute), out var minVal))
                return minVal;
            
            return null;
        }
        set => SetAttribute(FormElementField.MaxValueAttribute, value.ToString());
    }

    public JJTextBox(IFormValues formValues) : base(formValues)
    {
        InputType = InputType.Text;
        Visible = true;
        Enabled = true;
    }
    
    protected internal override ValueTask<HtmlBuilder> GetHtmlBuilderAsync()
    {
        var html = GetHtmlBuilder();
        
        return html.AsValueTask();
    }

    public virtual HtmlBuilder GetHtmlBuilder()
    {
        var inputType = InputType.GetInputType();
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
            .WithAttributeIf(NumberOfDecimalPlaces == 0 && InputType is InputType.Number, "onkeypress",
                "return jjutil.justNumber(event);")
            .WithAttributeIf(NumberOfDecimalPlaces > 0 && InputType is InputType.Number or InputType.Currency, "jj-decimal-places",
                NumberOfDecimalPlaces.ToString())
            
            .WithAttributeIf(NumberOfDecimalPlaces > 0 && InputType is InputType.Number, "jj-decimal-separator",CultureInfo.NumberFormat.NumberDecimalSeparator)
            .WithAttributeIf(NumberOfDecimalPlaces > 0 && InputType is InputType.Number, "jj-group-separator",CultureInfo.NumberFormat.NumberGroupSeparator)
            
            .WithAttributeIf(NumberOfDecimalPlaces > 0 && InputType is InputType.Currency, "jj-decimal-separator",CultureInfo.NumberFormat.CurrencyDecimalSeparator)
            .WithAttributeIf(NumberOfDecimalPlaces > 0 && InputType is InputType.Currency, "jj-group-separator",CultureInfo.NumberFormat.CurrencyGroupSeparator)
            
            .WithCssClassIf(NumberOfDecimalPlaces > 0 && InputType is InputType.Number or InputType.Currency, "jj-numeric")
            .WithAttributeIfNotEmpty("value", Text)
            .WithAttributeIf(ReadOnly, "readonly", "readonly")
            .WithAttributeIf(!Enabled, "disabled", "disabled");
        return html;
    }
}