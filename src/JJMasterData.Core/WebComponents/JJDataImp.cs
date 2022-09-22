using System;
using System.IO;
using System.Text;
using JJMasterData.Commons.Dao;
using JJMasterData.Commons.Extensions;
using JJMasterData.Commons.Language;
using JJMasterData.Commons.Tasks.Progress;
using JJMasterData.Commons.Util;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataDictionary.AuditLog;
using JJMasterData.Core.DataManager.Imports;
using JJMasterData.Core.FormEvents.Args;
using Newtonsoft.Json;

namespace JJMasterData.Core.WebComponents;

public class JJDataImp : JJBaseProcess
{

    #region "Events"

    /// <summary>
    /// Evento disparado antes de inserir o registro no banco de dados.
    /// </summary>
    public EventHandler<FormBeforeActionEventArgs> OnBeforeImport;


    /// <summary>
    /// Evento disparado após incluir o registro no banco de dados.
    /// </summary>
    public EventHandler<FormAfterActionEventArgs> OnAfterImport;


    /// <summary>
    /// Evento disparado após processar todo arquivo
    /// </summary>
    public EventHandler<FormAfterActionEventArgs> OnAfterProcess;

    #endregion

    #region "Properties"

    private JJUploadFile _upload;
    private JJLinkButton _backButton;
    private JJLinkButton _helpButton;
    private JJLinkButton _logButton;


    /// <summary>
    /// Configurações botão voltar
    /// </summary>
    public JJLinkButton BackButton 
    {
        get
        {
            if (_backButton == null)
            {
                _backButton = new JJLinkButton
                {
                    IconClass = "fa fa-arrow-left",
                    Text = "Back",
                    ShowAsButton = true,
                    OnClientClick = "uploadFile1Obj.remove();"
                };
            }

            return _backButton;
        }
        set
        {
            _backButton = value;
        }
    }

    /// <summary>
    /// Configurações botão de ajuda
    /// </summary>
    public JJLinkButton HelpButton
    {
        get
        {
            if (_helpButton == null)
            {
                _helpButton = new JJLinkButton
                {
                    IconClass = "fa fa-question-circle",
                    Text = "Help",
                    ShowAsButton = true,
                    OnClientClick = "$('#current_uploadaction').val('process_help'); $('form:first').submit();"
                };
            }

            return _helpButton;
        }
        set
        {
            _helpButton = value;
        }
    }

    /// <summary>
    /// Configurações botão de log
    /// </summary>
    public JJLinkButton LogButton
    {
        get
        {
            if (_logButton == null)
            {
                _logButton = new JJLinkButton();
                _logButton.IconClass = "fa fa-film";
                _logButton.Text = "Last Import";
                _logButton.ShowAsButton = true;
                _logButton.OnClientClick = "$('#current_uploadaction').val('process_finished'); $('form:first').submit();";
            }

            return _logButton;
        }
        set
        {
            _logButton = value;
        }
    }

    /// <summary>
    /// Objeto responsável por realizar upload dos arquivos
    /// </summary>
    public JJUploadFile Upload
    {
        get
        {
            if (_upload == null)
            {
                _upload = new JJUploadFile();
                _upload.Multiple = false;
                _upload.CopyPaste = false;
                _upload.Name = Name + "_upload";
                _upload.AllowedTypes = "txt,csv,log";
            }
            return _upload;
        }
        set
        {
            _upload = value;
        }
    }

    
    /// <summary>
    /// Expandir ou recolher painel de upload de arquivos.
    /// Default: true (painel aberto)
    /// </summary>
    internal bool CollapseAriaExpanded { get; set; }


    public bool EnableHistoryLog { get; set; }

    #endregion

    #region "Constructors"

