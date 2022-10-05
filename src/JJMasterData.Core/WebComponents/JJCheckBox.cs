using JJMasterData.Commons.Language;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.Html;

namespace JJMasterData.Core.WebComponents;

public class JJCheckBox : JJBaseView
{
    private bool? _isChecked;
    
    public bool Enabled { get; set; }
    
    public string ToolTip { get; set; }

    /// <remarks>
    /// Default: 1
    /// </remarks>
    public string Value { get; set; }
    
    /// <summary>
    /// Description at the left of the component
    /// </summary>
    public string Text { get; set; }
    
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

    internal static JJCheckBox GetInstance(FormElementField f,
                                 PageState pagestate,
                                 bool isChecked,
                                 bool enable = true,
                                 string name = null)
    {
        JJCheckBox check = new()
        {
            Name = name ?? f.Name,
            Enabled = enable,
            IsChecked = isChecked
        };
        
        if (pagestate != PageState.List)
            check.Text = string.IsNullOrEmpty(f.Label) ? f.Name : f.Label;

        check.ToolTip = f.HelpDescription;
        return check;
    }

    internal override HtmlElement GetHtmlElement()
    {
        var html = new HtmlElement(HtmlTag.Div)
            .WithCssClass(BootstrapHelper.Version == 3 ? "form-check" : "checkbox")
            .WithCssClassIf(!Enabled, "disabled")
            .AppendElement(HtmlTag.Label, label =>
            {
                label.AppendElement(GetInputHtml());
                label.AppendText(Translate.Key(Text));
                
            });

        return html;
    }

    private HtmlElement GetInputHtml()
    {
        var input = new HtmlElement(HtmlTag.Input)
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
