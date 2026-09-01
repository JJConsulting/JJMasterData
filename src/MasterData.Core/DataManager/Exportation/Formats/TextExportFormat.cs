using System.IO;
using System.Threading;
using System.Threading.Tasks;
using JJMasterData.Core.DataManager.Exportation.Abstractions;

namespace JJMasterData.Core.DataManager.Exportation.Formats;

public sealed class TextExportFormat : IExportFormat<DelimitedTextExportOptions>
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
                Kind = FormatOptionKind.Select,
                DefaultValue = "\\t",
                Choices = [new("\t", "Tab"), new(";", "Semicolon (;)"), new(",", "Comma (,)")]
            }
        ]
    };

    public Task WriteAsync(ExportContext context, DelimitedTextExportOptions options, Stream output,
        CancellationToken cancellationToken) => DelimitedTextWriter.WriteAsync(context, options, output, cancellationToken);
}
