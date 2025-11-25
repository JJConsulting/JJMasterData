using System.Threading.Tasks;
using JJConsulting.Html;
using JJConsulting.Html.Extensions;
using JJMasterData.Core.Html;
using JJMasterData.Core.Http.Abstractions;

using Microsoft.Extensions.Localization;

namespace JJMasterData.Core.UI.Components;

public class JJCheckBox : ControlBase
{
    private readonly IStringLocalizer<MasterDataResources> _stringLocalizer;
    private bool? _isChecked;

    /// <remarks>
    /// Default: "true"
    /// </remarks>
    public string Value { get; set; }

    public CheckboxLayout Layout { get; set; }

    private bool IsSwitch => Layout is CheckboxLayout.Switch;
    private bool IsButton => Layout is CheckboxLayout.Button;

    public new string Text { get; set; }

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

    public JJCheckBox(IFormValues formValues, IStringLocalizer<MasterDataResources> stringLocalizer) : base(formValues)
    {
        _stringLocalizer = stringLocalizer;
        Visible = true;
        Enabled = true;
        Value = "true";
    }


    protected override ValueTask<ComponentResult> BuildResultAsync()
    {
        var html = GetHtmlBuilder();

        var result = new RenderedComponentResult(html);

        return new ValueTask<ComponentResult>(result);
    }

    public HtmlBuilder GetHtmlBuilder()
    {
        var html = new HtmlBuilder(HtmlTag.Div)
            .WithCssClassIf(!IsButton, BootstrapHelper.Version == 3 ? "form-check" : "checkbox")
            .WithCssClassIf(!Enabled, "disabled")
            .Append(GetInputHtml());
        return html;
    }

    private HtmlBuilder GetInputHtml()
    {
        var div = new HtmlBuilder(HtmlTag.Div);

        div.WithCssClassIf(IsSwitch, "form-switch");
        div.WithCssClassIf(!string.IsNullOrEmpty(Text) && !IsButton, "form-check");

        var checkBoxName = Name + "-checkbox";
        div.Append(HtmlTag.Input, input =>
        {
            input.WithAttribute("hidden", "hidden");
            input.WithName(Name);
            input.WithId(Name.Replace(".", "_"));
            input.WithValue(IsChecked ? Value : "false");
        });
        
        div.Append(HtmlTag.Input, input =>
        {
            var checkboxHelperScript = $"CheckboxHelper.check('{Name.Replace(".", "_")}');";

            if (Attributes.TryGetValue("onchange", out string value))
            {
                Attributes["onchange"] = checkboxHelperScript + value;
            }
            else
            {
                Attributes["onchange"] = checkboxHelperScript;
            }

            if (ReadOnly)
                Attributes["onclick"] = "return false";

            input.WithAttributes(Attributes)
                .WithAttribute("type", "checkbox")
                .WithName(checkBoxName)
                .WithId(checkBoxName.Replace(".", "_"))
                .WithAttribute("value", Value)
                .WithCssClass(IsButton ? "btn-check" : "form-check-input")
                .WithAttributeIf(IsSwitch && BootstrapHelper.Version is 3, "data-toggle", "toggle")
                .WithAttributeIf(IsSwitch && BootstrapHelper.Version is 3, "data-on", _stringLocalizer["Yes"])
                .WithAttributeIf(IsSwitch && BootstrapHelper.Version is 3, "data-off", _stringLocalizer["No"])
                .WithAttributeIf(IsSwitch && BootstrapHelper.Version is 3, "data-size", "small")
                .WithAttributeIf(IsSwitch, "role", "switch")
                .WithCssClass(CssClass)
                .WithAttributeIf(IsChecked, "checked", "checked")
                .WithAttributeIf(!Enabled, "disabled", "disabled");
        });
        
        if (IsButton)
        {
            div.Append(HtmlTag.Label, label =>
            {
                label.WithCssClass("btn btn-outline-primary")
                    .WithAttribute("for", checkBoxName.Replace(".", "_"))
                    .AppendText(Text);
            });
        }
        else
        {
            div.AppendIf(!string.IsNullOrEmpty(Text), HtmlTag.Label, label =>
            {
                label.WithAttribute("for", checkBoxName.Replace(".", "_"));
                label.WithCssClass("form-check-label");
                label.WithToolTip(Tooltip);
                label.AppendText(Text);
            });
        }

        return div;
    }
}