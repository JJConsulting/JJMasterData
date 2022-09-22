using System;
using System.Collections;
using System.Data;
using System.Linq;
using System.Text;
using JJMasterData.Commons.Language;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataDictionary.Action;
using JJMasterData.Core.DataDictionary.AuditLog;
using Newtonsoft.Json;
using CommandType = JJMasterData.Commons.Dao.Entity.CommandType;

namespace JJMasterData.Core.WebComponents;

public class JJFormLog : JJBaseView
{
    private AuditLogService _auditLog;
    private JJGridView _gridView;
    private JJDataPanel _dataPainel;

    public AuditLogService Service
    {
        get
        {
            if (_auditLog != null) return _auditLog;
            
            _auditLog = new(AuditLogSource.Form)
            {
                DataAccess = DataAccess,
                Factory = Factory
            };
            return _auditLog;
        }
    }

    public JJGridView GridView
    {
        get
        {
            if (_gridView == null)
            {
                _gridView = CreateGridViewLog();
            }
            return _gridView;
        }
    }


    /// <summary>
    /// Configuração do painel com os campos do formulário
    /// </summary>
    internal JJDataPanel DataPainel
    {
        get
        {
            if (_dataPainel == null)
            {
                _dataPainel = new JJDataPanel(FormElement);
                _dataPainel.Name = "jjpainellog_" + Name;
                _dataPainel.DataAccess = DataAccess;
                _dataPainel.UserValues = UserValues;
            }

            return _dataPainel;
        }
        set { _dataPainel = value; }
    }

    public FormElement FormElement { get; set; }


    private JJFormLog()
    {
        Name = "loghistory";
    }

    public JJFormLog(string elementName) : this()
    {
        if (string.IsNullOrEmpty(elementName))
            throw new ArgumentNullException(nameof(elementName), Translate.Key("Dictionary name cannot be empty"));

        Name = "jjlog" + elementName.ToLower();

        var dicParser = GetDictionary(elementName);
        FormElement = dicParser.GetFormElement();
    }

    public JJFormLog(FormElement formElement) : this()
    {
        if (formElement == null)
            throw new ArgumentNullException(nameof(formElement));

        FormElement = formElement;
    }

    protected override string RenderHtml()
    {
        string ajax = CurrentContext.Request.QueryString("t");
        string viewId = CurrentContext.Request.Form("viewid_" + Name);
        var sHtml = new StringBuilder();

        if (string.IsNullOrEmpty(viewId))
        {
            sHtml.AppendLine(GridView.GetHtml());
        }
        else
        {
            if ("ajax".Equals(ajax))
            {
                sHtml.AppendLine(GetDetailPanel(viewId));
                CurrentContext.Response.SendResponse(sHtml.ToString());
                return null;
            }

            sHtml.AppendLine(GetDetailLog(viewId));
            sHtml.AppendLine(GetHtmlFormToolbarDefault());
        }

        sHtml.Append("\t<input type=\"hidden\" ");
        sHtml.Append($"id=\"viewid_{Name}\" ");
        sHtml.Append($"name=\"viewid_{Name}\" ");
        sHtml.AppendLine($"value=\"{viewId}\"/>");

        return sHtml.ToString();
    }

    private string GetKeyLog(Hashtable values)
    {
        var filter = new Hashtable();
        filter.Add(AuditLogService.DIC_NAME, FormElement.Name);
        filter.Add(AuditLogService.DIC_KEY, Service.GetKey(FormElement,values));

        string orderby = AuditLogService.DIC_MODIFIED + " DESC";
        int tot = 1;

        string viewId = "";
        DataTable dt = Factory.GetDataTable(GridView.FormElement, filter, orderby, int.MaxValue, 1, ref tot);

        if (dt.Rows.Count > 0)
            viewId = dt.Rows[0].ItemArray[0].ToString();

        return viewId;
    }

    public string GetDetailLog(Hashtable values)
    {
        var sHtml = new StringBuilder();
        string viewId = (GetKeyLog(values));

        sHtml.Append("\t<input type=\"hidden\" ");
        sHtml.Append($"id=\"viewid_{Name}\" ");
        sHtml.Append($"name=\"viewid_{Name}\" ");
        sHtml.AppendLine($"value=\"{viewId}\"/>");
        sHtml.AppendLine(GetDetailLog(viewId));

        return sHtml.ToString();
    }

