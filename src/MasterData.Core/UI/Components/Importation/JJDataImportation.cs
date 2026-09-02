#nullable disable warnings
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JJConsulting.FontAwesome;
using JJConsulting.Html;
using JJConsulting.Html.Bootstrap.Components;
using JJConsulting.Html.Bootstrap.Extensions;
using JJConsulting.Html.Bootstrap.Models;
using JJConsulting.Html.Extensions;
using JJConsulting.MasterData.Storage.Abstractions;
using JJMasterData.Commons.Background;
using JJMasterData.Commons.Extensions;
using JJMasterData.Commons.Security;
using JJMasterData.Commons.Storage;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.DataDictionary.Models.Actions;
using JJMasterData.Core.DataManager;
using JJMasterData.Core.DataManager.Expressions;
using JJMasterData.Core.DataManager.Importation;
using JJMasterData.Core.DataManager.Importation.Background;
using JJMasterData.Core.DataManager.Models;
using JJMasterData.Core.DataManager.Services;
using JJMasterData.Core.Events.Args;
using JJMasterData.Core.UI.Events.Args;

using JJMasterData.Core.UI.Routing;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace JJMasterData.Core.UI.Components;

public class JJDataImportation : ProcessComponent
{
    private const string ImportationFolderPath = "{app.path}/MasterDataImportFiles/";

    #region "Properties"

    private JJUploadArea _uploadArea;
    private JJLinkButton _backButton;
    private JJLinkButton _helpButton;
    private JJLinkButton _logButton;
    private JJLinkButton _closeButton;
    private RouteContext _routeContext;
    private DataImportationScripts _dataImportationScripts;

    public JJLinkButton BackButton => _backButton ??= GetBackButton();

    public JJLinkButton HelpButton => _helpButton ??= GetHelpButton();

    public JJLinkButton CloseButton => _closeButton ??= GetCloseButton();
    
    public JJLinkButton LogButton => _logButton ??= GetLogButton();

    public JJUploadArea UploadArea => _uploadArea ??= GetUploadArea();

    internal ImportAction ImportAction { get; }
    
    public bool EnableAuditLog { get; set; }

    /// <summary>
    /// Default: true (panel is open by default)
    /// </summary>
    public bool ExpandedByDefault { get; set; } = true;

    internal FormService FormService { get; }
    internal IComponentFactory ComponentFactory { get; }
    
    internal DataItemService DataItemService { get; }
    
    internal FieldValuesService FieldValuesService { get; }
    
    private ImportJobService ImportJobService { get; }

    private IFileStorage FileStorage { get; }

    private RouteContext RouteContext
    {
        get
        {
            if (_routeContext != null)
                return _routeContext;

            var factory = new RouteContextFactory(HttpContextAccessor, DataProtectionService);
            _routeContext = factory.Create();

            return _routeContext;
        }
    }

    private ComponentContext ComponentContext => RouteContext.ComponentContext;

    internal DataImportationScripts DataImportationScripts =>
        _dataImportationScripts ??= new DataImportationScripts(this);

    public Dictionary<string, object> RelationValues { get; set; }
    
    #endregion
    
    #region "Constructors"

    public JJDataImportation(
        FormElement formElement,
        IMasterDataUser masterDataUser,
        ExpressionsService expressionsService,
        FormService formService,
        FieldValuesService fieldValuesService,
        IHttpContextAccessor httpContextAccessor,
        IComponentFactory componentFactory,
        DataItemService dataItemService,
        ImportJobService importJobService,
        IFileStorage fileStorage,
        DataProtectionService dataProtectionService,
        ILoggerFactory loggerFactory,
        IStringLocalizer<MasterDataResources> stringLocalizer)
        : base(httpContextAccessor,masterDataUser, expressionsService,
            loggerFactory.CreateLogger<ProcessComponent>(), dataProtectionService, stringLocalizer)
    {
        HttpContextAccessor = httpContextAccessor;
        ImportJobService = importJobService;
        FileStorage = fileStorage;
        FormService = formService;
        FieldValuesService = fieldValuesService;
        ComponentFactory = componentFactory;
        DataItemService = dataItemService;
        FormElement = formElement;
        ImportAction = formElement.Options.GridToolbarActions.ImportAction;
        if (ImportAction is not null)
        {
            ProcessOptions = ImportAction.ProcessOptions;
        }
    }

    #endregion

