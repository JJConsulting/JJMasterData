using System;
using System.Collections;
using System.Text;
using JJMasterData.Commons.Language;
using JJMasterData.Core.DataDictionary;

namespace JJMasterData.Core.WebComponents;

/// <summary>
/// Representa um Label padrão
/// </summary>
public class JJLabel : JJBaseView
{
    /// <summary>
    /// Texto exibido quando o ponteiro do mouse passa sobre o controle
    /// </summary>
    public string ToolTip { get; set; }

    /// <summary>
    /// Nome do controle referente ao label
    /// </summary>
    public string LabelFor { get; set; }

    /// <summary>
    /// Descrição do label
    /// </summary>
    public string Text { get; set; }

    public Boolean IsRequired { get; set; }

    /// <summary>
    /// Inicializa uma nova instância da classe JJLabel
    /// </summary>
    public JJLabel()
    {
        
    }

    /// <summary>
    /// Inicializa uma nova instância da classe JJLabel a partir de um FormElementField
    /// </summary>
    public JJLabel(FormElementField f)
    {
        if (f == null)
            throw new ArgumentNullException(nameof(f), "FormElementField can not be null");

        LabelFor = f.Name;
        Text = string.IsNullOrEmpty(f.Label) ? f.Name : f.Label;
        ToolTip = f.HelpDescription;
        IsRequired = f.IsRequired;
    }

    protected override string RenderHtml()
    {
        StringBuilder sHtml = new StringBuilder();
        sHtml.Append($"<label class=\"{BootstrapHelper.Label} ");
        sHtml.Append(!string.IsNullOrEmpty(CssClass) ? CssClass : "");
        sHtml.Append("\" for=\"");
        sHtml.Append(LabelFor);
        sHtml.Append("\"");

        foreach (DictionaryEntry attr in Attributes)
        {
            sHtml.Append(" ");
            sHtml.Append(attr.Key);
            if (attr.Value != null)
            {
                sHtml.Append("=\"");
                sHtml.Append(attr.Value);
                sHtml.Append("\"");
            }
        }

        sHtml.Append(">");
        sHtml.Append(Translate.Key(Text));

        if (IsRequired)
        {
            sHtml.Append("<span class=\"required-symbol\">*</span>");
        }

        if (!string.IsNullOrEmpty(ToolTip))
        {
            sHtml.Append("<span class=\"fa fa-question-circle help-description\" ");
            sHtml.Append($" {BootstrapHelper.DataToggle}=\"tooltip\" title=\"");
            sHtml.Append(Translate.Key(ToolTip));
            sHtml.Append("\"></span>");
        }

        sHtml.Append("</label>");

        

        return sHtml.ToString();
    }
}
