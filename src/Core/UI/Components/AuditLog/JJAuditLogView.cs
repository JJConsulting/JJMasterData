using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JJMasterData.Commons.Data.Entity.Models;
using JJMasterData.Commons.Data.Entity.Repository;
using JJMasterData.Commons.Data.Entity.Repository.Abstractions;
using JJMasterData.Commons.Localization;
using JJMasterData.Commons.Security.Cryptography.Abstractions;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.DataManager;
using JJMasterData.Core.DataManager.Models;
using JJMasterData.Core.DataManager.Services;
using JJMasterData.Core.Extensions;
using JJMasterData.Core.Http.Abstractions;
using JJMasterData.Core.Tasks;
using JJMasterData.Core.UI.Html;
using JJMasterData.Core.UI.Routing;
using Microsoft.Extensions.Localization;
using Newtonsoft.Json;

namespace JJMasterData.Core.UI.Components;

public class JJAuditLogView : AsyncComponent
{
    private readonly IComponentFactory _componentFactory;
    private JJGridView _gridView;
    private JJDataPanel _dataPainel;
    private string _userId;
    private RouteContext _routeContext;


    private RouteContext RouteContext
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

    private ComponentContext ComponentContext => RouteContext.ComponentContext;

    /// <summary>
    /// Id do usuário Atual
    /// </summary>
    /// <remarks>
    /// Se a variavel não for atribuida diretamente,
    /// o sistema tenta recuperar em UserValues ou nas variaveis de Sessão
    /// </remarks>
    internal string UserId => _userId ??= DataHelper.GetCurrentUserId(CurrentContext, UserValues);

    private IHttpContext CurrentContext { get; }

    private AuditLogService AuditLogService { get; }
    private IEncryptionService EncryptionService { get; }

    public JJGridView GridView => _gridView ??= CreateGridViewLog();

    private JJDataPanel DataPanel
    {
        get
        {
            if (_dataPainel == null)
            {
                _dataPainel = _componentFactory.DataPanel.Create(FormElement);
                _dataPainel.Name = $"auditlogview-panel-{FormElement.ParentName}";
            }

            return _dataPainel;
        }
    }

    public FormElement FormElement { get; }
    private IStringLocalizer<MasterDataResources> StringLocalizer { get; }

    private IEntityRepository EntityRepository { get; }

    public JJAuditLogView(
        FormElement formElement,
        IHttpContext currentContext,
        IEntityRepository entityRepository,
        AuditLogService auditLogService,
        IComponentFactory componentFactory,
        IEncryptionService encryptionService,
        IStringLocalizer<MasterDataResources> stringLocalizer)
    {
        Name = $"{formElement.Name.ToLowerInvariant()}-audit-log-view";
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
        string logId = CurrentContext.Request.Form[$"audit-log-id-{FormElement.Name}"];
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

        html.AppendHiddenInput($"audit-log-id-{FormElement.Name}", logId!);

        return new RenderedComponentResult(html);
    }

