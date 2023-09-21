using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JJMasterData.Commons.Cryptography;
using JJMasterData.Commons.Data.Entity;
using JJMasterData.Commons.Data.Entity.Abstractions;
using JJMasterData.Commons.Localization;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataManager;
using JJMasterData.Core.DataManager.Services.Abstractions;
using JJMasterData.Core.Extensions;
using JJMasterData.Core.UI.Components;
using JJMasterData.Core.Web.Factories;
using JJMasterData.Core.Web.Html;
using JJMasterData.Core.Web.Http.Abstractions;
using Microsoft.Extensions.Localization;
using Newtonsoft.Json;

namespace JJMasterData.Core.Web.Components;

public class JJAuditLogView : AsyncComponent
{
    private readonly IComponentFactory _componentFactory;
    private JJGridView _gridView;
    private JJDataPanel _dataPainel;
    private string _userId;
    private RouteContext _routeContext;


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
    
    /// <summary>
    /// Id do usuário Atual
    /// </summary>
    /// <remarks>
    /// Se a variavel não for atribuida diretamente,
    /// o sistema tenta recuperar em UserValues ou nas variaveis de Sessão
    /// </remarks>
    internal string UserId => _userId ??= DataHelper.GetCurrentUserId(CurrentContext.Session, UserValues);

    private IHttpContext CurrentContext { get; }

    public IAuditLogService AuditLogService { get; }
    private IEncryptionService EncryptionService { get; }
    
    public JJGridView GridView => _gridView ??= CreateGridViewLog();

    /// <summary>
    /// Configuração do painel com os campos do formulário
    /// </summary>
    internal JJDataPanel DataPanel
    {
        get
        {
            var panel = _componentFactory.DataPanel.Create(FormElement);
            panel.Name = $"auditlogview-panel-{Name}";
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
        IComponentFactory componentFactory,
        IEncryptionService encryptionService,
        IStringLocalizer<JJMasterDataResources> stringLocalizer)
    {
        Name = $"{ComponentNameGenerator.Create(formElement.Name)}-audit-log-view";
        _componentFactory = componentFactory;
        FormElement = formElement;
        CurrentContext = currentContext;
        EntityRepository = entityRepository;
        AuditLogService = auditLogService;
        EncryptionService = encryptionService;
        StringLocalizer = stringLocalizer;
    }

    protected override async Task<ComponentResult> BuildResultAsync()
    {
        string logId = CurrentContext.Request.Form[$"audit-log-id-{Name}"];
        var html = new HtmlBuilder(HtmlTag.Div);

        if (string.IsNullOrEmpty(logId))
        {
            var gridResult = await GridView.GetResultAsync();
            if (gridResult is RenderedComponentResult renderedComponentResult)
            {
                html.Append(renderedComponentResult.HtmlBuilder);
            }
            else
            {
                return gridResult;
            }
        }
        else
        {
            if (ComponentContext is ComponentContext.AuditLogView)
            {
                var panel = await GetDetailsPanel(logId);

                var panelHtmlBuilder = await panel.GetPanelHtmlBuilderAsync();
                
                return new ContentComponentResult(panelHtmlBuilder);
            }

            html.Append(await GetLogDetailsHtmlAsync(logId));
            html.AppendComponent(GetFormBottombar());
        }

        html.AppendHiddenInput($"audit-log-id-{Name}", logId);

        return new RenderedComponentResult(html);
    }

    private async Task<string> GetEntryKey(IDictionary<string, object> values)
    {
        var filter = new Dictionary<string, object>
        {
            { DataManager.Services.AuditLogService.DicName, FormElement.Name },
            {
                DataManager.Services.AuditLogService.DicKey, AuditLogService.GetKey(FormElement, values)
            }
        };

        var orderBy = new OrderByData();
        orderBy.AddOrReplace(DataManager.Services.AuditLogService.DicModified,OrderByDirection.Desc);

        var entryId = string.Empty;
        var result = await EntityRepository.GetDictionaryListResultAsync(GridView.FormElement, new EntityParameters()
        {
            Filters = filter,
            OrderBy = orderBy,
            CurrentPage = 1,
            RecordsPerPage = 1
        });

        if (result.Data.Count > 0)
            entryId = result.Data.ElementAt(0).ElementAt(0).Value.ToString();

        return entryId;
    }

    public async Task<HtmlBuilder> GetLogDetailsHtmlAsync(IDictionary<string, object> values)
    {
        string entryId = await GetEntryKey(values);
        var html = await GetLogDetailsHtmlAsync(entryId);
        html.AppendHiddenInput($"audit-log-id-{Name}", entryId);
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

        var filter = new Dictionary<string, object> { { DataManager.Services.AuditLogService.DicId, logId } };

        var values = await EntityRepository.GetFieldsAsync(AuditLogService.GetElement(), filter);
        string json = values[DataManager.Services.AuditLogService.DicJson]?.ToString();
        string recordsKey = values[DataManager.Services.AuditLogService.DicKey]?.ToString();
        var fields = JsonConvert.DeserializeObject<Dictionary<string, object>>(json ?? string.Empty);

        var panel = DataPanel;
        panel.PageState = PageState.View;
        panel.Values = fields;
        panel.Name = $"auditlogview-panel-{Name}";

        var row = new HtmlBuilder(HtmlTag.Div)
            .WithCssClass("row");

        await row.AppendAsync(HtmlTag.Div, async d =>
        {
            d.WithCssClass("col-sm-3");
            await d.AppendAsync(HtmlTag.Div, async div =>
            {
                div.WithCssClass("jjrelative");
                await div.AppendAsync(HtmlTag.Div, async divFields =>
                {
                    divFields.WithCssClass("listField")
                        .Append(HtmlTag.P,
                            p =>
                            {
                                p.Append(HtmlTag.B, b => { b.AppendText($"{StringLocalizer["Change History"]}:"); });
                            });
                    await divFields.AppendAsync(HtmlTag.Div, async group =>
                    {
                        group.WithAttribute("id", "sortable-grid");
                        group.WithCssClass("list-group sortable-grid");
                        group.Append(await GetHtmlGridInfo(recordsKey, logId));
                    });
                });
            });
        });

        await row.AppendAsync(HtmlTag.Div, async d =>
        {
            d.WithCssClass("col-sm-9");
            await d.AppendAsync(HtmlTag.Div, async div =>
            {
                div.WithCssClass("jjrelative");
                await div.AppendAsync(HtmlTag.Div, async divDetail =>
                {
                    divDetail.WithCssClass("field-details")
                        .Append(HtmlTag.P,
                            p =>
                            {
                                p.Append(HtmlTag.B, b => { b.AppendText($"{StringLocalizer["Record Snapshot"]}:"); });
                            });
                    divDetail.Append( await panel.GetPanelHtmlBuilderAsync());
                });
            });
        });

        html.Append(row);
        return html;
    }

    private async Task<JJDataPanel> GetDetailsPanel(string logId)
    {
        var filter = new Dictionary<string, object> { { DataManager.Services.AuditLogService.DicId, logId } };

        var values = await EntityRepository.GetFieldsAsync(AuditLogService.GetElement(), filter);
        string json = values[DataManager.Services.AuditLogService.DicJson].ToString();

        IDictionary<string, object> fields = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);

        var panel = DataPanel;
        panel.PageState = PageState.View;
        panel.Values = fields;
        panel.Name = $"jjpainellog_{Name}";

        return panel;
    }

