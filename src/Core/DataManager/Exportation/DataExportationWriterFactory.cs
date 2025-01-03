#nullable enable
using System;
using JJMasterData.Commons.Tasks;
using JJMasterData.Core.DataManager.Exportation.Abstractions;
using JJMasterData.Core.DataManager.Exportation.Configuration;
using JJMasterData.Core.UI.Components;
using JJMasterData.Core.UI.Events.Args;
using Microsoft.Extensions.DependencyInjection;

namespace JJMasterData.Core.DataManager.Exportation;

public class DataExportationWriterFactory(IServiceProvider serviceProvider)
{
    public event AsyncEventHandler<GridCellEventArgs>? OnRenderCellAsync;

    private IPdfWriter? GetPdfWriter()
    {
        return serviceProvider.GetService<IPdfWriter>();
    }
    
    public bool PdfWriterExists()
    {
        return GetPdfWriter() != null;
    }

    private IExcelWriter GetExcelWriter()
    {
        return serviceProvider.GetRequiredService<IExcelWriter>();
    }

    private ITextWriter GetTextWriter()
    {
        return serviceProvider.GetRequiredService<ITextWriter>();
    }

    public DataExportationWriterBase GetInstance(JJDataExportation dataExportation)
    {
        DataExportationWriterBase writer;
        switch (dataExportation.ExportOptions.FileExtension)
        {
            case ExportFileExtension.CSV:
            case ExportFileExtension.TXT:
                var textWriter = GetTextWriter();
                textWriter.Delimiter = dataExportation.ExportOptions.Delimiter;
                textWriter.OnRenderCellAsync += OnRenderCellAsync;
                
                writer = (DataExportationWriterBase)textWriter;
                break;

            case ExportFileExtension.XLS:
                var excelWriter = GetExcelWriter();
                excelWriter.ShowRowStriped = dataExportation.ShowRowStriped;
                excelWriter.ShowBorder = dataExportation.ShowBorder;
                excelWriter.OnRenderCellAsync += OnRenderCellAsync;
                
                writer = (DataExportationWriterBase)excelWriter;

                break;
            case ExportFileExtension.PDF:
                var pdfWriter = GetPdfWriter();

                if (pdfWriter == null)
                    throw new NotImplementedException("Please implement IPdfWriter in your application services.");

                pdfWriter.ShowRowStriped = dataExportation.ShowRowStriped;
                pdfWriter.ShowBorder = dataExportation.ShowBorder;
                pdfWriter.OnRenderCellAsync += OnRenderCellAsync;
                
                // ReSharper disable once SuspiciousTypeConversion.Global;
                // PdfWriter is dynamic loaded by plugin.
                //TODO: I think this is bad, things from DataExportationWriterBase should be a parameter at IExportationWriter
                writer = (pdfWriter as DataExportationWriterBase)!;

                break;
            default:
                throw new NotImplementedException();
        }

        ConfigureWriter(dataExportation, writer);

        return writer;
    }

    private static void ConfigureWriter(JJDataExportation dataExportation, DataExportationWriterBase writer)
    {
        writer.FormElement = dataExportation.FormElement;
        writer.Configuration = dataExportation.ExportOptions;
        writer.UserId = dataExportation.UserId;
        writer.ProcessOptions = dataExportation.ProcessOptions;
        writer.AbsoluteUri = dataExportation.CurrentContext.Request.AbsoluteUri;
    }


}
