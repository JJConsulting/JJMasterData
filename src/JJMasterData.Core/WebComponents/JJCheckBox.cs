using JJMasterData.Commons.Language;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.Html;

namespace JJMasterData.Core.WebComponents;

public class JJCheckBox : JJBaseView
{
    private bool? _IsChecked;

    /// <summary>
    /// Obtém ou define um valor que indica se o controle está habilitado.
    /// </summary>
    public bool Enable { get; set; }
    
    /// <summary>
    /// Texto exibido quando o ponteiro do mouse passa sobre o controle
    /// </summary>
    public string ToolTip { get; set; }

    /// <summary>
    /// Valor do campo (Default = 1)
    /// </summary>
    public string Value { get; set; }
    
    /// <summary>
    /// Descrição alinhado a direita do controle
    /// </summary>
    public string Text { get; set; }

    /// <summary>
    /// Selecionado Sim | Não
    /// </summary>
    public bool IsChecked
    {
        get
        {
            if (_IsChecked == null && IsPostBack)
                _IsChecked = Value.Equals(CurrentContext.Request[Name]);

            return (!_IsChecked.HasValue ? false : _IsChecked.Value);
        }
        set
        {
            _IsChecked = value;
        }
    }

    public JJCheckBox()
    {
        Visible = true;
        Enable = true;
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
            Enable = enable,
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
            .WithCssClassIf(!Enable, "disabled")
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
            .WithAttributeIf(!Enable, "disabled", "disabled");

        return input;
    }
    
}
