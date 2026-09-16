using System.ComponentModel.DataAnnotations;
using JJMasterData.Core.DataManager.Exportation.Abstractions;

namespace JJMasterData.Core.DataManager.Exportation.Formats;

public sealed class ExcelXlsExportOptions : ExportFormatOptions
{
    [Display(Name = "Show Borders")]
    public bool ShowBorders { get; set; }

    [Display(Name = "Show Striped Rows")]
    public bool ShowStripedRows { get; set; }
}
