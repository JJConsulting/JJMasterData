using JJMasterData.Commons.Language;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataManager;
using JJMasterData.Core.Html;

namespace JJMasterData.Core.WebComponents;

public class JJCheckBox : JJBaseControl
{
    private bool? _isChecked;

    /// <remarks>
    /// Default: 1
    /// </remarks>
    public string Value { get; set; }
    
    public bool IsChecked
    {
        get
        {
            if (_isChecked == null && IsPostBack)
                _isChecked = Value.Equals(CurrentContext.Request[Name]);

            return _isChecked ?? false;
        }
        set => _isChecked = value;
    }

    public JJCheckBox()
    {
        Visible = true;
        Enabled = true;
        Value = "1";
    }

    internal static JJCheckBox GetInstance(FormElementField f, object value)
    {
        var check = new JJCheckBox();
        check.Name = f.Name;
        check.IsChecked = ExpressionManager.ParseBool(value);
        check.ToolTip = f.HelpDescription;
        return check;
    }

    internal override HtmlBuilder RenderHtml()
    {
        var html = new HtmlBuilder(HtmlTag.Div)
            .WithCssClass(BootstrapHelper.Version == 3 ? "form-check" : "checkbox")
            .WithCssClassIf(!Enabled, "disabled")
            .AppendElement(HtmlTag.Label, label =>
            {
                label.AppendElement(GetInputHtml());
                label.AppendText(Translate.Key(Text));
                
            });

        return html;
    }

    private HtmlBuilder GetInputHtml()
    {
        var input = new HtmlBuilder(HtmlTag.Input)
            .WithAttributes(Attributes)
            .WithAttribute("type", "checkbox")
            .WithNameAndId(Name)
            .WithAttribute("value", Value)
            .WithCssClass("form-check-input")
            .WithCssClass(CssClass)
            .WithToolTip(Translate.Key(ToolTip))
            .WithAttributeIf(IsChecked, "checked", "checked")
            .WithAttributeIf(!Enabled, "disabled", "disabled");

        return input;
    }
    
}
