#nullable enable

// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable EventNeverSubscribedTo.Global

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JJMasterData.Commons.Data.Entity.Models;
using JJMasterData.Commons.Data.Entity.Repository.Abstractions;
using JJMasterData.Commons.Exceptions;
using JJMasterData.Commons.Localization;
using JJMasterData.Commons.Security.Cryptography.Abstractions;
using JJMasterData.Commons.Tasks;
using JJMasterData.Core.Configuration.Options;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.DataDictionary.Models.Actions;
using JJMasterData.Core.DataDictionary.Repository.Abstractions;
using JJMasterData.Core.DataManager;
using JJMasterData.Core.DataManager.Expressions;
using JJMasterData.Core.DataManager.Models;
using JJMasterData.Core.DataManager.Services;
using JJMasterData.Core.Events.Abstractions;
using JJMasterData.Core.Events.Args;
using JJMasterData.Core.Extensions;
using JJMasterData.Core.Http.Abstractions;
using JJMasterData.Core.Logging;
using JJMasterData.Core.Tasks;
using JJMasterData.Core.UI.Events.Args;
using JJMasterData.Core.UI.Html;
using JJMasterData.Core.UI.Routing;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
#if NET48
using JJMasterData.Commons.Configuration;
#endif

namespace JJMasterData.Core.UI.Components;

/// <summary>
/// Represents a CRUD.
/// </summary>
/// <example>
/// [!code-cshtml[Example](../../../example/JJMasterData.WebExample/Pages/Components/JJFormViewExample.cshtml)]
/// The GetHtml method will return something like this:
/// <img src="../media/JJFormViewExample.png"/>
/// </example>
public class JJFormView : AsyncComponent
{
    #region "Events"

    public event AsyncEventHandler<FormBeforeActionEventArgs>? OnBeforeImportAsync;
    public event AsyncEventHandler<FormBeforeActionEventArgs>? OnBeforeInsertAsync;
    public event AsyncEventHandler<FormBeforeActionEventArgs>? OnBeforeUpdateAsync;
    public event AsyncEventHandler<FormBeforeActionEventArgs>? OnBeforeDeleteAsync;
    public event AsyncEventHandler<FormAfterActionEventArgs>? OnAfterInsertAsync;
    public event AsyncEventHandler<FormAfterActionEventArgs>? OnAfterUpdateAsync;
    public event AsyncEventHandler<FormAfterActionEventArgs>? OnAfterDeleteAsync;

    #endregion

    #region "Fields"

    private JJDataPanel? _dataPanel;
    private JJGridView? _gridView;
    private JJFormView? _insertSelectionFormView;
    private FormViewScripts? _scripts;
    private ActionMap? _currentActionMap;
    private BasicAction? _currentAction;
    private JJAuditLogView? _auditLogView;
    private JJDataImportation? _dataImportation;
    private string? _userId;
    private PageState? _pageState;
    private Dictionary<string, object> _relationValues = new();
    private RouteContext? _routeContext;
    private FormStateData? _formStateData;
    private bool _isCustomCurrentActionMap;
    private RelationshipType? _relationshipType;

    private readonly FormValuesService _formValuesService;
    private readonly FieldValuesService _fieldValuesService;
    private readonly HtmlTemplateService _htmlTemplateService;
    private readonly IEnumerable<IPluginHandler> _pluginHandlers;
    private readonly MasterDataCoreOptions _options;
    private readonly IStringLocalizer<MasterDataResources> _stringLocalizer;
    private readonly ILogger<JJFormView> _logger;
    private readonly IDataDictionaryRepository _dataDictionaryRepository;
    private readonly FormService _formService;

    #endregion

    #region "Properties"

    private JJAuditLogView AuditLogView
    {
        get
        {
            if (_auditLogView != null)
                return _auditLogView;

            _auditLogView = ComponentFactory.AuditLog.Create(FormElement);
            _auditLogView.FormElement.ParentName =
                RouteContext.ParentElementName ?? FormElement.ParentName ?? FormElement.Name;

            return _auditLogView;
        }
    }

    /// <summary>
    /// Url used after the events of Insert/Update/Delete
    /// </summary>
    private string? UrlRedirect { get; set; }


    /// <summary>
    /// Id of the current user.
    /// </summary>
    /// <remarks>
    /// If the value is null, the value is recovered at UserValues or HttpContext.
    /// </remarks>
    private string? UserId => _userId ??= DataHelper.GetCurrentUserId(CurrentContext, UserValues);

    /// <summary>
    /// Configurações de importação
    /// </summary>
    public JJDataImportation DataImportation
    {
        get
        {
            if (_dataImportation != null)
                return _dataImportation;

            _dataImportation = GridView.DataImportation;
            _dataImportation.OnBeforeImportAsync += OnBeforeImportAsync;
            _dataImportation.OnAfterDeleteAsync += OnAfterDeleteAsync;
            _dataImportation.OnAfterInsertAsync += OnAfterInsertAsync;
            _dataImportation.OnAfterUpdateAsync += OnAfterUpdateAsync;
            _dataImportation.RelationValues = RelationValues;

            return _dataImportation;
        }
    }

    public JJDataPanel DataPanel
    {
        get
        {
            if (_dataPanel == null)
            {
                _dataPanel = ComponentFactory.DataPanel.Create(FormElement);
                _dataPanel.AutoReloadFormFields = _dataPanel.PageState is not PageState.View || IsInsertAtGridView;
                _dataPanel.UserValues = UserValues;
                _dataPanel.RenderPanelGroup = true;

                if (IsInsertAtGridView)
                {
                    _dataPanel.Name += "_insert";
                    _dataPanel.FieldNamePrefix = "insert_";
                }

                if (IsChildFormView)
                    _dataPanel.FieldNamePrefix += $"{Name}_";

                _dataPanel.ParentComponentName = Name;
                _dataPanel.FormUI = FormElement.Options.Form;

                DataHelper.CopyIntoDictionary(_dataPanel.UserValues, RelationValues!);
            }

            return _dataPanel;
        }
    }

    /// <summary>
    /// Values to be replaced by relationship.
    /// If the field name exists in the relationship, the value will be replaced
    /// </summary>
    /// <remarks>
    /// Key = Field name, Value=Field value
    /// </remarks>
    public Dictionary<string, object> RelationValues
    {
        get
        {
            if (_relationValues.Count == 0)
            {
                _relationValues = GetRelationValuesFromForm();
            }

            return _relationValues;
        }
        set
        {
            _relationValues = value;
            GridView.RelationValues = _relationValues;
        }
    }

    public FormElement FormElement { get; }

