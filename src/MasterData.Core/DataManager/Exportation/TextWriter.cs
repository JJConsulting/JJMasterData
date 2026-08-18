#nullable disable warnings
using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CsvHelper;
using CsvHelper.Configuration;
using JJConsulting.Html.Bootstrap.Components;
using JJMasterData.Commons.Data.Entity.Models;
using JJMasterData.Commons.Data.Entity.Repository;
using JJMasterData.Commons.Data.Entity.Repository.Abstractions;
using JJMasterData.Core.Configuration.Options;
using JJMasterData.Core.DataManager.Exportation.Abstractions;
using JJMasterData.Core.DataManager.Expressions;
using JJMasterData.Core.UI.Events.Args;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace JJMasterData.Core.DataManager.Exportation;

public class TextWriter(
    ExpressionsService expressionsService,
        IStringLocalizer<MasterDataResources> stringLocalizer,
        IOptionsSnapshot<MasterDataCoreOptions> options,
        ILoggerFactory logger,
        IEntityRepository entityRepository)
    : DataExportationWriterBase(expressionsService,
        stringLocalizer,
        options,
        logger.CreateLogger<DataExportationWriterBase>()), ITextWriter
{
    public event EventHandler<GridCellEventArgs> OnRenderCell;
    public string Delimiter { get; set; }

    public override async Task GenerateDocument(Stream stream, CancellationToken token)
    {
        await using var streamWriter = new StreamWriter(stream, Encoding.UTF8, leaveOpen: true);
        await using var csvWriter = new CsvWriter(streamWriter, new CsvConfiguration(CultureInfo.CurrentCulture)
        {
            Delimiter = Delimiter,
            HasHeaderRecord = false
        });

        if (Configuration.ExportFirstLine)
        {
            await GenerateHeader(csvWriter);
        }

        await GenerateBody(csvWriter, token);
        await streamWriter.FlushAsync(token);
    }


    private async Task GenerateBody(CsvWriter csvWriter, CancellationToken token)
    {

        if (DataSource == null)
        {
            var entityParameters = new EntityParameters
            {
                Filters = CurrentFilter,
                RecordsPerPage = RecordsPerPage,
                OrderBy = CurrentOrder,
                CurrentPage = 1,
            };
            var result = await entityRepository.GetDictionaryListResultAsync(FormElement, entityParameters);
            DataSource = result.Data;
            ProcessReporter.TotalOfRecords = result.TotalOfRecords;
            ProcessReporter.Message = StringLocalizer["Exporting {0} records...",  result.TotalOfRecords.ToString("N0")];
            Reporter(ProcessReporter);
            await GenerateRows(csvWriter, token);

            var totalOfPages = (int)Math.Ceiling((double)TotalOfRecords / RecordsPerPage);
            
            for (var i = 2; i <= totalOfPages; i++)
            {
                entityParameters = new EntityParameters
                {
                    Filters = CurrentFilter,
                    RecordsPerPage = RecordsPerPage,
                    OrderBy = CurrentOrder,
                    CurrentPage = i,
                };
                result = await entityRepository.GetDictionaryListResultAsync(FormElement, entityParameters);
                DataSource = result.Data;
                TotalOfRecords = result.TotalOfRecords;
                await GenerateRows(csvWriter, token);
            }
        }
        else
        {
            ProcessReporter.TotalOfRecords = TotalOfRecords;
            await GenerateRows(csvWriter, token);
        }
    }

    private async Task GenerateRows(CsvWriter csvWriter, CancellationToken token)
    {
        foreach (var row in DataSource ?? [])
        {
            foreach (var field in VisibleFields)
            {
                string value = string.Empty;
                if (field.DataBehavior is not FieldBehavior.Virtual && field.DataBehavior is not FieldBehavior.WriteOnly)
                {
                    if (row.TryGetValue(field.Name, out var cellValue))
                        value = cellValue?.ToString();
                }

                if (OnRenderCell != null)
                {
                    var args = new GridCellEventArgs
                    {
                        Field = field,
                        DataRow = row,
                        Sender = new JJText(value)
                    };

                    OnRenderCell(this, args);

                    if(args.HtmlResult != null)
                        value = args.HtmlResult.ToString();
                }

                csvWriter.WriteField(value);
            }
            await csvWriter.NextRecordAsync();

            ProcessReporter.TotalProcessed++;
            Reporter(ProcessReporter);
            token.ThrowIfCancellationRequested();
        }
    }

    private async Task GenerateHeader(CsvWriter csvWriter)
    {
        foreach (var field in VisibleFields)
        {
            csvWriter.WriteField(string.IsNullOrEmpty(field.Label) ? field.Name : StringLocalizer[field.Label]);
        }
        await csvWriter.NextRecordAsync();
    }
}
