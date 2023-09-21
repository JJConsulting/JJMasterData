using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using JJMasterData.Commons.Cryptography;
using JJMasterData.Commons.Data.Entity.Abstractions;
using JJMasterData.Commons.Extensions;
using JJMasterData.Commons.Localization;
using JJMasterData.Commons.Tasks;
using JJMasterData.Commons.Tasks.Progress;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataManager;
using JJMasterData.Core.DataManager.Imports;
using JJMasterData.Core.DataManager.Services;
using JJMasterData.Core.FormEvents.Args;
using JJMasterData.Core.UI.Components;
using JJMasterData.Core.UI.Components.Importation;
using JJMasterData.Core.Web.Factories;
using JJMasterData.Core.Web.Html;
using JJMasterData.Core.Web.Http.Abstractions;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace JJMasterData.Core.Web.Components;

public class JJDataImportation : ProcessComponent
{
    #region "Events"

    internal event EventHandler<FormAfterActionEventArgs> OnAfterDelete;
    internal event EventHandler<FormAfterActionEventArgs> OnAfterInsert;
    internal event EventHandler<FormAfterActionEventArgs> OnAfterUpdate;

    public event EventHandler<FormBeforeActionEventArgs> OnBeforeImport;
    public event EventHandler<FormAfterActionEventArgs> OnAfterProcess;

    #endregion

    #region "Properties"

    private JJUploadArea _upload;
    private JJLinkButton _backButton;
    private JJLinkButton _helpButton;
    private JJLinkButton _logButton;
    private RouteContext _routeContext;
    private DataImportationScripts _dataImportationScripts;

    public JJLinkButton BackButton => _backButton ??= GetBackButton();

    public JJLinkButton HelpButton => _helpButton ??= GetHelpButton();

    public JJLinkButton LogButton => _logButton ??= GetLogButton();

    public JJUploadArea UploadArea => _upload ??= GetUploadArea();
    
    public bool EnableAuditLog { get; set; }

    /// <summary>
    /// Default: true (panel is open by default)
    /// </summary>
    public bool ExpandedByDefault { get; set; } = true;
    
    internal FormService FormService { get; }
    internal IComponentFactory ComponentFactory { get; }
    private DataImportationWorkerFactory DataImportationWorkerFactory { get; }

    internal RouteContext RouteContext
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
    
    internal ComponentContext ComponentContext => RouteContext.ComponentContext;
    internal DataImportationScripts DataImportationScripts => _dataImportationScripts ??= new DataImportationScripts(this);

    #endregion

    #region "Constructors"
    public JJDataImportation(
        FormElement formElement,
        IEntityRepository entityRepository,
        ExpressionsService expressionsService,
        FormService formService,
        FieldsService fieldsService,
        IBackgroundTaskManager backgroundTaskManager,
        IHttpContext currentContext,
        IComponentFactory componentFactory,
        DataImportationWorkerFactory dataImportationWorkerFactory,
        IEncryptionService encryptionService,
        ILoggerFactory loggerFactory,
        IStringLocalizer<JJMasterDataResources> stringLocalizer) 
        : base(currentContext,entityRepository, expressionsService, fieldsService, backgroundTaskManager, loggerFactory.CreateLogger<ProcessComponent>(),encryptionService, stringLocalizer)
    {
        CurrentContext = currentContext;
        DataImportationWorkerFactory = dataImportationWorkerFactory;
        FormService = formService;
        ComponentFactory = componentFactory;
        FormElement = formElement;
        var importAction = formElement.Options.GridToolbarActions.ImportAction;
        if (importAction is not null)
        {
            ProcessOptions = importAction.ProcessOptions;
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
                StopExportation();
                return new JsonComponentResult(new {isProcessing = false});
            case "log":
                htmlBuilder = GetHtmlLogProcess();
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
                htmlBuilder = GetHtmlWaitProcess();
                break;
            }
            default:
            {
                if (IsRunning())
                    htmlBuilder = GetHtmlWaitProcess();
                else
                    htmlBuilder = GetUploadAreaCollapse(ProcessKey);
                break;
            }
        }

