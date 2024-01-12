using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using JJMasterData.Commons.Data.Entity.Repository;
using JJMasterData.Commons.Data.Entity.Repository.Abstractions;
using JJMasterData.Commons.Extensions;
using JJMasterData.Commons.Localization;
using JJMasterData.Commons.Security.Cryptography.Abstractions;
using JJMasterData.Commons.Tasks;
using JJMasterData.Commons.Util;
using JJMasterData.Core.Configuration.Options;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.DataManager.Exportation;
using JJMasterData.Core.DataManager.Exportation.Abstractions;
using JJMasterData.Core.DataManager.Exportation.Configuration;
using JJMasterData.Core.DataManager.Expressions;
using JJMasterData.Core.DataManager.Services;
using JJMasterData.Core.Http.Abstractions;
using JJMasterData.Core.UI.Events.Args;
using JJMasterData.Core.UI.Html;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace JJMasterData.Core.UI.Components;

/// <summary>
/// Exporta dados para um arquivo
/// </summary>
public class JJDataExportation : ProcessComponent
{
    private DataExportationScripts _dataExportationScripts;


    #region "Events"

    /// <summary>
    /// Event fired when the cell is rendered.
    /// </summary>
    public event AsyncEventHandler<GridCellEventArgs> OnRenderCellAsync;
    #endregion

    #region "Properties"

    private ExportOptions _exportOptions;

    public ExportOptions ExportOptions
    {
        get => _exportOptions ??= new ExportOptions();
        internal set => _exportOptions = value;
    }
    
    public bool ShowBorder { get; set; }

    public bool ShowRowStriped { get; set; }
    internal MasterDataCoreOptions MasterDataOptions { get; }

    internal DataExportationScripts Scripts => _dataExportationScripts ??= new DataExportationScripts(this);
    internal IComponentFactory ComponentFactory { get; }
    public DataExportationWriterFactory DataExportationWriterFactory { get; }

    #endregion

    #region "Constructors"
    internal JJDataExportation(
        FormElement formElement,
        ExpressionsService expressionsService,
        FieldsService fieldsService,
        IOptions<MasterDataCoreOptions> masterDataOptions,
        IBackgroundTaskManager backgroundTaskManager, 
        IStringLocalizer<MasterDataResources> stringLocalizer,
        IComponentFactory componentFactory,
        ILoggerFactory loggerFactory,
        IHttpContext currentContext, 
        IEncryptionService encryptionService, 
        DataExportationWriterFactory dataExportationWriterFactory) : 
        base(currentContext, expressionsService, fieldsService, backgroundTaskManager, loggerFactory.CreateLogger<ProcessComponent>(),encryptionService,stringLocalizer)
    {
        DataExportationWriterFactory = dataExportationWriterFactory;
        ComponentFactory = componentFactory;
        CurrentContext = currentContext;
        MasterDataOptions = masterDataOptions.Value;
        FormElement = formElement;
    }
    #endregion
    
    protected override Task<ComponentResult> BuildResultAsync()
    {
        ComponentResult result;
        
        if (IsRunning())
            result = new ContentComponentResult(new DataExportationLog(this).GetLoadingHtml());
        else
            result = new ContentComponentResult(new DataExportationSettings(this).GetHtmlBuilder());
        
        return Task.FromResult(result);
    }

    internal static JJIcon GetFileIcon(string ext)
    {
        if (ext.EndsWith("xls"))
            return new JJIcon(IconType.FileExcelO);
        if (ext.EndsWith("pdf"))
            return new JJIcon(IconType.FilePdfO);
        return new JJIcon(IconType.FileTextO);
    }

    internal string GetDownloadUrl(string filePath)
    {
        var downloader = ComponentFactory.Downloader.Create();
        downloader.FilePath = filePath;
        return downloader.GetDownloadUrl();
    }

