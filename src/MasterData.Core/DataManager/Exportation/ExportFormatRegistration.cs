using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using JJMasterData.Core.DataManager.Exportation.Abstractions;

namespace JJMasterData.Core.DataManager.Exportation;

internal sealed class ExportFormatRegistration<TFormat, TOptions>(TFormat format) : IExportFormatRegistration
    where TFormat : class, IExportFormat<TOptions>
    where TOptions : class, new()
{
    public ExportFormatConfiguration Configuration => format.Configuration;

    public Task WriteAsync(
        ExportContext context,
        Dictionary<string, string?> options,
        Stream output,
        CancellationToken cancellationToken)
    {
        var typedOptions = FormatOptionsBinder.Bind<TOptions>(Configuration.Options, options);
        return format.WriteAsync(context, typedOptions, output, cancellationToken);
    }

    public void ValidateOptions(Dictionary<string, string?> options) =>
        _ = FormatOptionsBinder.Bind<TOptions>(Configuration.Options, options);
}