        return new ContentComponentResult(htmlBuilder);
    }

    private HtmlBuilder GetHtmlLogProcess()
    {
        var html = new DataImportationLog(this).GetHtmlLog()
         .AppendHiddenInput("filename")
         .AppendComponent(BackButton);

        return html;
    }

    private HtmlBuilder GetHtmlWaitProcess()
    {
        var reporter = GetCurrentReporter();
        if (reporter == null)
            return null;

        var html = new HtmlBuilder(HtmlTag.Div)
            .WithAttribute("id", "divProcess")
            .WithAttribute("style", "text-align: center;")
            .Append(HtmlTag.Div, spin =>
            {
                spin.WithAttribute("id", "impSpin")
                    .WithAttribute("style", "position: relative; height: 80px");
            })
            .AppendText("&nbsp;&nbsp;&nbsp;")
            .AppendText(StringLocalizer["Waiting..."])
            .Append(HtmlTag.Br).Append(HtmlTag.Br)
            .Append(HtmlTag.Div, msg =>
            {
                msg.WithAttribute("id", "process-status")
                   .WithAttribute("style", "display:none")
                   .Append(HtmlTag.Div, status =>
                   {
                       status.WithAttribute("id", "divStatus");
                   })
                   .Append(HtmlTag.Span, resume =>
                   {
                       resume.WithAttribute("id", "process-message");
                   });
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
            .Append(new DataImportationLog(this).GetHtmlResume())
            .Append(HtmlTag.Br).Append(HtmlTag.Br);

        var btnStop = ComponentFactory.Html.LinkButton.Create();
        btnStop.OnClientClick = DataImportationScripts.GetStopScript(StringLocalizer["Stopping Processing..."]);
        btnStop.IconClass = IconType.Stop.GetCssClass();
        btnStop.Text = StringLocalizer["Stop the import."];
        html.AppendComponent(btnStop);

        return html;
    }

    private HtmlBuilder GetUploadAreaCollapse(string keyprocess)
    {
        var html = new HtmlBuilder(HtmlTag.Div)
            .WithNameAndId(Name)
            .AppendScript("DataImportationHelper.addPasteListener();")
            .AppendHiddenInput("filename")
            .Append(HtmlTag.TextArea, area =>
            {
                area.WithNameAndId("pasteValue");
                area.WithAttribute("style", "display:none");
            });
            

        var collapsePanel = new JJCollapsePanel(CurrentContext.Request.Form)
        {
            TitleIcon = new JJIcon(IconType.FolderOpenO),
            Title = "Import File",
            ExpandedByDefault = ExpandedByDefault,
            HtmlBuilderContent = new HtmlBuilder(HtmlTag.Div)
                .Append(HtmlTag.Label, label =>
                {
                    label.AppendText(StringLocalizer["Paste Excel rows or drag and drop files of type: {0}", UploadArea.AllowedTypes]);
                })
                .Append( UploadArea.GetUploadAreaHtmlBuilder())
        };

        html.AppendComponent(collapsePanel);
        html.Append(HtmlTag.Div, row =>
        {
            row.WithCssClass("row");
            row.Append(HtmlTag.Div, col =>
            {
                col.WithCssClass("col-sm-12");
                col.AppendComponent(BackButton);
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
        var dataContext = new DataContext(CurrentContext.Request,DataContextSource.Upload, UserId);
        
        var worker = DataImportationWorkerFactory.Create(new DataImportationContext(FormElement, dataContext, postedText, separator));
        worker.UserId = UserId;
        worker.ProcessOptions = ProcessOptions;
        
        worker.FormService.OnAfterUpdate += OnAfterUpdate;
        worker.FormService.OnAfterInsert += OnAfterInsert;
        worker.FormService.OnAfterDelete += OnAfterDelete;
        worker.FormService.OnBeforeImport += OnBeforeImport;
        
        worker.OnAfterProcess += OnAfterProcess;
        
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
        button.OnClientClick = "uploadFile1Obj.remove();";
        return button;
    }

    private JJLinkButton GetHelpButton()
    {
        var button = ComponentFactory.Html.LinkButton.Create();;
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
        area.JsCallback = DataImportationScripts.GetLogScript();
        area.Name += "-import";
        area.AllowedTypes = "txt,csv,log";
        return area;
    }
}
