using System;
using JJMasterData.Commons.Tasks;
using JJMasterData.Core.DataManager.Exports.Abstractions;
using JJMasterData.Core.DataManager.Exports.Configuration;
using JJMasterData.Core.FormEvents.Args;
using JJMasterData.Core.Web.Components;
using Microsoft.Extensions.DependencyInjection;

namespace JJMasterData.Core.DataManager.Exports;

public class DataExportationWriterFactory
{
    private IServiceProvider ServiceProvider { get; }

    public event EventHandler<GridCellEventArgs> OnRenderCell;
    public event AsyncEventHandler<GridCellEventArgs> OnRenderCellAsync;
    public DataExportationWriterFactory(IServiceProvider serviceProvider)
    {
        ServiceProvider = serviceProvider;
    }
    
    private IPdfWriter GetPdfWriter()
    {
        return ServiceProvider.GetRequiredService<IPdfWriter>();
    }
    
    public bool PdfWriterExists()
    {
        return GetPdfWriter() != null;
    }

    private IExcelWriter GetExcelWriter()
    {
        return ServiceProvider.GetRequiredService<IExcelWriter>();
    }

    private ITextWriter GetTextWriter()
    {
        return ServiceProvider.GetRequiredService<ITextWriter>();
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
                textWriter.OnRenderCell += OnRenderCell;
                textWriter.OnRenderCellAsync += OnRenderCellAsync;
                
                writer = (DataExportationWriterBase)textWriter;
                break;

            case ExportFileExtension.XLS:
                var excelWriter = GetExcelWriter();
                excelWriter.ShowRowStriped = dataExportation.ShowRowStriped;
                excelWriter.ShowBorder = dataExportation.ShowBorder;
                excelWriter.OnRenderCell += OnRenderCell;
                excelWriter.OnRenderCellAsync += OnRenderCellAsync;
                
                writer = (DataExportationWriterBase)excelWriter;

                break;
            case ExportFileExtension.PDF:
                var pdfWriter = GetPdfWriter();

                if (pdfWriter == null)
                    throw new NotImplementedException("Please implement IPdfWriter in your application services.");

                pdfWriter.ShowRowStriped = dataExportation.ShowRowStriped;
                pdfWriter.ShowBorder = dataExportation.ShowBorder;
                pdfWriter.OnRenderCell += OnRenderCell;
                pdfWriter.OnRenderCellAsync += OnRenderCellAsync;
                
                // ReSharper disable once SuspiciousTypeConversion.Global;
                // PdfWriter is dynamic loaded by plugin.
                //TODO: I think this is bad, things from DataExportationWriterBase should be a parameter at IExportationWriter
                writer = pdfWriter as DataExportationWriterBase;

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
    }


}
