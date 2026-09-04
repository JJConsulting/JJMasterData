using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CsvHelper;
using CsvHelper.Configuration;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.DataManager.Exportation.Abstractions;
using JJMasterData.Core.DataManager.Models;
using JJMasterData.Core.DataManager.Services;

namespace JJMasterData.Core.DataManager.Exportation.Formats;

internal abstract class DelimitedTextExportFormat<TOptions>(FieldFormattingService fieldFormattingService) : IExportFormat<TOptions> where TOptions : ExportFormatOptions, new()
{
    protected abstract string GetDelimiter(TOptions options);

    public abstract string Id { get; }
    public abstract string DisplayName { get; }
    public abstract string FileExtension { get; }
    public abstract string ContentType { get; }

    public async Task WriteAsync(
        ExportContext context,
        TOptions options,
        Stream output,
        CancellationToken cancellationToken)
    {
        await using var textWriter = new StreamWriter(output, new UTF8Encoding(true), leaveOpen: true);
        await using var csv = new CsvWriter(textWriter,
            new CsvConfiguration(System.Globalization.CultureInfo.CurrentCulture)
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
        await foreach (var row in context.Rows.WithCancellation(cancellationToken))
        {
            var formState = new FormStateData(row, context.UserValues, PageState.List);
            foreach (var field in context.Columns)
            {
                var value = await fieldFormattingService.FormatGridValueAsync(
                    new FormElementFieldSelector(context.FormElement, field.Name), formState);
                csv.WriteField(value);
            }
            await csv.NextRecordAsync();
            processed++;
            context.Progress.Report(new ExportProgress(
                processed, context.TotalRecords, $"Exporting {processed:N0} records..."));
        }

        await textWriter.FlushAsync(cancellationToken);
    }
}