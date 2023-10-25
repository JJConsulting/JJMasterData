using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using JJMasterData.Commons.Data.Entity.Models;
using JJMasterData.Commons.Data.Entity.Repository;
using JJMasterData.Commons.Data.Entity.Repository.Abstractions;
using JJMasterData.Commons.Localization;
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

public class TextWriter : DataExportationWriterBase, ITextWriter
{
    public event EventHandler<GridCellEventArgs> OnRenderCell;
    public event AsyncEventHandler<GridCellEventArgs> OnRenderCellAsync;
    public string Delimiter { get; set; }
    private IEntityRepository EntityRepository { get; } 
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
            var result = await EntityRepository.GetDictionaryListResultAsync(FormElement, entityParameters);
            DataSource = result.Data;
            ProcessReporter.TotalOfRecords = result.TotalOfRecords;
            ProcessReporter.Message = StringLocalizer["Exporting {0} records...",  result.TotalOfRecords.ToString("N0")];
            Reporter(ProcessReporter);
            await GenerateRows(sw, token);

            int totPag = (int)Math.Ceiling((double)TotalOfRecords / RecordsPerPage);
            for (int i = 2; i <= totPag; i++)
            {
                entityParameters = new EntityParameters
                {
                    Filters = CurrentFilter,
                    RecordsPerPage = RecordsPerPage,
                    OrderBy = CurrentOrder,
                    CurrentPage = i,
                };
                result = await EntityRepository.GetDictionaryListResultAsync(FormElement, entityParameters);
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
                if (field.DataBehavior != FieldBehavior.Virtual)
                {
                    if (row.Keys.Contains(field.Name))
                        value = row[field.Name]?.ToString();
                }
                
                if (OnRenderCell != null || OnRenderCellAsync != null)
                {
                    var args = new GridCellEventArgs
                    {
                        Field = field,
                        DataRow = row,
                        Sender = new JJText(value)
                    };
                    OnRenderCell?.Invoke(this, args);

                    if (OnRenderCellAsync != null)
                    {
                        await OnRenderCellAsync(this, args);
                    }
                    
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

    private async Task GenerateHeader(System.IO.TextWriter sw)
    {
        bool isFirst = true;
        foreach (var field in VisibleFields)
        {
            if (isFirst)
                isFirst = false;
            else
                await sw.WriteAsync(Delimiter);

            await sw.WriteAsync(StringLocalizer[field.LabelOrName]);
        }
        await sw.WriteLineAsync("");
        await sw.FlushAsync();
    }

    public TextWriter(ExpressionsService expressionsService, 
        IStringLocalizer<MasterDataResources> stringLocalizer, 
        IOptions<MasterDataCoreOptions> options, 
        ControlFactory controlFactory, 
        ILoggerFactory logger, 
        IEntityRepository entityRepository) : base(expressionsService, 
                stringLocalizer, 
                options, 
                controlFactory, 
                logger.CreateLogger<DataExportationWriterBase>())
    {
        EntityRepository = entityRepository;
    }
}
