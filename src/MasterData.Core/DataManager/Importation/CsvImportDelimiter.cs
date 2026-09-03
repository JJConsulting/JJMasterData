using System.ComponentModel.DataAnnotations;

namespace JJMasterData.Core.DataManager.Importation;

public enum CsvImportDelimiter
{
    [Display(Name = "Semicolon (;)", ShortName = ";")]
    Semicolon,
    [Display(Name = "Comma (,)", ShortName = ",")]
    Comma,
    [Display(Name = "Pipe (|)", ShortName = "|")]
    Pipe,
    [Display(Name = "Tab", ShortName = "\\t")]
    Tab
}