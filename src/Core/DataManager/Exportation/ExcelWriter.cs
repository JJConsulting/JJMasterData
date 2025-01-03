using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using JJMasterData.Commons.Data.Entity.Models;
using JJMasterData.Commons.Data.Entity.Repository;
using JJMasterData.Commons.Data.Entity.Repository.Abstractions;
using JJMasterData.Commons.Localization;
using JJMasterData.Commons.Security.Cryptography.Abstractions;
using JJMasterData.Commons.Tasks;
using JJMasterData.Core.Configuration.Options;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.DataManager.Exportation.Abstractions;
using JJMasterData.Core.DataManager.Expressions;
using JJMasterData.Core.DataManager.Services;
using JJMasterData.Core.UI.Components;
using JJMasterData.Core.UI.Events.Args;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace JJMasterData.Core.DataManager.Exportation;

public class ExcelWriter(
        ExpressionsService expressionsService,
        IEncryptionService encryptionService,
        IStringLocalizer<MasterDataResources> stringLocalizer,
        IOptionsSnapshot<MasterDataCoreOptions> options,
        ILoggerFactory loggerFactory,
        IEntityRepository entityRepository)
    : DataExportationWriterBase(
        encryptionService,
        expressionsService,
        stringLocalizer,
        options,
        loggerFactory.CreateLogger<DataExportationWriterBase>()), IExcelWriter
{
    public event AsyncEventHandler<GridCellEventArgs> OnRenderCellAsync;

    public bool ShowBorder { get; set; }

    /// <summary>
    /// Exibir colunas zebradas 
    /// (Default = true)
    /// </summary>
    public bool ShowRowStriped { get; set; }

    private IEntityRepository EntityRepository { get; } = entityRepository;

    public override async Task GenerateDocument(Stream stream, CancellationToken token)
    {
        using var sw = new StreamWriter(stream, Encoding.UTF8);
        await sw.WriteLineAsync("<html  ");
        await sw.WriteLineAsync("	xmlns:o=\"urn:schemas-microsoft-com:office:office\"  ");
        await sw.WriteLineAsync("	xmlns:x=\"urn:schemas-microsoft-com:office:excel\"  ");
        await sw.WriteLineAsync("	xmlns:v=\"urn:schemas-microsoft-com:vml\"> ");
        await sw.WriteLineAsync("<head> ");
        await sw.WriteLineAsync("<meta http-equiv=\"content-type\" content=\"text/html; charset=UTF-8\"> ");
        await sw.WriteLineAsync("</head> ");
        await sw.WriteLineAsync("<body>");
        await sw.WriteLineAsync("\t<table>");

        if (Configuration.ExportFirstLine)
        {
            await GenerateHeader(sw);
        }

        await sw.FlushAsync();

        await GenerateBody(sw, token);

        await sw.WriteLineAsync("\t</table>");
        await sw.WriteLineAsync("</body>");
        await sw.WriteLineAsync("</html>");

        await sw.FlushAsync();
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
            TotalOfRecords = result.TotalOfRecords;
            ProcessReporter.TotalOfRecords = result.TotalOfRecords;
            ProcessReporter.Message = StringLocalizer["Exporting {0} records...", TotalOfRecords.ToString("N0")];
            Reporter(ProcessReporter);
            await GenerateRows(sw, token);

            var totalOfPages = (int)Math.Ceiling((double)TotalOfRecords / RecordsPerPage);
            for (int i = 2; i <= totalOfPages; i++)
            {
                entityParameters = new EntityParameters
                {
                    Filters = CurrentFilter,
                    CurrentPage = i,
                    RecordsPerPage = RecordsPerPage,
                    OrderBy = CurrentOrder
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
            await sw.WriteAsync("\t\t\t<tr>");
            foreach (var field in VisibleFields)
            {
                var value = await CreateCell(field, row);

                string tdStyle;
                if (field.DataType is FieldType.Float or FieldType.Int)
                {
                    tdStyle = " style=\"text-align:right;\" ";
                }
                else
                {
                    tdStyle = " style=\"mso-number-format:'@';\" ";
                }

                await sw.WriteAsync($"\t\t\t\t<td{tdStyle}>");
                await sw.WriteAsync(value.Replace("\n"," ").Replace("\t"," "));
                await sw.WriteLineAsync("</td>");
            }

            await sw.WriteLineAsync("\t\t\t</tr>");
            await sw.FlushAsync();
            ProcessReporter.TotalProcessed++;

            Reporter(ProcessReporter);
            token.ThrowIfCancellationRequested();
        }
    }

    private async Task<string> CreateCell(FormElementField field, Dictionary<string, object> row)
    {
        var value = string.Empty;
        if (field.DataBehavior is not FieldBehavior.Virtual && field.DataBehavior is not FieldBehavior.WriteOnly)
        {
            if (row.TryGetValue(field.Name, out var cellValue))
            {
                value = FieldFormattingService.FormatValue(field, cellValue);
            }
        }

        if (field.Component == FormComponent.File)
        {
            string link = GetFileLink(field, row, value);
            if (link != null)
                value = $"<a href=\"{link}\">{value}</a>";
            else
            {
                value = value.Replace(",", "<br style=\"mso-data-placement:same-cell;\"/>");
            }
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

            if (args.HtmlResult is not null)
            {
                value = args.HtmlResult.ToString();
            }
        }

        if (field.Component is FormComponent.File)
            return value;
        
        return HttpUtility.HtmlEncode(value);
    }

    private async Task GenerateHeader(StreamWriter sw)
    {
        await sw.WriteLineAsync("\t\t\t<tr>");
        foreach (var field in VisibleFields)
        {
            string thStyle = "";
            if (field.DataType is FieldType.Float or FieldType.Int)
            {
                thStyle = " style=\"text-align:right;\" ";
            }

            await sw.WriteAsync($"\t\t\t\t<td{thStyle}>");
            await sw.WriteAsync(StringLocalizer[field.LabelOrName]);
            await sw.WriteLineAsync("</td>");
        }

        await sw.WriteLineAsync("\t\t\t</tr>");
    }
}