    private string GetFinishedMessageHtml(DataExportationReporter reporter)
    {
        if (!reporter.HasError)
        {
            string url = GetDownloadUrl(reporter.FilePath);
            var html = new HtmlBuilder(HtmlTag.Div);

            if (reporter.HasError)
            {
                var panel = ComponentFactory.Html.ValidationSummary.Create();
                panel.ShowCloseButton = false;
                panel.MessageTitle = reporter.Message;
                html.AppendComponent(panel);
            }
            else
            {
                var file = new FileInfo(reporter.FilePath);
                var icon = GetFileIcon(file.Extension);
                icon.CssClass = "fa-3x ";

                html.Append(HtmlTag.Div, div =>
                {
                    div.WithCssClass("text-center");
                    div.Append(HtmlTag.Br);
                    div.Append(HtmlTag.Span, span =>
                    {
                        span.WithCssClass("text-success");
                        span.Append(HtmlTag.Span, span =>
                        {
                            span.WithCssClass("fa fa-check fa-lg");
                            span.WithAttribute("aria-hidden", "true");
                        });

                        span.AppendText(StringLocalizer["File generated successfully!"]);
                    });
                    div.Append(HtmlTag.Br);

                    string elapsedTime = Format.FormatTimeSpan(reporter.StartDate, reporter.EndDate);

                    div.AppendText(StringLocalizer["Process performed on {0}", elapsedTime]);

                    div.Append(HtmlTag.Br);

                    div.Append(HtmlTag.I, i =>
                    {
                        i.AppendText(
                            StringLocalizer["If the download does not start automatically, click on the icon below."]);
                    });

                    div.Append(HtmlTag.Br);
                    div.Append(HtmlTag.Br);
                    div.Append(HtmlTag.Br);

                    div.Append(HtmlTag.A, a =>
                    {
                        a.WithAttribute("id", $"export_link_{Name}");
                        a.WithAttribute("href", url);
                        a.AppendComponent(icon);
                        a.Append(HtmlTag.Br);
                        a.AppendText(file.Name);
                    });
                    div.Append(HtmlTag.Br);
                    div.Append(HtmlTag.Br);
                });
            }

            var btnCancel = ComponentFactory.Html.LinkButton.Create();
            btnCancel.Text = "Close";
            btnCancel.IconClass = "fa fa-times";
            btnCancel.ShowAsButton = true;
            btnCancel.Attributes.Add(BootstrapHelper.DataDismiss, "modal");

            html.Append(HtmlTag.Hr);

            html.Append(HtmlTag.Div, div =>
            {
                div.WithCssClass("row");
                div.Append(HtmlTag.Div, div =>
                {
                    div.WithCssClass($"col-sm-12 {BootstrapHelper.TextRight}");
                    div.AppendComponent(btnCancel);
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

    private DataExportationWriterBase CreateWriter()
    {
        DataExportationWriterFactory.OnRenderCellAsync += OnRenderCellAsync;
        return DataExportationWriterFactory.GetInstance(this);
    }

    public void StartExportation(DictionaryListResult result)
    {
        var exporter = CreateWriter();

#if NETFRAMEWORK
        exporter.HttpContext = System.Web.HttpContext.Current;
#endif
        exporter.DataSource = result.Data;
        exporter.TotalOfRecords = result.TotalOfRecords;
        BackgroundTaskManager.Run(ProcessKey, exporter);
    }

    internal void ExportFileInBackground(IDictionary<string, object> filter, OrderByData orderByData)
    {
        var exporter = CreateWriter();

        exporter.CurrentFilter = filter;
        exporter.CurrentOrder = orderByData;
#if NETFRAMEWORK
        exporter.HttpContext = System.Web.HttpContext.Current;
#endif
        BackgroundTaskManager.Run(ProcessKey, exporter);
    }

    internal DataExportationProgressDto GetCurrentProgress()
    {
        bool isRunning = BackgroundTaskManager.IsRunning(ProcessKey);
        var reporter = BackgroundTaskManager.GetProgress<DataExportationReporter>(ProcessKey);
        var dto = new DataExportationProgressDto();
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
            dto.Message = StringLocalizer["Waiting..."];
            dto.StartDate = DateTime.Now.ToShortDateString();
        }

        return dto;
    }
}