    private string GetDetailLog(string logId)
    {

        var sHtml = new StringBuilder();

        if (GridView.ShowTitle)
            sHtml.AppendLine(GridView.GetHtmlTitle());

        if (string.IsNullOrEmpty(logId))
        {
            var alert = new JJAlert();
            alert.Icon = IconType.ExclamationTriangle;
            alert.Type = PanelColor.Warning;
            alert.Message = "No Records Found";

            return alert.GetHtml();
        }

        var filter = new Hashtable();
        filter.Add(AuditLogService.DIC_ID, logId);

        var values = Factory.GetFields(Service.GetElement(), filter);
        string json = values[AuditLogService.DIC_JSON].ToString();
        string recordsKey = values[AuditLogService.DIC_KEY].ToString();

        Hashtable fields = JsonConvert.DeserializeObject<Hashtable>(json);


        var panel = DataPainel;
        panel.PageState = PageState.View;
        panel.Values = fields;
        panel.Name = "jjpainellog_" + Name;


        sHtml.AppendLine("<div class=\"col-sm-3\">");
        sHtml.AppendLine("<div class=\"jjrelative\">");
        sHtml.AppendLine("<div id=\"listField\"");
        sHtml.AppendLine($"<p><b>{Translate.Key("Change History")}:</b></p>");
        sHtml.AppendLine("<div class=\"list-group sortable_grid\" id=\"sortable_grid\">");
        sHtml.AppendLine(GetHtmlGridInfo(recordsKey, logId));
        sHtml.AppendLine("</div>");
        sHtml.AppendLine("</div>");
        sHtml.AppendLine("</div>");
        sHtml.AppendLine("</div>");


        sHtml.AppendLine("<div class=\"col-sm-9\">");
        sHtml.AppendLine("<div class=\"jjrelative\">");
        sHtml.AppendLine("<div id=\"fieldDetail\"");
        sHtml.AppendLine($"<p><b>{Translate.Key("Snapshot Record")}:</b></p>");
        sHtml.AppendLine(panel.GetHtmlPanel());
        sHtml.AppendLine("</div>");
        sHtml.AppendLine("</div>");
        sHtml.AppendLine("</div>");


        return sHtml.ToString();
    }
    public string GetDetailPanel(string logId)
    {
        var filter = new Hashtable();
        filter.Add(AuditLogService.DIC_ID, logId);

        var values = Factory.GetFields(Service.GetElement(), filter);
        string json = values[AuditLogService.DIC_JSON].ToString();

        Hashtable fields = JsonConvert.DeserializeObject<Hashtable>(json);

        var sHtml = new StringBuilder();

        var panel = DataPainel;
        panel.PageState = PageState.View;
        panel.Values = fields;
        panel.Name = "jjpainellog_" + Name;

        sHtml.AppendLine(panel.GetHtmlPanel());

        return sHtml.ToString();
    }

    private JJGridView CreateGridViewLog()
    {
        if (FormElement == null)
            throw new ArgumentNullException(nameof(FormElement));

        var grid = new JJGridView(Service.GetFormElement());
        grid.FormElement.Title = FormElement.Title;
        grid.SetCurrentFilter(AuditLogService.DIC_NAME, FormElement.Name);
        grid.CurrentOrder = AuditLogService.DIC_MODIFIED + " DESC";

        var fieldKey = grid.FormElement.Fields[AuditLogService.DIC_KEY];
        int qtdPk = FormElement.Fields.Count(x => x.IsPk);
        if (qtdPk == 1)
        {
            var f = FormElement.Fields.First(x => x.IsPk);
            fieldKey.Label = f.Label;
            fieldKey.HelpDescription = "Primary key record";
        }
        else
        {
            fieldKey.HelpDescription = "Primary key separated by semicolons";
        }

        var btnViewLog = new ScriptAction();
        btnViewLog.Icon = IconType.Eye;
        btnViewLog.ToolTip = "View";
        btnViewLog.Name = nameof(btnViewLog);
        btnViewLog.OnClientClick = $"jjview.viewLog('{Name}','{{{AuditLogService.DIC_ID}}}');";
        //btnViewLog.EnableExpression = "exp:{" + AuditLogService.DIC_ACTION + "} <> '" + (int)TCommand.DELETE + "'";

        grid.GridActions.Add(btnViewLog);

        return grid;
    }

