using System.IO;
using JJMasterData.Core.DataManager.Services;

namespace JJMasterData.Core.DataManager.Exportation.Formats;

internal sealed class CsvExportFormat(FieldFormattingService fieldFormattingService)
    : DelimitedTextExportFormat<CsvExportOptions>(fieldFormattingService)
{
    protected override string GetDelimiter(CsvExportOptions options) => options.Delimiter switch
    {
        CsvExportDelimiter.Semicolon => ";",
        CsvExportDelimiter.Comma => ",",
        CsvExportDelimiter.Pipe => "|",
        _ => throw new InvalidDataException($"Unsupported CSV delimiter '{options.Delimiter}'.")
    };
}