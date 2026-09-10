using System.ComponentModel.DataAnnotations;
using JJMasterData.Core.DataManager.Exportation.Abstractions;

namespace JJMasterData.Core.DataManager.Exportation.Formats;

internal sealed class ExcelXlsxExportOptions : ExportFormatOptions
{
    [Display(Name = "Show Table Style")]
    public bool ShowTableStyle { get; set; } = true;
}
