using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using iText.IO.Font;
using iText.Kernel.Colors;
using iText.Kernel.Font;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Action;
using iText.Kernel.Pdf.Canvas.Draw;
using iText.Layout;
using iText.Layout.Borders;
using iText.Layout.Element;
using iText.Layout.Properties;
using JJMasterData.Commons.Data.Entity;
using JJMasterData.Commons.Data.Entity.Abstractions;
using JJMasterData.Commons.Localization;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataManager.Exports.Abstractions;
using JJMasterData.Core.DataManager.Services;
using JJMasterData.Core.DataManager.Services.Abstractions;
using JJMasterData.Core.FormEvents.Args;
using JJMasterData.Core.Options;
using JJMasterData.Core.Web;
using JJMasterData.Core.Web.Components;
using JJMasterData.Core.Web.Factories;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace JJMasterData.Pdf;

public class PdfWriter : DataExportationWriterBase, IPdfWriter
{
    public event EventHandler<GridCellEventArgs> OnRenderCell;
    
    public bool ShowBorder { get; set; }
    
    public bool ShowRowStriped { get; set; }

    public bool IsLandscape { get; set; }

    public IEntityRepository EntityRepository { get; } 
    
    public IFieldFormattingService FieldFormattingService { get; }
    
    public ControlFactory ControlFactory { get; }
    public PdfWriter(IExpressionsService expressionsService, 
                     IStringLocalizer<JJMasterDataResources> stringLocalizer, 
                     IOptions<JJMasterDataCoreOptions> options, 
                     IControlFactory<JJTextFile> textFileFactory, 
                     ILogger<PdfWriter> logger, 
                     IEntityRepository entityRepository, 
                     IFieldFormattingService fieldFormattingService, ControlFactory controlFactory) : base(expressionsService, stringLocalizer, options, textFileFactory, logger)
    {
        EntityRepository = entityRepository;
        FieldFormattingService = fieldFormattingService;
        ControlFactory = controlFactory;
    }
    
    public override async Task GenerateDocument(Stream ms, CancellationToken token)
    {
        using var writer = new iText.Kernel.Pdf.PdfWriter(ms);

        var pdf = new PdfDocument(writer);

        var pageSize = IsLandscape ? PageSize.A4.Rotate() : PageSize.A4;
        pdf.SetDefaultPageSize(pageSize);

        var document = new Document(pdf);
        document.SetFontSize(8);
        var today = new Paragraph(DateTime.Now.ToLongDateString()).SetTextAlignment(TextAlignment.RIGHT);
        document.Add(today);

        var title = FormElement.Title;
        if (!string.IsNullOrWhiteSpace(title))
        {
            var header = new Paragraph(title)
                .SetTextAlignment(TextAlignment.LEFT)
                .SetFontSize(16)
                .SetBold();
            document.Add(header);
        }

        var line = new SolidLine(1f);
        line.SetColor(WebColors.GetRGBColor("black"));

        var ls = new LineSeparator(line);
        document.Add(ls);

        var paragraph = new Paragraph("\n");
        document.Add(paragraph);

        var fields = await GetVisibleFieldsAsync();
        
        var table = new Table(fields.Count, true);
        table.UseAllAvailableWidth();
        document.Add(table);

        await GenerateHeaderAsync(table);
        await GenerateBody(table, token);

        table.Complete();
        document.Close();
        pdf.Close();
    }

    private async Task GenerateHeaderAsync(Table table)
    {
        var fields = await GetVisibleFieldsAsync();
        foreach (var field in fields)
        {
            Cell cell = new();
            cell.Add(new Paragraph(new Text(field.LabelOrName).SetBold()));
            SetHeaderCellStyle(field, ref cell);
            table.AddHeaderCell(cell);
        }
        table.Flush();
    }

    private async Task GenerateBody(Table table, CancellationToken token)
    {
        int tot = 0;
        if (DataSource == null)
        {
            var entityParameters = new EntityParameters
            {
                Parameters = CurrentFilter,
                RecordsPerPage = RecordsPerPage,
                OrderBy = CurrentOrder,
                CurrentPage = 1,
            };
            DataSource = await EntityRepository.GetDictionaryListAsync(FormElement,entityParameters);
            ProcessReporter.TotalRecords = tot;
            ProcessReporter.Message = StringLocalizer["Exporting {0} records...", tot.ToString("N0")];
            Reporter(ProcessReporter);
            await GenerateRows(table, token);

            int totPag = (int)Math.Ceiling((double)tot / RecordsPerPage);
            for (int i = 2; i <= totPag; i++)
            {
                entityParameters = new EntityParameters
                {
                    Parameters = CurrentFilter,
                    RecordsPerPage = RecordsPerPage,
                    OrderBy = CurrentOrder,
                    CurrentPage = i,
                };
                DataSource = await EntityRepository.GetDictionaryListAsync(FormElement,entityParameters);
                await GenerateRows(table, token);
            }
        }
        else
        {
            ProcessReporter.TotalRecords = DataSource.Count;
            await GenerateRows(table, token);
        }
    }

    private async Task GenerateRows(Table table, CancellationToken token)
    {
        foreach (Dictionary<string,object> row in DataSource.Data)
        {
            var scolor = (ShowRowStriped && (ProcessReporter.TotalProcessed % 2) == 0) ? "white" : "#f2fdff";
            var wcolor = WebColors.GetRGBColor(scolor);
            table.SetBackgroundColor(wcolor);
            var fields = await GetVisibleFieldsAsync();
            foreach (FormElementField field in fields)
            {
                var cell = await CreateCellAsync(row, field);
                table.AddCell(cell);
                table.Flush();
            }

            ProcessReporter.TotalProcessed++;
            Reporter(ProcessReporter);
            token.ThrowIfCancellationRequested();
        }
    }
    
