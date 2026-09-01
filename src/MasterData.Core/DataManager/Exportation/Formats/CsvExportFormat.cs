using System.IO;
using System.Threading;
using System.Threading.Tasks;
using JJMasterData.Core.DataManager.Exportation.Abstractions;

namespace JJMasterData.Core.DataManager.Exportation.Formats;

public sealed class CsvExportFormat : IExportFormat<DelimitedTextExportOptions>
{
    public ExportFormatConfiguration Configuration { get; } = new()
    {
        Id = "csv",
        DisplayName = "CSV",
        FileExtension = "csv",
        ContentType = "text/csv",
        Options =
        [
            new ExportFormatOption
            {
                Name = nameof(DelimitedTextExportOptions.Delimiter),
                DisplayName = "Delimiter",
                Kind = FormatOptionKind.Select,
                DefaultValue = ";",
                Choices = [new(";", "Semicolon (;)"), new(",", "Comma (,)"), new("|", "Pipe (|)")]
            }
        ]
    };

    public Task WriteAsync(ExportContext context, DelimitedTextExportOptions options, Stream output,
        CancellationToken cancellationToken) => DelimitedTextWriter.WriteAsync(context, options, output, cancellationToken);
}
