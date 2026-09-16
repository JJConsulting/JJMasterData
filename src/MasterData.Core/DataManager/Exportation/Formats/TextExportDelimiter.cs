using System.ComponentModel.DataAnnotations;

namespace JJMasterData.Core.DataManager.Exportation.Formats;

public enum TextExportDelimiter
{
    [Display(Name = "Tab", ShortName = "\\t")]
    Tab,
    [Display(Name = "Semicolon (;)", ShortName = ";")]
    Semicolon,
    [Display(Name = "Comma (,)", ShortName = ",")]
    Comma
}