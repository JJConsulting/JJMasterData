using System;
using System.Collections.Generic;
using System.Linq;
using JJMasterData.Core.DataManager.Exports.Abstractions;
using JJMasterData.Core.DataManager.Exports.Configuration;
using JJMasterData.Core.WebComponents;

namespace JJMasterData.Core.DataManager.Exports;

public static class WriterFactory
{

    public static IPdfWriter GetPdfWriter(IEnumerable<IExportationWriter> writers)
    {
        return (IPdfWriter)writers.FirstOrDefault(writer=> writer.GetType() == typeof(IPdfWriter));
    }

    public static IExcelWriter GetExcelWriter(IEnumerable<IExportationWriter> writers)
    {
        return (IExcelWriter)writers.FirstOrDefault(writer=> writer.GetType().GetInterface("IExcelWriter") == typeof(IExcelWriter));
    }

    public static ITextWriter GetTextWriter(IEnumerable<IExportationWriter> writers)
    {
        return (ITextWriter)writers.FirstOrDefault(writer=> writer.GetType().GetInterface("ITextWriter") == typeof(ITextWriter));
    }

    public static IExportationWriter ConfigureWriter(JJDataExp exporter, IEnumerable<IExportationWriter> writers)
    {
        IExportationWriter exportationWriter;
        switch (exporter.ExportOptions.FileExtension)
        {
            case ExportFileExtension.CSV:
            case ExportFileExtension.TXT:
                var textWriter = GetTextWriter(writers);
                textWriter.Delimiter = exporter.ExportOptions.Delimiter;
                textWriter.OnRenderCell += exporter.OnRenderCell;

                exportationWriter = textWriter;
                break;

            case ExportFileExtension.XLS:
                var excelWriter = GetExcelWriter(writers);
                excelWriter.ShowRowStriped = exporter.ShowRowStriped;
                excelWriter.ShowBorder = exporter.ShowBorder;
                excelWriter.OnRenderCell += exporter.OnRenderCell;

                exportationWriter = excelWriter;

                break;
            case ExportFileExtension.PDF:
                var pdfWriter = GetPdfWriter(writers);

                if (pdfWriter == null)
                    throw new NotImplementedException("Please implement IPdfWriter in your application services.");

                pdfWriter.ShowRowStriped = exporter.ShowRowStriped;
                pdfWriter.ShowBorder = exporter.ShowBorder;
                pdfWriter.OnRenderCell += exporter.OnRenderCell;
                
                exportationWriter = pdfWriter;

                break;
            default:
                throw new NotImplementedException();
        }

        ConfigureWriter(exporter, exportationWriter);

        return exportationWriter;
    }

    private static void ConfigureWriter(JJDataExp exporter, IExportationWriter exportationWriter)
    {
        exportationWriter.FormElement = exporter.FormElement;
        exportationWriter.FieldManager = exporter.FieldManager;
        exportationWriter.Configuration = exporter.ExportOptions;
        exportationWriter.UserId = exporter.UserId;
        exportationWriter.ProcessOptions = exporter.ProcessOptions;
    }
}
