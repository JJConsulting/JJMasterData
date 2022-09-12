using System.Collections;
using System.Text;
using JJMasterData.Commons.Language;
using JJMasterData.Core.DataDictionary;

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
            {
                _IsChecked = Value.Equals(CurrentContext.Request[Name]);
            }

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

    protected override string RenderHtml()
    {
        StringBuilder html = new();
        string cssClass = !string.IsNullOrEmpty(CssClass) ? CssClass : "";

        if (!string.IsNullOrEmpty(Text))
        {
            if (Enable)
                html.Append($"<div class=\"{(BootstrapHelper.Version == 3 ? "form-check" : "checkbox")}\">");
            else
                html.Append($"<div class=\"{(BootstrapHelper.Version  == 3 ? "form-check" : "checkbox")} disabled\">");

            html.Append("<label>");
        }

        html.Append($"<input id=\"{Name}\" ");
        html.Append($"name=\"{Name}\" ");
        html.Append("type=\"checkbox\" ");
        html.Append($"value=\"{Value}\" ");
        html.Append($"onchange=\"$('#{Name}_hidden').val($(this).is(':checked') ? '{Value}' : '0');\" ");

        if (!string.IsNullOrEmpty(CssClass))
        {
            html.Append($"class=\"{cssClass}\" ");
        }
        
        if (!string.IsNullOrEmpty(ToolTip))
        {
            html.Append($"{BootstrapHelper.DataToggle}=\"tooltip\" ");
            html.AppendFormat("title=\"{0}\" ", Translate.Key(ToolTip));
        }

        if (IsChecked)
        {
            html.Append("checked=\"checked\" ");
        }
           
        if (!Enable)
            html.Append("disabled");

        foreach (DictionaryEntry attr in Attributes)
        {
            html.Append(" ");
            html.Append(attr.Key);
            if (attr.Value != null)
            {
                html.Append("=\"");
                html.Append(attr.Value);
                html.Append("\"");
            }
        }

        html.Append("/>");

        html.Append(GetHiddenInputHtml());

        if (!string.IsNullOrEmpty(Text))
        {
            html.Append("&nbsp");
            html.Append(Text);
            html.Append("</label>");
            html.Append("</div>");
        }

        return html.ToString();
    }

    private string GetHiddenInputHtml()
    {
        StringBuilder html = new();
        
        html.Append($"<input id=\"{Name}_hidden\" ");
        html.Append($"name=\"{Name}_hidden\" ");
        html.Append("type=\"hidden\" ");
        html.Append("value=\"");
        html.Append(IsChecked ? Value : "0");
        html.Append("\"/>");

        return html.ToString();
    }
    
}
