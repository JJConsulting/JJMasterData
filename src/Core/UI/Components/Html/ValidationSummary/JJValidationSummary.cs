using System;
using System.Collections.Generic;
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
    /// Enable close panel
    /// (Default = True)
    /// </summary>
    public bool ShowCloseButton { get; set; }

    internal JJValidationSummary() 
    {
        Visible = true;
        Errors = new List<string>();
        ShowCloseButton = true;
    }

    public void SetErrors(IDictionary<string, string>errors)
    {
        if (errors == null) 
            return;
            
        foreach (var err in errors)
        {
            Errors.Add(err.Value);
        }
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
