using System.ComponentModel.DataAnnotations;

namespace JJMasterData.Core.DataManager.Exportation.Formats;

public enum CsvExportDelimiter
{
    [Display(Name = "Semicolon (;)", ShortName = ";")]
    Semicolon,
    [Display(Name = "Comma (,)", ShortName = ",")]
    Comma,
    [Display(Name = "Pipe (|)", ShortName = "|")]
    Pipe
}