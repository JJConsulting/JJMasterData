using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Web;
using JJMasterData.Commons.DI;
using JJMasterData.Commons.Extensions;
using JJMasterData.Commons.Language;
using JJMasterData.Commons.Util;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataManager.Exports;
using JJMasterData.Core.DataManager.Exports.Abstractions;
using JJMasterData.Core.DataManager.Exports.Configuration;
using JJMasterData.Core.FormEvents.Args;
using JJMasterData.Core.Html;

namespace JJMasterData.Core.WebComponents;

/// <summary>
/// Exporta dados para um arquivo
/// </summary>
public class JJDataExp : JJBaseProcess
{
    #region "Events"

    /// <summary>
    /// Event fired when the cell is rendered.
    /// </summary>
    public EventHandler<GridCellEventArgs> OnRenderCell = null;

    #endregion

    #region "Properties"

    private ExportOptions _exportOptions;

    /// <summary>
    /// Recupera as configurações de exportação 
    /// </summary>
    public ExportOptions ExportOptions
    {
        get
        {
            if (_exportOptions == null)
                _exportOptions = new ExportOptions();
            return _exportOptions;
        }
        set { _exportOptions = value; }
    }


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

    #endregion

    #region "Constructors"

    public JJDataExp()
    {
        Name = "JJDataExp1";
    }

    public JJDataExp(FormElement formElement) : this()
    {
        FormElement = formElement;
    }

    #endregion

    internal override HtmlElement GetHtmlElement()
    {
        //TODO: GetHtmlExport
        return IsRunning() ? GetRunningProcessHtmlElement() : new HtmlElement(GetHtmlExport());
    }

    private HtmlElement GetRunningProcessHtmlElement()
    {
        var div = new HtmlElement(HtmlTag.Div);

        div.AppendElement(GetLoading());

        div.AppendElement(GetProgressData());

        div.AppendElement(HtmlTag.Br);

        div.AppendText(Translate.Key("Exportation started on"));

        div.AppendElement(HtmlTag.Span, span =>
        {
            span.WithAttribute("id", "lblStartDate");
        });
        
        div.AppendElement(HtmlTag.Br);
        div.AppendElement(HtmlTag.Br);
        div.AppendElement(HtmlTag.Br);

        div.AppendElement(HtmlTag.A, a =>
        {
            a.WithAttribute("href",
                $"javascript:JJDataExp.stopProcess('{Name}','{Translate.Key("Stopping Processing...")}')");
            a.AppendElement(HtmlTag.Span, span =>
            {
                a.WithCssClass("fa fa-stop");
                a.AppendText("&nbsp;" + Translate.Key("Stop the exportation."));
            });
        });

        div.AppendScript($"JJDataExp.startProcess('{Name}')");
        
        return div;
    }

    private static HtmlElement GetLoading()
    {
        return new HtmlElement(HtmlTag.Div)
            .WithAttribute("id", "divProcess")
            .WithAttribute("style", "text-align:center;")
            .AppendHiddenInput("current_uploadaction", string.Empty)
            .AppendElement(HtmlTag.Div, div =>
            {
                div.WithAttribute("id", "impSpin");
                div.WithAttribute("style", "position: relative; height: 80px");
            });
    }

    private HtmlElement GetProgressData()
    {
        return new HtmlElement(HtmlTag.Div)
            .WithAttribute("id", "divMsgProcess")
            .WithCssClass("text-center")
            .WithAttribute("style", "display:none;")
            .AppendElement(HtmlTag.Div, div =>
            {
                div.WithAttribute("id", "divStatus");
                div.WithCssClass("text-center");
                div.WithAttribute("style", "display:none");
                div.AppendElement(HtmlTag.Span, span => { span.WithAttribute("id", "lblResumeLog"); });
            })
            .AppendElement(HtmlTag.Div, div =>
            {
                div.WithAttribute("style", "width:50%");
                div.WithCssClass(BootstrapHelper.CenterBlock);
                div.AppendElement(HtmlTag.Div, div =>
                {
                    div.WithCssClass("progress");
                    div.AppendElement(HtmlTag.Div, div =>
                    {
                        div.WithCssClass("progress-bar");
                        div.WithAttribute("role", "progressbar");
                        div.WithAttribute("style", "width: 0;");
                        div.WithAttribute("aria-valuemin", "0");
                        div.WithAttribute("aria-valuemax", "100");
                        div.AppendText("0%");
                    });
                });
            });
    }


