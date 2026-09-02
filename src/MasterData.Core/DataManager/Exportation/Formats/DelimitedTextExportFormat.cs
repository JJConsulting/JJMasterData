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

internal sealed class DelimitedTextExportFormat(
    FieldFormattingService fieldFormattingService,
    ExportFormatConfiguration configuration) : IExportFormat<DelimitedTextExportOptions>
{
    public ExportFormatConfiguration Configuration { get; } = configuration;

    public static DelimitedTextExportFormat CreateCsv(FieldFormattingService fieldFormattingService)
    {
        return new DelimitedTextExportFormat(fieldFormattingService, new ExportFormatConfiguration
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
                    Kind = ExportFormatOptionKind.Select,
                    DefaultValue = ";",
                    Choices = [new(";", "Semicolon (;)"), new(",", "Comma (,)"), new("|", "Pipe (|)")]
                }
            ]
        });
    }

    public static DelimitedTextExportFormat CreateText(FieldFormattingService fieldFormattingService)
    {
        return new DelimitedTextExportFormat(fieldFormattingService, new ExportFormatConfiguration
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
        });
    }

    public async Task WriteAsync(
        ExportContext context,
        DelimitedTextExportOptions options,
        Stream output,
        CancellationToken cancellationToken)
    {
        await using var textWriter = new StreamWriter(output, new UTF8Encoding(true), leaveOpen: true);
        await using var csv = new CsvWriter(textWriter,
            new CsvConfiguration(System.Globalization.CultureInfo.CurrentCulture)
            {
                Delimiter = options.Delimiter.Replace("\\t", "\t"),
                HasHeaderRecord = false
            });

        if (context.IncludeHeader)
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
