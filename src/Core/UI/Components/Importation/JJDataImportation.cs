using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using JJMasterData.Commons.Extensions;
using JJMasterData.Commons.Localization;
using JJMasterData.Commons.Security.Cryptography.Abstractions;
using JJMasterData.Commons.Tasks;
using JJMasterData.Commons.Tasks.Progress;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.DataDictionary.Models.Actions;
using JJMasterData.Core.DataManager.Expressions;
using JJMasterData.Core.DataManager.Importation;
using JJMasterData.Core.DataManager.Models;
using JJMasterData.Core.DataManager.Services;
using JJMasterData.Core.Events.Args;
using JJMasterData.Core.Http.Abstractions;
using JJMasterData.Core.UI.Events.Args;
using JJMasterData.Core.UI.Html;
using JJMasterData.Core.UI.Routing;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace JJMasterData.Core.UI.Components;

public class JJDataImportation : ProcessComponent
{
    #region "Events"

    internal event AsyncEventHandler<FormAfterActionEventArgs> OnAfterDeleteAsync;
    internal event AsyncEventHandler<FormAfterActionEventArgs> OnAfterInsertAsync;
    internal event AsyncEventHandler<FormAfterActionEventArgs> OnAfterUpdateAsync;

    public event AsyncEventHandler<FormBeforeActionEventArgs> OnBeforeImportAsync;
    public event AsyncEventHandler<FormAfterActionEventArgs> OnAfterProcessAsync;

    #endregion

    #region "Properties"

    private JJUploadArea _uploadArea;
    private JJLinkButton _backButton;
    private JJLinkButton _helpButton;
    private JJLinkButton _logButton;
    private RouteContext _routeContext;
    private DataImportationScripts _dataImportationScripts;

    public JJLinkButton BackButton => _backButton ??= GetBackButton();

    public JJLinkButton HelpButton => _helpButton ??= GetHelpButton();

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
    private DataImportationWorkerFactory DataImportationWorkerFactory { get; }

    private RouteContext RouteContext
    {
        get
        {
            if (_routeContext != null)
                return _routeContext;

            var factory = new RouteContextFactory(CurrentContext.Request.QueryString, EncryptionService);
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
        ExpressionsService expressionsService,
        FormService formService,
        FieldsService fieldsService,
        IBackgroundTaskManager backgroundTaskManager,
        IHttpContext currentContext,
        IComponentFactory componentFactory,
        DataImportationWorkerFactory dataImportationWorkerFactory,
        IEncryptionService encryptionService,
        ILoggerFactory loggerFactory,
        IStringLocalizer<MasterDataResources> stringLocalizer)
        : base(currentContext, expressionsService, fieldsService, backgroundTaskManager,
            loggerFactory.CreateLogger<ProcessComponent>(), encryptionService, stringLocalizer)
    {
        CurrentContext = currentContext;
        DataImportationWorkerFactory = dataImportationWorkerFactory;
        FormService = formService;
        ComponentFactory = componentFactory;
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
            UploadArea.OnFileUploaded += FileUploaded;
            return await UploadArea.GetResultAsync();
        }

        string action = CurrentContext.Request.QueryString["dataImportationOperation"];

        switch (action)
        {
            case "checkProgress":
            {
                var reporterProgress = GetCurrentProgress();

                return new JsonComponentResult(reporterProgress);
            }
            case "stop":
                StopImportation();
                return new JsonComponentResult(new { IsProcessing = false });
            case "log":
                htmlBuilder = GetLogHtml();
                break;
            case "help":
                htmlBuilder = await new DataImportationHelp(this).GetHtmlHelpAsync();
                break;
            case "processPastedText":
            {
                if (!IsRunning())
                {
                    string pasteValue = CurrentContext.Request.Form["pasteValue"];
                    ImportInBackground(pasteValue);
                }

                htmlBuilder = GetLoadingHtml();
                break;
            }
            case "loading":
                htmlBuilder = GetLoadingHtml();
                break;
            default:
            {
                if (IsRunning())
                    htmlBuilder = GetLoadingHtml();
                else
                    htmlBuilder = GetUploadAreaCollapse(ProcessKey);
                break;
            }
        }

        if (ComponentContext is not ComponentContext.RenderComponent)
        {
            return new ContentComponentResult(htmlBuilder);
        }

        return new RenderedComponentResult(htmlBuilder);
    }

    private HtmlBuilder GetLogHtml()
    {
        var html = new DataImportationLog(this).GetHtmlLog()
            .AppendHiddenInput("filename")
            .AppendComponent(BackButton);

        return html;
    }