    private string GetHtmlFormToolbarDefault()
    {
        StringBuilder html = new();
        html.AppendLine("");
        html.AppendLine("<!-- Start Toolbar -->");
        html.AppendLine($"<div class=\"{BootstrapHelper.FormGroup}\"> ");
        html.AppendLine("\t<div class=\"row\"> ");
        html.AppendLine("\t\t<div class=\"col-sm-12\"> ");


        html.Append($"\t\t\t<button type=\"button\" class=\"{BootstrapHelper.DefaultButton} btn-small\" onclick=\"");
        html.AppendLine($"jjview.viewLog('{Name}','');\"> ");
        html.AppendLine("\t\t\t\t<span class=\"fa fa-arrow-left\"></span> ");
        html.Append("\t\t\t\t<span>&nbsp;");
        html.Append(Translate.Key("Back"));
        html.AppendLine("</span>");
        html.AppendLine("\t\t\t</button> ");


        html.AppendLine("\t\t</div> ");
        html.AppendLine("\t</div> ");
        html.AppendLine("</div> ");
        html.AppendLine("");
        html.AppendLine("<!-- End Toolbar -->");

        return html.ToString();
    }

    private string GetHtmlGridInfo(string recordsKey, string viewId)
    {
        var filter = new Hashtable();
        filter.Add(AuditLogService.DIC_KEY, recordsKey);
        filter.Add(AuditLogService.DIC_NAME, FormElement.Name);

        string orderby = AuditLogService.DIC_MODIFIED + " DESC";
        int tot = 1;

        DataTable dt = Factory.GetDataTable(GridView.FormElement, filter, orderby, int.MaxValue, 1, ref tot);

        var sHtml = new StringBuilder();


        foreach (DataRow row in dt.Rows)
        {
            string icon = "";
            string color = "";
            string action = "";
            string origem = "";

            if (row["actionType"].Equals((int)CommandType.Update))
            {
                icon = "fa fa-pencil fa-lg fa-fw";
                color = "#ffbf00";
                action = Translate.Key("Edited");
            }
            else if (row["actionType"].Equals((int)CommandType.Insert))
            {
                icon = "fa fa-plus fa-lg fa-fw";
                color = "#387c44;";
                action = Translate.Key("Added");

            }
            else if (row["actionType"].Equals((int)CommandType.Delete))
            {
                icon = "fa fa-trash fa-lg fa-fw";
                color = "#b20000";
                action = Translate.Key("Deleted");
            }

            if (row["origin"].Equals((int)AuditLogSource.Api))
                origem = AuditLogSource.Api.ToString();
            else if (row["origin"].Equals((int)AuditLogSource.Form))
                origem = AuditLogSource.Form.ToString();
            else if (row["origin"].Equals((int)AuditLogSource.Api))
                origem = AuditLogSource.Upload.ToString();

            string logId = row["id"].ToString();

            string p = Translate.Key("{0} from {1} by user:{2}", action, origem, row["userId"].ToString());

            sHtml.AppendFormat("<a href=\"javascript:jjview.loadFrameLog('{1}','{2}')\" class=\"list-group-item {0} ui-sortable-handle\" id=\"{2}\">", logId.Equals(viewId) ? "active" : "", Name, logId);
            sHtml.AppendLine("<div style=\"height: 50px;\">");
            sHtml.AppendLine($"<span class=\"{icon}\" style=\"color:{color};\" {BootstrapHelper.DataToggle}=\"tooltip\" title=\"\" data-original-title=\"{action}\"></span>");
            sHtml.AppendLine($"<b>{p}<b><b>");
            sHtml.AppendLine($"<span style=\"float:right\">{Translate.Key("Browser info.")}");
            sHtml.AppendLine($"<span class=\"fa fa-info-circle help-description\" {BootstrapHelper.DataToggle}=\"tooltip\" title=\"\" data-original-title=\"{row["browser"]}\"></span>");
            sHtml.AppendLine("</span></b></br>");
            sHtml.AppendLine($"<b>{row["modified"]}</b>");
            sHtml.AppendLine("</br>");
            sHtml.AppendLine($"<b>IP: {row["ip"]}</b>");
            sHtml.AppendLine("</div>");
            sHtml.AppendLine("</a>");

        }

        return sHtml.ToString();
    }
}
