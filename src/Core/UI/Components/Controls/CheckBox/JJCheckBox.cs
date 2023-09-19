using System.Threading.Tasks;
using JJMasterData.Core.Http.Abstractions;
using JJMasterData.Core.UI.Components;
using JJMasterData.Core.Web.Html;

namespace JJMasterData.Core.Web.Components;

public class JJCheckBox : ControlBase
{
    private bool? _isChecked;

    /// <remarks>
    /// Default: 1
    /// </remarks>
    public string Value { get; set; }

    public new string Text { get; set;}

    public bool IsChecked
    {
        get
        {
            if (_isChecked == null && FormValues.ContainsFormValues())
                _isChecked = Value.Equals(FormValues[Name]);

            return _isChecked ?? false;
        }
        set => _isChecked = value;
    }

    public JJCheckBox(IFormValues formValues) : base(formValues)
    {
        Visible = true;
        Enabled = true;
        Value = "1";
    }


    protected override async Task<ComponentResult> BuildResultAsync()
    {
        var html = new HtmlBuilder(HtmlTag.Div)
            .WithCssClass(BootstrapHelper.Version == 3 ? "form-check" : "checkbox")
            .WithCssClassIf(!Enabled, "disabled")
            .Append(GetInputHtml());

        var result = new RenderedComponentResult(html);
        
        return await Task.FromResult(result);
        
    }

    private HtmlBuilder GetInputHtml()
    {
        var div = new HtmlBuilder(HtmlTag.Div);

        div.WithCssClass("form-check");
        
        div.Append(HtmlTag.Input, input =>
        {
            var checkboxHelperScript = $"CheckboxHelper.check('{Name}', '{Value}')";
            
            if (Attributes.ContainsKey("onchange"))
            {
                Attributes["onchange"] += checkboxHelperScript;
            }
            else
            {
                Attributes["onchange"] = checkboxHelperScript;
            }
            
            input.WithAttributes(Attributes)
                .WithAttribute("type", "checkbox")
                .WithNameAndId(Name)
                .WithAttribute("value", Value)
                .WithCssClass("form-check-input")
                .WithCssClass(CssClass)
                .WithToolTip(Tooltip)
                .WithAttributeIf(IsChecked, "checked", "checked")
                .WithAttributeIf(!Enabled, "disabled", "disabled");
        });

        div.AppendHiddenInput($"{Name}-hidden", IsChecked ? Value : "0");

        div.AppendIf(!string.IsNullOrEmpty(Text), HtmlTag.Label, label =>
        {
            label.WithAttribute("for", Name);
            label.AppendText(Text);
        });

        return div;
    }
}
