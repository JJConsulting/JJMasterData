using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using JJMasterData.Commons.Cryptography;
using JJMasterData.Commons.Data.Entity;
using JJMasterData.Commons.Data.Entity.Abstractions;
using JJMasterData.Commons.Localization;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataDictionary.Actions.UserCreated;
using JJMasterData.Core.DataManager;
using JJMasterData.Core.DataManager.Services.Abstractions;
using JJMasterData.Core.Extensions;
using JJMasterData.Core.Web.Factories;
using JJMasterData.Core.Web.Html;
using JJMasterData.Core.Web.Http.Abstractions;
using Microsoft.Extensions.Localization;
using Newtonsoft.Json;

namespace JJMasterData.Core.Web.Components;

public class JJAuditLogView : JJAsyncBaseView
{
    private readonly ComponentFactory _componentFactory;
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
    private JJMasterDataEncryptionService EncryptionService { get; }
    private JJMasterDataUrlHelper UrlHelper { get; }
    public JJGridView GridView => _gridView ??= CreateGridViewLog();

    /// <summary>
    /// Configuração do painel com os campos do formulário
    /// </summary>
    internal JJDataPanel DataPanel
    {
        get
        {
            var panel = _componentFactory.DataPanel.Create(FormElement);
            panel.Name = "auditlogview-panel-" + Name;
            return _dataPainel ??= panel;
        }
        set => _dataPainel = value;
    }

    public FormElement FormElement { get; private set; }
    private IStringLocalizer<JJMasterDataResources> StringLocalizer { get; }

    internal IEntityRepository EntityRepository { get; }

    public JJAuditLogView(
        FormElement formElement,
        IHttpContext currentContext,
        IEntityRepository entityRepository,
        IAuditLogService auditLogService,
        ComponentFactory componentFactory,
        JJMasterDataEncryptionService encryptionService,
        JJMasterDataUrlHelper urlHelper,
        IStringLocalizer<JJMasterDataResources> stringLocalizer)
    {
        _componentFactory = componentFactory;
        FormElement = formElement ?? throw new ArgumentNullException(nameof(formElement));
        CurrentContext = currentContext;
        EntityRepository = entityRepository;
        AuditLogService = auditLogService;
        EncryptionService = encryptionService;
        UrlHelper = urlHelper;
        StringLocalizer = stringLocalizer;
    }

    protected override async Task<HtmlBuilder> RenderHtmlAsync()
    {
        string ajax = CurrentContext.Request.QueryString("t");
        string viewId = CurrentContext.Request.Form("logId-" + Name);
        var html = new HtmlBuilder(HtmlTag.Div);

        if (string.IsNullOrEmpty(viewId))
        {
            await html.AppendComponentAsync(GridView);
        }
        else
        {
            if ("ajax".Equals(ajax))
            {
                var panel = await GetDetailsPanelAsync(viewId);

                CurrentContext.Response.SendResponse(await panel.GetHtmlAsync());

                return null;
            }

            html.Append(await GetLogDetailsHtmlAsync(viewId));
            html.AppendComponent(GetFormBottombar());
        }

        html.AppendHiddenInput($"logId-{Name}", viewId);

        return html;
    }

    private string GetEntryKey(IDictionary<string, dynamic> values)
    {
        var filter = new Dictionary<string, dynamic>
        {
            { DataManager.Services.AuditLogService.DicName, FormElement.Name },
            {
                DataManager.Services.AuditLogService.DicKey, AuditLogService.GetKey(FormElement, values)
            }
        };

        string orderby = DataManager.Services.AuditLogService.DicModified + " DESC";
        int tot = 1;

        string viewId = "";
        DataTable dt = EntityRepository.GetDataTable(GridView.FormElement, filter, orderby, int.MaxValue, 1, ref tot);

        if (dt.Rows.Count > 0)
            viewId = dt.Rows[0].ItemArray[0].ToString();

        return viewId;
    }

    public async Task<HtmlBuilder> GetLogDetailsHtmlAsync(IDictionary<string, dynamic> values)
    {
        string viewId = GetEntryKey(values);
        var html = await GetLogDetailsHtmlAsync(viewId);
        html.AppendHiddenInput($"viewid_{Name}", viewId);
        return html;
    }

