using System.IO;
using JJMasterData.Core.DataManager.Services;

namespace JJMasterData.Core.DataManager.Exportation.Formats;

internal sealed class TextExportFormat(FieldFormattingService fieldFormattingService)
    : DelimitedTextExportFormat<TextExportOptions>(fieldFormattingService)
{
    protected override string GetDelimiter(TextExportOptions options) => options.Delimiter switch
    {
        TextExportDelimiter.Tab => "\t",
        TextExportDelimiter.Semicolon => ";",
        TextExportDelimiter.Comma => ",",
        _ => throw new InvalidDataException($"Unsupported text delimiter '{options.Delimiter}'.")
    };
}