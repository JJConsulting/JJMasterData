using System.ComponentModel.DataAnnotations;
using JJMasterData.Core.DataManager.Exportation.Abstractions;

namespace JJMasterData.Core.DataManager.Exportation.Formats;

public sealed class CsvExportOptions : ExportFormatOptions
{
    [Display(Name = "Delimiter")]
    public CsvExportDelimiter Delimiter { get; set; } = CsvExportDelimiter.Semicolon;
}