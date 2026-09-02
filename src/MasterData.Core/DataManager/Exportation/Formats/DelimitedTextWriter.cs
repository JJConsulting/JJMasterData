using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CsvHelper;
using CsvHelper.Configuration;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.DataManager.Exportation;
using JJMasterData.Core.DataManager.Models;
using JJMasterData.Core.DataManager.Services;

namespace JJMasterData.Core.DataManager.Exportation.Formats;

internal static class DelimitedTextWriter
{
    public static async Task WriteAsync(ExportContext context, DelimitedTextExportOptions options,
        FieldFormattingService fieldFormattingService, Stream output,
        CancellationToken cancellationToken)
    {
        await using var textWriter = new StreamWriter(output, new UTF8Encoding(true), leaveOpen: true);
        await using var csv = new CsvWriter(textWriter, new CsvConfiguration(System.Globalization.CultureInfo.CurrentCulture)
        {
            Delimiter = options.Delimiter.Replace("\\t", "\t"),
            HasHeaderRecord = false
        });
        if (context.IncludeHeader)
        {
            foreach (var column in context.Columns)
                csv.WriteField(column.DisplayName);
            await csv.NextRecordAsync();
        }

        long processed = 0;
        await foreach (var row in context.Rows.WithCancellation(cancellationToken))
        {
            var formState = new FormStateData(row, context.UserValues, PageState.List);
            foreach (var column in context.Columns)
            {
                var value = await fieldFormattingService.FormatGridValueAsync(
                    new FormElementFieldSelector(context.FormElement, column.Name), formState);
                csv.WriteField(value);
            }
            await csv.NextRecordAsync();
            processed++;
            context.Progress.Report(new ExportProgress(processed, context.TotalRecords, $"Exporting {processed:N0} records..."));
        }
        await textWriter.FlushAsync(cancellationToken);
    }
}