    private HtmlElement GetSettingsHtmlElement()
    {
        return null;
    }

    internal string GetHtmlWaitProcess()
    {
        var html = new StringBuilder();
        html.AppendLine("<div id='divProcess' style='text-align: center;'>");
        html.AppendLine("    <input type='hidden' id='current_uploadaction' name='current_uploadaction' value='' />");
        html.AppendLine("    <div id='impSpin' style='position: relative; height: 80px'></div>");
        html.Append("    &nbsp;&nbsp;&nbsp;");
        html.AppendLine("</div>");

        html.AppendLine("    <div id='divMsgProcess' class='text-center' style='display:none'>");
        html.Append(' ', 8);

        html.AppendLine("<div id='divStatus'>");
        html.Append(' ', 12);
        html.AppendLine("<span id='lblResumeLog'></span>");
        html.Append(' ', 8);
        html.AppendLine("</div>");

        html.AppendLine($"        <div style='width: 50%;' class='{BootstrapHelper.CenterBlock}'>");
        html.AppendLine("            <div class='progress'>");
        html.AppendLine(
            "	            <div class='progress-bar' role='progressbar' style='width: 0;' aria-valuemin='0' aria-valuemax='100'>0%</div>");
        html.AppendLine("            </div>");
        html.AppendLine("        </div>");
        html.AppendLine("        <div>");

        html.Append(' ', 8);
        html.AppendLine("<br>");
        html.Append(Translate.Key("Exportation started on"));
        html.Append(" <span id='lblStartDate'>{0}</span></div>");
        html.AppendLine("");

        html.Append(' ', 8);
        html.AppendLine("<br>");
        html.AppendLine("<br>");
        html.AppendLine("<br>");

        html.Append(' ', 8);
        html.AppendFormat("<a href='javascript:JJDataExp.stopProcess(\"{0}\",\"{1}\");'>",
            Name, Translate.Key("Stopping Processing..."));
        html.Append("<span class='fa fa-stop'></span>&nbsp;");
        html.Append(Translate.Key("Stop the exportation."));
        html.AppendLine("</a>");

        html.Append(' ', 4);
        html.AppendLine("</div>");
        html.AppendLine("</div>");
        html.AppendLine("<script>");
        html.AppendLine($"JJDataExp.startProcess('{Name}')");
        html.AppendLine("</script>");

        return html.ToString();
    }

    private string GetHtmlExport()
    {
        var html = new StringBuilder();

        var btnOk = new JJLinkButton
        {
            Text = Translate.Key("Export"),
            IconClass = "fa fa-check",
            ShowAsButton = true,
            OnClientClick = $"JJDataExp.doExport('{Name}');"
        };

        var btnCancel = new JJLinkButton
        {
            Text = "Cancel",
            IconClass = "fa fa-times",
            ShowAsButton = true
        };
        btnCancel.Attributes.Add(BootstrapHelper.DataDismiss, "modal");

        html.AppendLine(GetHtmlFormExport());
        html.AppendLine("<hr/>");
        html.AppendLine("<div class=\"row\">");
        html.AppendLine($"<div class=\"col-sm-12 {BootstrapHelper.TextRight}\">");
        html.AppendLine(btnOk.GetHtml());
        html.AppendLine(btnCancel.GetHtml());
        html.AppendLine("</div>");
        html.AppendLine("</div>");
        return html.ToString();
    }

