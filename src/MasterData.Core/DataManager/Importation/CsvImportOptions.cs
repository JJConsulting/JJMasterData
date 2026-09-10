using System.ComponentModel.DataAnnotations;

namespace JJMasterData.Core.DataManager.Importation;

public sealed class CsvImportOptions
{
    [Display(Name = "Delimiter")]
    public CsvImportDelimiter Delimiter { get; set; } = CsvImportDelimiter.Semicolon;

    [Display(Name = "Detect delimiter")]
    public bool DetectDelimiter { get; set; } = true;
}