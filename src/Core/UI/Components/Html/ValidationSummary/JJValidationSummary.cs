﻿using System.Collections.Generic;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.UI.Html;

namespace JJMasterData.Core.UI.Components;

/// <summary>
/// Represents a <see cref="JJAlert"/> with error messages.
/// </summary>
/// <example>
/// The result will look like this:
/// <img src="../media/JJValidationSumaryExample.png"/>
/// </example>
public class JJValidationSummary : HtmlComponent
{
    public List<string> Errors { get; }

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
        Errors = [];
        ShowCloseButton = true;
    }

    public void SetErrors(Dictionary<string, string>errors)
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
            Color = BootstrapColor.Danger,
            Icon = IconType.ExclamationTriangle,
            Title = MessageTitle,
            ShowCloseButton = ShowCloseButton,
        };

        alert.Messages.AddRange(Errors);

        return alert.BuildHtml();
    }
}