    public JJGridView GridView
    {
        get
        {
            if (_gridView is not null)
                return _gridView;

            _gridView = ComponentFactory.GridView.Create(FormElement);
            _gridView.Name = Name;
            _gridView.ParentComponentName = Name;
            _gridView.FormElement = FormElement;
            _gridView.UserValues = UserValues;
            _gridView.ShowTitle = ShowTitle;
            _gridView.TitleActions = TitleActions;

            if (_gridView.InsertAction.InsertActionLocation is InsertActionLocation.AboveGrid)
                _gridView.OnBeforeTableRenderAsync += RenderInsertActionAtGrid;

            if (_gridView.InsertAction.InsertActionLocation is InsertActionLocation.BelowGrid)
                _gridView.OnAfterTableRenderAsync += RenderInsertActionAtGrid;

            return _gridView;
        }
    }

    private async ValueTask RenderInsertActionAtGrid(object _, GridRenderEventArgs args)
    {
        var insertAction = GridView.InsertAction;
        var formStateData = await GridView.GetFormStateDataAsync();
        var insertActionVisible = ExpressionsService.GetBoolValue(insertAction.VisibleExpression, formStateData);
        var insertActionEnabled = ExpressionsService.GetBoolValue(insertAction.EnableExpression, formStateData);
        if (!insertActionVisible || !insertActionEnabled)
            return;

        DataPanel.PageState = PageState.Insert;

        var result = await GetFormResult(formStateData.Values, DataPanel.Errors, PageState.Insert, true);

        if (result is HtmlComponentResult htmlComponentResult)
            args.HtmlBuilder.Append(htmlComponentResult.HtmlBuilder);

        if (CurrentAction is SaveAction && PageState == PageState.List && DataPanel.Errors.Count is 0)
            AppendInsertSuccessAlert(args.HtmlBuilder);
    }

    public PageState PageState
    {
        get
        {
            if (_pageState is null &&
                CurrentContext.Request.Form.TryGetValue($"form-view-page-state-{Name}", out var formPageState))
                _pageState = (PageState)int.Parse(formPageState);

            if (_pageState is null &&
                CurrentContext.Request.QueryString.TryGetValue($"{FormElement.Name}_PageState",
                    out var queryStringPageState))
            {
                if (Enum.TryParse(queryStringPageState, ignoreCase: true, out PageState pageState))
                    _pageState = pageState;
            }

            return _pageState ?? PageState.List;
        }
        internal set => _pageState = value;
    }

    internal ActionMap? CurrentActionMap
    {
        get
        {
            if (_currentActionMap != null || _isCustomCurrentActionMap)
                return _currentActionMap;

            var encryptedActionMap = CurrentContext.Request.Form[$"current-action-map-{Name}"];
            if (string.IsNullOrEmpty(encryptedActionMap))
                return null;

            _currentActionMap = EncryptionService.DecryptActionMap(encryptedActionMap);

            return _currentActionMap;
        }
        set
        {
            _isCustomCurrentActionMap = true;
            _currentActionMap = value;
        }
    }

    internal BasicAction? CurrentAction
    {
        get
        {
            if (_currentAction != null || _isCustomCurrentActionMap)
                return _currentAction;

            if (CurrentActionMap is null)
                return null;

            _currentAction = CurrentActionMap.GetAction(FormElement);
            return _currentAction;
        }
    }

    public RouteContext RouteContext
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

    public bool ShowTitle { get; set; }

    public List<TitleAction>? TitleActions { get; set; }

    internal ComponentContext ComponentContext => RouteContext.IsCurrentFormElement(FormElement.Name)
        ? RouteContext.ComponentContext
        : default;

    internal FormViewScripts Scripts => _scripts ??= new(this);

    internal bool IsChildFormView => RelationshipType is not RelationshipType.Parent;

    internal RelationshipType RelationshipType
    {
        get
        {
            if (_relationshipType is null &&
                CurrentContext.Request.Form.TryGetValue($"form-view-relationship-type-{Name}",
                    out var relationshipType))
                _relationshipType = (RelationshipType)int.Parse(relationshipType);

            return _relationshipType ?? RelationshipType.Parent;
        }
        set => _relationshipType = value;
    }

    internal bool IsInsertAtGridView => PageState is PageState.List &&
                                        FormElement.Options.GridToolbarActions.InsertAction.ShowOpenedAtGrid;

    internal IHttpContext CurrentContext { get; }
    internal IFormValues FormValues => CurrentContext.Request.Form;
    internal IEncryptionService EncryptionService { get; }
    internal IComponentFactory ComponentFactory { get; }
    internal IEntityRepository EntityRepository { get; }
    internal ExpressionsService ExpressionsService { get; }

    #endregion

    #region "Constructors"

    public JJFormView(
        FormElement formElement,
        IHttpContext currentContext,
        IEntityRepository entityRepository,
        IDataDictionaryRepository dataDictionaryRepository,
        FormService formService,
        IEncryptionService encryptionService,
        FormValuesService formValuesService,
        FieldValuesService fieldValuesService,
        ExpressionsService expressionsService,
        HtmlTemplateService htmlTemplateService,
        IEnumerable<IPluginHandler> pluginHandlers,
        IOptionsSnapshot<MasterDataCoreOptions> options,
        IStringLocalizer<MasterDataResources> stringLocalizer,
        ILogger<JJFormView> logger,
        IComponentFactory componentFactory)
    {
        Name = formElement.Name.ToLowerInvariant();
        FormElement = formElement;
        ShowTitle = formElement.Options.Grid.ShowTitle;

        CurrentContext = currentContext;
        EntityRepository = entityRepository;
        EncryptionService = encryptionService;
        ExpressionsService = expressionsService;
        ComponentFactory = componentFactory;

        _formService = formService;
        _formValuesService = formValuesService;
        _fieldValuesService = fieldValuesService;
        _htmlTemplateService = htmlTemplateService;
        _pluginHandlers = pluginHandlers;
        _options = options.Value;
        _stringLocalizer = stringLocalizer;
        _logger = logger;
        _dataDictionaryRepository = dataDictionaryRepository;
    }

    #endregion

    protected override async Task<ComponentResult> BuildResultAsync()
    {
        if (!RouteContext.CanRender(FormElement))
            return EmptyComponentResult.Value;

        if (RouteContext.IsCurrentFormElement(FormElement.Name))
            return await GetFormResultAsync();

        if (RouteContext.ElementName == _options.AuditLogTableName)
            return await AuditLogView.GetResultAsync();

        var childFormView = await GetChildFormView();

        return await childFormView.GetFormResultAsync();
    }

    private async Task<JJFormView> GetChildFormView()
    {
        var childFormView = await ComponentFactory.FormView.CreateAsync(RouteContext.ElementName);


        childFormView.FormElement.ParentName = RouteContext.ParentElementName;
        childFormView.UserValues = UserValues;
        childFormView.RelationValues = childFormView.GetRelationValuesFromForm();

        var isInsertSelection = PageState is PageState.Insert &&
                                GridView.ToolbarActions.InsertAction.ElementNameToSelect ==
                                childFormView.FormElement.Name;

        childFormView.ShowTitle = isInsertSelection;

        var panelState = DataPanel.PageState;

        if (PageState is PageState.View || panelState is PageState.Insert || panelState is PageState.Update)
            childFormView.DisableActionsAtViewMode();

        if (!isInsertSelection)
            return childFormView;

        childFormView.GridView.TableActions.Add(new InsertSelectionAction());
        childFormView.GridView.ToolbarActions.Add(GetInsertSelectionBackAction());
        childFormView.GridView.OnRenderActionAsync += InsertSelectionOnRenderAction;

        return childFormView;
    }


