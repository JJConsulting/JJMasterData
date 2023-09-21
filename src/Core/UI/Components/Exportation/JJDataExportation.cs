using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using JJMasterData.Commons.Cryptography;
using JJMasterData.Commons.Data.Entity;
using JJMasterData.Commons.Data.Entity.Abstractions;
using JJMasterData.Commons.Data.Entity.Repository;
using JJMasterData.Commons.Extensions;
using JJMasterData.Commons.Localization;
using JJMasterData.Commons.Tasks;
using JJMasterData.Commons.Util;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataManager.Exports;
using JJMasterData.Core.DataManager.Exports.Abstractions;
using JJMasterData.Core.DataManager.Exports.Configuration;
using JJMasterData.Core.DataManager.Expressions.Abstractions;
using JJMasterData.Core.DataManager.Services.Abstractions;
using JJMasterData.Core.FormEvents.Args;
using JJMasterData.Core.Options;
using JJMasterData.Core.UI.Components;
using JJMasterData.Core.Web.Components.Scripts;
using JJMasterData.Core.Web.Factories;
using JJMasterData.Core.Web.Html;
using JJMasterData.Core.Web.Http.Abstractions;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace JJMasterData.Core.Web.Components;

/// <summary>
/// Exporta dados para um arquivo
/// </summary>
public class JJDataExportation : ProcessComponent
{
    private readonly JJMasterDataUrlHelper _urlHelper;
    private readonly IEncryptionService _encryptionService;
    private DataExportationScripts _dataExportationScripts;


    #region "Events"

    /// <summary>
    /// Event fired when the cell is rendered.
    /// </summary>
    public event EventHandler<GridCellEventArgs> OnRenderCell;
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
    internal JJMasterDataCoreOptions MasterDataOptions { get; }

    internal DataExportationScripts Scripts => _dataExportationScripts ??= new DataExportationScripts(this);
    internal IComponentFactory ComponentFactory { get; }
    public DataExportationWriterFactory DataExportationWriterFactory { get; }

    #endregion

    #region "Constructors"
    internal JJDataExportation(
        FormElement formElement,
        IEntityRepository entityRepository,
        IExpressionsService expressionsService,
        IFieldsService fieldsService,
        IOptions<JJMasterDataCoreOptions> masterDataOptions,
        IBackgroundTaskManager backgroundTaskManager, 
        IStringLocalizer<JJMasterDataResources> stringLocalizer,
        IComponentFactory componentFactory,
        ILoggerFactory loggerFactory,
        IHttpContext currentContext,
        JJMasterDataUrlHelper urlHelper, 
        IEncryptionService encryptionService, 
        DataExportationWriterFactory dataExportationWriterFactory) : 
        base(currentContext,entityRepository, expressionsService, fieldsService, backgroundTaskManager, loggerFactory.CreateLogger<ProcessComponent>(),encryptionService,stringLocalizer)
    {
        _urlHelper = urlHelper;
        _encryptionService = encryptionService;
        DataExportationWriterFactory = dataExportationWriterFactory;
        ComponentFactory = componentFactory;
        CurrentContext = currentContext;
        MasterDataOptions = masterDataOptions.Value;
        FormElement = formElement;
    }
    #endregion
    
    protected override async Task<ComponentResult> BuildResultAsync()
    {
        ComponentResult result;
        
        if (IsRunning())
            result = new ContentComponentResult(new DataExportationLog(this).GetHtmlProcess());
        else
            result = new ContentComponentResult(new DataExportationSettings(this).GetHtmlBuilder());
        
        return await Task.FromResult(result);
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
        return downloader.GetDownloadUrl(filePath);
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
        DataExportationWriterFactory.OnRenderCell += OnRenderCell;
        DataExportationWriterFactory.OnRenderCellAsync += OnRenderCellAsync;
        return DataExportationWriterFactory.GetInstance(this);
    }

    public void StartExportation(DictionaryListResult result)
    {
        var exporter = CreateWriter();

        exporter.DataSource = result.Data;
        exporter.TotalOfRecords = result.TotalOfRecords;
        BackgroundTaskManager.Run(ProcessKey, exporter);
    }

    internal void ExportFileInBackground(IDictionary<string, object> filter, OrderByData orderByData)
    {
        var exporter = CreateWriter();

        exporter.CurrentFilter = filter;
        exporter.CurrentOrder = orderByData;

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