    private async Task<HtmlBuilder> GetLogDetailsHtmlAsync(string logId)
    {
        var html = new HtmlBuilder(HtmlTag.Div);

        if (GridView.ShowTitle)
            html.AppendComponent(GridView.GetTitle(UserValues));

        if (string.IsNullOrEmpty(logId))
        {
            var alert = new JJAlert
            {
                ShowIcon = true,
                Icon = IconType.ExclamationTriangle,
                Color = PanelColor.Warning
            };
            alert.Messages.Add(StringLocalizer["No Records Found"]);

            return alert.GetHtmlBuilder();
        }

        var filter = new Dictionary<string, dynamic> { { DataManager.Services.AuditLogService.DicId, logId } };

        var values = await EntityRepository.GetDictionaryAsync(AuditLogService.GetElement(), filter);
        string json = values[DataManager.Services.AuditLogService.DicJson]?.ToString();
        string recordsKey = values[DataManager.Services.AuditLogService.DicKey]?.ToString();
        var fields = JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(json ?? string.Empty);

        var panel = DataPanel;
        panel.PageState = PageState.View;
        panel.Values = fields;
        panel.Name = "auditlogview-panel-" + Name;

        var row = new HtmlBuilder(HtmlTag.Div)
            .WithCssClass("row");

        row.Append(HtmlTag.Div, d =>
        {
            d.WithCssClass("col-sm-3");
            d.Append(HtmlTag.Div, div =>
            {
                div.WithCssClass("jjrelative");
                div.Append(HtmlTag.Div, divFields =>
                {
                    divFields.WithCssClass("listField")
                        .Append(HtmlTag.P,
                            p =>
                            {
                                p.Append(HtmlTag.B, b => { b.AppendText($"{StringLocalizer["Change History"]}:"); });
                            });
                    divFields.Append(HtmlTag.Div, group =>
                    {
                        group.WithAttribute("id", "sortable-grid");
                        group.WithCssClass("list-group sortable-grid");
                        group.Append(GetHtmlGridInfo(recordsKey, logId));
                    });
                });
            });
        });

        row.Append(HtmlTag.Div, d =>
        {
            d.WithCssClass("col-sm-9");
            d.Append(HtmlTag.Div, div =>
            {
                div.WithCssClass("jjrelative");
                div.Append(HtmlTag.Div, divDetail =>
                {
                    divDetail.WithCssClass("fieldDetail")
                        .Append(HtmlTag.P,
                            p =>
                            {
                                p.Append(HtmlTag.B, b => { b.AppendText($"{StringLocalizer["Record Snapshot"]}:"); });
                            });
                    divDetail.AppendComponent(panel);
                });
            });
        });

        html.Append(row);
        return html;
    }

    public async Task<JJDataPanel> GetDetailsPanelAsync(string logId)
    {
        var filter = new Dictionary<string, dynamic> { { DataManager.Services.AuditLogService.DicId, logId } };

        var values = await EntityRepository.GetDictionaryAsync(AuditLogService.GetElement(), filter);
        string json = values[DataManager.Services.AuditLogService.DicJson].ToString();

        IDictionary<string, dynamic> fields = JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(json);

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

        var grid = _componentFactory.GridView.Create(AuditLogService.GetFormElement());
        grid.FormElement.Title = FormElement.Title;
        grid.SetCurrentFilterAsync(DataManager.Services.AuditLogService.DicName, FormElement.Name).GetAwaiter()
            .GetResult();
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

        var btnViewLog = new ScriptAction
        {
            Icon = IconType.Eye,
            ToolTip = "View"
        };
        btnViewLog.Name = nameof(btnViewLog);
        btnViewLog.OnClientClick = $"JJView.viewLog('{Name}','{{{DataManager.Services.AuditLogService.DicId}}}');";

        grid.GridActions.Add(btnViewLog);

        return grid;
    }

    private JJToolbar GetFormBottombar()
    {
        var btn = new JJLinkButton
        {
            Type = LinkButtonType.Button,
            CssClass = $"{BootstrapHelper.DefaultButton} btn-small",
            OnClientClick = $"JJView.viewLog('{Name}','');",
            IconClass = IconType.ArrowLeft.GetCssClass(),
            Text = "Back"
        };

        var toolbar = new JJToolbar();
        toolbar.Items.Add(btn.GetHtmlBuilder());
        return toolbar;
    }

    private HtmlBuilder GetHtmlGridInfo(string recordsKey, string viewId)
    {
        var filter = new Dictionary<string, dynamic>
        {
            { DataManager.Services.AuditLogService.DicKey, recordsKey },
            { DataManager.Services.AuditLogService.DicName, FormElement.Name }
        };

        string orderby = DataManager.Services.AuditLogService.DicModified + " DESC";
        int tot = 1;

        var dt = EntityRepository.GetDataTable(GridView.FormElement, filter, orderby, int.MaxValue, 1, ref tot);

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

            html.Append(HtmlTag.A, a =>
            {
                var url = IsExternalRoute
                    ? UrlHelper.GetUrl("GetDetailsPanel", "AuditLog",
                        new
                        {
                            dictionaryName = EncryptionService.EncryptStringWithUrlEscape(FormElement.Name),
                            componentName = Name
                        })
                    : string.Empty;
                a.WithAttribute("href", $"javascript:loadAuditLog('{Name}','{logId}', '{url}')");
                a.WithNameAndId(logId);
                a.WithCssClass("list-group-item ui-sortable-handle");
                a.WithCssClassIf(logId.Equals(viewId), "active");

                a.Append(HtmlTag.Div, div =>
                {
                    div.WithAttribute("style", "height: 60px;");
                    div.Append(HtmlTag.Span, span =>
                    {
                        span.WithCssClass(icon);
                        span.WithAttribute("style", $"color:{color};");
                        span.WithToolTip(action);
                    });
                    div.Append(HtmlTag.B, b => { b.AppendText(message); });
                    div.Append(HtmlTag.Span, span =>
                    {
                        span.WithAttribute("style", "float:right");
                        span.AppendText(StringLocalizer["Browser info."]);
                        span.WithToolTip(row["browser"].ToString());

                        var icon = new JJIcon(IconType.InfoCircle)
                        {
                            CssClass = "help-description"
                        };
                        span.AppendComponent(icon);
                    });
                    div.Append(HtmlTag.Br);
                    div.Append(HtmlTag.B, b => { b.AppendText(row["modified"].ToString()); });
                    div.Append(HtmlTag.Br);
                    div.Append(HtmlTag.B, b => { b.AppendText("IP: " + row["ip"]); });
                });
            });
        }

        return html;
    }
}