    internal async Task<ComponentResult> GetFormResultAsync()
    {
        switch (ComponentContext)
        {
            case ComponentContext.TextFileUploadView:
            case ComponentContext.TextFileFileUpload:
            case ComponentContext.SearchBox:
                return await DataPanel.GetResultAsync();
            case ComponentContext.LookupDescription:
                return await DataPanel.GetResultAsync();
            case ComponentContext.UrlRedirect:
                return await DataPanel.GetUrlRedirectResult(CurrentActionMap);
            case ComponentContext.DataPanelReload:
                return await GetReloadPanelResultAsync();
            case ComponentContext.DataExportation:
            case ComponentContext.GridViewReload:
            case ComponentContext.GridViewFilterReload:
            case ComponentContext.SearchBoxFilter:
                return await GridView.GetResultAsync();
            case ComponentContext.DownloadFile:
                return ComponentFactory.Downloader.Create().GetDownloadResult();
            case ComponentContext.AuditLogView:
                return await AuditLogView.GetResultAsync();
            case ComponentContext.DataImportation or ComponentContext.DataImportationFileUpload:
                return await GetImportationResult();
            case ComponentContext.InsertSelection:
                return await GetInsertSelectionResult();
            default:
                return await GetFormActionResult();
        }
    }

    internal async Task<ComponentResult> GetReloadPanelResultAsync()
    {
        var filter = GridView.GetSelectedRowId();
        Dictionary<string, object?>? values;
        if (filter is { Count: > 0 })
            values = await EntityRepository.GetFieldsAsync(FormElement, filter);
        else
            values = await GetFormValuesAsync();

        var fieldName = CurrentContext.Request.QueryString["fieldName"];

        var field = FormElement.Fields[fieldName];

        var scripts = new HtmlBuilder();

        var formStateData = new FormStateData
        {
            Values = values,
            UserValues = UserValues,
            PageState = PageState
        };

        foreach (var action in field.Actions)
        {
            if (action is not PluginFieldAction { TriggerOnChange: true } pluginAction)
                continue;

            var result = await GetPluginActionResult(pluginAction, formStateData, fieldName);

            DataPanel.AutoReloadFormFields = false;

            if (result.JsCallback is not null)
                scripts.AppendScript(result.JsCallback);
        }

        DataPanel.Values = values;

        var dataPanelResult = await DataPanel.GetResultAsync();

        if (dataPanelResult is HtmlComponentResult htmlComponentResult)
        {
            htmlComponentResult.HtmlBuilder.Append(scripts);
        }

        return dataPanelResult;
    }


    private async Task<ComponentResult> GetSaveActionResult()
    {
        var insertAction = GridView.ToolbarActions.InsertAction;
        var values = await GetFormValuesAsync();

        Dictionary<string, string> errors;
        if (PageState is PageState.Insert || IsInsertAtGridView)
            errors = await InsertFormValuesAsync(values);
        else
            errors = await UpdateFormValuesAsync(values);

        DataPanel.Errors = errors;

        if (errors.Count != 0 && !IsInsertAtGridView)
            return await GetFormResult(values, errors, PageState, true);

        if (!string.IsNullOrEmpty(UrlRedirect))
        {
            CurrentActionMap = null;
            return new RedirectComponentResult(UrlRedirect!);
        }

        if (PageState is PageState.Insert && insertAction.ReopenForm)
        {
            var formResult =
                await GetFormResult(new Dictionary<string, object?>(RelationValues!), PageState.Insert, false);

            if (formResult is HtmlComponentResult htmlComponent)
            {
                AppendInsertSuccessAlert(htmlComponent.HtmlBuilder);

                return htmlComponent;
            }

            return formResult;
        }

        var isAtRelationship =
            IsChildFormView && RelationshipType is not RelationshipType.OneToMany ||
            (ContainsRelationshipLayout(new FormStateData(values, PageState)) && DataPanel.ContainsPanelState());

        if (isAtRelationship)
        {
            DataPanel.PageState = PageState.View;
            return await GetFormResult(values, PageState.View, false);
        }

        if (PageState is PageState.Insert)
        {
            var visibleRelationships = GetVisibleRelationships(values, PageState.Update);

            if (visibleRelationships.Count > 0)
            {
                PageState = PageState.Update;
                return await GetFormResult(values, PageState.View, false);
            }
        }

        if (DataPanel.IsAtModal)
            return CloseModal();

        PageState = PageState.List;
        CurrentActionMap = null;

        return await GridView.GetResultAsync();
    }

    private static JsonComponentResult CloseModal() => new(new { closeModal = true });

    private void AppendInsertSuccessAlert(HtmlBuilder htmlBuilder)
    {
        var alert = new JJAlert
        {
            Name = $"insert-alert-{Name}",
            Color = BootstrapColor.Success,
            Title = _stringLocalizer[GridView.InsertAction.SuccessMessage],
            ShowIcon = true,
            Icon = IconType.CheckCircleO
        };

        htmlBuilder.Append(HtmlTag.Div, div =>
        {
            div.WithAttribute("id", $"insert-alert-div-{Name}")
                .WithCssClass("fade-out")
                .AppendComponent(alert);
        });
        htmlBuilder.AppendScript(Scripts.GetShowInsertSuccessScript());
    }

    private Task<ComponentResult> GetCancelActionResult()
    {
        PageState = PageState.List;

        ClearTempFiles();

        return GridView.GetResultAsync();
    }

    private Task<ComponentResult> GetBackActionResult()
    {
        PageState = PageState.List;
        return GridView.GetResultAsync();
    }

    private async Task<ComponentResult> GetFormActionResult()
    {
        SetFormServiceEvents();

        var result = CurrentAction switch
        {
            ViewAction => await GetViewResult(),
            EditAction => await GetUpdateResult(),
            InsertAction => await GetInsertResult(),
            AuditLogFormToolbarAction or AuditLogGridToolbarAction => await GetAuditLogResult(),
            DeleteAction => await GetDeleteResult(),
            SaveAction => await GetSaveActionResult(),
            BackAction => await GetBackActionResult(),
            CancelAction => await GetCancelActionResult(),
            SqlCommandAction => await GetSqlCommandActionResult(),
            HtmlTemplateAction => await GetHtmlTemplateActionResult(),
            PluginAction => await GetPluginActionResult(),
            _ => await GetDefaultResult()
        };

        if (result is not HtmlComponentResult htmlComponent)
            return result;

        var html = htmlComponent.HtmlBuilder;

        html.WithNameAndId(Name);

        AppendFormViewHiddenInputs(html);

        if (ComponentContext is ComponentContext.FormViewReload)
        {
            return new ContentComponentResult(html);
        }

        return result;
    }