    private string GetHtmlFormExport()
    {
        string colSm = BootstrapHelper.Version > 3 ? "col-sm-2" : "col-sm-4";
        string bs4Row = BootstrapHelper.Version > 3 ? "row" : string.Empty;
        string label = BootstrapHelper.Version > 3 ? BootstrapHelper.Label + "  form-label" : string.Empty;
        string objname = Name;
        var html = new StringBuilder();
        char TAB = '\t';
        html.Append(TAB, 5);
        html.AppendLine($"<div class=\"{BootstrapHelper.FormHorizontal}\" role=\"form\">");
        html.Append(TAB, 6);
        html.AppendLine($"<div class=\"{BootstrapHelper.FormGroup} {bs4Row}\">");
        html.Append(TAB, 7);
        html.Append($"<label for=\"{objname}{ExportOptions.FileName}\" class=\" {label} col-sm-4\">");
        html.Append(Translate.Key("Export to"));
        html.AppendLine("</label>");
        html.Append(TAB, 7);
        html.AppendLine($"<div class=\"{colSm}\">");
        html.Append(TAB, 8);
        html.AppendLine(
            $"<select class=\"form-control form-select\" id=\"{objname}{ExportOptions.FileName}\" name=\"{objname}{ExportOptions.FileName}\" onchange=\"jjview.showExportOptions('{objname}',this.value);\">");
        html.Append(TAB, 9);
        html.AppendLine("<option selected value=\"" + (int)ExportFileExtension.XLS + "\">Excel</option>");
        html.Append(TAB, 9);
        if (PdfWriterExists())
        {
            html.AppendLine("<option value=\"" + (int)ExportFileExtension.PDF + "\">PDF</option>");
            html.Append(TAB, 9);
        }

        html.AppendLine("<option value=\"" + (int)ExportFileExtension.CSV + "\">CSV</option>");
        html.Append(TAB, 9);
        html.AppendLine("<option value=\"" + (int)ExportFileExtension.TXT + "\">TXT</option>");
        html.Append(TAB, 8);
        html.AppendLine("</select>");
        html.Append(TAB, 7);
        html.AppendLine("</div>");
        html.Append(TAB, 7);
        html.AppendLine("<div class=\"col-sm-4\"></div>");
        html.Append(TAB, 6);
        html.AppendLine("</div>");
        html.Append(TAB, 6);
        html.AppendLine(
            $"<div class=\"{BootstrapHelper.FormGroup} {bs4Row}\" id=\"{objname}_div_export_orientation\" style=\"display:none;\">");
        html.Append(TAB, 7);
        html.Append($"<label for=\"{objname}{ExportOptions.TableOrientation}\" class=\"{label} col-sm-4\">");
        html.Append(Translate.Key("Orientation"));
        html.AppendLine("</label>");
        html.Append(TAB, 7);
        html.AppendLine($"<div class=\"{colSm}\">");
        html.Append(TAB, 8);
        html.AppendLine(
            $"<select class=\"form-control form-select\" id=\"{objname}{ExportOptions.TableOrientation}\" name=\"{objname}{ExportOptions.TableOrientation}\">");
        html.Append(TAB, 9);
        html.Append("<option selected value=\"1\">");
        html.Append(Translate.Key("Landscape"));
        html.AppendLine("</option>");
        html.Append(TAB, 9);
        html.Append("<option value=\"0\">");
        html.Append(Translate.Key("Portrait"));
        html.AppendLine("</option>");
        html.Append(TAB, 8);
        html.AppendLine("</select>");
        html.Append(TAB, 7);
        html.AppendLine("</div>");
        html.Append(TAB, 7);
        html.AppendLine("<div class=\"col-sm-4\"></div>");
        html.Append(TAB, 6);
        html.AppendLine("</div>");
        html.Append(TAB, 6);
        html.AppendLine($"<div class=\"{BootstrapHelper.FormGroup} {bs4Row}\" id=\"{objname}_div_export_all\">");
        html.Append(TAB, 7);
        html.Append($"<label for=\"{objname}{ExportOptions.ExportAll}\" class=\"{label} col-sm-4\">");
        html.Append(Translate.Key("Fields"));
        html.AppendLine(" </label>");
        html.Append(TAB, 7);
        html.AppendLine($"<div class=\"{colSm}\">");
        html.Append(TAB, 8);
        html.AppendLine(
            $"<select class=\"form-control form-select\" id=\"{objname}{ExportOptions.ExportAll}\" name=\"{objname}{ExportOptions.ExportAll}\">");
        html.Append(TAB, 9);
        html.Append("<option selected value=\"1\">");
        html.Append(Translate.Key("All"));
        html.AppendLine("</option>");
        html.Append(TAB, 9);
        html.Append("<option value=\"2\">");
        html.Append(Translate.Key("Only the fields visible on the screen"));
        html.AppendLine("</option>");
        html.Append(TAB, 8);
        html.AppendLine("</select>");
        html.Append(TAB, 7);
        html.AppendLine("</div>");
        html.Append(TAB, 7);
        html.AppendLine("<div class=\"col-sm-4\"></div>");
        html.Append(TAB, 6);
        html.AppendLine("</div>");
        html.Append(TAB, 6);
        html.AppendLine(
            $"<div class=\"{BootstrapHelper.FormGroup} {bs4Row}\" id=\"{objname}_div_export_delimiter\" style=\"display:none;\">");
        html.Append(TAB, 7);
        html.Append($"<label for=\"{objname}{ExportOptions.ExportDelimiter}\" class=\"{label} col-sm-4\">");
        html.Append(Translate.Key("Delimiter"));
        html.AppendLine("</label>");
        html.Append(TAB, 7);
        html.AppendLine("<div class=\"col-sm-4\">");
        html.Append(TAB, 8);
        html.AppendLine(
            $"<select class=\"form-control form-select\" id=\"{objname}{ExportOptions.ExportDelimiter}\" name=\"{objname}{ExportOptions.ExportDelimiter}\">");
        html.Append(TAB, 9);
        html.Append("<option selected value=\";\">");
        html.Append(Translate.Key("Semicolon (;)"));
        html.AppendLine("</option>");
        html.Append(TAB, 9);
        html.Append("<option value=\",\">");
        html.Append(Translate.Key("Comma (,)"));
        html.AppendLine("</option>");
        html.Append(TAB, 9);
        html.Append("<option value=\"|\">");
        html.Append(Translate.Key("Pipe (|)"));
        html.AppendLine("</option>");
        html.Append(TAB, 8);
        html.AppendLine("</select>");
        html.Append(TAB, 7);
        html.AppendLine("</div>");
        html.Append(TAB, 7);
        html.AppendLine("<div class=\"col-sm-4\"></div>");
        html.Append(TAB, 6);
        html.AppendLine("</div>");
        html.Append(TAB, 6);
        html.AppendLine($"<div class=\"{BootstrapHelper.FormGroup} {bs4Row}\" id=\"{objname}_div_export_fistline\">");
        html.Append(TAB, 7);
        html.Append($"<label for=\"{objname}{ExportOptions.ExportFirstLine}\" class=\"{label} col-sm-4\">");
        html.Append(Translate.Key("Export first line as title"));
        html.AppendLine("</label>");
        html.Append(TAB, 7);
        html.AppendLine("<div class=\"col-sm-8\">");
        html.Append(TAB, 8);
        html.Append("<input type=\"checkbox\" ");
        html.Append("value =\"1\" ");
        html.Append("class=\"form-control\" ");
        html.Append($"id=\"{objname}{ExportOptions.ExportFirstLine}\" ");
        html.Append($"name=\"{objname}{ExportOptions.ExportFirstLine}\" ");
        html.Append("data-toggle=\"toggle\" ");
        html.AppendFormat("data-on=\"{0}\" ", Translate.Key("Yes"));
        html.AppendFormat("data-off=\"{0}\"", Translate.Key("No"));

        if (ExportOptions.ExportFirstLine)
            html.Append(" checked=\"checked\"");

        html.AppendLine("> ");
        html.Append(TAB, 7);
        html.AppendLine("</div>");
        html.Append(TAB, 6);
        html.AppendLine("</div>");

        html.Append(TAB, 6);
        html.Append(GetFilesPanelHtml());

        html.Append(TAB, 6);
        html.AppendLine("<div class=\"row\"> ");
        html.Append(TAB, 7);
        html.AppendLine($"<label class=\"small {label} col-sm-12\">");
        html.Append(TAB, 8);
        html.AppendLine("<br><span class=\"text-info fa fa-info-circle\"></span>");
        html.Append(TAB, 8);
        html.AppendLine(Translate.Key("Filters performed in the previous screen will be considered in the export"));
        html.Append(TAB, 7);
        html.AppendLine("</label>");
        html.Append(TAB, 6);
        html.AppendLine("</div>");


        var alert = new JJAlert();
        alert.ShowIcon = true;
        alert.Name = $"warning_exp_{objname}";
        alert.ShowCloseButton = true;
        alert.Title = "Warning!";
        alert.Messages.Add(Translate.Key(
            "You are trying to export more than 50,000 records, this can cause system overhead and slowdowns."));
        alert.Messages.Add(Translate.Key(
            "Use filters to reduce export volume, if you need to perform this operation frequently, contact your system administrator."));
        alert.Icon = IconType.ExclamationTriangle;
        alert.Color = PanelColor.Warning;
        alert.SetAttr("style", "display:none;");

        html.AppendLine(alert.GetHtml());

        return html.ToString();
    }

