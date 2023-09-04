using System.Threading.Tasks;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.UI.Components;
using JJMasterData.Core.UI.Components.Controls;
using JJMasterData.Core.Web.Html;
using JJMasterData.Core.Web.Http.Abstractions;

namespace JJMasterData.Core.Web.Components;

public class JJTextBox : ControlBase
{
    public InputType InputType { get; set; }

    public int NumberOfDecimalPlaces { get; set; }

    public double? MinValue
    {
        get
        {
            if (double.TryParse(GetAttr(FormElementField.MinValueAttribute), out var minVal))
                return minVal;
            
            return null;
        }
        set
        {
            SetAttr(FormElementField.MinValueAttribute, value);
        }
    }

    public double? MaxValue
    {
        get
        {
            if (double.TryParse(GetAttr(FormElementField.MaxValueAttribute), out var minVal))
                return minVal;
            
            return null;
        }
        set
        {
            SetAttr(FormElementField.MaxValueAttribute, value);
        }
    }

    public JJTextBox(IHttpRequest request) : base(request)
    {
        InputType = InputType.Text;
        Visible = true;
        Enabled = true;
    }
    
    protected override async Task<ComponentResult> BuildResultAsync()
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
            .WithAttributeIfNotEmpty("placeholder", PlaceHolder)
            .WithAttribute("type", inputType)
            .WithCssClass("form-control")
            .WithCssClass(CssClass)
            .WithToolTip(ToolTip)
            .WithAttributeIf(MaxLength > 0, "maxlength", MaxLength.ToString())
            .WithAttributeIf(NumberOfDecimalPlaces == 0 && InputType == InputType.Number, "onkeypress",
                "return jjutil.justNumber(event);")
            .WithAttributeIf(NumberOfDecimalPlaces > 0 && InputType == InputType.Number, "jjdecimalplaces",
                NumberOfDecimalPlaces.ToString())
            .WithCssClassIf(NumberOfDecimalPlaces > 0 && InputType == InputType.Number, "jjdecimal")
            .WithAttributeIfNotEmpty("value", Text)
            .WithAttributeIf(ReadOnly, "readonly", "readonly")
            .WithAttributeIf(!Enabled, "disabled", "disabled");

        var result = new RenderedComponentResult(html);

        return await Task.FromResult(result);
    }
}