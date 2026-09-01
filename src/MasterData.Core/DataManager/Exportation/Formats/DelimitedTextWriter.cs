using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CsvHelper;
using CsvHelper.Configuration;
using JJMasterData.Core.DataManager.Exportation.Abstractions;

namespace JJMasterData.Core.DataManager.Exportation.Formats;

internal static class DelimitedTextWriter
{
    public static async Task WriteAsync(ExportContext context, DelimitedTextExportOptions options, Stream output,
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
            foreach (var column in context.Columns)
                csv.WriteField(row.FormattedValues.GetValueOrDefault(column.Name));
            await csv.NextRecordAsync();
            processed++;
            context.Progress.Report(new ExportProgress(processed, context.TotalRecords, $"Exporting {processed:N0} records..."));
        }
        await textWriter.FlushAsync(cancellationToken);
    }
}