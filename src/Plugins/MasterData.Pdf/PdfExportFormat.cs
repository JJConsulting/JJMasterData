using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using iText.Kernel.Colors;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Borders;
using iText.Layout.Element;
using iText.Layout.Properties;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.DataManager.Exportation;
using JJMasterData.Core.DataManager.Exportation.Abstractions;
using JJMasterData.Core.DataManager.Models;
using JJMasterData.Core.DataManager.Services;

namespace JJMasterData.Pdf;

public sealed class PdfExportOptions
{
    public bool Landscape { get; set; } = true;
    public bool ShowBorder { get; set; }
    public bool ShowRowStriped { get; set; }
}

public sealed class PdfExportFormat(FieldFormattingService fieldFormattingService) : IExportFormat<PdfExportOptions>
{
    public ExportFormatConfiguration Configuration { get; } = new()
    {
        Id = "pdf",
        DisplayName = "PDF",
        FileExtension = "pdf",
        ContentType = "application/pdf",
        Options =
        [
            new ExportFormatOption
            {
                Name = nameof(PdfExportOptions.Landscape),
                DisplayName = "Landscape",
                Kind = ExportFormatOptionKind.Boolean,
                DefaultValue = "true"
            },
            new ExportFormatOption
            {
                Name = nameof(PdfExportOptions.ShowBorder),
                DisplayName = "Show borders",
                Kind = ExportFormatOptionKind.Boolean,
                DefaultValue = "false"
            },
            new ExportFormatOption
            {
                Name = nameof(PdfExportOptions.ShowRowStriped),
                DisplayName = "Striped rows",
                Kind = ExportFormatOptionKind.Boolean,
                DefaultValue = "false"
            }
        ]
    };

    public async Task WriteAsync(
        ExportContext context,
        PdfExportOptions options,
        Stream output,
        CancellationToken cancellationToken)
    {
        if (context.Columns.Count == 0)
            throw new InvalidOperationException("A PDF export requires at least one column.");

        using var writer = new iText.Kernel.Pdf.PdfWriter(output);
        using var pdf = new PdfDocument(writer);
        pdf.SetDefaultPageSize(options.Landscape ? PageSize.A4.Rotate() : PageSize.A4);
        using var document = new Document(pdf);
        document.SetFontSize(8);
        document.Add(new Paragraph(DateTime.Now.ToLongDateString()).SetTextAlignment(TextAlignment.RIGHT));
        if (!string.IsNullOrWhiteSpace(context.FormElement.Title))
            document.Add(new Paragraph(context.FormElement.Title).SetFontSize(16));

        var table = new Table(context.Columns.Count, true).UseAllAvailableWidth();
        document.Add(table);
        if (context.IncludeHeader)
        {
            foreach (var column in context.Columns)
                table.AddHeaderCell(Style(new Cell().Add(new Paragraph(column.DisplayName)), options, true, false));
        }

        long processed = 0;
        await foreach (var row in context.Rows.WithCancellation(cancellationToken))
        {
            var striped = options.ShowRowStriped && processed % 2 == 1;
            var formState = new FormStateData(row, context.UserValues, PageState.List);
            foreach (var column in context.Columns)
            {
                var value = await fieldFormattingService.FormatGridValueAsync(
                    new FormElementFieldSelector(context.FormElement, column.Name), formState);
                table.AddCell(Style(new Cell().Add(new Paragraph(value)), options, false, striped));
            }
            processed++;
            context.Progress.Report(new ExportProgress(processed, context.TotalRecords,
                $"Exporting {processed:N0} records..."));
            table.Flush();
        }
        table.Complete();
    }

    private static Cell Style(Cell cell, PdfExportOptions options, bool header, bool striped)
    {
        if (!options.ShowBorder)
            cell.SetBorder(Border.NO_BORDER);
        if (striped)
            cell.SetBackgroundColor(new DeviceRgb(242, 253, 255));
        return cell;
    }
}
