using JJMasterData.Commons.Dao;
using JJMasterData.Commons.Extensions;
using JJMasterData.Commons.Language;
using JJMasterData.Commons.Tasks.Progress;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataManager.AuditLog;
using JJMasterData.Core.DataManager.Imports;
using JJMasterData.Core.FormEvents.Args;
using JJMasterData.Core.Html;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;

namespace JJMasterData.Core.WebComponents;

public class JJDataImp : JJBaseProcess
{
    #region "Events"

    public event EventHandler<FormBeforeActionEventArgs> OnBeforeImport;

    public event EventHandler<FormAfterActionEventArgs> OnAfterImport;

    public event EventHandler<FormAfterActionEventArgs> OnAfterProcess;

    #endregion

    #region "Properties"

    private JJUploadFile _upload;
    private JJLinkButton _backButton;
    private JJLinkButton _helpButton;
    private JJLinkButton _logButton;

    public JJLinkButton BackButton
    {
        get
        {
            if (_backButton == null)
                _backButton = GetBackButtonInstance();

            return _backButton;
        }
    }

    public JJLinkButton HelpButton
    {
        get
        {
            if (_helpButton == null)
                _helpButton = GetHelpButtonInstance();

            return _helpButton;
        }
    }

    public JJLinkButton LogButton
    {
        get
        {
            if (_logButton == null)
                _logButton = GetLogButtonInstance();

            return _logButton;
        }
    }

    public JJUploadFile Upload
    {
        get
        {
            if (_upload == null)
                _upload = GetUploadFileInstance();

            return _upload;
        }
    }

    public bool EnableHistoryLog { get; set; }

    /// <summary>
    /// Default: true (panel is open by default)
    /// </summary>
    public bool ExpandedByDefault { get; set; }

    #endregion

    #region "Constructors"

    public JJDataImp()
    {
        ExpandedByDefault = true;
        Name = "jjdataimp1";
    }

    public JJDataImp(string elementName) : this()
    {
        var dicParser = GetDictionary(elementName);
        FormElement = dicParser.GetFormElement();
        ProcessOptions = dicParser.UIOptions.ToolBarActions.ImportAction.ProcessOptions;
    }

    public JJDataImp(FormElement formElement) : this()
    {
        FormElement = formElement;
    }

    public JJDataImp(FormElement formElement, IDataAccess dataAccess) : this(formElement)
    {
        DataAccess = dataAccess;
    }

    #endregion

    internal override HtmlBuilder RenderHtml()
    {
        HtmlBuilder html = null;
        Upload.OnPostFile += Upload_OnPostFile;

        string action = CurrentContext.Request["current_uploadaction"];
        string t = CurrentContext.Request.QueryString("t");

        if ("process_check".Equals(action))
        {
            var reporterProgress = GetCurrentProcess();
            string json = JsonConvert.SerializeObject(reporterProgress);
            CurrentContext.Response.SendResponse(json, "text/json");
        }
        else if ("process_stop".Equals(action))
        {
            AbortProcess();
            CurrentContext.Response.SendResponse("{\"isProcessing\": \"false\"}", "text/json");
        }
        else if ("process_finished".Equals(action))
        {
            html = GetHtmlLogProcess();
        }
        else if ("process_help".Equals(action))
        {
            html = new DataImpHelp(this).GetHtmlHelp();
        }
        else if ("posted_past_text".Equals(action))
        {
            //Process de text from clipboard
            if (!IsRunning())
            {
                string pasteValue = CurrentContext.Request.Form("pasteValue");
                ImportInBackground(pasteValue);
            }
            html = GetHtmlWaitProcess();
        }
        else
        {
            if (Upload.IsPostAfterUploadAllFiles() || IsRunning())
                html = GetHtmlWaitProcess();
            else
                html = GetHtmlForm(ProcessKey);
        }

        return html;
    }

    private HtmlBuilder GetHtmlLogProcess()
    {
        var html = new DataImpLog(this).GetHtmlLog()
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
            .AppendElement(new DataImpLog(this).GetHtmlResume())
            .AppendElement(HtmlTag.Br).AppendElement(HtmlTag.Br);

        var btnStop = new JJLinkButton
        {
            OnClientClick = $"javascript:JJDataImp.stopProcess('{Upload.Name}','{Translate.Key("Stopping Processing...")}');",
            IconClass = IconHelper.GetClassName(IconType.Stop),
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
            

        var collapsePanel = new JJCollapsePanel();
        collapsePanel.TitleIcon = new JJIcon(IconType.FolderOpenO);
        collapsePanel.Title = "Import File";
        collapsePanel.ExpandedByDefault = ExpandedByDefault;
        collapsePanel.HtmlBuilderContent = new HtmlBuilder(HtmlTag.Div)
            .AppendElement(HtmlTag.Label, label =>
            {
                label.AppendText(Translate.Key("Paste Excel rows or drag and drop files of type: {0}", Upload.AllowedTypes));
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

    /// <summary>
    /// Função executada ao finalizar o upload do arquivo
    /// </summary>
    private void Upload_OnPostFile(object sender, FormUploadFileEventArgs e)
    {
        var sb = new StringBuilder();
        Stream stream = e.File.FileStream;
        using (StreamReader reader = new StreamReader(stream))
        {
            while (!reader.EndOfStream)
            {
                sb.AppendLine(reader.ReadLine());
            }
        }

        if (!BackgroundTask.IsRunning(ProcessKey))
        {
            string pasteValue = CurrentContext.Request.Form("pasteValue");
            var worker = CreateImpTextWorker(sb.ToString(), ';');
            BackgroundTask.Run(ProcessKey, worker);
        }
    }

    private ImpTextWorker CreateImpTextWorker(string postedText, char splitChar)
    {
        var auditLogData = new AuditLogData(AuditLogSource.Upload);
        var worker = new ImpTextWorker(auditLogData, FieldManager, FormManager, postedText, splitChar)
        {
            UserId = UserId,
            OnBeforeImport = OnBeforeImport,
            OnAfterImport = OnAfterImport,
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

    internal  DataImpDto GetCurrentProcess()
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

    private JJLinkButton GetBackButtonInstance()
    {
        var _backButton = new JJLinkButton
        {
            IconClass = "fa fa-arrow-left",
            Text = "Back",
            ShowAsButton = true,
            OnClientClick = "uploadFile1Obj.remove();"
        };
        return _backButton;
    }

    private JJLinkButton GetHelpButtonInstance()
    {
        var helpButton = new JJLinkButton
        {
            IconClass = "fa fa-question-circle",
            Text = "Help",
            ShowAsButton = true,
            OnClientClick = "$('#current_uploadaction').val('process_help'); $('form:first').submit();"
        };
        return helpButton;
    }

    private JJLinkButton GetLogButtonInstance()
    {
        var helpButton = new JJLinkButton
        {
            IconClass = "fa fa-film",
            Text = "Last Import",
            ShowAsButton = true,
            OnClientClick = "$('#current_uploadaction').val('process_finished'); $('form:first').submit();"
        };
        return helpButton;
    }

    private JJUploadFile GetUploadFileInstance()
    {
        var upload = new JJUploadFile
        {
            Multiple = false,
            EnableCopyPaste = false,
            Name = Name + "_upload",
            AllowedTypes = "txt,csv,log"
        };

        return upload;
    }

}