    private string GetFilesPanelHtml()
    {
        var files = GetGeneratedFiles();
        var panel = new JJCollapsePanel
        {
            Name = "exportCollapse",
            ExpandedByDefault = false,
            TitleIcon = new JJIcon(IconType.FolderOpenO),
            Title = Translate.Key("Recently generated files") + $" ({files.Count})",
            HtmlContent = GetLastFilesHtml(files)
        };

        return panel.GetHtml();
    }

    private string GetLastFilesHtml(List<FileInfo> files)
    {
        if (files == null || files.Count == 0)
            return Translate.Key("No recently generated files.");

        var html = new StringBuilder();
        foreach (FileInfo file in files)
        {
            if (FileIO.IsFileLocked(file))
                continue;

            var icon = GetFileIcon(file.Extension);
            string url = GetDownloadUrl(file.FullName);
            html.Append("<div class=\"mb-1\">");
            html.Append(icon.GetHtml());
            html.Append("&nbsp;");
            html.Append($"<a href=\"{url}\" title=\"Download\">{file.Name}</a>");
            html.AppendLine("</div>");
        }

        return html.ToString();
    }

    private JJIcon GetFileIcon(string ext)
    {
        if (ext.EndsWith("xls"))
            return new JJIcon(IconType.FileExcelO);
        if (ext.EndsWith("pdf"))
            return new JJIcon(IconType.FilePdfO);
        return new JJIcon(IconType.FileTextO);
    }

