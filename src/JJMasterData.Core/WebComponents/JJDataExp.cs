using JJMasterData.Commons.Extensions;
using JJMasterData.Commons.Language;
using JJMasterData.Commons.Util;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataManager.Exports;
using JJMasterData.Core.DataManager.Exports.Abstractions;
using JJMasterData.Core.DataManager.Exports.Configuration;
using JJMasterData.Core.FormEvents.Args;
using JJMasterData.Core.Html;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using JJMasterData.Commons.Cryptography;
using JJMasterData.Core.Facades;
using JJMasterData.Core.Http.Abstractions;

namespace JJMasterData.Core.WebComponents;

/// <summary>
/// Exporta dados para um arquivo
/// </summary>
public class JJDataExp : JJBaseProcess
{
    private readonly CoreServicesFacade _coreServicesFacade;

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
        set => _exportOptions = value;
    }


    /// <summary>
    /// Exibi borda na grid 
    /// (Default = false)
    /// </summary>
    public bool ShowBorder { get; set; }

    public bool ShowRowStriped { get; set; }

    public string ExportationFolderPath { get; }

    public IEnumerable<IExportationWriter> Writers { get; }

    internal JJMasterDataEncryptionService EncryptionService { get; }

    #endregion

    #region "Constructors"

    public JJDataExp(
        IHttpContext httpContext,
        RepositoryServicesFacade repositoryServicesFacade,
        CoreServicesFacade coreServicesFacade,
        IEnumerable<IExportationWriter> exportationWriters) : base(httpContext, repositoryServicesFacade, coreServicesFacade)
    {
        Writers = exportationWriters;
        EncryptionService = coreServicesFacade.EncryptionService;
        ExportationFolderPath = coreServicesFacade.Options.Value.ExportationFolderPath;
        Name = "JJDataExp1";
        _coreServicesFacade = coreServicesFacade;
    }

    public JJDataExp(
        FormElement formElement,
        IHttpContext httpContext,
        RepositoryServicesFacade repositoryServicesFacade,
        CoreServicesFacade coreServicesFacade,
        IEnumerable<IExportationWriter> exportationWriters) :
        this(httpContext, repositoryServicesFacade, coreServicesFacade, exportationWriters)
    {
        FormElement = formElement;
    }

    #endregion

    internal override HtmlBuilder RenderHtml()
    {
        return IsRunning() ? new DataExpLog(Name).GetHtmlProcess() : new DataExpSettings(this).GetHtmlElement();
    }

    internal JJIcon GetFileIcon(string ext)
    {
        if (ext.EndsWith("xls"))
            return new JJIcon(IconType.FileExcelO);
        if (ext.EndsWith("pdf"))
            return new JJIcon(IconType.FilePdfO);
        return new JJIcon(IconType.FileTextO);
    }

    internal static string GetDownloadUrl(string filePath, IHttpContext httpContext, JJMasterDataEncryptionService encryptionService)
    {
        return JJDownloadFile.GetDownloadUrl(filePath, httpContext, encryptionService);
    }

    private string GetFinishedMessageHtml(DataExpReporter reporter)
    {
        if (!reporter.HasError)
        {
            string url = GetDownloadUrl(reporter.FilePath, HttpContext, _coreServicesFacade.EncryptionService);
            var html = new HtmlBuilder(HtmlTag.Div);

            if (reporter.HasError)
            {
                var panel = new JJValidationSummary
                {
                    ShowCloseButton = false,
                    MessageTitle = reporter.Message
                };
                html.AppendElement(panel);
            }
            else
            {
                var file = new FileInfo(reporter.FilePath);
                var icon = GetFileIcon(file.Extension);
                icon.CssClass = "fa-3x ";

                html.AppendElement(HtmlTag.Div, div =>
                {
                    div.WithCssClass("text-center");
                    div.AppendElement(HtmlTag.Br);
                    div.AppendElement(HtmlTag.Span, span =>
                    {
                        span.WithCssClass("text-success");
                        span.AppendElement(HtmlTag.Span, span =>
                        {
                            span.WithCssClass("fa fa-check fa-lg");
                            span.WithAttribute("aria-hidden", "true");
                        });

                        span.AppendText(Translate.Key("File generated successfully!"));
                    });
                    div.AppendElement(HtmlTag.Br);

                    string elapsedTime = Format.FormatTimeSpan(reporter.StartDate, reporter.EndDate);

                    div.AppendText(Translate.Key("Process performed on {0}", elapsedTime));

                    div.AppendElement(HtmlTag.Br);

                    div.AppendElement(HtmlTag.I, i =>
                    {
                        i.AppendText(
                            Translate.Key("If the download does not start automatically, click on the icon below."));
                    });

                    div.AppendElement(HtmlTag.Br);
                    div.AppendElement(HtmlTag.Br);
                    div.AppendElement(HtmlTag.Br);

                    div.AppendElement(HtmlTag.A, a =>
                    {
                        a.WithAttribute("id", $"export_link_{Name}");
                        a.WithAttribute("href", url);
                        a.AppendElement(icon);
                        a.AppendElement(HtmlTag.Br);
                        a.AppendText(file.Name);
                    });
                    div.AppendElement(HtmlTag.Br);
                    div.AppendElement(HtmlTag.Br);
                });
            }

            var btnCancel = new JJLinkButton
            {
                Text = "Close",
                IconClass = "fa fa-times",
                ShowAsButton = true
            };
            btnCancel.Attributes.Add(BootstrapHelper.DataDismiss, "modal");

            html.AppendElement(HtmlTag.Hr);

            html.AppendElement(HtmlTag.Div, div =>
            {
                div.WithCssClass("row");
                div.AppendElement(HtmlTag.Div, div =>
                {
                    div.WithCssClass($"col-sm-12 {BootstrapHelper.TextRight}");
                    div.AppendElement(btnCancel);
                });
            });

            return html.ToString();
        }

        var alert = new JJAlert
        {
            Title = reporter.Message,
            Icon = IconType.Warning,
            Color = PanelColor.Danger
        };

        return alert.GetHtml();
    }

    private IExportationWriter CreateWriter()
    {
        return WriterFactory.ConfigureWriter(this, Writers);
    }

    public void DoExport(DataTable dt)
    {
        var writer = CreateWriter();

        writer.DataSource = dt;
        writer.CurrentContext = HttpContext;
        writer.AbsoluteUri = HttpContext.Request.AbsoluteUri;

        Task.Run(async () => await writer.RunWorkerAsync(CancellationToken.None));

        var download = new JJDownloadFile(HttpContext,_coreServicesFacade.EncryptionService, _coreServicesFacade.LoggerFactory)
        {
            FilePath = writer.FolderPath
        };

        download.DirectDownload();
    }

    internal void ExportFileInBackground(Hashtable filter, string order)
    {
        var writer = CreateWriter();

        writer.CurrentFilter = filter;
        writer.CurrentOrder = order;
        writer.CurrentContext = HttpContext;
        writer.AbsoluteUri = HttpContext.Request.AbsoluteUri;

        BackgroundTask.Run(ProcessKey, writer);
    }

    internal DataExpDto GetCurrentProcess()
    {
        bool isRunning = BackgroundTask.IsRunning(ProcessKey);
        var reporter = BackgroundTask.GetProgress<DataExpReporter>(ProcessKey);
        var dto = new DataExpDto();
        if (reporter != null)
        {
            dto.Message = reporter.Message;
            dto.HasError = reporter.HasError;
            dto.StartDate = reporter.StartDate.ToDateTimeString();
            dto.PercentProcess = reporter.Percentage;
            dto.IsProcessing = isRunning;

            if (!isRunning && !reporter.EndDate.Equals(DateTime.MinValue))
                dto.FinishedMessage = GetFinishedMessageHtml(reporter);
        }
        else
        {
            dto.Message = Translate.Key("Waiting...");
            dto.StartDate = DateTime.Now.ToShortDateString();
        }

        return dto;
    }
}