    private async Task<Cell> CreateCellAsync(Dictionary<string,object> row, FormElementField field)
    {
        string value = string.Empty;
        Text image = null;

        if (field.DataBehavior != FieldBehavior.Virtual)
        {
            if (field.Component == FormComponent.ComboBox && field.DataItem != null)
            {
                var valRet = await GetComboBoxValueAsync(field, row);
                value = valRet.Item1;
                image = valRet.Item2;
            }
            else
            {
                value = await FieldFormattingService.FormatGridValueAsync(field, row,null);
            }
        }

        var cell = new Cell();
        SetCellStyle(field, ref cell);

        var ev = OnRenderCell;
        if (ev != null)
        {
            var args = new GridCellEventArgs
            {
                Field = field,
                DataRow = row,
                Sender = new JJText(value)
            };

            ev.Invoke(this, args);

            value = args.HtmlResult;
            value = value.Replace("<br>", "\r\n");
            value = value.Replace("<center>", string.Empty);
            value = value.Replace("</center>", string.Empty);
        }


        Link link = null;

        if (field.Component == FormComponent.File)
        {
            string url = GetLinkFile(field, row, value);

            if (url != null)
            {
                link = new Link(value, PdfAction.CreateURI(url));
            }
            else
            {
                value = value?.Replace(",", "\r\n");
            }
        }

        var paragraph = new Paragraph();

        if (image != null)
            paragraph.Add(image);

        if (link != null)
            paragraph.Add(link);
        else
            paragraph.Add(value);

        cell.Add(paragraph);

        return cell;
    }

    private void SetHeaderCellStyle(FormElementField field, ref Cell cell)
    {
        cell.SetBorderTop(ShowBorder ? new GrooveBorder(WebColors.GetRGBColor("black"), 1f) : null);
        cell.SetBorderRight(ShowBorder ? new GrooveBorder(WebColors.GetRGBColor("black"), 1f) : null);
        cell.SetBorderLeft(ShowBorder ? new GrooveBorder(WebColors.GetRGBColor("black"), 1f) : null);

        if (!field.IsPk && field.Component != FormComponent.ComboBox &&
            field.DataType is FieldType.Float or FieldType.Int)
        {
            cell.SetHorizontalAlignment(HorizontalAlignment.RIGHT);
        }
        else
        {
            cell.SetHorizontalAlignment(HorizontalAlignment.LEFT);
        }
    }

    private void SetCellStyle(FormElementField field, ref Cell cell)
    {
        cell.SetBorderBottom(new GrooveBorder(WebColors.GetRGBColor("black"), 1f));
        cell.SetBorderTop(ShowBorder ? new GrooveBorder(WebColors.GetRGBColor("black"), 1f) : null);
        cell.SetBorderRight(ShowBorder ? new GrooveBorder(WebColors.GetRGBColor("black"), 1f) : null);
        cell.SetBorderLeft(ShowBorder ? new GrooveBorder(WebColors.GetRGBColor("black"), 1f) : null);

        if (!field.IsPk && field.Component != FormComponent.ComboBox &&
            (field.DataType == FieldType.Float || field.DataType == FieldType.Int))
        {
            cell.SetHorizontalAlignment(HorizontalAlignment.RIGHT);
        }
        else
        {
            cell.SetHorizontalAlignment(HorizontalAlignment.LEFT);
        }
    }

    private async Task<(string, Text)> GetComboBoxValueAsync(FormElementField field, IDictionary<string, object> values)
    {
        if (values == null || !values.ContainsKey(field.Name) || values[field.Name] == null)
            return (string.Empty, null);
        
        Text image = null;
        string value = string.Empty;
        string selectedValue = values[field.Name].ToString();
        var cbo = (JJComboBox)(await ControlFactory.CreateAsync(FormElement,field, values,null, PageState.List,null, selectedValue));
        var item = cbo.GetValue(selectedValue);

        if (item != null)
        {
            if (field.DataItem!.ReplaceTextOnGrid)
            {
                value = " " + item.Description.Trim();
            }
            if (field.DataItem.ShowImageLegend)
            {
                image = new Text(item.Icon.GetUnicode().ToString());
                var color = ColorTranslator.FromHtml(item.ImageColor);

                var rgbColor = $"rgb({Convert.ToInt16(color.R)},{Convert.ToInt16(color.G)},{Convert.ToInt16(color.B)})";

                image.SetFont(CreateFontAwesomeIcon());
                image.SetFontColor(WebColors.GetRGBColor(rgbColor));
            }
        }
        else
        {
            value = selectedValue;
        }

        return (value, image);
    }

    private PdfFont CreateFontAwesomeIcon()
    {
        var fontBytes = ExtractResource("JJMasterData.Pdf.Fonts.fontawesome-webfont.ttf");
        var fontProgram = FontProgramFactory.CreateFont(fontBytes, true);
        
        return PdfFontFactory.CreateFont(fontProgram);
    }
    
    private static byte[] ExtractResource(string filename)
    {
        var assembly = Assembly.GetExecutingAssembly();
        using var resFilestream = assembly.GetManifestResourceStream(filename);
        
        if (resFilestream == null)
            return null;
        
        byte[] ba = new byte[resFilestream.Length];
        // ReSharper disable once MustUseReturnValue
        resFilestream.Read(ba, 0, ba.Length);
        return ba;
    }


}
