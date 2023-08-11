using System;
using System.Collections;
using System.Data;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using JJMasterData.Commons.Configuration;
using JJMasterData.Commons.Data.Entity;
using JJMasterData.Commons.Data.Entity.Abstractions;
using JJMasterData.Commons.DI;
using JJMasterData.Commons.Localization;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataManager.Exports.Abstractions;
using JJMasterData.Core.DataManager.Services;
using JJMasterData.Core.FormEvents.Args;
using JJMasterData.Core.Web.Components;
using Microsoft.Extensions.Localization;

namespace JJMasterData.Core.DataManager.Exports;

public class ExcelWriter : BaseWriter, IExcelWriter
{
    public event EventHandler<GridCellEventArgs> OnRenderCell;

    /// <summary>
    /// Exibi borda na grid 
    /// (Default = false)
    /// </summary>
    public bool ShowBorder { get; set; }

    /// <summary>
    /// Exibir colunas zebradas 
    /// (Default = true)
    /// </summary>
    public bool ShowRowStriped { get; set; }
    public IEntityRepository EntityRepository { get; } 
    public IFieldFormattingService FieldFormattingService { get; } 

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

        sw.Close();
    }

    public async Task GenerateBody(StreamWriter sw, CancellationToken token)
    {
        int tot = 0;
        if (DataSource == null)
        {
            DataSource = EntityRepository.GetDataTable(FormElement, CurrentFilter as IDictionary, CurrentOrder, RegPerPag, 1, ref tot);
            ProcessReporter.TotalRecords = tot;
            ProcessReporter.Message = StringLocalizer["Exporting {0} records...", tot.ToString("N0")];
            Reporter(ProcessReporter);
            await GenerateRows(sw, token);

            int totPag = (int)Math.Ceiling((double)tot / RegPerPag);
            for (int i = 2; i <= totPag; i++)
            {
                DataSource = EntityRepository.GetDataTable(FormElement, CurrentFilter as IDictionary, CurrentOrder, RegPerPag, i, ref tot);
                await GenerateRows(sw, token);
            }
        }
        else
        {
            ProcessReporter.TotalRecords = DataSource.Rows.Count;
            await GenerateRows(sw, token);
        }
    }

    public async Task GenerateRows(StreamWriter sw, CancellationToken token)
    {
        foreach (DataRow row in DataSource.Rows)
        {
            await sw.WriteAsync("\t\t\t<tr>");
            foreach (var field in await GetVisibleFieldsAsync())
            {
                string value = CreateCell(row, field);

                string tdStyle;
                if (field.DataType is FieldType.Float or FieldType.Int)
                {
                    tdStyle = " style=\"text-align:right;\" ";
                }
                else
                {
                    tdStyle = " style=\"mso-number-format:'@';\" ";
                }

                await sw.WriteAsync("\t\t\t\t<td" + tdStyle + ">");
                await sw.WriteAsync(value);
                await sw.WriteLineAsync("</td>");
            }

            await sw.WriteLineAsync("\t\t\t</tr>");
            await sw.FlushAsync();
            ProcessReporter.TotalProcessed++;

            Reporter(ProcessReporter);
            token.ThrowIfCancellationRequested();
        }
    }

    private string CreateCell(DataRow row, FormElementField field)
    {
        string value = string.Empty;
        if (field.DataBehavior != FieldBehavior.Virtual)
        {
            if (DataSource.Columns.Contains(field.Name))
            {
                value = FieldFormattingService.FormatValue(field, row[field.Name]);
            }
        }

        if (field.Component == FormComponent.File)
        {
            string link = GetLinkFile(field, row, value);
            if (link != null)
                value = $"<a href=\"{link}\">{value}</a>";
            else
            {
                if (value != null)
                    value = value.Replace(",", "<br style=\"mso-data-placement:same-cell;\"/>");
            }
        }

        var renderCell = OnRenderCell;
        if (renderCell != null)
        {
            var args = new GridCellEventArgs();
            args.Field = field;
            args.DataRow = row;
            args.Sender = new JJText(value);
            OnRenderCell?.Invoke(this, args);
            value = args.HtmlResult;
        }

        return value;
    }

    private async Task GenerateHeader(StreamWriter sw)
    {
        await sw.WriteLineAsync("\t\t\t<tr>");
        foreach (var field in await GetVisibleFieldsAsync())
        {
            string thStyle = "";
            if (field.DataType == FieldType.Float ||
                field.DataType == FieldType.Int)
            {
                thStyle = " style=\"text-align:right;\" ";
            }
            await sw.WriteAsync("\t\t\t\t<td" + thStyle + ">");
            await sw.WriteAsync(StringLocalizer[field.Label]);
            await sw.WriteLineAsync("</td>");
        }
        await sw.WriteLineAsync("\t\t\t</tr>");
    }
}
