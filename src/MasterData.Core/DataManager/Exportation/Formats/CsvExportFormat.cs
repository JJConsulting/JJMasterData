using System.IO;

namespace JJMasterData.Core.DataManager.Exportation.Formats;

internal sealed class CsvExportFormat : DelimitedTextExportFormat<CsvExportOptions>
{
    public override string Id => "csv";
    public override string DisplayName => "CSV";
    public override string FileExtension => "csv";

    protected override string GetDelimiter(CsvExportOptions options) => options.Delimiter switch
    {
        CsvExportDelimiter.Semicolon => ";",
        CsvExportDelimiter.Comma => ",",
        CsvExportDelimiter.Pipe => "|",
        _ => throw new InvalidDataException($"Unsupported CSV delimiter '{options.Delimiter}'.")
    };
}