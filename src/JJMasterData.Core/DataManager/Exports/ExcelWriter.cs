using System;
using System.Data;
using System.IO;
using System.Text;
using System.Threading;
using JJMasterData.Commons.Dao.Entity;
using JJMasterData.Commons.Language;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataManager.Exports.Abstractions;
using JJMasterData.Core.Facades;
using JJMasterData.Core.FormEvents.Args;
using JJMasterData.Core.Http.Abstractions;
using JJMasterData.Core.WebComponents;

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

    public ExcelWriter(
        IHttpContext httpContext, 
        RepositoryServicesFacade repositoryServicesFacade,
        CoreServicesFacade coreServicesFacade) : base(httpContext, repositoryServicesFacade, coreServicesFacade)
    {
    }

    public override void GenerateDocument(Stream stream, CancellationToken token)
    {
        using var sw = new StreamWriter(stream, Encoding.UTF8);

        sw.WriteLine("<html  ");
        sw.WriteLine("	xmlns:o=\"urn:schemas-microsoft-com:office:office\"  ");
        sw.WriteLine("	xmlns:x=\"urn:schemas-microsoft-com:office:excel\"  ");
        sw.WriteLine("	xmlns:v=\"urn:schemas-microsoft-com:vml\"> ");
        sw.WriteLine("<head> ");
        sw.WriteLine("<meta http-equiv=\"content-type\" content=\"text/html; charset=UTF-8\"> ");
        sw.WriteLine("</head> ");
        sw.WriteLine("<body>");
        sw.WriteLine("\t<table>");

        if (Configuration.ExportFirstLine)
        {
            GenerateHeader(sw);
        }

        sw.Flush();

        GenerateBody(sw, token);

        sw.WriteLine("\t</table>");
        sw.WriteLine("</body>");
        sw.WriteLine("</html>");

        sw.Close();
    }

    public void GenerateBody(StreamWriter sw, CancellationToken token)
    {
        int tot = 0;
        if (DataSource == null)
        {
            var factory = FieldManager.Expression.EntityRepository;
            DataSource = factory.GetDataTable(FormElement, CurrentFilter, CurrentOrder, RegPerPag, 1, ref tot);
            ProcessReporter.TotalRecords = tot;
            ProcessReporter.Message = Translate.Key("Exporting {0} records...", tot.ToString("N0"));
            Reporter(ProcessReporter);
            GenerateRows(sw, token);

            int totPag = (int)Math.Ceiling((double)tot / RegPerPag);
            for (int i = 2; i <= totPag; i++)
            {
                DataSource = factory.GetDataTable(FormElement, CurrentFilter, CurrentOrder, RegPerPag, i, ref tot);
                GenerateRows(sw, token);
            }
        }
        else
        {
            ProcessReporter.TotalRecords = DataSource.Rows.Count;
            GenerateRows(sw, token);
        }
    }

    public void GenerateRows(StreamWriter sw, CancellationToken token)
    {
        foreach (DataRow row in DataSource.Rows)
        {
            sw.Write("\t\t\t<tr>");
            foreach (var field in Fields)
            {
                string value = CreateCell(row, field);

                string tdStyle;
                if (field.DataType == FieldType.Float ||
                    field.DataType == FieldType.Int)
                {
                    tdStyle = " style=\"text-align:right;\" ";
                }
                else
                {
                    tdStyle = " style=\"mso-number-format:'@';\" ";
                }

                sw.Write("\t\t\t\t<td" + tdStyle + ">");
                sw.Write(value);
                sw.WriteLine("</td>");
            }

            sw.WriteLine("\t\t\t</tr>");
            sw.Flush();
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
                value = FieldManager.FormatVal(field, row[field.Name]);
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
            OnRenderCell.Invoke(this, args);
            value = args.HtmlResult;
        }

        return value;
    }

    private void GenerateHeader(StreamWriter sw)
    {
        sw.WriteLine("\t\t\t<tr>");
        foreach (var field in Fields)
        {
            string thStyle = "";
            if (field.DataType == FieldType.Float ||
                field.DataType == FieldType.Int)
            {
                thStyle = " style=\"text-align:right;\" ";
            }

            sw.Write("\t\t\t\t<td" + thStyle + ">");
            sw.Write(field.GetTranslatedLabel());
            sw.WriteLine("</td>");
        }

        sw.WriteLine("\t\t\t</tr>");
    }
}