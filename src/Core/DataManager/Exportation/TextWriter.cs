using System;
using System.Collections;
using System.Data;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using JJMasterData.Commons.Data.Entity;
using JJMasterData.Commons.Data.Entity.Abstractions;
using JJMasterData.Commons.Localization;
using JJMasterData.Core.DataManager.Exports.Abstractions;
using JJMasterData.Core.DataManager.Services.Abstractions;
using JJMasterData.Core.FormEvents.Args;
using JJMasterData.Core.Options;
using JJMasterData.Core.Web;
using JJMasterData.Core.Web.Components;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace JJMasterData.Core.DataManager.Exports;

public class TextWriter : DataExportationWriterBase, ITextWriter
{
    public event EventHandler<GridCellEventArgs> OnRenderCell;

    public string Delimiter { get; set; }
    public IEntityRepository EntityRepository { get; } 
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
        int tot = 0;
        if (DataSource == null)
        {
            DataSource = EntityRepository.GetDataTable(FormElement, (IDictionary)CurrentFilter, CurrentOrder, RegPerPag, 1, ref tot);
            ProcessReporter.TotalRecords = tot;
            ProcessReporter.Message = StringLocalizer["Exporting {0} records...", tot.ToString("N0")];
            Reporter(ProcessReporter);
            await GenerateRows(sw, token);

            int totPag = (int)Math.Ceiling((double)tot / RegPerPag);
            for (int i = 2; i <= totPag; i++)
            {
                DataSource = EntityRepository.GetDataTable(FormElement, (IDictionary)CurrentFilter, CurrentOrder, RegPerPag, i, ref tot);
                await GenerateRows(sw, token);
            }
        }
        else
        {
            ProcessReporter.TotalRecords = DataSource.Rows.Count;
            await GenerateRows(sw, token);
        }
    }

    private async Task GenerateRows(StreamWriter sw, CancellationToken token)
    {
        foreach (DataRow row in DataSource.Rows)
        {
            bool isFirst = true;
            foreach (var field in await GetVisibleFieldsAsync())
            {
                if (isFirst)
                    isFirst = false;
                else
                    await sw.WriteAsync(Delimiter);

                string value = string.Empty;
                if (field.DataBehavior != FieldBehavior.Virtual)
                {
                    if (DataSource.Columns.Contains(field.Name))
                        value = row[field.Name].ToString();
                }

                var renderCell = OnRenderCell;
                if (renderCell != null)
                {
                    var args = new GridCellEventArgs
                    {
                        Field = field,
                        DataRow = row,
                        Sender = new JJText(value)
                    };
                    renderCell.Invoke(this, args);
                    value = args.HtmlResult;
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
        foreach (var field in await GetVisibleFieldsAsync())
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

    public TextWriter(IExpressionsService expressionsService, IStringLocalizer<JJMasterDataResources> stringLocalizer, IOptions<JJMasterDataCoreOptions> options, IControlFactory<JJTextFile> textFileFactory, ILogger<DataExportationWriterBase> logger) : base(expressionsService, stringLocalizer, options, textFileFactory, logger)
    {
    }
}
