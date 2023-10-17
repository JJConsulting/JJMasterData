using System.Threading.Tasks;
using JJMasterData.Commons.Extensions;
using JJMasterData.Commons.Localization;
using JJMasterData.Core.Http.Abstractions;
using JJMasterData.Core.UI.Html;
using Microsoft.Extensions.Localization;

namespace JJMasterData.Core.UI.Components;

public class JJCheckBox : ControlBase
{
    private IStringLocalizer<JJMasterDataResources> StringLocalizer { get; }
    private bool? _isChecked;

    /// <remarks>
    /// Default: "true"
    /// </remarks>
    public string Value { get; set; }
    
    public bool IsSwitch { get; set; }

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

    public CheckBoxSwitchSize? SwitchSize { get; set; }
    
    public JJCheckBox(IFormValues formValues, IStringLocalizer<JJMasterDataResources> stringLocalizer) : base(formValues)
    {
        StringLocalizer = stringLocalizer;
        Visible = true;
        Enabled = true;
        Value = "true";
    }


    protected override async Task<ComponentResult> BuildResultAsync()
    {
        var html = GetHtmlBuilder();

        var result = new RenderedComponentResult(html);
        
        return await Task.FromResult(result);
        
    }

    public HtmlBuilder GetHtmlBuilder()
    {
        var html = new HtmlBuilder(HtmlTag.Div)
            .WithCssClass(BootstrapHelper.Version == 3 ? "form-check" : "checkbox")
            .WithCssClassIf(!Enabled, "disabled")
            .Append(GetInputHtml());
        return html;
    }

    private HtmlBuilder GetInputHtml()
    {
        var div = new HtmlBuilder(HtmlTag.Div);

        div.WithCssClassIf(IsSwitch, $"form-switch");
        div.WithCssClassIf(IsSwitch && SwitchSize is not null, SwitchSize?.GetDescription());
        div.WithCssClass("form-check");
        
        div.Append(HtmlTag.Input, input =>
        {
            var checkboxHelperScript = $"CheckboxHelper.check('{Name}')";
            
            if (Attributes.ContainsKey("onchange"))
            {
                Attributes["onchange"] += checkboxHelperScript;
            }
            else
            {
                Attributes["onchange"] = checkboxHelperScript;
            }

            if (ReadOnly)
                Attributes["onclick"] = "return false";
            
            input.WithAttributes(Attributes)
                .WithAttribute("type", "checkbox")
                .WithNameAndId(Name +"-checkbox")
                .WithAttribute("value", Value)
                .WithCssClass("form-check-input")
                .WithAttributeIf(IsSwitch && BootstrapHelper.Version is 3,"data-toggle","toggle")
                .WithAttributeIf(IsSwitch && BootstrapHelper.Version is 3,"data-on",StringLocalizer["Yes"])
                .WithAttributeIf(IsSwitch && BootstrapHelper.Version is 3,"data-off",StringLocalizer["No"])
                .WithAttributeIf(IsSwitch && BootstrapHelper.Version is 3,"data-size","small")
                .WithAttributeIf(IsSwitch,"role","switch")
                .WithCssClass(CssClass)
                .WithToolTip(Tooltip)
                .WithAttributeIf(IsChecked, "checked", "checked")
                .WithAttributeIf(!Enabled, "disabled", "disabled");
        });

        div.AppendHiddenInput($"{Name}", IsChecked ? Value : "false");

        div.AppendIf(!string.IsNullOrEmpty(Text), HtmlTag.Label, label =>
        {
            label.WithAttribute("for", Name);
            label.WithCssClass("form-check-label");
            label.AppendText(Text);
        });

        return div;
    }
}