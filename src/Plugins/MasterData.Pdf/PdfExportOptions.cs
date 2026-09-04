using System.ComponentModel.DataAnnotations;
using JJMasterData.Core.DataManager.Exportation;
using JJMasterData.Core.DataManager.Exportation.Abstractions;

namespace JJMasterData.Pdf;

public sealed class PdfExportOptions : ExportFormatOptions
{
    [Display(Name = "Landscape")]
    public bool Landscape { get; set; } = true;

    [Display(Name = "Show Borders")]
    public bool ShowBorders { get; set; }

    [Display(Name = "Show Striped Rows")]
    public bool ShowStripedRows { get; set; }
}