using System.ComponentModel.DataAnnotations;

namespace JJMasterData.Core.DataManager.Exportation.Formats;

public sealed class CsvExportOptions : ExportFormatOptions
{
    protected internal override string Id => "csv";
    protected internal override string DisplayName => "CSV";
    protected internal override string FileExtension => "csv";
    protected internal override string ContentType => "text/csv";

    [Display(Name = "Delimiter")]
    public CsvExportDelimiter Delimiter { get; set; } = CsvExportDelimiter.Semicolon;
}