    protected override async Task<ComponentResult> BuildResultAsync()
    {
        HtmlBuilder htmlBuilder;

        if (ComponentContext is ComponentContext.DataImportationFileUpload)
        {
            UploadArea.OnFileUploadedAsync += FileUploadedAsync;
            return await UploadArea.GetResultAsync();
        }

        string action = HttpContextAccessor.HttpContext!.Request.Query["dataImportationOperation"];

        switch (action)
        {
            case "checkProgress":
            {
                var reporterProgress = GetCurrentProgress();

                return new JsonComponentResult(reporterProgress);
            }
            case "stop":
                if (TryGetJobId(out var stopJobId))
                    ImportJobService.Cancel(stopJobId, UserId);
                return new JsonComponentResult(new { IsProcessing = false });
            case "log":
                htmlBuilder = GetLogHtml();
                break;
            case "help":
                htmlBuilder = await new DataImportationHelp(this).GetHtmlHelpAsync();
                break;
            case "processPastedText":
            {
                var form = await HttpContextAccessor.HttpContext!.Request.ReadFormAsync();
                var pastedFile = form.Files.GetFile("pastedFile") ??
                                 throw new InvalidOperationException("Pasted content was not provided.");
                await using var source = pastedFile.OpenReadStream();
                var jobId = await ImportInBackgroundAsync(source, pastedFile.FileName, pastedFile.ContentType, '\t', false);
                htmlBuilder = GetLoadingHtml();
                htmlBuilder.AppendHiddenInput($"{Name}-import-job-id", jobId.ToString());
                break;
            }
            case "loading":
                htmlBuilder = GetLoadingHtml();
                if (TryGetJobId(out var loadingJobId))
                    htmlBuilder.AppendHiddenInput($"{Name}-import-job-id", loadingJobId.ToString());
                break;
            default:
            {
                var currentJob = ImportJobService.GetCurrentStatus(FormElement.Name, UserId);
                if (currentJob is null)
                {
                    htmlBuilder = GetUploadAreaCollapse();
                    break;
                }

                htmlBuilder = new HtmlBuilder(HtmlTag.Div)
                    .WithId(Name)
                    .Append(GetLoadingHtml())
                    .AppendHiddenInput($"{Name}-import-job-id", currentJob.Id.ToString())
                    .AppendScript(DataImportationScripts.GetStartProgressVerificationScript());
                break;
            }
        }
        
        if (ComponentContext is not ComponentContext.RenderComponent)
        {
            return new ContentComponentResult(htmlBuilder);
        }

        return new RenderedComponentResult(htmlBuilder);
    }

    internal bool IsRunning() => ImportJobService.GetCurrentStatus(FormElement.Name, UserId) is not null;

    private HtmlBuilder GetLogHtml()
    {
        var html = new DataImportationLog(this).GetHtmlLog()
            .AppendHiddenInput("filename")
            .AppendComponent(BackButton);

        html.AppendDiv(div =>
        {
            div.WithCssClass(BootstrapHelper.PullRight);
            div.AppendComponent(CloseButton);
        });
        
        return html;
    }

    private HtmlBuilder GetLoadingHtml()
    {
        var reporter = GetCurrentReporter();
        if (reporter == null)
            return null;

        var html = new HtmlBuilder(HtmlTag.Div)
            .WithAttribute("id", "divProcess")
            .WithStyle( "text-align: center;")
            .Append(HtmlTag.Div, spin =>
            {
                spin.WithAttribute("id", "data-importation-spinner")
                    .WithStyle( "position: relative; height: 80px");
            })
            .AppendDiv(div =>
            {
                div.AppendText(StringLocalizer["Waiting..."]);
                div.WithCssClass("mt-1 mb-1");
            })
            .Append(HtmlTag.Div, msg =>
            {
                msg.WithAttribute("id", "process-status")
                    .WithStyle( "display:none")
                    .Append(HtmlTag.Div, status => status.WithAttribute("id", "divStatus"))
                    .Append(HtmlTag.Span, resume => resume.WithAttribute("id", "process-message"));
            })
            .Append(HtmlTag.Div, div =>
            {
                div.WithStyle( "width:50%;")
                    .WithCssClass(BootstrapHelper.CenterBlock)
                    .Append(HtmlTag.Div, progress =>
                    {
                        progress.WithStyle("height: 15px");
                        progress.WithCssClass("progress")
                            .Append(HtmlTag.Div, bar =>
                            {
                                bar.WithCssClass("progress-bar")
                                    .WithAttribute("role", "progressbar")
                                    .WithStyle( "width:0;")
                                    .WithAttribute("aria-valuemin", "0")
                                    .WithAttribute("aria-valuemax", "100")
                                    .AppendText("0%");
                            });
                    });
            })
            .AppendDiv(div =>
            {
                div.Append(new DataImportationLog(this).GetSummaryHtml());
                div.WithCssClass("mb-2");
            });

        var btnStop = new JJLinkButton
        {
            Type = LinkButtonType.Button,
            ShowAsButton = true,
            Visible = reporter.UserId == UserId,
            OnClientClick = DataImportationScripts.GetStopScript(StringLocalizer["Stopping Processing..."]),
            Icon = FontAwesomeIcon.Stop,
            Text = StringLocalizer["Stop the importation"]
        };
        html.AppendComponent(btnStop);

        return html;
    }

