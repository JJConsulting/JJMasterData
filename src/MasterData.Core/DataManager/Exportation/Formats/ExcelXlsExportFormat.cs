using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using JJMasterData.Core.DataManager.Exportation.Abstractions;
using JJMasterData.Core.DataManager.Services;

namespace JJMasterData.Core.DataManager.Exportation.Formats;

public sealed class ExcelXlsExportFormat : IExportFormat<ExcelXlsExportOptions>
{
    public string Id => "excel";
    public string DisplayName => "Excel (.xls)";
    public string FileExtension => "xls";
    public async Task WriteAsync(ExportContext context, ExcelXlsExportOptions options, Stream output, CancellationToken cancellationToken)
    {
        await using var writer = new StreamWriter(output, new UTF8Encoding(true), leaveOpen: true);
        var tableClass = options.ShowStripedRows ? " class=\"striped\"" : string.Empty;
        var border = options.ShowBorders ? " border=\"1\"" : string.Empty;
        await writer.WriteAsync($"<html><head><meta charset=\"utf-8\"></head><body><table{tableClass}{border}>");
        if (options.IncludeFirstRowAsHeader)
        {
            await writer.WriteAsync("<thead><tr>");
            foreach (var column in context.Columns)
                await writer.WriteAsync($"<th>{HttpUtility.HtmlEncode(column.LabelOrName)}</th>");
            await writer.WriteAsync("</tr></thead>");
        }
        await writer.WriteAsync("<tbody>");
        long processed = 0;
        var progress = new ExportProgressReporter(context);
        await foreach (var row in context.Rows.WithCancellation(cancellationToken))
        {
            await writer.WriteAsync("<tr>");
            foreach (var column in context.Columns)
            {
                row.TryGetValue(column.Name, out var rawValue);
                var value = FieldFormattingService.FormatValue(column, rawValue);
                await writer.WriteAsync($"<td>{HttpUtility.HtmlEncode(value)}</td>");
            }
            await writer.WriteAsync("</tr>");
            processed++;
            progress.Report(processed);
        }
        progress.Report(processed, completed: true);
        await writer.WriteAsync("</tbody></table></body></html>");
        await writer.FlushAsync(cancellationToken);
    }
}
