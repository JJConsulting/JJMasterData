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
        var html = HtmlBuilder.Input()
            .WithNameAndId(Name)
            .WithAttributes(Attributes)
            .WithAttributeIfNotEmpty("placeholder", PlaceHolder)
            .WithCssClass("form-control")
            .WithCssClass(CssClass)
            .WithToolTip(Tooltip)
            .WithAttributeIf(MaxLength > 0, "maxlength", MaxLength.ToString());

        if (InputType.IsNumeric && NumberOfDecimalPlaces > 0)
        {
            var numberFormat = CultureInfo.NumberFormat;

            html.WithAttribute("type", "text");
            html.WithCssClass("jj-numeric");
            html.WithAttribute("jj-decimal-places", NumberOfDecimalPlaces.ToString());

            html.WithAttribute("jj-decimal-separator",
                    InputType == InputType.Number
                        ? numberFormat.NumberDecimalSeparator
                        : numberFormat.CurrencyDecimalSeparator)
                .WithAttribute("jj-group-separator",
                    InputType == InputType.Number
                        ? numberFormat.NumberGroupSeparator
                        : numberFormat.CurrencyGroupSeparator);
        }
        else
        {
            html.WithAttribute("type", InputType.GetHtmlInputType());
        }

        html.WithAttributeIfNotEmpty("value", Text)
            .WithAttributeIf(NumberOfDecimalPlaces == 0 && InputType is InputType.Number, "onkeypress",
                "return jjutil.justNumber(event);")
            .WithAttributeIf(ReadOnly, "readonly", "readonly")
            .WithAttributeIf(!Enabled, "disabled", "disabled");

        return html;
    }
}