using System.IO;
using System.Threading;
using System.Threading.Tasks;
using JJMasterData.Core.DataManager.Exportation.Abstractions;
using JJMasterData.Core.DataManager.Services;

namespace JJMasterData.Core.DataManager.Exportation.Formats;

public sealed class TextExportFormat(FieldFormattingService fieldFormattingService) : IExportFormat<DelimitedTextExportOptions>
{
    public ExportFormatConfiguration Configuration { get; } = new()
    {
        Id = "txt",
        DisplayName = "Text",
        FileExtension = "txt",
        ContentType = "text/plain",
        Options =
        [
            new ExportFormatOption
            {
                Name = nameof(DelimitedTextExportOptions.Delimiter),
                DisplayName = "Delimiter",
                Kind = ExportFormatOptionKind.Select,
                DefaultValue = "\\t",
                Choices = [new("\t", "Tab"), new(";", "Semicolon (;)"), new(",", "Comma (,)")]
            }
        ]
    };

    public Task WriteAsync(ExportContext context, DelimitedTextExportOptions options, Stream output,
        CancellationToken cancellationToken) =>
        DelimitedTextWriter.WriteAsync(context, options, fieldFormattingService, output, cancellationToken);
}
