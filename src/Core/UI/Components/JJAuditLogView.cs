using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using JJMasterData.Commons.Data.Entity;
using JJMasterData.Commons.Data.Entity.Abstractions;
using JJMasterData.Commons.DI;
using JJMasterData.Commons.Localization;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataDictionary.Actions.UserCreated;
using JJMasterData.Core.DataManager;
using JJMasterData.Core.DataManager.Services;
using JJMasterData.Core.DataManager.Services.Abstractions;
using JJMasterData.Core.Web.Html;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace JJMasterData.Core.Web.Components;

public class JJAuditLogView : JJBaseView
{
    private IAuditLogService _service;
    private JJGridView _gridView;
    private JJDataPanel _dataPainel;

    public DataContext DataContext => new(DataContextSource.Form, UserId);
    
    public IAuditLogService Service
    {
        get 
        {
            if (_service == null)
            {
                _service = JJService.Provider.GetRequiredService<IAuditLogService>();
            }

            return _service;
        }
    }

    public JJGridView GridView => _gridView ??= CreateGridViewLog();

    /// <summary>
    /// Configuração do painel com os campos do formulário
    /// </summary>
    internal JJDataPanel DataPainel
    {
        get =>
            _dataPainel ??= new JJDataPanel(FormElement)
            {
                Name = "jjpainellog_" + Name
            };
        set => _dataPainel = value;
    }

    public FormElement FormElement { get; private set; }

    internal IEntityRepository EntityRepository { get; private set; }

    private JJAuditLogView()
    {
        Name = "loghistory";
    }

    public JJAuditLogView(FormElement formElement, IEntityRepository entityRepository) : this()
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

    private string GetKeyLog(IDictionary<string,dynamic>values)
    {
        var filter = new Hashtable();
        filter.Add(AuditLogService.DicName, FormElement.Name);
        filter.Add(AuditLogService.DicKey, Service.GetKey(FormElement, values));

        string orderby = AuditLogService.DicModified + " DESC";
        int tot = 1;

        string viewId = "";
        DataTable dt = EntityRepository.GetDataTable(GridView.FormElement, filter, orderby, int.MaxValue, 1, ref tot);

        if (dt.Rows.Count > 0)
            viewId = dt.Rows[0].ItemArray[0].ToString();

        return viewId;
    }

    public HtmlBuilder GetDetailLog(IDictionary<string,dynamic>values)
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
            html.AppendElement(GridView.GetTitle(UserValues));

        if (string.IsNullOrEmpty(logId))
        {
            var alert = new JJAlert();
            alert.ShowIcon = true;
            alert.Icon = IconType.ExclamationTriangle;
            alert.Color = PanelColor.Warning;
            alert.Messages.Add(Translate.Key("No Records Found"));

            return alert.GetHtmlBuilder();
        }

        var filter = new Dictionary<string,dynamic>() { { AuditLogService.DicId, logId } };

        var values = EntityRepository.GetFields(Service.GetElement(), filter);
        string json = values[AuditLogService.DicJson]?.ToString();
        string recordsKey = values[AuditLogService.DicKey]?.ToString();
        var fields = JsonConvert.DeserializeObject<Dictionary<string,dynamic>>(json ?? string.Empty);

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
        filter.Add(AuditLogService.DicId, logId);

        var values = EntityRepository.GetFields(Service.GetElement(), filter);
        string json = values[AuditLogService.DicJson].ToString();

        IDictionary<string,dynamic> fields = JsonConvert.DeserializeObject<Dictionary<string,dynamic>>(json);

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

        var grid = new JJGridView(Service.GetFormElement(), true);
        grid.FormElement.Title = FormElement.Title;
        grid.SetCurrentFilter(AuditLogService.DicName, FormElement.Name);
        grid.CurrentOrder = AuditLogService.DicModified + " DESC";

        var fieldKey = grid.FormElement.Fields[AuditLogService.DicKey];
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
        btnViewLog.OnClientClick = $"jjview.viewLog('{Name}','{{{AuditLogService.DicId}}}');";

        grid.GridActions.Add(btnViewLog);

        return grid;
    }

    private JJToolbar GetFormBottombar()
    {
        var btn = new JJLinkButton();
        btn.Type = LinkButtonType.Button;
        btn.CssClass = $"{BootstrapHelper.DefaultButton} btn-small";
        btn.OnClientClick = $"jjview.viewLog('{Name}','');";
        btn.IconClass = IconType.ArrowLeft.GetCssClass();
        btn.Text = "Back";

        var toolbar = new JJToolbar();
        toolbar.Items.Add(btn.GetHtmlBuilder());
        return toolbar;
    }

    private HtmlBuilder GetHtmlGridInfo(string recordsKey, string viewId)
    {
        var filter = new Hashtable();
        filter.Add(AuditLogService.DicKey, recordsKey);
        filter.Add(AuditLogService.DicName, FormElement.Name);

        string orderby = AuditLogService.DicModified + " DESC";
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
                        b.AppendText("IP: " + row["ip"]);
                    });
                });
            });
        }

        return html;
    }
}