    private void AppendFormViewHiddenInputs(HtmlBuilder html)
    {
        if (CurrentAction is not IModalAction { ShowAsModal: true } && (_dataPanel is null || !DataPanel.IsAtModal))
        {
            html.AppendHiddenInput($"form-view-page-state-{Name}", ((int)PageState).ToString());

            html.AppendHiddenInput($"form-view-relationship-type-{Name}", ((int)RelationshipType).ToString());


            html.AppendHiddenInput($"current-action-map-{Name}",
                EncryptionService.EncryptObject(CurrentActionMap));
            html.AppendHiddenInput($"form-view-relation-values-{FormElement.Name}",
                EncryptionService.EncryptObject(RelationValues));
            html.AppendHiddenInput($"form-view-route-context-{Name}",
                EncryptionService.EncryptObject(RouteContext.FromFormElement(FormElement,
                    ComponentContext.FormViewReload)));
        }
    }

    private async Task<ComponentResult> GetHtmlTemplateActionResult()
    {
        var htmlTemplateAction = (HtmlTemplateAction)CurrentAction!;

        var html = await _htmlTemplateService.RenderTemplate(
            htmlTemplateAction,
            FormElement.ConnectionId,
            CurrentActionMap!.PkFieldValues);

        return new ContentComponentResult(html);
    }

    private async Task<ComponentResult> GetSqlCommandActionResult()
    {
        JJMessageBox? messageBox = null;
        var sqlAction = CurrentActionMap?.GetAction<SqlCommandAction>(FormElement);

        if (sqlAction is null)
            throw new JJMasterDataException("Action not found at your FormElement");

        var formStateData = await GetFormStateDataAsync();
        var parsedValues = ExpressionsService.ParseExpression(sqlAction.SqlCommand, formStateData);
        var sqlCommand = ExpressionDataAccessCommandFactory.Create(sqlAction.SqlCommand, parsedValues);

        try
        {
            await EntityRepository.SetCommandAsync(sqlCommand, FormElement.ConnectionId);
        }
        catch (Exception ex)
        {
            _logger.LogSqlActionException(ex, sqlCommand.Sql);
            var message = _stringLocalizer[ExceptionManager.GetMessage(ex)];
            messageBox = ComponentFactory.Html.MessageBox.Create(message, MessageIcon.Error);
        }

        //When the action is from the form toolbar
        if (CurrentAction!.Location is not null)
            DataPanel.Values = await EntityRepository.GetFieldsAsync(FormElement, CurrentActionMap!.PkFieldValues);

        if (messageBox is null)
        {
            CurrentActionMap = null;
            if (!string.IsNullOrEmpty(sqlAction.RedirectUrl))
            {
                return new RedirectComponentResult(sqlAction.RedirectUrl!);
            }
        }

        var result = await GetDefaultResult();

        if (result is HtmlComponentResult htmlComponentResult)
        {
            htmlComponentResult.HtmlBuilder.AppendComponentIf(messageBox is not null, () => messageBox);
        }

        return result;
    }

    private async Task<ComponentResult> GetPluginActionResult()
    {
        var currentAction = (PluginAction)CurrentAction!;
        var formStateData = await GetFormStateDataAsync(currentAction is PluginFieldAction);

        PluginActionResult result;

        try
        {
            result = await GetPluginActionResult(currentAction, formStateData, CurrentActionMap!.FieldName);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Error while executing Plugin Action.");
            result = PluginActionResult.Error(_stringLocalizer["Error"], exception.Message);
        }

        var formResult = await GetDefaultResult(formStateData.Values);

        if (formResult is HtmlComponentResult htmlComponentResult)
        {
            htmlComponentResult.HtmlBuilder.AppendScriptIf(!string.IsNullOrEmpty(result.JsCallback),
                result.JsCallback!);
        }

        return formResult;
    }

    private async Task<PluginActionResult> GetPluginActionResult(PluginAction pluginAction,
        FormStateData formStateData, string? fieldName)
    {
        var pluginHandler = _pluginHandlers.First(p => p.Id == pluginAction.PluginId);

        switch (pluginHandler)
        {
            case IPluginActionHandler pluginActionHandler:
            {
                var context = new PluginActionContext
                {
                    ActionContext = GetActionContext(pluginAction, formStateData),
                    ConfigurationMap = pluginAction.ConfigurationMap
                };

                var result = await pluginActionHandler.ExecuteActionAsync(context);

                if (context.SecretValues.Count > 0)
                    SetSecretValues(context.SecretValues);

                return result;
            }
            case IPluginFieldActionHandler pluginFieldActionHandler:
            {
                var context = new PluginFieldActionContext
                {
                    ActionContext = GetActionContext(pluginAction,
                        formStateData, fieldName),
                    FieldMap = ((PluginFieldAction)pluginAction).FieldMap,
                    ConfigurationMap = pluginAction.ConfigurationMap
                };

                var result = await pluginFieldActionHandler.ExecuteActionAsync(context);

                if (context.SecretValues.Count > 0)
                    SetSecretValues(context.SecretValues);

                return result;
            }
            default:
                throw new JJMasterDataException("Invalid plugin handler");
        }
    }

    private void SetSecretValues(Dictionary<string, object?> secretValues)
    {
        DataPanel.SecretValues ??= new();
        DataHelper.CopyIntoDictionary(DataPanel.SecretValues, secretValues, true);
    }

    private void SetFormServiceEvents()
    {
        _formService.OnBeforeInsertAsync += OnBeforeInsertAsync;
        _formService.OnBeforeDeleteAsync += OnBeforeDeleteAsync;
        _formService.OnBeforeUpdateAsync += OnBeforeUpdateAsync;

        _formService.OnAfterInsertAsync += OnAfterInsertAsync;
        _formService.OnAfterUpdateAsync += OnAfterUpdateAsync;
        _formService.OnAfterDeleteAsync += OnAfterDeleteAsync;
    }

    private Task<ComponentResult> GetGridViewResult()
    {
        return GridView.GetResultAsync();
    }

    private bool ContainsHiddenPkValues()
    {
        return CurrentContext.Request.Form[$"data-panel-pk-values-{FormElement.Name}"] is not null;
    }

    private async Task<ComponentResult> GetUpdateResult()
    {
        bool autoReloadFields;
        Dictionary<string, object?>? values;
        if (PageState is PageState.Update && ContainsHiddenPkValues())
        {
            autoReloadFields = true;
            values = await GetFormValuesAsync();
        }
        else
        {
            autoReloadFields = false;

            var filters = await GetFiltersWithDefaultValues(PageState.Update);

            var letter = await _formService.GetAsync(FormElement, filters);
            values = letter.Result!;
            var errors = letter.Errors;
            if (errors.Count > 0)
            {
                return await GetGridResultWithErrors(errors);
            }
        }

        PageState = PageState.Update;
        return await GetFormResult(values, PageState, autoReloadFields);
    }