    private List<FileInfo> GetGeneratedFiles()
    {
        var list = new List<FileInfo>();

        var oDir = new DirectoryInfo(JJService.Settings.ExportationFolderPath);

        if (oDir.Exists)
            list.AddRange(oDir.GetFiles("*", SearchOption.AllDirectories));

        return list.OrderByDescending(f => f.CreationTime).ToList();
    }

    private string GetDownloadUrl(string filePath)
    {
        var uriBuilder = new UriBuilder(CurrentContext.Request.AbsoluteUri);
        var query = HttpUtility.ParseQueryString(uriBuilder.Query);
        query[JJDownloadFile.PARAM_DOWNLOAD] = Cript.Cript64(filePath);
        uriBuilder.Query = query.ToString() ?? string.Empty;

        return uriBuilder.ToString();
    }

    private string GetFinishedMessage(DataExpReporter reporter)
    {
        string url = GetDownloadUrl(reporter.FilePath);
        var html = new StringBuilder();

        if (reporter.HasError)
        {
            var panel = new JJValidationSummary
            {
                ShowCloseButton = false,
                MessageTitle = reporter.Message
            };
            html.Append(panel.GetHtml());
        }
        else
        {
            var file = new FileInfo(reporter.FilePath);
            var icon = GetFileIcon(file.Extension);
            icon.CssClass = "fa-3x ";

            html.Append("<div class=\"text-center\">");
            html.Append("<br>");
            html.Append("<span class=\"text-success\">");
            html.Append("<span class=\"fa fa-check fa-lg\" aria-hidden=\"true\"></span>");
            html.Append(Translate.Key("File generated successfully!"));
            html.Append("</span>");
            html.Append("<br>");

            string elapsedTime = Format.FormatTimeSpan(reporter.StartDate, reporter.EndDate);
            html.Append(Translate.Key("Process performed on {0}", elapsedTime));

            html.Append("<br>");
            html.Append("<i>");
            html.Append(Translate.Key("If the download does not start automatically, click on the icon below."));
            html.Append("</i>");
            html.Append("<br>");
            html.Append("<br>");
            html.Append("<br>");
            html.Append($"<a id=\"export_link_{Name}\" href=\"{url}\">");
            html.Append(icon.GetHtml());
            html.Append("<br>");
            html.Append(file.Name);
            html.Append("</a>");
            html.Append("<br>");
            html.Append("<br>");
            html.Append("</div>");
        }

        var btnCancel = new JJLinkButton
        {
            Text = "Close",
            IconClass = "fa fa-times",
            ShowAsButton = true
        };
        btnCancel.Attributes.Add(BootstrapHelper.DataDismiss, "modal");

        html.AppendLine("</hr>");
        html.AppendLine("<div class=\"row\">");
        html.AppendLine($"<div class=\"col-sm-12 {BootstrapHelper.TextRight}\">");
        html.AppendLine(btnCancel.GetHtml());
        html.AppendLine("</div>");
        html.AppendLine("</div>");

        return html.ToString();
    }

