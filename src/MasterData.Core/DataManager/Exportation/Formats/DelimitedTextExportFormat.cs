using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CsvHelper;
using CsvHelper.Configuration;
using JJMasterData.Core.DataManager.Exportation.Abstractions;
using JJMasterData.Core.DataManager.Services;

namespace JJMasterData.Core.DataManager.Exportation.Formats;

internal abstract class DelimitedTextExportFormat<TOptions> : IExportFormat<TOptions> where TOptions : ExportFormatOptions, new()
{
    protected abstract string GetDelimiter(TOptions options);

    public abstract string Id { get; }
    public abstract string DisplayName { get; }
    public abstract string FileExtension { get; }

    public async Task WriteAsync(
        ExportContext context,
        TOptions options,
        Stream output,
        CancellationToken cancellationToken)
    {
        await using var textWriter = new StreamWriter(output, new UTF8Encoding(true), leaveOpen: true);
        await using var csv = new CsvWriter(textWriter,
            new CsvConfiguration(CultureInfo.CurrentCulture)
            {
                Delimiter = GetDelimiter(options),
                HasHeaderRecord = false
            });

        if (options.IncludeFirstRowAsHeader)
        {
            foreach (var field in context.Columns)
                csv.WriteField(field.LabelOrName);
            await csv.NextRecordAsync();
        }

        long processed = 0;
        var progress = new ExportProgressReporter(context);
        await foreach (var row in context.Rows.WithCancellation(cancellationToken))
        {
            foreach (var field in context.Columns)
            {
                row.TryGetValue(field.Name, out var rawValue);
                var value = FieldFormattingService.FormatValue(field, rawValue);
                csv.WriteField(value);
            }
            await csv.NextRecordAsync();
            processed++;
            progress.Report(processed);
        }

        progress.Report(processed, completed: true);
        await textWriter.FlushAsync(cancellationToken);
    }
}
