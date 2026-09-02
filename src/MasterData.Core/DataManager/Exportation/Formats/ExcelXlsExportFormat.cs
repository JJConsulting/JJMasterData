using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.DataManager.Exportation.Abstractions;
using JJMasterData.Core.DataManager.Models;
using JJMasterData.Core.DataManager.Services;

namespace JJMasterData.Core.DataManager.Exportation.Formats;

public sealed class ExcelXlsExportFormat(FieldFormattingService fieldFormattingService) : IExportFormat<ExcelXlsExportOptions>
{
    public ExportFormatConfiguration Configuration { get; } = new()
    {
        Id = "excel",
        DisplayName = "Excel (.xls)",
        FileExtension = "xls",
        ContentType = "application/vnd.ms-excel",
        Options =
        [
            new ExportFormatOption
            {
                Name = nameof(ExcelXlsExportOptions.ShowBorder),
                DisplayName = "Show borders",
                Kind = ExportFormatOptionKind.Boolean,
                DefaultValue = "false"
            },
            new ExportFormatOption
            {
                Name = nameof(ExcelXlsExportOptions.ShowRowStriped),
                DisplayName = "Striped rows",
                Kind = ExportFormatOptionKind.Boolean,
                DefaultValue = "false"
            }
        ]
    };

    public async Task WriteAsync(ExportContext context, ExcelXlsExportOptions options, Stream output,
        CancellationToken cancellationToken)
    {
        await using var writer = new StreamWriter(output, new UTF8Encoding(true), leaveOpen: true);
        var tableClass = options.ShowRowStriped ? " class=\"striped\"" : string.Empty;
        var border = options.ShowBorder ? " border=\"1\"" : string.Empty;
        await writer.WriteAsync($"<html><head><meta charset=\"utf-8\"></head><body><table{tableClass}{border}>");
        if (context.IncludeHeader)
        {
            await writer.WriteAsync("<thead><tr>");
            foreach (var column in context.Columns)
                await writer.WriteAsync($"<th>{HttpUtility.HtmlEncode(column.DisplayName)}</th>");
            await writer.WriteAsync("</tr></thead>");
        }
        await writer.WriteAsync("<tbody>");
        long processed = 0;
        await foreach (var row in context.Rows.WithCancellation(cancellationToken))
        {
            await writer.WriteAsync("<tr>");
            var formState = new FormStateData(row, context.UserValues, PageState.List);
            foreach (var column in context.Columns)
            {
                var value = await fieldFormattingService.FormatGridValueAsync(
                    new FormElementFieldSelector(context.FormElement, column.Name), formState);
                await writer.WriteAsync($"<td>{HttpUtility.HtmlEncode(value)}</td>");
            }
            await writer.WriteAsync("</tr>");
            processed++;
            context.Progress.Report(new ExportProgress(processed, context.TotalRecords, $"Exporting {processed:N0} records..."));
        }
        await writer.WriteAsync("</tbody></table></body></html>");
        await writer.FlushAsync(cancellationToken);
    }
}
