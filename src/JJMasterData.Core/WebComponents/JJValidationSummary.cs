using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.Html;
using System.Collections;
using System.Collections.Generic;

namespace JJMasterData.Core.WebComponents;

/// <summary>
/// Represents a <see cref="JJAlert"/> with error messages.
/// </summary>
/// <example>
/// The result will look like this:
/// <img src="../media/JJValidationSumaryExample.png"/>
/// </example>
public class JJValidationSummary : JJBaseView
{
    public List<string> Errors { get; set; }

    /// <summary>
    /// Panel title
    /// (Default = "Invalid Data")
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

    internal override HtmlBuilder RenderHtml()
    {
        var alert = new JJAlert()
        {
            Color = PanelColor.Danger,
            Icon = IconType.ExclamationTriangle,
            Title = MessageTitle,
            ShowCloseButton = ShowCloseButton,
            Messages = Errors
        };

        return alert.RenderHtml();
    }
}