    private HtmlBuilder GetLoadingHtml()
    {
        var reporter = GetCurrentReporter();
        if (reporter == null)
            return null;

        var html = new HtmlBuilder(HtmlTag.Div)
            .WithAttribute("id", "divProcess")
            .WithAttribute("style", "text-align: center;")
            .Append(HtmlTag.Div, spin =>
            {
                spin.WithAttribute("id", "data-importation-spinner")
                    .WithAttribute("style", "position: relative; height: 80px");
            })
            .AppendDiv(div =>
            {
                div.AppendText(StringLocalizer["Waiting..."]);
                div.WithCssClass("mt-1 mb-1");
            })
            .Append(HtmlTag.Div, msg =>
            {
                msg.WithAttribute("id", "process-status")
                    .WithAttribute("style", "display:none")
                    .Append(HtmlTag.Div, status => { status.WithAttribute("id", "divStatus"); })
                    .Append(HtmlTag.Span, resume => { resume.WithAttribute("id", "process-message"); });
            })
            .Append(HtmlTag.Div, div =>
            {
                div.WithAttribute("style", "width:50%;")
                    .WithCssClass(BootstrapHelper.CenterBlock)
                    .Append(HtmlTag.Div, progress =>
                    {
                        progress.WithCssClass("progress")
                            .Append(HtmlTag.Div, bar =>
                            {
                                bar.WithCssClass("progress-bar")
                                    .WithAttribute("role", "progressbar")
                                    .WithAttribute("style", "width:0;")
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

        var btnStop = ComponentFactory.Html.LinkButton.Create();
        btnStop.Type = LinkButtonType.Button;
        btnStop.OnClientClick = DataImportationScripts.GetStopScript(StringLocalizer["Stopping Processing..."]);
        btnStop.Icon = IconType.Stop;
        btnStop.Text = StringLocalizer["Stop the importation."];
        html.AppendComponent(btnStop);

        return html;
    }

    private HtmlBuilder GetUploadAreaCollapse(string keyprocess)
    {
        var html = new HtmlBuilder(HtmlTag.Div)
            .WithNameAndId(Name)
            .AppendHiddenInput("filename")
            .Append(HtmlTag.TextArea, area =>
            {
                area.WithNameAndId("pasteValue");
                area.WithAttribute("style", "display:none");
            });


        var collapsePanel = new JJCollapsePanel(CurrentContext.Request.Form)
        {
            TitleIcon = new JJIcon(IconType.Upload),
            Title = StringLocalizer["Upload File"],
            ExpandedByDefault = ExpandedByDefault,
            HtmlBuilderContent = UploadArea.GetUploadAreaHtmlBuilder()
        };

        html.AppendComponent(collapsePanel);
        html.Append(HtmlTag.Div, row =>
        {
            row.WithCssClass("row");
            row.Append(HtmlTag.Div, col =>
            {
                col.WithCssClass("col-sm-12");
                col.AppendComponent(HelpButton);

                var pipeline = BackgroundTaskManager.GetProgress<IProgressReporter>(keyprocess);
                if (pipeline != null)
                {
                    col.AppendComponent(LogButton);
                }
            });
        });

        return html;
    }

    private void FileUploaded(object sender, FormUploadFileEventArgs e)
    {
        var sb = new StringBuilder();
        Stream stream = new MemoryStream(e.File.Bytes);
        using (var reader = new StreamReader(stream))
        {
            while (!reader.EndOfStream)
            {
                sb.AppendLine(reader.ReadLine());
            }
        }

        if (!BackgroundTaskManager.IsRunning(ProcessKey))
        {
            var worker = CreateImportationTextWorker(sb.ToString(), ';');
            BackgroundTaskManager.Run(ProcessKey, worker);
            e.SuccessMessage = StringLocalizer["File successfuly imported."];
        }
    }

    private DataImportationWorker CreateImportationTextWorker(string postedText, char separator)
    {
        var dataContext = new DataContext(CurrentContext.Request, DataContextSource.Upload, UserId);
        var dataImportationContext = new DataImportationContext(FormElement, dataContext, RelationValues, postedText, separator);
        var worker = DataImportationWorkerFactory.Create(dataImportationContext);
        worker.UserId = UserId;
        worker.ProcessOptions = ProcessOptions;
        worker.UserValues = UserValues;
        worker.FormService.OnAfterUpdateAsync += OnAfterUpdateAsync;
        worker.FormService.OnAfterInsertAsync += OnAfterInsertAsync;
        worker.FormService.OnAfterDeleteAsync += OnAfterDeleteAsync;
        worker.FormService.OnBeforeImportAsync += OnBeforeImportAsync;

        worker.OnAfterProcessAsync += OnAfterProcessAsync;

        return worker;
    }

    internal DataImportationReporter GetCurrentReporter()
    {
        var progress = BackgroundTaskManager.GetProgress<DataImportationReporter>(ProcessKey);
        if (progress != null)
            return progress;
        return new DataImportationReporter();
    }

    internal void ImportInBackground(string pasteValue)
    {
        var worker = CreateImportationTextWorker(pasteValue, '\t');
        BackgroundTaskManager.Run(ProcessKey, worker);
    }

    internal DataImportationDto GetCurrentProgress()
    {
        bool isRunning = BackgroundTaskManager.IsRunning(ProcessKey);
        var reporter = BackgroundTaskManager.GetProgress<DataImportationReporter>(ProcessKey);
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
            dto.IsProcessing = isRunning || reporter.EndDate.Equals(DateTime.MinValue);
        }
        else
        {
            dto.Message = StringLocalizer["Waiting..."];
            dto.StartDate = DateTime.Now.ToDateTimeString();
        }

        return dto;
    }

    private JJLinkButton GetBackButton()
    {
        var button = ComponentFactory.Html.LinkButton.Create();
        button.IconClass = "fa fa-arrow-left";
        button.Text = "Back";
        button.ShowAsButton = true;
        button.OnClientClick = DataImportationScripts.GetBackScript();
        return button;
    }

    private JJLinkButton GetHelpButton()
    {
        var button = ComponentFactory.Html.LinkButton.Create();
        button.IconClass = "fa fa-question-circle";
        button.Text = "Help";
        button.ShowAsButton = true;
        button.OnClientClick = DataImportationScripts.GetHelpScript();
        return button;
    }

    private JJLinkButton GetLogButton()
    {
        var button = ComponentFactory.Html.LinkButton.Create();
        button.IconClass = "fa fa-film";
        button.Text = "Last Importation";
        button.ShowAsButton = true;
        button.OnClientClick = DataImportationScripts.GetLogScript();
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