    private async Task<RenderedComponentResult> GetGridResultWithErrors(Dictionary<string, string> errors)
    {
        var messageFactory = ComponentFactory.Html.MessageBox;
        var html = new HtmlBuilder(HtmlTag.Div);
        html.AppendComponent(messageFactory.Create(errors, MessageIcon.Warning));
        html.Append(await GridView.GetHtmlBuilderAsync());
        PageState = PageState.List;
        return new RenderedComponentResult(html);
    }

    private async Task<Dictionary<string, object>> GetFiltersWithDefaultValues(PageState pageState)
    {
        var fieldValuesService = GridView.FieldValuesService;

        var formStateData = new FormStateData
        {
            Values = CurrentActionMap!.PkFieldValues!,
            UserValues = UserValues,
            PageState = pageState
        };

        var filters = new Dictionary<string, object?>(formStateData.Values);

        var defaultValues = await fieldValuesService.GetDefaultValuesAsync(
            FormElement,
            formStateData,
            allowSqlValues: false);
        
        DataHelper.CopyIntoDictionary(filters, defaultValues, true);

        DataHelper.RemoveNullValues(filters);

        return filters!;
    }

    private async Task<ComponentResult> GetDefaultResult(Dictionary<string, object?>? formValues = null)
    {
        var containsGridAction = false;
        if (GridView.TableActions.Any(a => a is InsertSelectionAction))
        {
            containsGridAction = !string.IsNullOrEmpty(CurrentContext.Request.Form[$"grid-view-action-map-{Name}"]);
        }

        if (PageState is PageState.List || containsGridAction)
            return await GetGridViewResult();

        switch (PageState)
        {
            case PageState.Insert:
            {
                return await GetInsertResult(formValues);
            }
            case PageState.Update or PageState.View:
            {
                formValues ??= await GetFormValuesAsync();
                var reloadFields = IsReloadFields();
                return await GetFormResult(formValues, PageState, reloadFields);
            }
            default:
                return await GetGridViewResult();
        }
    }

    private bool IsReloadFields()
    {
        return CurrentContext.Request.Form.ContainsFormValues() &&
               DataPanel.PageState is not PageState.View &&
               CurrentAction is not PluginAction;
    }

    private async Task<ComponentResult> GetInsertResult(Dictionary<string, object?>? formValues = null)
    {
        var insertAction = GridView.ToolbarActions.InsertAction;
        var formData = new FormStateData(RelationValues!, UserValues, PageState.List);
        var isInsertSelection = !string.IsNullOrEmpty(insertAction.ElementNameToSelect);

        var isVisible = ExpressionsService.GetBoolValue(insertAction.VisibleExpression, formData);

        if (!isVisible)
            throw new UnauthorizedAccessException(_stringLocalizer["Insert action is not enabled"]);

        if (isInsertSelection)
        {
            var insertSelectionFormView = await GetInsertSelectionFormView();
            if (insertSelectionFormView.GridView.HasAction() || PageState is PageState.Insert)
            {
                return await GetInsertSelectionListResult();
            }
        }

        formValues ??= await GetFormValuesAsync();

        var reloadFields = DataPanel.PageState is not PageState.View && CurrentAction is not PluginAction;

        if (PageState == PageState.Insert)
        {
            DataHelper.CopyIntoDictionary(formValues, RelationValues!);

            return await GetFormResult(formValues, PageState.Insert, reloadFields);
        }

        PageState = PageState.Insert;

        if (isInsertSelection)
            return await GetInsertSelectionListResult();

        return await GetFormResult(formValues, PageState.Insert, reloadFields);
    }

    private async Task<ComponentResult> GetInsertSelectionListResult()
    {
        var formView = await GetInsertSelectionFormView();
        var html = new HtmlBuilder(HtmlTag.Div);
        html.AppendHiddenInput($"form-view-insert-selection-values-{Name}");

        var result = await formView.GetFormResultAsync();

        CurrentActionMap = null;

        if (result is HtmlComponentResult htmlComponentResult)
        {
            html.Append(htmlComponentResult.HtmlBuilder);
        }
        else
        {
            return result;
        }

        return new RenderedComponentResult(html);
    }

    private async ValueTask<JJFormView> GetInsertSelectionFormView()
    {
        if (_insertSelectionFormView != null)
            return _insertSelectionFormView;

        _insertSelectionFormView =
            await ComponentFactory.FormView.CreateAsync(GridView.InsertAction.ElementNameToSelect);
        _insertSelectionFormView.FormElement.ParentName = FormElement.Name;
        _insertSelectionFormView.UserValues = UserValues;
        _insertSelectionFormView.GridView.OnRenderActionAsync += InsertSelectionOnRenderAction;
        _insertSelectionFormView.GridView.ToolbarActions.Add(GetInsertSelectionBackAction());
        _insertSelectionFormView.GridView.TableActions.Add(new InsertSelectionAction());

        return _insertSelectionFormView;
    }

    private ScriptAction GetInsertSelectionBackAction()
    {
        return new ScriptAction
        {
            Name = "back-action",
            Icon = IconType.ArrowLeft,
            Text = _stringLocalizer["Back"],
            ShowAsButton = true,
            OnClientClick = Scripts.GetSetPageStateScript(PageState.List),
            IsDefaultOption = true
        };
    }

    private async Task<ComponentResult> GetInsertSelectionResult()
    {
        var insertValues = EncryptionService.DecryptDictionary(FormValues[$"form-view-insert-selection-values-{Name}"]);
        var html = new HtmlBuilder(HtmlTag.Div);

        var childElementName = GridView.ToolbarActions.InsertAction.ElementNameToSelect;
        var childElement = await _dataDictionaryRepository.GetFormElementAsync(childElementName);

        var selectionValues = await EntityRepository.GetFieldsAsync(childElement, insertValues);

        var mappedFkValues = DataHelper.GetRelationValues(FormElement, selectionValues, true);

        var values =
            await _fieldValuesService.MergeWithExpressionValuesAsync(FormElement,
                new FormStateData(mappedFkValues!, UserValues, PageState.Insert));

        var errors = await InsertFormValuesAsync(values, false);

        if (errors.Count > 0)
        {
            html.AppendComponent(ComponentFactory.Html.MessageBox.Create(errors, MessageIcon.Warning));
            var insertSelectionResult = await GetInsertSelectionListResult();

            if (insertSelectionResult is RenderedComponentResult renderedComponentResult)
            {
                html.Append((HtmlBuilder?)renderedComponentResult.HtmlBuilder);
            }
            else
            {
                return insertSelectionResult;
            }

            PageState = PageState.Insert;
        }
        else
        {
            var pks = DataHelper.GetPkValues(FormElement, values);

            var dbValues = await EntityRepository.GetFieldsAsync(FormElement, pks);

            DataHelper.CopyIntoDictionary(values, dbValues);

            PageState = PageState.Update;

            var result = await GetFormResult(values, PageState, false);

            if (result is RenderedComponentResult renderedComponentResult)
            {
                html.Append((HtmlBuilder?)renderedComponentResult.HtmlBuilder);
            }
            else
            {
                return result;
            }
        }

        AppendFormViewHiddenInputs(html);

        html.WithId(Name);

        return new ContentComponentResult(html);
    }

