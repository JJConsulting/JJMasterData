using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.UI.Components;
using JJMasterData.Core.Web.Html;

namespace JJMasterData.Core.Web.Components;

/// <summary>
/// Represents a <see cref="JJAlert"/> with error messages.
/// </summary>
/// <example>
/// The result will look like this:
/// <img src="../media/JJValidationSumaryExample.png"/>
/// </example>
public class JJValidationSummary : HtmlComponent
{
    public IList<string> Errors { get; set; }

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

    public JJValidationSummary(IDictionary<string, object>errors) : this()
    {
        if (errors != null)
        {
            foreach (var err in errors)
            {
                Errors.Add(err.Value.ToString());
            }
        }
    }

    public JJValidationSummary(IEnumerable<string> errors) : this()
    {
        Errors = errors.ToList();
    }

    public JJValidationSummary(string error) : this()
    {
        Errors.Add(error);
    }

    internal override HtmlBuilder BuildHtml()
    {
        var alert = new JJAlert
        {
            Color = PanelColor.Danger,
            Icon = IconType.ExclamationTriangle,
            Title = MessageTitle,
            ShowCloseButton = ShowCloseButton,
            Messages = Errors
        };

        return alert.BuildHtml();
    }
}