    private HtmlBuilder GetUploadAreaCollapse()
    {
        var html = new HtmlBuilder(HtmlTag.Div)
            .WithId(Name)
            .AppendHiddenInput("filename");
        
        var collapsePanel = new JJMasterDataCollapsePanel(HttpContextAccessor)
        {
            TitleIcon = new JJIcon(FontAwesomeIcon.Upload),
            Title = StringLocalizer["Upload File"],
            ExpandedByDefault = ExpandedByDefault,
            Content = UploadArea.GetUploadAreaHtmlBuilder()
        };

        html.AppendComponent(collapsePanel);
        html.Append(HtmlTag.Div, row =>
        {
            row.WithCssClass("row");
            row.Append(HtmlTag.Div, col =>
            {
                col.WithCssClass("col-sm-12");
                col.AppendComponent(HelpButton);

                if (TryGetJobId(out _))
                {
                    col.AppendComponent(LogButton);
                }

                col.AppendDiv(div =>
                {
                    div.WithCssClass(BootstrapHelper.PullRight);
                    div.AppendComponent(CloseButton);
                });
            });
        });

        return html;
    }

    private async ValueTask FileUploadedAsync(object sender, FormUploadFileEventArgs e)
    {
        await using var source = e.File.OpenReadStream();
        var jobId = await ImportInBackgroundAsync(source, e.File.FileName, e.File.ContentType, ';', true);
        e.JobId = jobId.ToString();
        e.SuccessMessage = StringLocalizer["File uploaded. Importation was queued."];
    }

    internal DataImportationReporter GetCurrentReporter()
    {
        var reporter = new DataImportationReporter(StringLocalizer) { UserId = UserId };
        if (!TryGetJobId(out var jobId))
            return reporter;
        var status = ImportJobService.GetStatus(jobId, UserId);
        var details = status?.Result ?? status?.Progress?.Details as ImportJobResult;
        if (status is null)
            return reporter;
        reporter.StartDate = status.StartedAt?.LocalDateTime ?? status.CreatedAt.LocalDateTime;
        reporter.EndDate = status.CompletedAt?.LocalDateTime ?? DateTime.MinValue;
        reporter.Message = status.State == BackgroundJobState.Cancelled
            ? StringLocalizer["Process aborted by user"]
            : status.Progress?.Message ?? status.Error ?? StringLocalizer["Waiting..."];
        reporter.HasError = status.State == BackgroundJobState.Failed;
        if (details is not null)
        {
            reporter.TotalProcessed = (int)details.TotalProcessed;
            reporter.TotalRecords = status.State is BackgroundJobState.Succeeded ? (int)details.TotalProcessed : 0;
            reporter.Insert = details.Inserted;
            reporter.Update = details.Updated;
            reporter.Delete = details.Deleted;
            reporter.Ignore = details.Ignored;
            reporter.Error = details.Errors;
            foreach (var error in details.ErrorMessages)
                reporter.AddError(error);
        }
        return reporter;
    }