    private async Task<ComponentResult> GetViewResult()
    {
        if (CurrentActionMap == null)
        {
            PageState = PageState.List;
            return await GetGridViewResult();
        }

        PageState = PageState.View;
        var filters = await GetFiltersWithDefaultValues(PageState.View);

        var letter = await _formService.GetAsync(FormElement, filters);
        var values = letter.Result!;
        var errors = letter.Errors;
        if (errors.Count > 0)
        {
            return await GetGridResultWithErrors(errors);
        }
        
        return await GetFormResult(values, PageState, false);
    }

    private async Task<ComponentResult> GetDeleteResult()
    {
        var html = new HtmlBuilder(HtmlTag.Div);
        var messageFactory = ComponentFactory.Html.MessageBox;
  
        var filters = await GetFiltersWithDefaultValues(PageState.Delete);

        var errors = await DeleteFormValuesAsync(filters!);
        if (errors.Count > 0)
        {
            html.AppendComponent(messageFactory.Create(errors, MessageIcon.Warning));
        }
        else
        {
            if (GridView.EnableMultiSelect)
                GridView.ClearSelectedGridValues();
        }
        
        if (!string.IsNullOrEmpty(UrlRedirect))
        {
            return new RedirectComponentResult(UrlRedirect!);
        }

        html.Append(await GridView.GetHtmlBuilderAsync());
        PageState = PageState.List;

        return new RenderedComponentResult(html);
    }

    private async Task<ComponentResult> GetAuditLogResult()
    {
        var actionMap = _currentActionMap;
        var script = new StringBuilder();
        script.Append($"setPageState('{Name}',{(int)PageState.List});");
        script.Append($"document.getElementById('current-action-map-{Name}').value = null; ");
        script.AppendLine("getMasterDataForm().submit(); ");

        var goBackAction = new ScriptAction
        {
            Name = "go-back-action",
            Icon = IconType.Backward,
            ShowAsButton = true,
            Text = _stringLocalizer["Back"],
            OnClientClick = script.ToString()
        };

        if (PageState == PageState.View)
        {
            var html = new HtmlBuilder(HtmlTag.Div);

            var logDetailsHtml = await AuditLogView.GetLogDetailsHtmlAsync(actionMap?.PkFieldValues);

            html.Append(logDetailsHtml);

            if (actionMap?.PkFieldValues != null)
                html.AppendComponent(await GetAuditLogBottomBar());

            return new ContentComponentResult(html);
        }

        AuditLogView.GridView.AddToolbarAction(goBackAction);

        return await AuditLogView.GetResultAsync();
    }

    private async Task<ComponentResult> GetImportationResult()
    {
        var action = GridView.ImportAction;
        var formStateData = await GridView.GetFormStateDataAsync();
        bool isVisible = ExpressionsService.GetBoolValue(action.VisibleExpression, formStateData);
        if (!isVisible)
            throw new UnauthorizedAccessException(_stringLocalizer["Import action not enabled"]);

        var html = new HtmlBuilder(HtmlTag.Div);

        if (ShowTitle)
            html.AppendComponent(GetTitle(new FormStateData(UserValues, PageState.Import)));

        PageState = PageState.Import;

        DataImportation.UserValues = UserValues;
        DataImportation.ProcessOptions = action.ProcessOptions;
        DataImportation.EnableAuditLog = ExpressionsService.GetBoolValue(
            GridView.ToolbarActions.AuditLogGridToolbarAction.VisibleExpression, formStateData);

        var result = await DataImportation.GetResultAsync();

        if (result is RenderedComponentResult renderedComponentResult)
        {
            html.Append((HtmlBuilder?)renderedComponentResult.HtmlBuilder);
        }
        else
        {
            return result;
        }


        return new RenderedComponentResult(html);
    }

    private Task<ComponentResult> GetFormResult(Dictionary<string, object?> values, PageState pageState,
        bool autoReloadFormFields)
    {
        return GetFormResult(values, new(), pageState, autoReloadFormFields);
    }

    private async Task<ComponentResult> GetFormResult(Dictionary<string, object?> values, Dictionary<string, string> errors,
        PageState pageState, bool autoReloadFormFields)
    {
        var visibleRelationships = GetVisibleRelationships(values, pageState);
        var containsRelationshipLayout = ContainsRelationshipLayout(visibleRelationships);

        if (!containsRelationshipLayout && !DataPanel.HasCustomPanelState &&
            RelationshipType is not RelationshipType.OneToOne)
        {
            DataPanel.PageState = pageState;
        }

        DataPanel.Errors = errors;
        DataPanel.Values = values;
        DataPanel.AutoReloadFormFields = autoReloadFormFields;
        DataPanel.IsAtModal = CurrentAction is IModalAction { ShowAsModal: true };

        if (!containsRelationshipLayout)
            return await GetDataPanelResult();

        var relationshipResult = await GetRelationshipLayoutResult(visibleRelationships, values);

        if (relationshipResult is not HtmlComponentResult htmlComponentResult)
            return relationshipResult;
        
        var parentIsHidden = !visibleRelationships.Any(r => r.IsParent);

        if (parentIsHidden)
            DataPanel.AppendHiddenInputs(htmlComponentResult.HtmlBuilder);
        
        return relationshipResult;
    }
    
    internal async Task<ComponentResult> GetRelationshipLayoutResult(
        List<FormElementRelationship> visibleRelationships,
        Dictionary<string, object?> values)
    {
        var formStateData = new FormStateData(values, UserValues, PageState);
        var html = new HtmlBuilder(HtmlTag.Div);
        if (ShowTitle)
            html.AppendComponent(GetTitle(formStateData));

        var layout = new FormViewRelationshipLayout(this, visibleRelationships);

        var relationshipsResult = await layout.GetRelationshipsResult();

        if (relationshipsResult is HtmlComponentResult htmlResult)
        {
            var topActions = GetTopToolbarActions(FormElement);

            html.PrependComponent(await GetFormToolbarAsync(topActions));

            html.Append(htmlResult.HtmlBuilder);
            var toolbarActions = FormElement.Options.FormToolbarActions;

            var bottomActions =
                toolbarActions.FindAll(a => a.Location is FormToolbarActionLocation.Bottom);

            if (!IsChildFormView)
                toolbarActions.BackAction.SetVisible(true);

            html.AppendComponent(await GetFormToolbarAsync(bottomActions));

            return new RenderedComponentResult(html);
        }

        return relationshipsResult;
    }

