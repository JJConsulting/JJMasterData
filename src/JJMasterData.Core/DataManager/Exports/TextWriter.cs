using System;
using System.Data;
using System.IO;
using System.Text;
using System.Threading;
using JJMasterData.Commons.Dao.Entity;
using JJMasterData.Commons.Language;
using JJMasterData.Core.DataManager.Exports.Abstractions;
using JJMasterData.Core.FormEvents.Args;
using JJMasterData.Core.WebComponents;

namespace JJMasterData.Core.DataManager.Exports;

public class TextWriter : BaseWriter, ITextWriter
{
    public event EventHandler<GridCellEventArgs> OnRenderCell;

    public string Delimiter { get; set; }

    public override void GenerateDocument(Stream stream, CancellationToken token)
    {
        using var sw = new StreamWriter(stream, Encoding.UTF8);

        if (Configuration.ExportFirstLine)
        {
            GenerateHeader(sw);
        }

        GenerateBody(sw, token);

        sw.Close();
    }


    private void GenerateBody(StreamWriter sw, CancellationToken token)
    {
        int tot = 0;
        if (DataSource == null)
        {
            var factory = new Factory(FieldManager.DataAccess);
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

    private void GenerateRows(StreamWriter sw, CancellationToken token)
    {
        foreach (DataRow row in DataSource.Rows)
        {
            bool isFirst = true;
            foreach (var field in Fields)
            {
                if (isFirst)
                    isFirst = false;
                else
                    sw.Write(Delimiter);

                string value = string.Empty;
                if (field.DataBehavior != FieldBehavior.Virtual)
                {
                    if (DataSource.Columns.Contains(field.Name))
                        value = row[field.Name].ToString();
                }

                var renderCell = OnRenderCell;
                if (renderCell != null)
                {
                    var args = new GridCellEventArgs();
                    args.Field = field;
                    args.DataRow = row;
                    args.Sender = new JJText(value);
                    OnRenderCell.Invoke(this, args);
                    value = args.ResultHtml;
                }

                sw.Write(value);
            }
            sw.WriteLine("");
            sw.Flush();

            ProcessReporter.TotalProcessed++;
            Reporter(ProcessReporter);
            token.ThrowIfCancellationRequested();
        }
    }

    private void GenerateHeader(StreamWriter sw)
    {
        bool isFirst = true;
        foreach (var field in Fields)
        {
            if (isFirst)
                isFirst = false;
            else
                sw.Write(Delimiter);

            sw.Write(field.GetTranslatedLabel());
        }
        sw.WriteLine("");
        sw.Flush();
    }
}