    private async Task<Guid> ImportInBackgroundAsync(
        Stream source,
        string fileName,
        string? contentType,
        char separator,
        bool detectDelimiter)
    {
        var currentJob = ImportJobService.GetCurrentStatus(FormElement.Name, UserId);
        if (currentJob is not null)
            return currentJob.Id;

        var filePath = FileStoragePath.Combine(ImportationFolderPath, $"{Guid.NewGuid():N}.csv");

        await FileStorage.SaveAsync(filePath, source, cancellationToken: HttpContextAccessor.HttpContext!.RequestAborted);
        var httpContext = HttpContextAccessor.HttpContext!;
        try
        {
            return await ImportJobService.EnqueueAsync(new ImportRequest
            {
                ElementName = FormElement.Name,
                UserId = UserId,
                FilePath = filePath,
                FileName = fileName,
                ContentType = contentType,
                FormatOptions = separator == ';' && detectDelimiter
                    ? new Dictionary<string, string?>()
                    : new Dictionary<string, string?>
                    {
                        [nameof(CsvImportOptions.Delimiter)] = separator.ToString(),
                        [nameof(CsvImportOptions.DetectDelimiter)] = detectDelimiter.ToString()
                    },
                RelationValues = RelationValues?.ToDictionary(item => item.Key, item => (object?)item.Value) ??
                                 new Dictionary<string, object?>(),
                UserValues = UserValues,
                IpAddress = httpContext.Connection.RemoteIpAddress?.ToString(),
                BrowserInfo = httpContext.Request.Headers.UserAgent.ToString(),
                CommandBeforeProcess = ProcessOptions.CommandBeforeProcess,
                CommandAfterProcess = ProcessOptions.CommandAfterProcess
            }, httpContext.RequestAborted);
        }
        catch
        {
            await FileStorage.DeleteAsync(filePath, CancellationToken.None);
            throw;
        }
    }

    internal DataImportationDto GetCurrentProgress()
    {
        var reporter = GetCurrentReporter();
        ImportJobStatus? status = null;
        if (TryGetJobId(out var jobId))
            status = ImportJobService.GetStatus(jobId, UserId);
        var dto = new DataImportationDto();
        if (reporter != null)
        {
            dto.StartDate = reporter.StartDate.ToDateTimeString();
            dto.PercentProcess = reporter.Percentage;
            dto.Message = reporter.Message;
            dto.Insert = reporter.Insert;
            dto.Update = reporter.Update;
            dto.Delete = reporter.Delete;
            dto.Error = reporter.Error;
            dto.Ignore = reporter.Ignore;
            dto.IsProcessing = status?.State is BackgroundJobState.Queued or BackgroundJobState.Running;
        }
        else
        {
            dto.Message = StringLocalizer["Waiting..."];
            dto.StartDate = DateTime.Now.ToDateTimeString();
        }

        return dto;
    }

    private bool TryGetJobId(out Guid jobId)
    {
        var value = HttpContextAccessor.HttpContext?.Request.Query["jobId"].ToString();
        return Guid.TryParse(value, out jobId);
    }

    private JJLinkButton GetBackButton()
    {
        var button = new JJLinkButton
        {
            IconClass = "fa fa-arrow-left",
            Text = StringLocalizer["Back"],
            ShowAsButton = true,
            OnClientClick = DataImportationScripts.GetBackScript()
        };
        return button;
    }
    
    private JJLinkButton GetCloseButton()
    {
        var button = new JJLinkButton
        {
            Icon = FontAwesomeIcon.SolidXmark,
            Text = StringLocalizer["Close"],
            ShowAsButton = true,
            OnClientClick = DataImportationScripts.GetCloseModalScript()
        };
        return button;
    }

    private JJLinkButton GetHelpButton()
    {
        var button = new JJLinkButton
        {
            IconClass = "fa fa-question-circle",
            Text = StringLocalizer["Help"],
            ShowAsButton = true,
            OnClientClick = DataImportationScripts.GetHelpScript()
        };
        return button;
    }

    private JJLinkButton GetLogButton()
    {
        var button = new JJLinkButton
        {
            IconClass = "fa fa-film",
            Text = StringLocalizer["Last Importation"],
            ShowAsButton = true,
            OnClientClick = DataImportationScripts.GetLogScript()
        };
        return button;
    }

    private JJUploadArea GetUploadArea()
    {
        var area = ComponentFactory.UploadArea.Create();
        area.RouteContext.ComponentContext = ComponentContext.DataImportationFileUpload;
        area.Multiple = false;
        area.EnableCopyPaste = false;
        area.JsCallback = DataImportationScripts.GetUploadCallbackScript();
        area.Name += "-import";
        area.AllowedTypes = "txt,csv,log";
        area.CustomUploadAreaLabel =
            StringLocalizer["Paste Excel rows or drag and drop files of type: {0}", area.AllowedTypes];

        return area;
    }
}