    private JJGridView CreateGridViewLog()
    {
        if (FormElement == null)
            throw new ArgumentNullException(nameof(FormElement));

        var gridViewFormElement = AuditLogService.GetFormElement();
        gridViewFormElement.ParentName = FormElement.ParentName;
        var grid = _componentFactory.GridView.Create(gridViewFormElement);
        grid.FormElement.Title = FormElement.Title;
        grid.SetCurrentFilter(DataManager.Services.AuditLogService.DicName, FormElement.Name);
        grid.CurrentOrder = new OrderByData().AddOrReplace(DataManager.Services.AuditLogService.DicModified,OrderByDirection.Desc);

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

        return grid;
    }

    private JJToolbar GetFormBottombar()
    {
        var btn = _componentFactory.Html.LinkButton.Create();
        btn.Type = LinkButtonType.Button;
        btn.CssClass = $"{BootstrapHelper.DefaultButton} btn-small";
        btn.OnClientClick = $"AuditLogViewHelper.viewLog('{Name}','');";
        btn.IconClass = IconType.ArrowLeft.GetCssClass();
        btn.Text = "Back";

        var toolbar = new JJToolbar();
        toolbar.Items.Add(btn.GetHtmlBuilder());
        return toolbar;
    }

    private async Task<HtmlBuilder> GetHtmlGridInfo(string recordsKey, string viewId)
    {
        var filter = new Dictionary<string, object>
        {
            { DataManager.Services.AuditLogService.DicKey, recordsKey },
            { DataManager.Services.AuditLogService.DicName, FormElement.Name }
        };

        var orderBy = new OrderByData().AddOrReplace(DataManager.Services.AuditLogService.DicModified, OrderByDirection.Desc);


        var result = await EntityRepository.GetDictionaryListResultAsync(GridView.FormElement,new EntityParameters()
        {
            Filters = filter,
            OrderBy = orderBy,
            CurrentPage = 1,
            RecordsPerPage = 1
        });

        var html = new HtmlBuilder(HtmlTag.Div);
        foreach (var row in result.Data)
        {
            string icon = "";
            string color = "";
            string action = "";
            string origem = "";

            if (row["actionType"]!.Equals((int)CommandOperation.Update))
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

            if (row["origin"]!.Equals((int)DataContextSource.Api))
                origem = DataContextSource.Api.ToString();
            else if (row["origin"].Equals((int)DataContextSource.Form))
                origem = DataContextSource.Form.ToString();
            else if (row["origin"].Equals((int)DataContextSource.Api))
                origem = DataContextSource.Upload.ToString();

            string logId = row["id"].ToString();
            string message = StringLocalizer["{0} from {1} by user:{2}", action, origem, row["userId"].ToString()];

            html.Append(HtmlTag.A, a =>
            {
                var routeContext = RouteContext.FromFormElement(FormElement, ComponentContext.AuditLogView);

                var encryptedRouteContext = EncryptionService.EncryptRouteContext(routeContext);
                
                a.WithAttribute("href", $"javascript:AuditLogViewHelper.loadAuditLog('{Name}','{logId}', '{encryptedRouteContext}')");
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
                        span.WithToolTip(row["browser"]?.ToString());

                        var infoIcon = _componentFactory.Html.Icon.Create(IconType.InfoCircle);
                        infoIcon.CssClass = "help-description";
                        span.AppendComponent(infoIcon);
                    });
                    div.Append(HtmlTag.Br);
                    div.Append(HtmlTag.B, b => { b.AppendText(row["modified"]!.ToString()); });
                    div.Append(HtmlTag.Br);
                    div.Append(HtmlTag.B, b => { b.AppendText($"IP: {row["ip"]}"); });
                });
            });
        }

        return html;
    }
}