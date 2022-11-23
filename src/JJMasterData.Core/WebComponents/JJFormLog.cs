using JJMasterData.Commons.Dao;
using JJMasterData.Commons.Dao.Entity;
using JJMasterData.Commons.DI;
using JJMasterData.Commons.Language;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataDictionary.Action;
using JJMasterData.Core.DataManager;
using JJMasterData.Core.DataManager.AuditLog;
using JJMasterData.Core.Html;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Data;
using System.Linq;

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
            if (_auditLog == null)
            {
                var context = new DataContext(DataContextSource.Form, UserId);
                _auditLog = new AuditLogService(context, EntityRepository);
            }

            return _auditLog;
        }
    }

    public JJGridView GridView => _gridView ??= CreateGridViewLog();

    /// <summary>
    /// Configuração do painel com os campos do formulário
    /// </summary>
    internal JJDataPanel DataPainel
    {
        get
        {
            if (_dataPainel == null)
            {
                _dataPainel = new JJDataPanel(FormElement)
                {
                    Name = "jjpainellog_" + Name,
                    EntityRepository = EntityRepository,
                };
            }

            return _dataPainel;
        }
        set { _dataPainel = value; }
    }

    public FormElement FormElement { get; private set; }

    internal IEntityRepository EntityRepository { get; private set; }

    private JJFormLog()
    {
        Name = "loghistory";
    }

    public JJFormLog(FormElement formElement, IEntityRepository entityRepository) : this()
    {
        if (formElement == null)
            throw new ArgumentNullException(nameof(formElement));

        if (entityRepository == null)
            throw new ArgumentNullException(nameof(entityRepository));

        FormElement = formElement;
        EntityRepository = entityRepository;
    }

    internal override HtmlBuilder RenderHtml()
    {
        Service.CreateTableIfNotExist();
        string ajax = CurrentContext.Request.QueryString("t");
        string viewId = CurrentContext.Request.Form("viewid_" + Name);
        var html = new HtmlBuilder(HtmlTag.Div);

        if (string.IsNullOrEmpty(viewId))
        {
            html.AppendElement(GridView);
        }
        else
        {
            if ("ajax".Equals(ajax))
            {
                var panel = GetDetailPanel(viewId);
                CurrentContext.Response.SendResponse(panel.GetHtml());
                return null;
            }

            html.AppendElement(GetDetailLog(viewId));
            html.AppendElement(GetFormBottombar());
        }

        html.AppendHiddenInput($"viewid_{Name}", viewId);

        return html;
    }

    private string GetKeyLog(Hashtable values)
    {
        var filter = new Hashtable();
        filter.Add(AuditLogService.DIC_NAME, FormElement.Name);
        filter.Add(AuditLogService.DIC_KEY, Service.GetKey(FormElement, values));

        string orderby = AuditLogService.DIC_MODIFIED + " DESC";
        int tot = 1;

        string viewId = "";
        DataTable dt = EntityRepository.GetDataTable(GridView.FormElement, filter, orderby, int.MaxValue, 1, ref tot);

        if (dt.Rows.Count > 0)
            viewId = dt.Rows[0].ItemArray[0].ToString();

        return viewId;
    }

    public HtmlBuilder GetDetailLog(Hashtable values)
    {
        string viewId = GetKeyLog(values);
        var html = GetDetailLog(viewId);
        html.AppendHiddenInput($"viewid_{Name}", viewId);
        return html;
    }

    private HtmlBuilder GetDetailLog(string logId)
    {
        var html = new HtmlBuilder(HtmlTag.Div);

        if (GridView.ShowTitle)
            html.AppendElement(GridView.GetTitle());

        if (string.IsNullOrEmpty(logId))
        {
            var alert = new JJAlert();
            alert.ShowIcon = true;
            alert.Icon = IconType.ExclamationTriangle;
            alert.Color = PanelColor.Warning;
            alert.Messages.Add(Translate.Key("No Records Found"));

            return alert.GetHtmlBuilder();
        }

        var filter = new Hashtable();
        filter.Add(AuditLogService.DIC_ID, logId);

        var values = EntityRepository.GetFields(Service.GetElement(), filter);
        string json = values[AuditLogService.DIC_JSON].ToString();
        string recordsKey = values[AuditLogService.DIC_KEY].ToString();
        Hashtable fields = JsonConvert.DeserializeObject<Hashtable>(json);

        var panel = DataPainel;
        panel.PageState = PageState.View;
        panel.Values = fields;
        panel.Name = "jjpainellog_" + Name;

        var row = new HtmlBuilder(HtmlTag.Div)
            .WithCssClass("row");

        row.AppendElement(HtmlTag.Div, d =>
        {
            d.WithCssClass("col-sm-3");
            d.AppendElement(HtmlTag.Div, div =>
            {
                div.WithCssClass("jjrelative");
                div.AppendElement(HtmlTag.Div, divFields =>
                {
                    divFields.WithCssClass("listField")
                      .AppendElement(HtmlTag.P, p =>
                      {
                          p.AppendElement(HtmlTag.B, b =>
                          {
                              b.AppendText($"{Translate.Key("Change History")}:");
                          });
                      });
                    divFields.AppendElement(HtmlTag.Div, group =>
                    {
                        group.WithAttribute("id", "sortable_grid");
                        group.WithCssClass("list-group sortable_grid");
                        group.AppendElement(GetHtmlGridInfo(recordsKey, logId));
                    });
                });
            });
        });

        row.AppendElement(HtmlTag.Div, d =>
        {
            d.WithCssClass("col-sm-9");
            d.AppendElement(HtmlTag.Div, div =>
            {
                div.WithCssClass("jjrelative");
                div.AppendElement(HtmlTag.Div, divDetail =>
                {
                    divDetail.WithCssClass("fieldDetail")
                      .AppendElement(HtmlTag.P, p =>
                      {
                          p.AppendElement(HtmlTag.B, b =>
                          {
                              b.AppendText($"{Translate.Key("Snapshot Record")}:");
                          });
                      });
                    divDetail.AppendElement(panel);
                });
            });
        });

        html.AppendElement(row);
        return html;
    }

    public JJDataPanel GetDetailPanel(string logId)
    {
        var filter = new Hashtable();
        filter.Add(AuditLogService.DIC_ID, logId);

        var values = EntityRepository.GetFields(Service.GetElement(), filter);
        string json = values[AuditLogService.DIC_JSON].ToString();

        Hashtable fields = JsonConvert.DeserializeObject<Hashtable>(json);

        var panel = DataPainel;
        panel.PageState = PageState.View;
        panel.Values = fields;
        panel.Name = "jjpainellog_" + Name;

        return panel;
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

        grid.GridActions.Add(btnViewLog);

        return grid;
    }

    private JJToolbar GetFormBottombar()
    {
        var btn = new JJLinkButton();
        btn.Type = LinkButtonType.Button;
        btn.CssClass = $"{BootstrapHelper.DefaultButton} btn-small";
        btn.OnClientClick = $"jjview.viewLog('{Name}','');";
        btn.IconClass = IconHelper.GetClassName(IconType.ArrowLeft);
        btn.Text = "Back";

        var toolbar = new JJToolbar();
        toolbar.ListElement.Add(btn.GetHtmlBuilder());
        return toolbar;
    }

    private HtmlBuilder GetHtmlGridInfo(string recordsKey, string viewId)
    {
        var filter = new Hashtable();
        filter.Add(AuditLogService.DIC_KEY, recordsKey);
        filter.Add(AuditLogService.DIC_NAME, FormElement.Name);

        string orderby = AuditLogService.DIC_MODIFIED + " DESC";
        int tot = 1;

        DataTable dt = EntityRepository.GetDataTable(GridView.FormElement, filter, orderby, int.MaxValue, 1, ref tot);

        var html = new HtmlBuilder(HtmlTag.Div);
        foreach (DataRow row in dt.Rows)
        {
            string icon = "";
            string color = "";
            string action = "";
            string origem = "";

            if (row["actionType"].Equals((int)CommandOperation.Update))
            {
                icon = "fa fa-pencil fa-lg fa-fw";
                color = "#ffbf00";
                action = Translate.Key("Edited");
            }
            else if (row["actionType"].Equals((int)CommandOperation.Insert))
            {
                icon = "fa fa-plus fa-lg fa-fw";
                color = "#387c44;";
                action = Translate.Key("Added");

            }
            else if (row["actionType"].Equals((int)CommandOperation.Delete))
            {
                icon = "fa fa-trash fa-lg fa-fw";
                color = "#b20000";
                action = Translate.Key("Deleted");
            }

            if (row["origin"].Equals((int)DataContextSource.Api))
                origem = DataContextSource.Api.ToString();
            else if (row["origin"].Equals((int)DataContextSource.Form))
                origem = DataContextSource.Form.ToString();
            else if (row["origin"].Equals((int)DataContextSource.Api))
                origem = DataContextSource.Upload.ToString();

            string logId = row["id"].ToString();
            string message = Translate.Key("{0} from {1} by user:{2}", action, origem, row["userId"].ToString());

            html.AppendElement(HtmlTag.A, a =>
            {
                a.WithAttribute("href", $"javascript:jjview.loadFrameLog('{Name}','{logId}')");
                a.WithNameAndId(logId);
                a.WithCssClass("list-group-item ui-sortable-handle");
                a.WithCssClassIf(logId.Equals(viewId), "active");

                a.AppendElement(HtmlTag.Div, div =>
                {
                    div.WithAttribute("style", "height: 60px;");
                    div.AppendElement(HtmlTag.Span, span =>
                    {
                        span.WithCssClass(icon);
                        span.WithAttribute("style", $"color:{color};");
                        span.WithToolTip(action);
                    });
                    div.AppendElement(HtmlTag.B, b =>
                    {
                        b.AppendText(message);
                    });
                    div.AppendElement(HtmlTag.Span, span =>
                    {
                        span.WithAttribute("style", "float:right");
                        span.AppendText(Translate.Key("Browser info."));
                        span.WithToolTip(row["browser"].ToString());

                        var icon = new JJIcon(IconType.InfoCircle);
                        icon.CssClass = "help-description";
                        span.AppendElement(icon);
                    });
                    div.AppendElement(HtmlTag.Br);
                    div.AppendElement(HtmlTag.B, b =>
                    {
                        b.AppendText(row["modified"].ToString());
                    });
                    div.AppendElement(HtmlTag.Br);
                    div.AppendElement(HtmlTag.B, b =>
                    {
                        b.AppendText("IP: " + row["ip"].ToString());
                    });
                });
            });
        }

        return html;
    }
}