    private BaseWriter CreateWriter()
    {
        return WriterFactory.GetInstance(this);
    }

    /// <summary>
    /// Exporta o arquivo com base nas parametrizações do usuário
    /// </summary>
    public void DoExport(DataTable dt)
    {
        var exporter = CreateWriter();

        exporter.DataSource = dt;
        exporter.CurrentContext = HttpContext.Current;

        exporter.RunWorkerAsync(CancellationToken.None).Wait();

        var download = new JJDownloadFile
        {
            FilePath = exporter.FolderPath
        };

        download.ResponseDirectDownload();
    }

    internal void ExportFileInBackground(Hashtable filter, string order)
    {
        var exporter = CreateWriter();

        exporter.CurrentFilter = filter;
        exporter.CurrentOrder = order;
        exporter.CurrentContext = HttpContext.Current;

        BackgroundTask.Run(ProcessKey, exporter);
    }

    internal DataExpDTO GetCurrentProcess()
    {
        bool isRunning = BackgroundTask.IsRunning(ProcessKey);
        var reporter = BackgroundTask.GetProgress<DataExpReporter>(ProcessKey);
        var dto = new DataExpDTO();
        if (reporter != null)
        {
            dto.Message = reporter.Message;
            dto.HasError = reporter.HasError;
            dto.StartDate = reporter.StartDate.ToDateTimeString();
            dto.PercentProcess = reporter.Percentage;
            dto.IsProcessing = isRunning;

            if (!isRunning && !reporter.EndDate.Equals(DateTime.MinValue))
                dto.FinishedMessage = GetFinishedMessage(reporter);
        }
        else
        {
            dto.Message = Translate.Key("Waiting...");
            dto.StartDate = DateTime.Now.ToShortDateString();
        }

        return dto;
    }

    private bool PdfWriterExists() => WriterFactory.GetPdfWriter() != null;
}