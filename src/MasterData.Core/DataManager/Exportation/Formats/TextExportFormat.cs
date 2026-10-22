using System.IO;

namespace JJMasterData.Core.DataManager.Exportation.Formats;

internal sealed class TextExportFormat : DelimitedTextExportFormat<TextExportOptions>
{
    public override string Id => "txt";
    public override string DisplayName => "Text";
    public override string FileExtension => "txt";

    protected override string GetDelimiter(TextExportOptions options) => options.Delimiter switch
    {
        TextExportDelimiter.Tab => "\t",
        TextExportDelimiter.Semicolon => ";",
        TextExportDelimiter.Comma => ",",
        _ => throw new InvalidDataException($"Unsupported text delimiter '{options.Delimiter}'.")
    };
}