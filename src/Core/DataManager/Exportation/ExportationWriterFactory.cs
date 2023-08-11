using System;
using JJMasterData.Core.DataManager.Exports.Abstractions;
using JJMasterData.Core.DataManager.Exports.Configuration;
using JJMasterData.Core.Web.Components;
using Microsoft.Extensions.DependencyInjection;

namespace JJMasterData.Core.DataManager.Exports;

public class ExportationWriterFactory
{
    private IServiceProvider ServiceProvider { get; }

    public ExportationWriterFactory(IServiceProvider serviceProvider)
    {
        ServiceProvider = serviceProvider;
    }
    
    public IPdfWriter GetPdfWriter()
    {
        return ServiceProvider.GetRequiredService<IPdfWriter>();
    }

    public IExcelWriter GetExcelWriter()
    {
        return ServiceProvider.GetRequiredService<IExcelWriter>();
    }

    public ITextWriter GetTextWriter()
    {
        return ServiceProvider.GetRequiredService<ITextWriter>();
    }

    public ExportationWriterBase GetInstance(JJDataExportation exporter)
    {
        ExportationWriterBase writer;
        switch (exporter.ExportOptions.FileExtension)
        {
            case ExportFileExtension.CSV:
            case ExportFileExtension.TXT:
                var textWriter = GetTextWriter();
                textWriter.Delimiter = exporter.ExportOptions.Delimiter;
                textWriter.OnRenderCell += exporter.OnRenderCell;

                writer = (ExportationWriterBase)textWriter;
                break;

            case ExportFileExtension.XLS:
                var excelWriter = GetExcelWriter();
                excelWriter.ShowRowStriped = exporter.ShowRowStriped;
                excelWriter.ShowBorder = exporter.ShowBorder;
                excelWriter.OnRenderCell += exporter.OnRenderCell;

                writer = (ExportationWriterBase)excelWriter;

                break;
            case ExportFileExtension.PDF:
                var pdfWriter = GetPdfWriter();

                if (pdfWriter == null)
                    throw new NotImplementedException("Please implement IPdfWriter in your application services.");

                pdfWriter.ShowRowStriped = exporter.ShowRowStriped;
                pdfWriter.ShowBorder = exporter.ShowBorder;
                pdfWriter.OnRenderCell += exporter.OnRenderCell;

                // ReSharper disable once SuspiciousTypeConversion.Global;
                // PdfWriter is dynamic loaded by plugin.
                writer = pdfWriter as ExportationWriterBase;

                break;
            default:
                throw new NotImplementedException();
        }

        ConfigureWriter(exporter, writer);

        return writer;
    }

    private static void ConfigureWriter(JJDataExportation exporter, ExportationWriterBase writer)
    {
        writer.FormElement = exporter.FormElement;
        writer.Configuration = exporter.ExportOptions;
        writer.UserId = exporter.UserId;
        writer.ProcessOptions = exporter.ProcessOptions;
    }
}
