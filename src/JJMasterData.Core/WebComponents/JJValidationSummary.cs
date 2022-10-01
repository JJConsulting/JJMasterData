using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.Html;
using System.Collections;
using System.Collections.Generic;

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

    internal override HtmlElement GetHtmlElement()
    {
        var alert = new JJAlert()
        {
            Color = PanelColor.Danger,
            Icon = IconType.ExclamationTriangle,
            Title = MessageTitle,
            ShowCloseButton = ShowCloseButton,
            Messages = Errors
        };

        return alert.GetHtmlElement();
    }
}
