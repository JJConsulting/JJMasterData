using System.Collections;
using System.Collections.Generic;
using System.Text;
using JJMasterData.Commons.Language;
using JJMasterData.Core.DataDictionary;

namespace JJMasterData.Core.WebComponents;

/// <summary>
/// Representa um painel com as mensagens de erro
/// </summary>
/// <example>
/// Exemplo de como utilizar JJValidationSummary
/// [!code-cshtml[Example](../../../doc/JJMasterData.Sample/JJValidationSumaryExample.aspx)]
/// [!code-cs[Example](../../../doc/JJMasterData.Sample/JJValidationSumaryExample.aspx.cs)]
/// O Resultado html ficará parecido com esse:
/// <img src="../media/JJValidationSumaryExample.png"/>
/// </example>
public class JJValidationSummary : JJBaseView
{
    /// <summary>
    /// Lista com as mensagens de erro
    /// </summary>
    public List<string> Errors { get; set; }

    /// <summary>
    /// Titulo do painel com as mensagens de erro. 
    /// (Default = "Dados inválidos"
    /// </summary>
    public string MessageTitle { get; set; }
    
    /// <summary>
    /// Permitir fechar o panel.
    /// (Default = True)
    /// </summary>
    public bool ShowCloseButton { get; set; }

    public JJValidationSummary() 
    {
        Visible = true;
        MessageTitle = "Invalid data";
        Errors = new List<string>();
        ShowCloseButton = true;
    }

    public JJValidationSummary(Hashtable listError) : this()
    {
        if (listError != null)
        {
            foreach (DictionaryEntry err in listError)
            {
                Errors.Add(err.Value.ToString());
            }
        }
    }

    public JJValidationSummary(List<string> listError) : this()
    {
        Errors = listError;
    }

    public JJValidationSummary(string error) : this()
    {
        Errors.Add(error);
    }

    protected override string RenderHtml()
    {
        if (!Visible)
            return "";

        var icon = new JJIcon(IconType.ExclamationTriangle);
        var html = new StringBuilder();

        html.AppendLine("<!-- Start Validation Summary -->");
        html.AppendLine($"<div class=\"alert alert-danger alert-dismissable {(BootstrapHelper.Version == 3 ? "fade-in" : string.Empty)}\">");

        if (ShowCloseButton)
            html.AppendLine($"\t<a href=\"#\" class=\"{BootstrapHelper.Close}\" {BootstrapHelper.DataDismiss}=\"alert\" aria-label=\"close\">{BootstrapHelper.CloseButtonTimes}</a>");

        html.Append("\t");
        html.Append(icon.GetHtml());
        html.Append("&nbsp;<strong>");
        html.Append(Translate.Key(MessageTitle));
        html.AppendLine("</strong><br>");
        if (Errors != null)
        {
            foreach (string err in Errors)
            {
                html.Append("\t- ");
                html.Append(err);
                html.AppendLine("<br>");
            }
        }
        html.AppendLine("</div>");
        html.AppendLine("<!-- End Validation Summary -->");

        return html.ToString();
    }
}
