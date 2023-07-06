using System;
using System.IO;
using System.Text;
using JJMasterData.Commons.Data.Entity.Abstractions;
using JJMasterData.Commons.Extensions;
using JJMasterData.Commons.Localization;
using JJMasterData.Commons.Tasks;
using JJMasterData.Commons.Tasks.Progress;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataManager;
using JJMasterData.Core.DataManager.Imports;
using JJMasterData.Core.DataManager.Services;
using JJMasterData.Core.DataManager.Services.Abstractions;
using JJMasterData.Core.FormEvents.Args;
using JJMasterData.Core.Web.Factories;
using JJMasterData.Core.Web.Html;
using JJMasterData.Core.Web.Http.Abstractions;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace JJMasterData.Core.Web.Components;

public class JJDataImp : JJBaseProcess
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

    public JJLinkButton BackButton => _backButton ??= GetBackButton();

    public JJLinkButton HelpButton => _helpButton ??= GetHelpButton();

    public JJLinkButton LogButton => _logButton ??= GetLogButton();

    public JJUploadArea Upload => _upload ??= GetUploadArea();

    
    
    public bool EnableHistoryLog { get; set; }

    /// <summary>
    /// Default: true (panel is open by default)
    /// </summary>
    public bool ExpandedByDefault { get; set; }

    internal IFieldVisibilityService FieldVisibilityService { get; }

    internal IHttpContext CurrentContext { get; }
    private UploadAreaFactory UploadAreaFactory { get; }

    internal IFormService FormService { get; }
    internal ComboBoxFactory ComboBoxFactory { get; }

    #endregion

    #region "Constructors"
    public JJDataImp(
        FormElement formElement,
        IEntityRepository entityRepository,
        IExpressionsService expressionsService,
        IFieldValuesService fieldValuesService,
        IFormService formService,
        IFieldVisibilityService fieldVisibilityService,
        IBackgroundTask backgroundTask,
        IHttpContext currentContext,
        UploadAreaFactory uploadAreaFactory,
        ComboBoxFactory comboBoxFactory,
        ILoggerFactory loggerFactory,
        IStringLocalizer<JJMasterDataResources> stringLocalizer) 
        : base(entityRepository, expressionsService, fieldValuesService, backgroundTask, loggerFactory.CreateLogger<JJBaseProcess>(), stringLocalizer)
    {
        FieldVisibilityService = fieldVisibilityService;
        CurrentContext = currentContext;
        UploadAreaFactory = uploadAreaFactory;
        ComboBoxFactory = comboBoxFactory;
        FormService = formService;
        FormElement = formElement;
        var importAction = formElement.Options.GridToolbarActions.ImportAction;
        if (importAction is not null)
        {
            ProcessOptions = importAction.ProcessOptions;
        }
        
    }
    #endregion

    internal override HtmlBuilder RenderHtml()
    {
        HtmlBuilder html = null;
        Upload.OnPostFile += OnPostFile;

        string action = CurrentContext.Request["current_uploadaction"];

        switch (action)
        {
            case "process_check":
            {
                var reporterProgress = GetCurrentProcess();
                string json = JsonConvert.SerializeObject(reporterProgress);
                CurrentContext.Response.SendResponse(json, "text/json");
                break;
            }
            case "process_stop":
                StopExportation();
                CurrentContext.Response.SendResponse("{\"isProcessing\": \"false\"}", "text/json");
                break;
            case "process_finished":
                html = GetHtmlLogProcess();
                break;
            case "process_help":
                html = new DataImportationHelp(this).GetHtmlHelp();
                break;
            case "posted_past_text":
            {
                //Process de text from clipboard
                if (!IsRunning())
                {
                    string pasteValue = CurrentContext.Request.Form("pasteValue");
                    ImportInBackground(pasteValue);
                }
                html = GetHtmlWaitProcess();
                break;
            }
            default:
            {
                if (Upload.IsPostAfterUploadAllFiles() || IsRunning())
                    html = GetHtmlWaitProcess();
                else
                    html = GetHtmlForm(ProcessKey);
                break;
            }
        }

        return html;
    }

    private HtmlBuilder GetHtmlLogProcess()
    {
        var html = new DataImportationLog(this).GetHtmlLog()
         .AppendHiddenInput("current_uploadaction")
         .AppendHiddenInput("filename")
         .AppendElement(BackButton);

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
            .AppendScript($"JJDataImp.startProcess('{Upload.Name}'); ")
            .AppendHiddenInput("current_uploadaction")
            .AppendElement(HtmlTag.Div, spin =>
            {
                spin.WithAttribute("id", "impSpin")
                    .WithAttribute("style", "position: relative; height: 80px");
            })
            .AppendText("&nbsp;&nbsp;&nbsp;")
            .AppendText(Translate.Key("Waiting..."))
            .AppendElement(HtmlTag.Br).AppendElement(HtmlTag.Br)
            .AppendElement(HtmlTag.Div, msg =>
            {
                msg.WithAttribute("id", "divMsgProcess")
                   .WithAttribute("style", "display:none")
                   .AppendElement(HtmlTag.Div, status =>
                   {
                       status.WithAttribute("id", "divStatus");
                   })
                   .AppendElement(HtmlTag.Span, resume =>
                   {
                       resume.WithAttribute("id", "lblResumeLog");
                   });
            })
            .AppendElement(HtmlTag.Div, div =>
            {
                div.WithAttribute("style", "width:50%;")
                   .WithCssClass(BootstrapHelper.CenterBlock)
                   .AppendElement(HtmlTag.Div, progress =>
                   {
                       progress.WithCssClass("progress")
                           .AppendElement(HtmlTag.Div, bar =>
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
            .AppendElement(new DataImportationLog(this).GetHtmlResume())
            .AppendElement(HtmlTag.Br).AppendElement(HtmlTag.Br);

        var btnStop = new JJLinkButton
        {
            OnClientClick = $"javascript:JJDataImp.stopProcess('{Upload.Name}','{Translate.Key("Stopping Processing...")}');",
            IconClass = IconType.Stop.GetCssClass(),
            Text = Translate.Key("Stop the import.")
        };
        html.AppendElement(btnStop);

        return html;
    }

    private HtmlBuilder GetHtmlForm(string keyprocess)
    {
        var html = new HtmlBuilder(HtmlTag.Div)
            .WithNameAndId(Name)
            .AppendScript("JJDataImp.addPasteListener();")
            .AppendHiddenInput("current_uploadaction")
            .AppendHiddenInput("filename")
            .AppendElement(HtmlTag.TextArea, area =>
            {
                area.WithNameAndId("pasteValue");
                area.WithAttribute("style", "display:none");
            });
            

        var collapsePanel = new JJCollapsePanel(CurrentContext);
        collapsePanel.TitleIcon = new JJIcon(IconType.FolderOpenO);
        collapsePanel.Title = "Import File";
        collapsePanel.ExpandedByDefault = ExpandedByDefault;
        collapsePanel.HtmlBuilderContent = new HtmlBuilder(HtmlTag.Div)
            .AppendElement(HtmlTag.Label, label =>
            {
                label.AppendText(StringLocalizer["Paste Excel rows or drag and drop files of type: {0}", Upload.AllowedTypes]);
            })
            .AppendElement(Upload);

        html.AppendElement(collapsePanel);
        html.AppendElement(HtmlTag.Div, row =>
        {
            row.WithCssClass("row");
            row.AppendElement(HtmlTag.Div, col =>
            {
                col.WithCssClass("col-sm-12");
                col.AppendElement(BackButton);
                col.AppendElement(HelpButton);

                var pipeline = BackgroundTask.GetProgress<IProgressReporter>(keyprocess);
                if (pipeline != null)
                {
                    col.AppendElement(LogButton);
                }
            });
        });

        return html;
    }
    
    private void OnPostFile(object sender, FormUploadFileEventArgs e)
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

        if (!BackgroundTask.IsRunning(ProcessKey))
        {
            var worker = CreateImpTextWorker(sb.ToString(), ';');
            BackgroundTask.Run(ProcessKey, worker);
        }
    }

    private ImpTextWorker CreateImpTextWorker(string postedText, char splitChar)
    {
        var dataContext = new DataContext(DataContextSource.Upload, UserId);
        FormService.OnAfterUpdate += OnAfterUpdate;
        FormService.OnAfterInsert += OnAfterInsert;
        FormService.OnAfterDelete += OnAfterDelete;
        FormService.OnBeforeImport += OnBeforeImport;

        var worker = new ImpTextWorker(FormElement, FormService,dataContext, postedText, splitChar)
        {
            UserId = UserId,
            OnAfterProcess = OnAfterProcess,
            ProcessOptions = ProcessOptions
        };

        return worker;
    }

    internal DataImpReporter GetCurrentReporter()
    {
        var progress = BackgroundTask.GetProgress<DataImpReporter>(ProcessKey);
        if (progress != null)
            return progress;
        return new DataImpReporter();
    }

    internal void ImportInBackground(string pasteValue)
    {
        var worker = CreateImpTextWorker(pasteValue, '\t');
        BackgroundTask.Run(ProcessKey, worker);
    }

    internal DataImpDto GetCurrentProcess()
    {
        bool isRunning = BackgroundTask.IsRunning(ProcessKey);
        var reporter = BackgroundTask.GetProgress<DataImpReporter>(ProcessKey);
        var dto = new DataImpDto();
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
            dto.Message = Translate.Key("Waiting...");
            dto.StartDate = DateTime.Now.ToDateTimeString();
        }

        return dto;
    }

    private static JJLinkButton GetBackButton()
    {
        return new JJLinkButton
        {
            IconClass = "fa fa-arrow-left",
            Text = "Back",
            ShowAsButton = true,
            OnClientClick = "uploadFile1Obj.remove();"
        };
    }

    private static JJLinkButton GetHelpButton()
    {
        return new JJLinkButton
        {
            IconClass = "fa fa-question-circle",
            Text = "Help",
            ShowAsButton = true,
            OnClientClick = "$('#current_uploadaction').val('process_help'); $('form:first').submit();"
        };
    }

    private static JJLinkButton GetLogButton()
    {
        return new JJLinkButton
        {
            IconClass = "fa fa-film",
            Text = "Last Import",
            ShowAsButton = true,
            OnClientClick = "$('#current_uploadaction').val('process_finished'); $('form:first').submit();"
        };
    }

    private JJUploadArea GetUploadArea()
    {
        var area = UploadAreaFactory.CreateUploadArea();
        area.Multiple = false;
        area.EnableCopyPaste = false;
        area.Name = Name + "_upload";
        area.AllowedTypes = "txt,csv,log";
        return area;
    }
}
