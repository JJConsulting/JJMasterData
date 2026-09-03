using System.ComponentModel.DataAnnotations;

namespace JJMasterData.Core.DataManager.Exportation.Formats;

internal sealed class ExcelXlsxExportOptions : ExportFormatOptions
{
    protected internal override string Id => "xlsx";
    protected internal override string DisplayName => "Excel (.xlsx)";
    protected internal override string FileExtension => "xlsx";
    protected internal override string ContentType => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

    [Display(Name = "Show table style")]
    public bool ShowTableStyle { get; set; } = true;
}