    private void ConfigureFormToolbar()
    {
        var formToolbarActions = FormElement.Options.FormToolbarActions;
        var panelState = DataPanel.PageState;

        if (panelState is PageState.View)
        {
            formToolbarActions.CancelAction.SetVisible(false);
            formToolbarActions.SaveAction.SetVisible(false);
        }

        switch (PageState)
        {
            case PageState.Insert when panelState is PageState.Insert:
                formToolbarActions.BackAction.SetVisible(false);
                if (RelationshipType is RelationshipType.OneToOne)
                    formToolbarActions.CancelAction.SetVisible(false);
                break;
            case PageState.Insert when IsInsertAtGridView:
                formToolbarActions.CancelAction.SetVisible(false);
                break;
            case PageState.Update when panelState is PageState.View:
                formToolbarActions.FormEditAction.SetVisible(true);
                formToolbarActions.SaveAction.SetVisible(false);
                formToolbarActions.CancelAction.SetVisible(false);
                break;
            case PageState.Update:
                formToolbarActions.FormEditAction.SetVisible(false);
                formToolbarActions.BackAction.SetVisible(false);
                break;
            case PageState.View when RelationshipType is RelationshipType.OneToOne:
            {
                formToolbarActions.BackAction.SetVisible(false);
                break;
            }
        }
    }

    private async Task<ComponentResult> GetDataPanelResult()
    {
        var panelHtml = await GetDataPanelHtml();
        panelHtml.AppendScript($"setPageState('{Name}',{(int)PageState})");


        if (ShowTitle && !IsInsertAtGridView)
            panelHtml.PrependComponent(GetTitle(new FormStateData(DataPanel.Values, UserValues, PageState)));

        return new RenderedComponentResult(panelHtml);
    }

    private List<FormElementRelationship> GetVisibleRelationships(
        Dictionary<string, object?> values,
        PageState pageState)
    {
        var formStateData = new FormStateData(values, pageState);
        var visibleRelationships = FormElement
            .Relationships
            .FindAll(r => (r.ViewType != RelationshipViewType.None || r.IsParent) &&
                          ExpressionsService.GetBoolValue(r.Panel.VisibleExpression, formStateData));
        return visibleRelationships;
    }

    internal bool ContainsRelationshipLayout(FormStateData formStateData)
    {
        var visibleRelationships = GetVisibleRelationships(formStateData.Values, formStateData.PageState);
        return ContainsRelationshipLayout(visibleRelationships);
    }

    private static bool ContainsRelationshipLayout(List<FormElementRelationship> visibleRelationships)
    {
        return visibleRelationships.Count > 0 && visibleRelationships.Any(r => !r.IsParent);
    }

    private async Task<HtmlBuilder> GetDataPanelHtml()
    {
        var formHtml = new HtmlBuilder(HtmlTag.Div);

        ConfigureFormToolbar();

        var topToolbarActions = GetTopToolbarActions(FormElement);

        formHtml.AppendComponent(await GetFormToolbarAsync(topToolbarActions));

        if (!IsInsertAtGridView || (IsInsertAtGridView && DataPanel.Errors.Count != 0))
            DataPanel.Values = await DataPanel.GetFormValuesAsync();

        var parentPanelHtml = await DataPanel.GetPanelHtmlBuilderAsync();

        var panelAndBottomToolbarActions = GetPanelToolbarActions(FormElement);
        panelAndBottomToolbarActions.AddRange(GetBottomToolbarActions(FormElement));

        var toolbar = await GetFormToolbarAsync(panelAndBottomToolbarActions);

        formHtml.Append(parentPanelHtml);

        formHtml.AppendComponent(toolbar);

        if (DataPanel.Errors.Count > 0)
            formHtml.AppendComponent(ComponentFactory.Html.ValidationSummary.Create(DataPanel.Errors));

        return formHtml;
    }

    internal async Task<HtmlBuilder> GetParentPanelHtmlAtRelationship(FormElementRelationship relationship)
    {
        var formHtml = new HtmlBuilder(HtmlTag.Div);

        DataPanel.Values = await DataPanel.GetFormValuesAsync();

        if (!DataPanel.ContainsPanelState())
            DataPanel.PageState = relationship.EditModeOpenByDefault ? PageState : PageState.View;
        
        var parentPanelHtml = await DataPanel.GetPanelHtmlBuilderAsync();

        ConfigureFormToolbar();

        var panelToolbarActions = GetPanelToolbarActions(FormElement);

        var toolbar = await GetFormToolbarAsync(panelToolbarActions);

        formHtml.Append(parentPanelHtml);

        formHtml.AppendComponent(toolbar);

        if (DataPanel.Errors.Count > 0)
            formHtml.AppendComponent(ComponentFactory.Html.ValidationSummary.Create(DataPanel.Errors));

        return formHtml;
    }

    private static List<BasicAction> GetPanelToolbarActions(FormElement formElement)
    {
        var toolbarActions = formElement.Options.FormToolbarActions
            .FindAll(a => a.Location == FormToolbarActionLocation.Panel);

        return toolbarActions;
    }

    private static List<BasicAction> GetTopToolbarActions(FormElement formElement)
    {
        var toolbarActions = formElement.Options.FormToolbarActions
            .FindAll(a => a.Location == FormToolbarActionLocation.Top);

        return toolbarActions;
    }

    private static List<BasicAction> GetBottomToolbarActions(FormElement formElement)
    {
        var toolbarActions = formElement.Options.FormToolbarActions
            .FindAll(a => a.Location == FormToolbarActionLocation.Bottom);

        return toolbarActions;
    }

    private async ValueTask<JJToolbar> GetAuditLogBottomBar()
    {
        var formStateData = await GetFormStateDataAsync();
        var hideAuditLogButton =
            ComponentFactory.ActionButton.CreateFormToolbarButton(
                FormElement.Options.FormToolbarActions.BackAction, formStateData, this);

        var toolbar = new JJToolbar
        {
            CssClass = "pb-3 mt-3"
        };
        toolbar.Items.Add(hideAuditLogButton.GetHtmlBuilder());
        return toolbar;
    }

    private async ValueTask<JJToolbar> GetFormToolbarAsync(IList<BasicAction> actions)
    {
        var toolbar = new JJToolbar
        {
            CssClass = "mb-3"
        };


        var formStateData = await GetFormStateDataAsync();

        foreach (var action in actions.Where(a => !a.IsGroup))
        {
            if (action is SaveAction saveAction)
            {
                saveAction.EnterKeyBehavior = DataPanel.FormUI.EnterKey;
            }

            var factory = ComponentFactory.ActionButton;

            var linkButton = factory.CreateFormToolbarButton(action, formStateData, this);
            toolbar.Items.Add(linkButton.GetHtmlBuilder());
        }

        if (actions.Any(a => a.IsGroup))
        {
            var btnGroup = ComponentFactory.Html.LinkButtonGroup.Create();
            btnGroup.CaretText = _stringLocalizer["More"];
            btnGroup.CssClass += "float-end";

            foreach (var groupedAction in actions.Where(a => a.IsGroup))
            {
                btnGroup.ShowAsButton = groupedAction.ShowAsButton;
                var factory = ComponentFactory.ActionButton;
                var linkButton = factory.CreateFormToolbarButton(groupedAction, formStateData, this);
                btnGroup.Actions.Add(linkButton);
            }

            toolbar.Items.Add(btnGroup.GetHtmlBuilder());
        }

        return toolbar;
    }

