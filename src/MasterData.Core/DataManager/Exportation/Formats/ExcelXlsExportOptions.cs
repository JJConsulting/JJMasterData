using System.ComponentModel.DataAnnotations;

namespace JJMasterData.Core.DataManager.Exportation.Formats;

public sealed class ExcelXlsExportOptions : ExportFormatOptions
{
    protected internal override string Id => "excel";
    protected internal override string DisplayName => "Excel (.xls)";
    protected internal override string FileExtension => "xls";
    protected internal override string ContentType => "application/vnd.ms-excel";

    [Display(Name = "Show borders")]
    public bool ShowBorder { get; set; }

    [Display(Name = "Striped rows")]
    public bool ShowRowStriped { get; set; }
}
