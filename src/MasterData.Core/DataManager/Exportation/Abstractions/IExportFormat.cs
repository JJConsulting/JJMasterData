using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace JJMasterData.Core.DataManager.Exportation.Abstractions;

public interface IExportFormat
{
    string Id { get; }
    string DisplayName { get; }
    string FileExtension { get; }

    Type OptionsType { get; } 
    
    Task WriteAsync(
        ExportContext context,
        ExportFormatOptions options,
        Stream output,
        CancellationToken cancellationToken);
}

public interface IExportFormat<in TOptions> : IExportFormat where TOptions : ExportFormatOptions, new()
{
    Type IExportFormat.OptionsType => typeof(TOptions);
    
    Task WriteAsync(
        ExportContext context,
        TOptions options,
        Stream output,
        CancellationToken cancellationToken);

    Task IExportFormat.WriteAsync(
        ExportContext context,
        ExportFormatOptions options,
        Stream output,
        CancellationToken cancellationToken)
    {
        if (options is not TOptions typedOptions)
        {
            throw new ArgumentException(
                $@"Expected {typeof(TOptions).Name}, got {options.GetType().Name}.",
                nameof(options));
        }

        return WriteAsync(
            context,
            typedOptions,
            output,
            cancellationToken);
    }
}