    private async Task<string> GetEntryKey(Dictionary<string, object> values)
    {
        var filter = new Dictionary<string, object>
        {
            { AuditLogService.DicName, FormElement.Name },
            {
                AuditLogService.DicKey, AuditLogService.GetKey(FormElement, values)
            }
        };

        var orderBy = new OrderByData();
        orderBy.AddOrReplace(AuditLogService.DicModified, OrderByDirection.Desc);

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

    public async Task<HtmlBuilder> GetLogDetailsHtmlAsync(Dictionary<string, object> values)
    {
        string entryId = await GetEntryKey(values);
        var html = await GetLogDetailsHtmlAsync(entryId);
        html.AppendHiddenInput($"audit-log-id-{FormElement.Name}", entryId);
        return html;
    }

    private async Task<HtmlBuilder> GetLogDetailsHtmlAsync(string logId)
    {
        var html = new HtmlBuilder(HtmlTag.Div);
        html.WithCssClass("mb-2");
        if (GridView.ShowTitle)
            html.AppendComponent(GridView.GetTitle());

        if (string.IsNullOrEmpty(logId))
        {
            var alert = new JJAlert
            {
                ShowIcon = true,
                Icon = IconType.ExclamationTriangle,
                Color = BootstrapColor.Warning,
                Title = StringLocalizer["No records found."]
            };

            return alert.GetHtmlBuilder();
        }

        var filter = new Dictionary<string, object> { { AuditLogService.DicId, logId } };

        var values = await EntityRepository.GetFieldsAsync(AuditLogService.GetElement(FormElement.ConnectionId), filter);
        var json = values[AuditLogService.DicJson]?.ToString();
        var recordsKey = values[AuditLogService.DicKey]?.ToString();
        var auditLogValues = JsonConvert.DeserializeObject<Dictionary<string, object>>(json ?? string.Empty);
        
        DataPanel.PageState = PageState.View;
        DataPanel.Values = auditLogValues;
        DataPanel.Name = $"auditlogview-panel-{FormElement.ParentName}";

        var row = new HtmlBuilder(HtmlTag.Div)
            .WithCssClass("row");

        var logListGroupHtml = await GetLogListGroupHtml(recordsKey, logId);

        row.Append(HtmlTag.Div, d =>
        {
            d.WithCssClass("col-sm-3");
            d.Append(HtmlTag.Div, div =>
            {
                div.WithCssClass("jjrelative");
                div.Append(HtmlTag.Div, divFields =>
                {
                    divFields.WithCssClass("listField")
                        .AppendComponent(new JJTitle
                        {
                            Title = StringLocalizer["Change History"],
                            Size = HeadingSize.H5,
                        });
                    divFields.Append(HtmlTag.Div, group =>
                    {
                        group.WithAttribute("id", "sortable-grid");
                        group.WithCssClass("list-group jj-list-group sortable-grid");
                        group.Append(logListGroupHtml);
                    });
                });
            });
        });

        var panelHtml = await DataPanel.GetPanelHtmlBuilderAsync();

        row.Append(HtmlTag.Div, d =>
        {
            d.WithCssClass("col-sm-9");
            d.Append(HtmlTag.Div, div =>
            {
                div.WithCssClass("jjrelative");
                div.Append(HtmlTag.Div, divDetail =>
                {
                    divDetail.WithCssClass("field-details")
                        .AppendComponent(new JJTitle
                        {
                            Title = StringLocalizer["Record Snapshot"],
                            Size = HeadingSize.H5
                        });
                    divDetail.Append(panelHtml);
                });
            });
        });

        html.Append(row);
        return html;
    }

    private async Task<JJDataPanel> GetDetailsPanel(string logId)
    {
        var filter = new Dictionary<string, object> { { AuditLogService.DicId, logId } };

        var values = await EntityRepository.GetFieldsAsync(AuditLogService.GetElement(FormElement.ConnectionId), filter);
        string json = values[AuditLogService.DicJson].ToString();

        var fields = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);

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

        var gridViewFormElement = AuditLogService.GetFormElement(FormElement);
        gridViewFormElement.ParentName = FormElement.ParentName;
        var grid = _componentFactory.GridView.Create(gridViewFormElement);
        grid.FormElement.Title = FormElement.Title;
        grid.SetCurrentFilter(AuditLogService.DicName, FormElement.Name);
        if (!grid.CurrentOrder.Any())
        {
            grid.CurrentOrder.AddOrReplace(AuditLogService.DicModified, OrderByDirection.Desc);
        }
        grid.ExportAction.SetVisible(false);

        var fieldKey = grid.FormElement.Fields[AuditLogService.DicKey];
        int qtdPk = FormElement.Fields.Count(x => x.IsPk);
        if (qtdPk == 1)
        {
            var f = FormElement.Fields.First(x => x.IsPk);
            fieldKey.Label = f.Label;
            fieldKey.HelpDescription = StringLocalizer["Primary key record"];
        }
        else
        {
            fieldKey.HelpDescription = StringLocalizer["Primary key separated by semicolons"];
        }

        if (!FormElement.Options.EnableAuditLog)
        {
            grid.OnBeforeTableRenderAsync += (_, args) =>
            {
                var alert = _componentFactory.Html.Alert.Create();
                alert.Title = StringLocalizer["Warning"];
                alert.Color = BootstrapColor.Warning;
                alert.Icon = IconType.SolidTriangleExclamation;
                alert.Messages.Add(StringLocalizer["Audit Log is disabled. Please contact the administrator."]);
                args.HtmlBuilder.AppendComponent(alert);
                return ValueTaskHelper.CompletedTask;
            };
        }
        
        return grid;
    }

