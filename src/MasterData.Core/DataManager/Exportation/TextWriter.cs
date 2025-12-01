using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using JJConsulting.Html.Bootstrap.Components;
using JJMasterData.Commons.Data.Entity.Models;
using JJMasterData.Commons.Data.Entity.Repository;
using JJMasterData.Commons.Data.Entity.Repository.Abstractions;
using JJMasterData.Commons.Security.Cryptography.Abstractions;
using JJMasterData.Commons.Tasks;
using JJMasterData.Core.Configuration.Options;
using JJMasterData.Core.DataManager.Exportation.Abstractions;
using JJMasterData.Core.DataManager.Expressions;
using JJMasterData.Core.UI.Components;
using JJMasterData.Core.UI.Events.Args;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace JJMasterData.Core.DataManager.Exportation;

public class TextWriter(
        IEncryptionService encryptionService,
        ExpressionsService expressionsService,
        IStringLocalizer<MasterDataResources> stringLocalizer,
        IOptionsSnapshot<MasterDataCoreOptions> options,
        ILoggerFactory logger,
        IEntityRepository entityRepository)
    : DataExportationWriterBase(
        encryptionService,
        expressionsService,
        stringLocalizer,
        options,
        logger.CreateLogger<DataExportationWriterBase>()), ITextWriter
{
    public event AsyncEventHandler<GridCellEventArgs> OnRenderCellAsync;
    public string Delimiter { get; set; }

    public override async Task GenerateDocument(Stream stream, CancellationToken token)
    {
        using var sw = new StreamWriter(stream, Encoding.UTF8);

        if (Configuration.ExportFirstLine)
        {
            await GenerateHeader(sw);
        }

        await GenerateBody(sw, token);

        sw.Close();
    }


    private async Task GenerateBody(StreamWriter sw, CancellationToken token)
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
            await GenerateRows(sw, token);

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
                await GenerateRows(sw, token);
            }
        }
        else
        {
            ProcessReporter.TotalOfRecords = TotalOfRecords;
            await GenerateRows(sw, token);
        }
    }

    private async Task GenerateRows(StreamWriter sw, CancellationToken token)
    {
        foreach (var row in DataSource)
        {
            bool isFirst = true;
            foreach (var field in VisibleFields)
            {
                if (isFirst)
                    isFirst = false;
                else
                    await sw.WriteAsync(Delimiter);

                string value = string.Empty;
                if (field.DataBehavior is not FieldBehavior.Virtual && field.DataBehavior is not FieldBehavior.WriteOnly)
                {
                    if (row.TryGetValue(field.Name, out var cellValue))
                        value = cellValue?.ToString();
                }

                if (OnRenderCellAsync != null)
                {
                    var args = new GridCellEventArgs
                    {
                        Field = field,
                        DataRow = row,
                        Sender = new JJText(value)
                    };

                    await OnRenderCellAsync(this, args);

                    if(args.HtmlResult != null)
                        value = args.HtmlResult.ToString();
                }

                await sw.WriteAsync(value);
            }
            await sw.WriteLineAsync("");
            await sw.FlushAsync();

            ProcessReporter.TotalProcessed++;
            Reporter(ProcessReporter);
            token.ThrowIfCancellationRequested();
        }
    }

    private async Task GenerateHeader(StreamWriter sw)
    {
        bool isFirst = true;
        foreach (var field in VisibleFields)
        {
            if (isFirst)
                isFirst = false;
            else
                await sw.WriteAsync(Delimiter);

            await sw.WriteAsync(string.IsNullOrEmpty(field.Label) ? field.Name : StringLocalizer[field.Label]);
        }
        await sw.WriteLineAsync("");
        await sw.FlushAsync();
    }
}