    public JJDataImp() 
    {
        CollapseAriaExpanded = true;
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

    protected override string RenderHtml()
    {
        var sHtml = new StringBuilder();
        Upload.OnPostFile += Upload_OnPostFile;

        string action = CurrentContext.Request["current_uploadaction"];
        string t = CurrentContext.Request.QueryString("t");
        

        if ("process_check".Equals(action))
        {
            var reporterProgress = GetCurrentProcess();
            string json = JsonConvert.SerializeObject(reporterProgress);
            CurrentContext.Response.SendResponse(json, "text/json");
        }
        //Parando processamento
        else if ("process_stop".Equals(action))
        {
            AbortProcess();
            CurrentContext.Response.SendResponse("{\"isProcessing\": \"false\"}", "text/json");
        }
        //Se finalizou o processamento do arquivo
        else if ("process_finished".Equals(action))
        {
            sHtml.AppendLine(GetHtmlLogProcess());
        }
        //Se for ajuda
        else if ("process_help".Equals(action))
        {
            sHtml.AppendLine(GetHtmlHelp());
        }
        //Se for post from postedText
        else if ("posted_past_text".Equals(action))
        {
            //Processamento de texto from clipboard
            
            if (!IsRunning())
            {
                string pasteValue = CurrentContext.Request.Form("pasteValue");
                ImportInBackground(pasteValue);
            }
            sHtml.AppendLine(GetHtmlWaitProcess());
        }
        else
        {
            if (Upload.IsPostAfterUploadAll() || IsRunning())
                sHtml.AppendLine(GetHtmlWaitProcess());
            else
                sHtml.Append(GetHtmlForm(ProcessKey));
        }


        return sHtml.ToString();
    }

    /// <summary>
    /// Recupera html com o log do processamento
    /// </summary>
    private string GetHtmlLogProcess()
    {
        DataImpReporter reporter = GetCurrentReporter();

        StringBuilder html = new StringBuilder();
        html.AppendLine("<input type=\"hidden\" id=\"current_uploadaction\" name=\"current_uploadaction\" value=\"\" />");
        html.AppendLine("<input type=\"hidden\" id=\"filename\" name=\"filename\" />");
        html.AppendLine("");

        var alert = new JJAlert();
        alert.CssClass = "text-center";


        if (reporter.HasError || reporter.TotalProcessed == reporter.Error)
        {
            alert.Icon = IconType.ExclamationTriangle;
            alert.Type = PanelColor.Danger;
            alert.Title = "Error importing file!";
            alert.Message = reporter.Message;
        }
        else if (reporter.Error > 0)
        {
            alert.Icon = IconType.InfoCircle;
            alert.Type = PanelColor.Info;
            alert.Title = "File imported with errors!";
            alert.Message = reporter.Message;
        }
        else
        {
            alert.Icon = IconType.Check;
            alert.Type = PanelColor.Success;
            alert.Message = reporter.Message;
        }

        html.AppendLine(alert.GetHtml());

        html.AppendLine("<div style='text-align: center;'>");
        html.AppendLine("<div>");
        string elapsedTime = Format.FormatTimeSpan(reporter.StartDate, reporter.EndDate);
        html.Append(Translate.Key("Process performed on {0}", elapsedTime));
        html.AppendLine("</div>");
        if (reporter.Insert > 0)
            html.AppendFormat("<span class='label label-success'>{0} {1}</span>  ", 
                Translate.Key("Inserted:"), reporter.Insert);

        if (reporter.Update > 0)
            html.AppendFormat("<span class='label label-success'>{0} {1}</span>  ",
                Translate.Key("Updated:"), reporter.Update);

        if (reporter.Delete > 0)
            html.AppendFormat("<span class='label label-default'>{0} {1}</span>  ",
                Translate.Key("Deleted:"), reporter.Delete);

        if (reporter.Ignore > 0)
            html.AppendFormat("<span class='label label-warning'>{0} {1}</span>  ",
                Translate.Key("Ignored:"), reporter.Ignore);

        if (reporter.Error > 0)
            html.AppendFormat("<span class='label label-danger'>{0} {1}</span>  ",
                Translate.Key("Errors:"), reporter.Error);

        html.AppendLine("</div>");
        html.AppendLine("<div>&nbsp;</div>");

        html.AppendLine($"<div class=\"{BootstrapHelper.PanelGroup}\" id=\"divNovo\" runat=\"server\" enableviewstate=\"false\">");
        html.AppendLine($"\t<div class=\"{BootstrapHelper.GetPanel("default")}\">");
        html.Append($"\t\t<div class=\"{BootstrapHelper.GetPanelHeading("default")}\" href=\"#collapse1\" {BootstrapHelper.DataToggle}=\"collapse\" data-target=\"#collapse1\" aria-expanded=\"false\">");
        html.AppendLine($"\t\t\t<h4 class=\"{BootstrapHelper.PanelTitle}\">");

        html.Append('\t', 4);
        html.Append("<a>");
        html.Append("<span class=\"fa fa-film\"></span>  ");
        html.Append(Translate.Key("Log"));
        html.Append(" <span class='small'>");
        html.Append(Translate.Key("(Click here for more details)"));
        html.Append("</span>");
        html.AppendLine("</a>");

        html.AppendLine("\t\t\t</h4>");
        html.AppendLine("\t\t</div>");
        html.Append($"\t\t<div id=\"collapse1\" class=\"{BootstrapHelper.PanelCollapse}\">");
        html.AppendLine($"\t\t\t<div class=\"{BootstrapHelper.PanelBody}\">");

        html.Append("<label>");
        html.Append("Date:");
        html.Append("</label>&nbsp;");
        html.Append(Translate.Key("start"));
        html.Append(reporter.StartDate);
        html.Append(" &nbsp;&nbsp;");
        html.Append(Translate.Key("end"));
        html.Append("&nbsp;&nbsp;");
        html.Append(reporter.EndDate);
        html.AppendLine("<br>");

        if (!string.IsNullOrEmpty(reporter.UserId))
        {
            html.Append("<label>");
            html.Append(Translate.Key("User Id:"));
            html.Append("</label>&nbsp;");
            html.AppendLine(reporter.UserId);
            html.AppendLine("<br>");
        }

        html.AppendLine("<br>");
        html.AppendLine(reporter.ErrorLog.ToString().Replace("\r\n","<br>"));
        
        html.AppendLine("\t\t\t</div>");
        html.AppendLine("\t\t</div>");
        html.AppendLine("\t</div>");
        html.AppendLine("</div>");

        html.AppendLine("<div class=\"row\">");
        html.AppendLine("\t<div class=\"col-sm-12\">");
        html.AppendLine(BackButton.GetHtml());


        var btnHelp = new JJLinkButton();
        btnHelp.IconClass = "fa fa-question-circle";
        btnHelp.Text = "Help";
        btnHelp.ShowAsButton = true;
        btnHelp.OnClientClick = "$('#current_uploadaction').val('process_help'); $('form:first').submit();";
        html.AppendLine(btnHelp.GetHtml());

        html.AppendLine("\t</div>");
        html.AppendLine("</div>");

        return html.ToString();
    }

    /// <summary>
    /// Recupera html de aguarde
    /// </summary>
    private string GetHtmlWaitProcess()
    {
        var reporter = GetCurrentReporter();
        if (reporter == null)
            return null;

        var html = new StringBuilder();
        html.AppendLine("<script type=\"text/javascript\">");
        html.AppendLine("    $(document).ready(function () {{");
        html.AppendLine($"        JJDataImp.startProcess('{Upload.Name}');");
        html.AppendLine("    }}); ");
        html.AppendLine("</script>");

        html.AppendLine("<div id='divProcess' style='text-align: center;'>");
        html.AppendLine("    <input type='hidden' id='current_uploadaction' name='current_uploadaction' value='' />");
        html.AppendLine("    <div id='impSpin' style='position: relative; height: 80px'></div>");
	        html.Append("    &nbsp;&nbsp;&nbsp;");
        html.AppendLine(Translate.Key("Waiting..."));
        html.AppendLine("    <br><br>");
        html.AppendLine("    <div id='divMsgProcess' style='display:none'>");

        html.Append(' ', 8);
        html.AppendLine("<div id='divStatus'>");
        html.Append(' ', 12);
        html.AppendLine("<span id='lblResumeLog'></span>");
        html.Append(' ', 8);
        html.AppendLine("</div>");

        html.AppendLine("        <div style='width: 50%;' class='center-block'>");
        html.AppendLine("            <div class='progress'>");
        html.AppendLine("	            <div class='progress-bar' role='progressbar' style='width: 0;' aria-valuemin='0' aria-valuemax='100'>0%</div>");
        html.AppendLine("            </div>");
        html.AppendLine("        </div>");
        html.AppendLine("        <br><br>");
        html.AppendLine("        <div>");

        html.Append(' ', 8);        
        html.Append(Translate.Key("Import started on"));
        html.AppendFormat(" <span id='lblStartDate'>{0}</span></div>", reporter.StartDate);
        html.AppendLine("");

        html.Append(' ', 8);
        html.Append("<span class='label label-success' id='lblInsert' style='display:none;'>");
        html.Append(Translate.Key("Inserted:"));
        html.Append(" <span id='lblInsertCount'>0</span>");
        html.AppendLine("</span>");

        html.Append(' ', 8);
        html.Append("<span class='label label-success' id='lblUpdate' style='display:none;'>");
        html.Append(Translate.Key("Updated:"));
        html.Append(" <span id='lblUpdateCount'>0</span>");
        html.AppendLine("</span>");

        html.Append(' ', 8);
        html.Append("<span class='label label-default' id='lblDelete' style='display:none;'>");
        html.Append(Translate.Key("Deleted:"));
        html.Append(" <span id='lblDeleteCount'>0</span>");
        html.AppendLine("</span>");

        html.Append(' ', 8);
        html.Append("<span class='label label-warning' id='lblIgnore' style='display:none;'>");
        html.Append(Translate.Key("Ignored:"));
        html.Append(" <span id='lblIgnoreCount'>0</span>");
        html.AppendLine("</span>");

        html.Append(' ', 8);
        html.Append("<span class='label label-danger' id='lblError' style='display:none;'>");
        html.Append(Translate.Key("Errors:"));
        html.Append(" <span id='lblErrorCount'>0</span>");
        html.AppendLine("</span>");

        html.Append(' ', 8);
        html.AppendLine("<br><br><br>");

        html.Append(' ', 8);
        html.AppendFormat("<a href='javascript:JJDataImp.stopProcess(\"{0}\",\"{1}\");'>", 
            Upload.Name, Translate.Key("Stopping Processing..."));
        html.Append("<span class='fa fa-stop'></span>");
        html.Append(Translate.Key("Stop the import."));
        html.AppendLine("</a>");

        html.Append(' ', 4);
        html.AppendLine("</div>");
        html.AppendLine("</div>");

        return html.ToString();
    }

    /// <summary>
    /// Recupera html para realizar upload do arquivo
    /// </summary>
    private string GetHtmlForm(string keyprocess)
    {
        StringBuilder html = new();

        html.AppendLine("<style>");
        html.AppendLine("\t.ajax-upload-dragdrop { height:100px; }");
        html.AppendLine("</style>");
        html.AppendLine("");

        html.AppendLine("<script type=\"text/javascript\"> ");
        html.AppendLine("$(document).ready(function () {");
        html.AppendLine("\tdocument.addEventListener(\"paste\", function (e) {");
        html.AppendLine("\t\tvar pastedText = undefined;");
        html.AppendLine("\t\tif (window.clipboardData && window.clipboardData.getData) { // IE ");
        html.AppendLine("\t\t\tpastedText = window.clipboardData.getData('Text');");
        html.AppendLine("\t\t} else if (e.clipboardData && e.clipboardData.getData) {");
        html.AppendLine("\t\t\tpastedText = e.clipboardData.getData('text/plain');");
        html.AppendLine("\t\t}");
        html.AppendLine("\t\te.preventDefault();");
        html.AppendLine("\t\tif (pastedText != undefined) {");
        html.AppendLine("\t\t\t$(\"#current_uploadaction\").val(\"posted_past_text\");");
        html.AppendLine("\t\t\t$(\"#pasteValue\").val(pastedText);");
        html.AppendLine("\t\t\t$(\"form:first\").submit();");
        html.AppendLine("\t\t}");
        html.AppendLine("\t\treturn false;");
        html.AppendLine("\t});");
        html.AppendLine("");
        html.AppendLine("});");
        html.AppendLine("</script> ");

        html.AppendLine("<input type=\"hidden\" id=\"current_uploadaction\" name=\"current_uploadaction\" value=\"\" />");
        html.AppendLine("<input type=\"hidden\" id=\"filename\" name=\"filename\" />");
        html.AppendLine("<input type=\"hidden\" id=\"pasteValue\" name=\"pasteValue\" />");
        html.AppendLine("");
        html.AppendLine($"<div class=\"{BootstrapHelper.PanelGroup}\" id=\"divNovo\" runat=\"server\" enableviewstate=\"false\">");
        html.AppendLine($"\t<div class=\"{BootstrapHelper.GetPanel("default")}\">");
        html.Append($"\t\t<div class=\"{BootstrapHelper.GetPanelHeading("default")}\" href=\"#collapse1\" {BootstrapHelper.DataToggle}=\"collapse\" data-target=\"#collapse1\" aria-expanded=\"");
        html.Append(CollapseAriaExpanded ? "true" : "false");
        html.AppendLine("\">");
        html.AppendLine($"\t\t\t<h4 class=\"{BootstrapHelper.PanelTitle}\">");

        html.Append('\t', 4);
        html.Append("<a>");
        html.Append("<span class=\"fa fa-folder-open-o\"></span> ");
        html.Append(Translate.Key("Import File"));
        html.AppendLine("</a>");

        html.AppendLine("\t\t\t</h4>");
        html.AppendLine("\t\t</div>");
        html.Append($"\t\t<div id=\"collapse1\" class=\"{BootstrapHelper.PanelCollapse} ");
        html.Append(CollapseAriaExpanded ? "in" : "");
        html.AppendLine("\">");
        html.AppendLine($"\t\t\t<div class=\"{BootstrapHelper.PanelBody}\">");

        html.Append('\t', 4);
        html.Append("<label>");
        html.Append(Translate.Key("Paste Excel rows or drag and drop files of type: {0}", Upload.AllowedTypes));
        html.AppendLine("</label>");

        html.AppendLine(Upload.GetHtml());
        html.AppendLine("\t\t\t</div>");
        html.AppendLine("\t\t</div>");
        html.AppendLine("\t</div>");
        html.AppendLine("</div>");

        html.AppendLine("<div class=\"row\">");
        html.AppendLine("\t<div class=\"col-sm-12\">");
        html.AppendLine(BackButton.GetHtml());
        html.AppendLine(HelpButton.GetHtml());

        var pipeline = BackgroundTask.GetProgress<IProgressReporter>(keyprocess);
        if (pipeline != null)
        {
            html.AppendLine(LogButton.GetHtml());
        }
        html.AppendLine("\t</div>");
        html.AppendLine("</div>");

        return html.ToString();
    }

    /// <summary>
    /// Recupera o html de ajuda
    /// </summary>
    private string GetHtmlHelp()
    {
        var help = new JJDataImpHelp(this);
        return help.GetHtml();
    }

    /// <summary>
    /// Função executada ao finalizar o upload do arquivo
    /// </summary>
    private void Upload_OnPostFile(object sender, FormUploadFileEventArgs e)
    {
        var sb = new StringBuilder();

        Stream stream;
#if NETFRAMEWORK
        stream = e.File.FileData.InputStream;
#else
        stream = e.File.FileData.OpenReadStream();
#endif
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

    private DataImpReporter GetCurrentReporter()
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


    internal  DataImpDTO GetCurrentProcess()
    {
        bool isRunning = BackgroundTask.IsRunning(ProcessKey);
        var reporter = BackgroundTask.GetProgress<DataImpReporter>(ProcessKey);
        var dto = new DataImpDTO();
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

}
