using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JJMasterData.Commons.DI;
using JJMasterData.Core.DataManager.Exports.Abstractions;
using JJMasterData.Core.DataManager.Exports.Configuration;
using JJMasterData.Core.WebComponents;
using Microsoft.Extensions.DependencyInjection;

namespace JJMasterData.Core.DataManager.Exports;

public static class WriterFactory
{

    public static IPdfWriter GetPdfWriter(IEnumerable<IWriter> writers)
    {
        return (IPdfWriter)writers.FirstOrDefault(writer=> writer.GetType() == typeof(IPdfWriter));
    }

    public static IExcelWriter GetExcelWriter(IEnumerable<IWriter> writers)
    {
        return (IExcelWriter)writers.FirstOrDefault(writer=> writer.GetType() == typeof(IExcelWriter));
    }

    public static ITextWriter GetTextWriter(IEnumerable<IWriter> writers)
    {
        return (ITextWriter)writers.FirstOrDefault(writer=> writer.GetType() == typeof(ITextWriter));
    }

    public static IWriter ConfigureWriter(JJDataExp exporter, IEnumerable<IWriter> writers)
    {
        IWriter writer;
        switch (exporter.ExportOptions.FileExtension)
        {
            case ExportFileExtension.CSV:
            case ExportFileExtension.TXT:
                var textWriter = GetTextWriter(writers);
                textWriter.Delimiter = exporter.ExportOptions.Delimiter;
                textWriter.OnRenderCell += exporter.OnRenderCell;

                writer = textWriter;
                break;

            case ExportFileExtension.XLS:
                var excelWriter = GetExcelWriter(writers);
                excelWriter.ShowRowStriped = exporter.ShowRowStriped;
                excelWriter.ShowBorder = exporter.ShowBorder;
                excelWriter.OnRenderCell += exporter.OnRenderCell;

                writer = excelWriter;

                break;
            case ExportFileExtension.PDF:
                var pdfWriter = GetPdfWriter(writers);

                if (pdfWriter == null)
                    throw new NotImplementedException("Please implement IPdfWriter in your application services.");

                pdfWriter.ShowRowStriped = exporter.ShowRowStriped;
                pdfWriter.ShowBorder = exporter.ShowBorder;
                pdfWriter.OnRenderCell += exporter.OnRenderCell;
                
                writer = pdfWriter;

                break;
            default:
                throw new NotImplementedException();
        }

        ConfigureWriter(exporter, writer);

        return writer;
    }

    private static void ConfigureWriter(JJDataExp exporter, IWriter writer)
    {
        writer.FormElement = exporter.FormElement;
        writer.FieldManager = exporter.FieldManager;
        writer.Configuration = exporter.ExportOptions;
        writer.UserId = exporter.UserId;
        writer.ProcessOptions = exporter.ProcessOptions;
    }
}
