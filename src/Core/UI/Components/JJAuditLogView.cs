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
using JJMasterData.Core.Web.Factories;
using JJMasterData.Core.Web.Html;
using JJMasterData.Core.Web.Http.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Newtonsoft.Json;

namespace JJMasterData.Core.Web.Components;

public class JJAuditLogView : JJBaseView
{
    private JJGridView _gridView;
    private JJDataPanel _dataPainel;
    private string _userId;

    /// <summary>
    /// Id do usuário Atual
    /// </summary>
    /// <remarks>
    /// Se a variavel não for atribuida diretamente,
    /// o sistema tenta recuperar em UserValues ou nas variaveis de Sessão
    /// </remarks>
    internal string UserId => _userId ??= DataHelper.GetCurrentUserId(CurrentContext, UserValues);
    
    private IHttpContext CurrentContext { get; }

    public IAuditLogService AuditLogService { get; }
    public JJGridView GridView => _gridView ??= CreateGridViewLog();

    /// <summary>
    /// Configuração do painel com os campos do formulário
    /// </summary>
    internal JJDataPanel DataPanel
    {
        get
        {
            var panel = DataPanelFactory.Value.CreateDataPanel(FormElement);
            panel.Name = "jjpainellog_" + Name;
            return _dataPainel ??= panel;
        }
        set => _dataPainel = value;
    }

    public FormElement FormElement { get; private set; }
    private Lazy<GridViewFactory> GridViewFactory { get; }
    private Lazy<DataPanelFactory> DataPanelFactory { get; }
    private IStringLocalizer<JJMasterDataResources> StringLocalizer { get; }

    internal IEntityRepository EntityRepository { get; }

    public JJAuditLogView(
        FormElement formElement, 
        Lazy<GridViewFactory> gridViewFactory,
        Lazy<DataPanelFactory> dataPanelFactory,
        IHttpContext currentContext, 
        IEntityRepository entityRepository,
        IAuditLogService auditLogService,
        IStringLocalizer<JJMasterDataResources> stringLocalizer) 
    {
        FormElement = formElement ?? throw new ArgumentNullException(nameof(formElement));
        GridViewFactory = gridViewFactory;
        DataPanelFactory = dataPanelFactory;
        CurrentContext = currentContext;
        EntityRepository = entityRepository;
        AuditLogService = auditLogService;
        StringLocalizer = stringLocalizer;
    }

    internal override HtmlBuilder RenderHtml()
    {
        AuditLogService.CreateTableIfNotExist();
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
        filter.Add(DataManager.Services.AuditLogService.DicName, FormElement.Name);
        filter.Add(DataManager.Services.AuditLogService.DicKey, AuditLogService.GetKey(FormElement, values));

        string orderby = DataManager.Services.AuditLogService.DicModified + " DESC";
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
            alert.Messages.Add(StringLocalizer["No Records Found"]);

            return alert.GetHtmlBuilder();
        }

        var filter = new Dictionary<string,dynamic> { { DataManager.Services.AuditLogService.DicId, logId } };

        var values = EntityRepository.GetFields(AuditLogService.GetElement(), filter);
        string json = values[DataManager.Services.AuditLogService.DicJson]?.ToString();
        string recordsKey = values[DataManager.Services.AuditLogService.DicKey]?.ToString();
        var fields = JsonConvert.DeserializeObject<Dictionary<string,dynamic>>(json ?? string.Empty);

        var panel = DataPanel;
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
                              b.AppendText($"{StringLocalizer["Change History"]}:");
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
                              b.AppendText($"{StringLocalizer["Snapshot Record"]}:");
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
        filter.Add(DataManager.Services.AuditLogService.DicId, logId);

        var values = EntityRepository.GetFields(AuditLogService.GetElement(), filter);
        string json = values[DataManager.Services.AuditLogService.DicJson].ToString();

        IDictionary<string,dynamic> fields = JsonConvert.DeserializeObject<Dictionary<string,dynamic>>(json);

        var panel = DataPanel;
        panel.PageState = PageState.View;
        panel.Values = fields;
        panel.Name = "jjpainellog_" + Name;

        return panel;
    }

    private JJGridView CreateGridViewLog()
    {
        if (FormElement == null)
            throw new ArgumentNullException(nameof(FormElement));

        var grid = GridViewFactory.Value.CreateGridView(FormElement);
        grid.FormElement.Title = FormElement.Title;
        grid.SetCurrentFilter(DataManager.Services.AuditLogService.DicName, FormElement.Name);
        grid.CurrentOrder = DataManager.Services.AuditLogService.DicModified + " DESC";

        var fieldKey = grid.FormElement.Fields[DataManager.Services.AuditLogService.DicKey];
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
        btnViewLog.OnClientClick = $"jjview.viewLog('{Name}','{{{DataManager.Services.AuditLogService.DicId}}}');";

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
        filter.Add(DataManager.Services.AuditLogService.DicKey, recordsKey);
        filter.Add(DataManager.Services.AuditLogService.DicName, FormElement.Name);

        string orderby = DataManager.Services.AuditLogService.DicModified + " DESC";
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
                action = StringLocalizer["Edited"];
            }
            else if (row["actionType"].Equals((int)CommandOperation.Insert))
            {
                icon = "fa fa-plus fa-lg fa-fw";
                color = "#387c44;";
                action = StringLocalizer["Added"];

            }
            else if (row["actionType"].Equals((int)CommandOperation.Delete))
            {
                icon = "fa fa-trash fa-lg fa-fw";
                color = "#b20000";
                action = StringLocalizer["Deleted"];
            }

            if (row["origin"].Equals((int)DataContextSource.Api))
                origem = DataContextSource.Api.ToString();
            else if (row["origin"].Equals((int)DataContextSource.Form))
                origem = DataContextSource.Form.ToString();
            else if (row["origin"].Equals((int)DataContextSource.Api))
                origem = DataContextSource.Upload.ToString();

            string logId = row["id"].ToString();
            string message = StringLocalizer["{0} from {1} by user:{2}", action, origem, row["userId"].ToString()];

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
                        span.AppendText(StringLocalizer["Browser info."]);
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
