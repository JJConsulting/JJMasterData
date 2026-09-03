using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using JJMasterData.Core.DataManager.Exportation.Abstractions;

namespace JJMasterData.Core.DataManager.Exportation;

internal sealed class ExportFormatRegistration<TFormat, TOptions>(TFormat format) : IExportFormatRegistration
    where TFormat : class, IExportFormat<TOptions>
    where TOptions : ExportFormatOptions, new()
{
    private static readonly TOptions Defaults = new();

    public ExportFormatMetadata Metadata { get; } = new(
        Defaults.Id,
        Defaults.DisplayName,
        Defaults.FileExtension,
        Defaults.ContentType,
        FormatOptionsMetadataFactory.CreateOptions(Defaults));

    public Task WriteAsync(
        ExportContext context,
        Dictionary<string, string?> options,
        Stream output,
        CancellationToken cancellationToken)
    {
        var typedOptions = FormatOptionsBinder.Bind<TOptions>(Metadata.Options, options);
        return format.WriteAsync(context, typedOptions, output, cancellationToken);
    }
}