    private ValueTask InsertSelectionOnRenderAction(object? sender, ActionEventArgs args)
    {
        if (sender is not JJGridView)
            return ValueTaskHelper.CompletedTask;

        if (args.ActionName is not InsertSelectionAction.ActionName)
            return ValueTaskHelper.CompletedTask;

        args.LinkButton.Tooltip = _stringLocalizer["Select"];
        args.LinkButton.OnClientClick = Scripts.GetInsertSelectionScript(args.FieldValues);

        return ValueTaskHelper.CompletedTask;
    }


    /// <summary>
    /// Insert the records in the database.
    /// </summary>
    /// <returns>The list of errors.</returns>
    public async Task<Dictionary<string, string>> InsertFormValuesAsync(
        Dictionary<string, object?> values,
        bool validateFields = true)
    {
        var dataContext = new DataContext(CurrentContext.Request, DataContextSource.Form, UserId);
        var result = await _formService.InsertAsync(FormElement, values, dataContext, validateFields);
        UrlRedirect = result.UrlRedirect;
        return result.Errors;
    }

    /// <summary>
    /// Update the records in the database.
    /// </summary>
    /// <returns>The list of errors.</returns>
    public async Task<Dictionary<string, string>> UpdateFormValuesAsync(Dictionary<string, object?> values)
    {
        var result = await _formService.UpdateAsync(FormElement, values,
            new DataContext(CurrentContext.Request, DataContextSource.Form, UserId));
        UrlRedirect = result.UrlRedirect;
        return result.Errors;
    }

    public async Task<Dictionary<string, string>> DeleteFormValuesAsync(Dictionary<string, object?>? filter)
    {
        var values =
            await _fieldValuesService.MergeWithExpressionValuesAsync(FormElement,
                new FormStateData(filter!, UserValues, PageState.Delete));
        var result = await _formService.DeleteAsync(FormElement, values,
            new DataContext(CurrentContext.Request, DataContextSource.Form, UserId));
        UrlRedirect = result.UrlRedirect;
        return result.Errors;
    }


    public async Task<Dictionary<string, object?>> GetFormValuesAsync()
    {
        var values = await DataPanel.GetFormValuesAsync();

        if (RelationValues.Count == 0)
            return values;

        DataHelper.CopyIntoDictionary(values, RelationValues!);

        return values;
    }

    public Dictionary<string, string> ValidateFields(Dictionary<string, object> values, PageState pageState)
    {
        DataPanel.Values = values;
        var errors = DataPanel.ValidateFields(values, pageState);
        return errors;
    }

    private void ClearTempFiles()
    {
        var uploadFields = FormElement.Fields.FindAll(x => x.Component == FormComponent.File);
        foreach (var field in uploadFields)
        {
            string sessionName = $"{field.Name}-upload-view_jjfiles";
            if (CurrentContext?.Session[sessionName] != null)
                CurrentContext.Session[sessionName] = null;
        }
    }

    public async ValueTask<FormStateData> GetFormStateDataAsync()
    {
        if (_formStateData != null)
            return _formStateData;

        var initialValues = new Dictionary<string, object?>();

        if (_dataPanel is not null)
            DataHelper.CopyIntoDictionary(initialValues, DataPanel.Values);

        if (_currentActionMap is not null)
            DataHelper.CopyIntoDictionary(initialValues, CurrentActionMap!.PkFieldValues!);

        var initialFormStateData = new FormStateData(initialValues, UserValues, PageState);
        var reloadFormFields = IsReloadFields();
        var values =
            await _formValuesService.GetFormValuesWithMergedValuesAsync(FormElement, initialFormStateData,
                reloadFormFields);

        _formStateData = new FormStateData(values, UserValues, PageState);
        return _formStateData;
    }

    public async ValueTask<FormStateData> GetFormStateDataAsync(bool reloadFormFields)
    {
        var initialValues = new Dictionary<string, object?>();

        if (_dataPanel is not null)
            DataHelper.CopyIntoDictionary(initialValues, DataPanel.Values);

        if (_currentActionMap is not null)
            DataHelper.CopyIntoDictionary(initialValues, CurrentActionMap!.PkFieldValues!);

        var initialFormStateData = new FormStateData(initialValues, UserValues, PageState);
        var values =
            await _formValuesService.GetFormValuesWithMergedValuesAsync(FormElement, initialFormStateData,
                reloadFormFields);

        var formStateData = new FormStateData(values, UserValues, PageState);
        return formStateData;
    }

    public Dictionary<string, object> GetRelationValuesFromForm()
    {
        var encryptedRelationValues = CurrentContext.Request.Form[$"form-view-relation-values-{FormElement.Name}"];

        if (encryptedRelationValues is null)
            return new Dictionary<string, object>();

        return EncryptionService.DecryptDictionary(encryptedRelationValues);
    }

    internal void DisableActionsAtViewMode()
    {
        foreach (var action in FormElement.Options.GridTableActions)
        {
            if (action is not ViewAction)
            {
                action.SetVisible(false);
            }
        }

        foreach (var action in FormElement.Options.GridToolbarActions)
        {
            if (action is not FilterAction &&
                action is not RefreshAction &&
                action is not LegendAction &&
                action is not ConfigAction)
            {
                action.SetVisible(false);
            }
        }
    }

    public ActionContext GetActionContext(BasicAction action, FormStateData formStateData, string? fieldName = null)
    {
        return new ActionContext
        {
            Action = action,
            FormElement = FormElement,
            FormStateData = formStateData,
            FieldName = fieldName,
            ParentComponentName = Name
        };
    }

    public void AddFormEventHandler(IFormEventHandler? formEventHandler)
    {
        if (formEventHandler != null)
        {
            AddEventHandlers(formEventHandler);
        }
    }

    private void AddEventHandlers(IFormEventHandler eventHandler)
    {
        OnBeforeInsertAsync += eventHandler.OnBeforeInsertAsync;
        OnBeforeDeleteAsync += eventHandler.OnBeforeDeleteAsync;
        OnBeforeUpdateAsync += eventHandler.OnBeforeUpdateAsync;
        OnBeforeImportAsync += eventHandler.OnBeforeImportAsync;
        OnAfterDeleteAsync += eventHandler.OnAfterDeleteAsync;
        OnAfterInsertAsync += eventHandler.OnAfterInsertAsync;
        OnAfterUpdateAsync += eventHandler.OnAfterUpdateAsync;
    }

    private JJTitle GetTitle(FormStateData formStateData)
    {
        return ComponentFactory.Html.Title.Create(FormElement, formStateData, TitleActions);
    }

    public static implicit operator JJGridView(JJFormView formView) => formView.GridView;
    public static implicit operator JJDataPanel(JJFormView formView) => formView.DataPanel;
}