    private JJToolbar GetFormBottombar()
    {
        var btn = _componentFactory.Html.LinkButton.Create();
        btn.Type = LinkButtonType.Button;
        btn.CssClass = $"{BootstrapHelper.BtnDefault} btn-small";
        btn.OnClientClick = $"AuditLogViewHelper.viewAuditLog('{FormElement.ParentName}','');";
        btn.IconClass = IconType.ArrowLeft.GetCssClass();
        btn.Text = StringLocalizer["Back"];

        var toolbar = new JJToolbar();
        toolbar.Items.Add(btn.GetHtmlBuilder());
        return toolbar;
    }

    private async Task<HtmlBuilder> GetLogListGroupHtml(string recordsKey, string viewId)
    {
        var filter = new Dictionary<string, object>
        {
            { AuditLogService.DicKey, recordsKey },
            { AuditLogService.DicName, FormElement.Name }
        };

        var orderBy = new OrderByData().AddOrReplace(AuditLogService.DicModified, OrderByDirection.Desc);
        
        var result = await EntityRepository.GetDictionaryListResultAsync(GridView.FormElement, new EntityParameters
        {
            Filters = filter,
            OrderBy = orderBy,
            CurrentPage = 1,
            RecordsPerPage = int.MaxValue
        });

        var html = new HtmlBuilder();
        foreach (var row in result.Data)
        {
            string icon = "";
            string color = "";
            string action = "";
            string origem = "";

            if (row["actionType"]!.Equals((int)CommandOperation.Update))
            {
                icon = "fa fa-pencil fa-lg fa-fw";
                color = AuditLogService.GetUpdateColor();
                action = StringLocalizer["Edited"];
            }
            else if (row["actionType"].Equals((int)CommandOperation.Insert))
            {
                icon = "fa fa-plus fa-lg fa-fw";
                color = AuditLogService.GetInsertColor();
                action = StringLocalizer["Added"];
            }
            else if (row["actionType"].Equals((int)CommandOperation.Delete))
            {
                icon = "fa fa-trash fa-lg fa-fw";
                color = AuditLogService.GetDeleteColor();
                action = StringLocalizer["Deleted"];
            }

            if (row["origin"]!.Equals((int)DataContextSource.Api))
                origem = nameof(DataContextSource.Api);
            else if (row["origin"].Equals((int)DataContextSource.Form))
                origem = nameof(DataContextSource.Form);
            else if (row["origin"].Equals((int)DataContextSource.Api))
                origem = nameof(DataContextSource.Upload);

            string logId = row["id"].ToString();
            string message = $"{action} [{origem}] {row["userId"]?.ToString() ?? string.Empty}";

            html.Append(HtmlTag.A, a =>
            {
                var routeContext = RouteContext.FromFormElement(FormElement, ComponentContext.AuditLogView);

                var encryptedRouteContext = EncryptionService.EncryptObject(routeContext);

                a.WithAttribute("href",
                    $"javascript:AuditLogViewHelper.loadAuditLog('{FormElement.ParentName}','{logId}', '{encryptedRouteContext}')");
                a.WithNameAndId(logId);
                a.WithCssClass("list-group-item ui-sortable-handle");
                a.WithCssClassIf(logId!.Equals(viewId), "active");

                a.Append(HtmlTag.Div, div =>
                {
                    div.WithStyle( "height: 4.75rem;");
                    div.Append(HtmlTag.Span, span =>
                    {
                        span.WithCssClass(icon);
                        span.WithStyle( $"color:{color};");
                        span.WithToolTip(action);
                    });
                    div.Append(HtmlTag.B, b => { b.AppendText(message); });
                    div.Append(HtmlTag.Br);
                    div.Append(HtmlTag.B, b => { b.AppendText(row["modified"]!.ToString()); });
                    div.Append(HtmlTag.Br);
                    div.Append(HtmlTag.B, b =>
                    {
                        b.AppendText($"IP: {row["ip"]}");
                        var infoIcon = _componentFactory.Html.Icon.Create(IconType.InfoCircle);
                        infoIcon.CssClass = "help-description";
                        b.AppendComponent(infoIcon);
                        b.WithToolTip(row["browser"]?.ToString());
                    });
                });
            });
        }

        return html;
    }
}