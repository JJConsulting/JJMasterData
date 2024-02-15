#nullable enable

// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable EventNeverSubscribedTo.Global

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JJMasterData.Commons.Data;
using JJMasterData.Commons.Data.Entity.Models;
using JJMasterData.Commons.Data.Entity.Repository.Abstractions;
using JJMasterData.Commons.Exceptions;
using JJMasterData.Commons.Localization;
using JJMasterData.Commons.Security.Cryptography.Abstractions;
using JJMasterData.Commons.Tasks;
using JJMasterData.Commons.Util;
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
using JJMasterData.Core.UI.Events.Args;
using JJMasterData.Core.UI.Html;
using JJMasterData.Core.UI.Routing;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
#if NET48
using JJMasterData.Commons.Configuration;
using JJMasterData.Core.Tasks;
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
    private FormViewScripts? _scripts;
    private ActionMap? _currentActionMap;
    private BasicAction? _currentAction;
    private JJAuditLogView? _auditLogView;
    private JJDataImportation? _dataImportation;
    private string? _userId;
    private PageState? _pageState;
    private PageState? _panelState;
    private Dictionary<string, object> _relationValues = new();
    private RouteContext? _routeContext;
    private FormStateData? _formStateData;
    private bool _isCustomCurrentActionMap;

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
                _dataPanel.PageState = PanelState ?? PageState;
                _dataPanel.AutoReloadFormFields = PanelState is not PageState.View;
                _dataPanel.UserValues = UserValues;
                _dataPanel.RenderPanelGroup = true;
                DataHelper.CopyIntoDictionary(_dataPanel.UserValues, RelationValues!);
            }
            _dataPanel.ParentComponentName = Name;
            _dataPanel.FormUI = FormElement.Options.Form;

            if (IsInsertAtGridView && (_dataPanel.FieldNamePrefix == null || !_dataPanel.FieldNamePrefix.Contains("insert_")))
            {
                _dataPanel.Name += "_insert";
                _dataPanel.FieldNamePrefix = "insert_";
            }

            if (IsChildFormView && (_dataPanel.FieldNamePrefix == null || !_dataPanel.FieldNamePrefix.Contains(Name)))
                _dataPanel.FieldNamePrefix += $"{Name}_";
            
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
            if (!_relationValues.Any())
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
            _gridView.ToolbarActions.Add(new DeleteSelectedRowsAction());

            if (_gridView.InsertAction.InsertActionLocation is InsertActionLocation.AboveGrid)
                _gridView.OnBeforeTableRenderAsync += RenderInsertActionAtGrid;
            
            if (_gridView.InsertAction.InsertActionLocation is InsertActionLocation.BelowGrid)
                _gridView.OnAfterTableRenderAsync += RenderInsertActionAtGrid;

            return _gridView;
        }
    }

    private async Task RenderInsertActionAtGrid(object _, GridRenderEventArgs args)
    {
        var insertAction = GridView.InsertAction;
        var formStateData = await GridView.GetFormStateDataAsync();
        var insertActionVisible = ExpressionsService.GetBoolValue(insertAction.VisibleExpression,formStateData );
        var insertActionEnabled = ExpressionsService.GetBoolValue(insertAction.EnableExpression,formStateData );
        if (!insertActionVisible || !insertActionEnabled)
            return;
        
        PageState = PageState.Insert;

        var result = await GetFormResult(new FormContext(formStateData.Values, DataPanel.Errors, PageState), true);

        if (result is HtmlComponentResult htmlComponentResult)
        {
            args.HtmlBuilder.Append(htmlComponentResult.HtmlBuilder);
        }
    }

    public PageState PageState
    {
        get
        {
            if (CurrentContext.Request.Form[$"form-view-page-state-{Name}"] != null && _pageState is null)
                _pageState = (PageState)int.Parse(CurrentContext.Request.Form[$"form-view-page-state-{Name}"]);

            return _pageState ?? PageState.List;
        }
        internal set => _pageState = value;
    }

    /// <summary>
    /// If inside a relationship, PageState of the parent DataPanel.
    /// </summary>
    internal PageState? PanelState
    {
        get
        {
            if (CurrentContext.Request.Form[$"form-view-panel-state-{Name}"] != null && _panelState is null)
                _panelState = (PageState)int.Parse(CurrentContext.Request.Form[$"form-view-panel-state-{Name}"]);

            return _panelState;
        }
        set => _panelState = value;
    }

    private ActionMap? CurrentActionMap
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

    internal ComponentContext ComponentContext => RouteContext.IsCurrentFormElement(FormElement.Name) 
        ? RouteContext.ComponentContext : default;

    internal FormViewScripts Scripts => _scripts ??= new(this);

    public bool ShowTitle { get; set; }
    
    internal bool IsChildFormView { get; set; }

    internal bool IsInsertAtGridView => PageState is PageState.Insert && CurrentAction is not EditAction &&
                                        FormElement.Options.GridToolbarActions.InsertAction.ShowOpenedAtGrid;
    
    internal IHttpContext CurrentContext { get; }
    internal IFormValues FormValues => CurrentContext.Request.Form;
    internal IQueryString QueryString => CurrentContext.Request.QueryString;
    internal IEntityRepository EntityRepository { get; }
    internal FieldValuesService FieldValuesService { get; }
    internal ExpressionsService ExpressionsService { get; }
    private IEnumerable<IPluginHandler> PluginHandlers { get; }
    private IOptions<MasterDataCoreOptions> Options { get; }
    private IStringLocalizer<MasterDataResources> StringLocalizer { get; }
    private ILogger<JJFormView> Logger { get; }
    internal IDataDictionaryRepository DataDictionaryRepository { get; }
    internal FormService FormService { get; }
    internal IEncryptionService EncryptionService { get; }
    internal IComponentFactory ComponentFactory { get; }

    #endregion

    #region "Constructors"
    
    internal JJFormView(
        FormElement formElement,
        IHttpContext currentContext,
        IEntityRepository entityRepository,
        IDataDictionaryRepository dataDictionaryRepository,
        FormService formService,
        IEncryptionService encryptionService,
        FieldValuesService fieldValuesService,
        ExpressionsService expressionsService,
        IEnumerable<IPluginHandler> pluginHandlers,
        IOptions<MasterDataCoreOptions> options,
        IStringLocalizer<MasterDataResources> stringLocalizer,
        ILogger<JJFormView> logger,
        IComponentFactory componentFactory)
    {
        FormElement = formElement;
        Name = ComponentNameGenerator.Create(FormElement.Name);
        CurrentContext = currentContext;
        EntityRepository = entityRepository;
        ShowTitle = formElement.Options.Grid.ShowTitle;
        FormService = formService;
        EncryptionService = encryptionService;
        FieldValuesService = fieldValuesService;
        ExpressionsService = expressionsService;
        PluginHandlers = pluginHandlers;
        Options = options;
        StringLocalizer = stringLocalizer;
        Logger = logger;
        DataDictionaryRepository = dataDictionaryRepository;
        ComponentFactory = componentFactory;
        formService.EnableErrorLinks = true;
    }

    #endregion

    protected override async Task<ComponentResult> BuildResultAsync()
    {
        if (!RouteContext.CanRender(FormElement))
            return new EmptyComponentResult();

        if (RouteContext.IsCurrentFormElement(FormElement.Name))
            return await GetFormResultAsync();

        if (RouteContext.ElementName == Options.Value.AuditLogTableName)
            return await AuditLogView.GetResultAsync();

        var childFormView = await GetChildFormView();

        return await childFormView.GetFormResultAsync();
    }

    private async Task<JJFormView> GetChildFormView()
    {
        var childFormView = await ComponentFactory.FormView.CreateAsync(RouteContext.ElementName);
        childFormView.IsChildFormView = true;
        
        childFormView.FormElement.ParentName = RouteContext.ParentElementName;
        childFormView.UserValues = UserValues;
        childFormView.RelationValues = childFormView.GetRelationValuesFromForm();
        
        var isInsertSelection = PageState is PageState.Insert &&
                                GridView.ToolbarActions.InsertAction.ElementNameToSelect ==
                                childFormView.FormElement.Name;
        
        childFormView.ShowTitle = isInsertSelection;
        
        if (PageState is PageState.View)
            childFormView.DisableActionsAtViewMode();
        
        
        if (!isInsertSelection)
            return childFormView;

        childFormView.GridView.GridTableActions.Add(new InsertSelectionAction());
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
            case ComponentContext.GridViewFilterSearchBox:
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

        var fieldName = QueryString["fieldName"];

        var field = FormElement.Fields[fieldName];

        var scripts = new HtmlBuilder();
        
        foreach (var action in field.Actions)
        {
            if (action is not PluginFieldAction { AutoTriggerOnChange: true } pluginAction) 
                continue;
            
            var result = await GetPluginActionResult(pluginAction, values, fieldName);

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
        if (PageState is PageState.Insert)
            errors = await InsertFormValuesAsync(values);
        else
            errors = await UpdateFormValuesAsync(values);

        DataPanel.Errors = errors;
        
        if (errors.Count != 0 && !insertAction.ShowOpenedAtGrid)
            return await GetFormResult(new FormContext(values, errors, PageState), true);

        if (!string.IsNullOrEmpty(UrlRedirect))
        {
            CurrentActionMap = null;
            return new RedirectComponentResult(UrlRedirect!);
        }

        
        if (PageState is PageState.Insert && insertAction.ReopenForm)
        {
            var formResult = await GetFormResult(new FormContext(new Dictionary<string,object?>(RelationValues!), PageState.Insert), false);

            if (formResult is HtmlComponentResult htmlComponent)
            {
                AppendInsertSuccessAlert(htmlComponent.HtmlBuilder);

                return htmlComponent;
            }

            return formResult;
        }

        if (PanelState is not null)
        {
            PanelState = PageState.View;
            return await GetFormResult(new FormContext(values, PanelState.Value), false);
        }

        if (PageState is PageState.Insert)
        {
            var visibleRelationships = GetVisibleRelationships(values, PageState.Update);
            
            if(visibleRelationships.Any())
            {
                PageState = PageState.Update;
                PanelState = PageState.View;
                return await GetFormResult(new FormContext(values, PanelState.Value), false);
            }
        }
        
        PageState = PageState.List;
        CurrentActionMap = null;
        
        return await GridView.GetResultAsync();
    }

    private void AppendInsertSuccessAlert(HtmlBuilder htmlBuilder)
    {
        var alert = new JJAlert
        {
            Name = $"insert-alert-{Name}",
            Color = PanelColor.Success,
            Title = StringLocalizer["Success"],
            ShowIcon = true,
            Icon = IconType.CheckCircleO
        };
        alert.Messages.Add(StringLocalizer["Record added successfully"]);
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
        
        ComponentResult? result;
        if (CurrentAction is ViewAction)
            result = await GetViewResult();
        else if (CurrentAction is EditAction)
            result = await GetUpdateResult();
        else if (CurrentAction is InsertAction)
            result = await GetInsertResult();
        else if (CurrentAction is AuditLogFormToolbarAction or AuditLogGridToolbarAction)
            result = await GetAuditLogResult();
        else if (CurrentAction is DeleteAction)
            result = await GetDeleteResult();
        else if (CurrentAction is DeleteSelectedRowsAction)
            result = await GetDeleteSelectedRowsResult();
        else if (CurrentAction is SaveAction)
            result = await GetSaveActionResult();
        else if (CurrentAction is BackAction)
            result = await GetBackActionResult();
        else if (CurrentAction is CancelAction)
            result = await GetCancelActionResult();
        else if (CurrentAction is SqlCommandAction)
            result = await GetSqlCommandActionResult();
        else if (CurrentAction is PluginAction)
            result = await GetPluginActionResult();
        else
            result = await GetDefaultResult();

        if (result is HtmlComponentResult htmlComponent)
        {
            var html = htmlComponent.HtmlBuilder;

            html.WithNameAndId(Name);
            AppendFormViewHiddenInputs(html);

            if (ComponentContext is ComponentContext.FormViewReload)
            {
                return new ContentComponentResult(html);
            }
        }

        return result;
    }

    private void AppendFormViewHiddenInputs(HtmlBuilder html)
    {
        if (CurrentAction is not IModalAction { ShowAsModal: true })
        {
            html.AppendHiddenInput($"form-view-page-state-{Name}", ((int)PageState).ToString());
        
            if(PageState is not PageState.List && PanelState is not null)
                html.AppendHiddenInput($"form-view-panel-state-{Name}", ((int)PanelState).ToString());

            html.AppendHiddenInput($"current-action-map-{Name}",
                EncryptionService.EncryptActionMap(CurrentActionMap));
            html.AppendHiddenInput($"form-view-relation-values-{FormElement.Name}",
                EncryptionService.EncryptDictionary(RelationValues));
        }

    }

    private async Task<ComponentResult> GetSqlCommandActionResult()
    {
        JJMessageBox? messageBox = null;
        var sqlAction = CurrentActionMap?.GetAction<SqlCommandAction>(FormElement);

        if (sqlAction is null)
            throw new JJMasterDataException("Action not found at your FormElement");
        
        try
        {
            var sqlCommand = ExpressionsService.ReplaceExpressionWithParsedValues(sqlAction.SqlCommand, await GetFormStateDataAsync());

            await EntityRepository.SetCommandAsync(new DataAccessCommand(sqlCommand!));
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error while executing SQL Command Action.");
            var message = ExceptionManager.GetMessage(ex);
            messageBox = ComponentFactory.Html.MessageBox.Create(message, MessageIcon.Error);
        }

        var result = await GetDefaultResult();

        if (result is HtmlComponentResult htmlComponentResult)
        {
            htmlComponentResult.HtmlBuilder.AppendComponentIf(messageBox is not null, messageBox);
        }

        return result;
    }

    private async Task<ComponentResult> GetPluginActionResult()
    {
        var formValues = await GetFormValuesAsync();


        PluginActionResult result;
        
        try
        {
            result = await GetPluginActionResult(formValues);
        }
        catch (Exception exception)
        {
            Logger.LogError(exception,"Error while executing Plugin Action.");
            result = PluginActionResult.Error(StringLocalizer["Error"], exception.Message);
        }
        
        var formResult = await GetDefaultResult(formValues);

        if (formResult is HtmlComponentResult htmlComponentResult)
        {
            htmlComponentResult.HtmlBuilder.AppendScriptIf(!string.IsNullOrEmpty(result.JsCallback),result.JsCallback!);
        }

        return formResult;
    }

    private Task<PluginActionResult> GetPluginActionResult(Dictionary<string, object?> formValues)
    {
        var pluginAction = (PluginAction)CurrentAction!;

        return GetPluginActionResult(pluginAction, formValues, CurrentActionMap!.FieldName);
    }

    private Task<PluginActionResult> GetPluginActionResult(PluginAction pluginAction,
        Dictionary<string, object?> values, string? fieldName)
    {
        var pluginHandler = PluginHandlers.First(p => p.Id == pluginAction.PluginId);

        var formStateData = new FormStateData
        {
            Values = values,
            UserValues = UserValues,
            PageState = PageState,
        };

        switch (pluginHandler)
        {
            case IPluginActionHandler pluginActionHandler:
                return pluginActionHandler.ExecuteActionAsync(new PluginActionContext
                {
                    ActionContext = GetActionContext(pluginAction, formStateData),
                    ConfigurationMap = pluginAction.ConfigurationMap
                });
            case IPluginFieldActionHandler pluginFieldActionHandler:
                return pluginFieldActionHandler.ExecuteActionAsync(context: new PluginFieldActionContext
                {
                    ActionContext = GetActionContext(pluginAction,
                        formStateData, fieldName),
                    FieldMap = ((PluginFieldAction)pluginAction).FieldMap,
                    ConfigurationMap = pluginAction.ConfigurationMap
                });
            default:
                throw new JJMasterDataException("Invalid plugin handler");
        }
    }

    private void SetFormServiceEvents()
    {
        FormService.OnBeforeInsertAsync += OnBeforeInsertAsync;
        FormService.OnBeforeDeleteAsync += OnBeforeDeleteAsync;
        FormService.OnBeforeUpdateAsync += OnBeforeUpdateAsync;

        FormService.OnAfterInsertAsync += OnAfterInsertAsync;
        FormService.OnAfterUpdateAsync += OnAfterUpdateAsync;
        FormService.OnAfterDeleteAsync += OnAfterDeleteAsync;
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
            values = await EntityRepository.GetFieldsAsync(FormElement, CurrentActionMap!.PkFieldValues);
        }

        PageState = PageState.Update;
        return await GetFormResult(new FormContext(values, PageState), autoReloadFields);
    }

    private async Task<ComponentResult> GetDefaultResult(Dictionary<string, object?>? formValues = null)
    {
        if (PageState is PageState.List || PanelState is PageState.List || ContainsGridAction())
            return await GetGridViewResult();
        
        switch (PageState)
        {
            case PageState.Insert:
            {
                formValues ??= new Dictionary<string, object?>();
                DataHelper.CopyIntoDictionary(formValues,RelationValues!);
                return await GetFormResult(
                    new FormContext(formValues, PageState),
                    false);
            }

            case PageState.Update or PageState.View:
            {
                formValues ??= await GetFormValuesAsync();
                return await GetFormResult(new FormContext(formValues, PageState), PanelState is not PageState.View);
            }
            default:
                return await GetGridViewResult();
        }
    }

    private async Task<ComponentResult> GetInsertResult()
    {
        var insertAction = GridView.ToolbarActions.InsertAction;
        var formData = new FormStateData(RelationValues!, UserValues, PageState.List);

        bool isVisible = ExpressionsService.GetBoolValue(insertAction.VisibleExpression, formData);
        if (!isVisible)
            throw new UnauthorizedAccessException(StringLocalizer["Insert action is not enabled"]);

        if (PageState == PageState.Insert)
        {
            var formValues = await GetFormValuesAsync();
            return await GetFormResult(new FormContext(formValues, PageState), true);
        }

        PageState = PageState.Insert;

        if (string.IsNullOrEmpty(insertAction.ElementNameToSelect))
            return await GetFormResult(new FormContext(new Dictionary<string,object?>(RelationValues!), PageState.Insert), false);

        return await GetInsertSelectionListResult();
    }

    private async Task<ComponentResult> GetInsertSelectionListResult()
    {
        var insertAction = GridView.ToolbarActions.InsertAction;
        var html = new HtmlBuilder(HtmlTag.Div);
        html.AppendHiddenInput($"form-view-insert-selection-values-{Name}");


        var formView = await ComponentFactory.FormView.CreateAsync(insertAction.ElementNameToSelect);
        formView.FormElement.ParentName = FormElement.Name;
        formView.UserValues = UserValues;
        formView.GridView.OnRenderActionAsync += InsertSelectionOnRenderAction;
        
        formView.GridView.ToolbarActions.Add(GetInsertSelectionBackAction());

        formView.GridView.GridTableActions.Add(new InsertSelectionAction());

        var result = await formView.GetFormResultAsync();

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

    private ScriptAction GetInsertSelectionBackAction()
    {
        return new ScriptAction
        {
            Name = "back-action",
            Icon = IconType.ArrowLeft,
            Text = StringLocalizer["Back"],
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
        var childElement = await DataDictionaryRepository.GetFormElementAsync(childElementName);
        
        var selectionValues = await EntityRepository.GetFieldsAsync(childElement, insertValues);
        
        var mappedFkValues = DataHelper.GetRelationValues(FormElement, selectionValues, true);

        var values =
            await FieldValuesService.MergeWithExpressionValuesAsync(FormElement, new FormStateData(mappedFkValues!,UserValues, PageState.Insert),
                true);
        
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
            PageState = PageState.Update;

            var result = await GetFormResult(new FormContext(values, PageState), false);

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
        var filter = CurrentActionMap.PkFieldValues;
        var values = await EntityRepository.GetFieldsAsync(FormElement, filter);
        return await GetFormResult(new FormContext(values, PageState), false);
    }

    private async Task<ComponentResult> GetDeleteResult()
    {
        var html = new HtmlBuilder(HtmlTag.Div);
        var messageFactory = ComponentFactory.Html.MessageBox;
        try
        {
            var filter = CurrentActionMap?.PkFieldValues;
            
            var errors = await DeleteFormValuesAsync(filter!);
            if (errors.Count > 0)
            {
                html.AppendComponent(messageFactory.Create(errors, MessageIcon.Warning));
            }
            else
            {
                if (GridView.EnableMultiSelect)
                    GridView.ClearSelectedGridValues();
            }
        }
        catch (Exception ex)
        {
            html.AppendComponent(messageFactory.Create(ex.Message, MessageIcon.Error));
        }

        if (!string.IsNullOrEmpty(UrlRedirect))
        {
            return new RedirectComponentResult(UrlRedirect!);
        }

        html.Append(await GridView.GetHtmlBuilderAsync());
        PageState = PageState.List;

        return new RenderedComponentResult(html);
    }

    private async Task<ComponentResult> GetDeleteSelectedRowsResult()
    {
        var html = new HtmlBuilder(HtmlTag.Div);
        var messageFactory = ComponentFactory.Html.MessageBox;
        var errorMessage = new StringBuilder();
        int errorCount = 0;
        int successCount = 0;

        try
        {
            var rows = GridView.GetSelectedGridValues();

            foreach (var row in rows)
            {
                var errors = await DeleteFormValuesAsync(row!);

                if (errors.Count > 0)
                {
                    foreach (var err in errors)
                    {
                        errorMessage.Append(" - ");
                        errorMessage.Append(err.Value);
                        errorMessage.Append("<br>");
                    }

                    errorCount++;
                }
                else
                {
                    successCount++;
                }
            }

            if (rows.Count > 0)
            {
                var message = new StringBuilder();
                var icon = MessageIcon.Info;
                if (successCount > 0)
                {
                    message.Append("<p class=\"text-success\">");
                    message.Append(StringLocalizer["{0} Record(s) deleted successfully", successCount]);
                    message.Append("</p><br>");
                }

                if (errorCount > 0)
                {
                    message.Append("<p class=\"text-danger\">");
                    message.Append(StringLocalizer["{0} Record(s) with error", successCount]);
                    message.Append(StringLocalizer["Details:"]);
                    message.Append("<br>");
                    message.Append(errorMessage);
                    icon = MessageIcon.Warning;
                }

                html.AppendComponent(messageFactory.Create(message.ToString(), icon));

                GridView.ClearSelectedGridValues();
            }
        }
        catch (Exception ex)
        {
            html.AppendComponent(messageFactory.Create(ex.Message, MessageIcon.Error));
        }

        var gridViewResult = await GetGridViewResult();

        if (gridViewResult is RenderedComponentResult)
        {
            html.Append(new HtmlBuilder(gridViewResult));
        }
        else
        {
            return gridViewResult;
        }

        PageState = PageState.List;

        return new RenderedComponentResult(html);
    }

    private async Task<ComponentResult> GetAuditLogResult()
    {
        var actionMap = _currentActionMap;
        var script = new StringBuilder();
        script.Append($"setPageState('{Name}',{(int)PageState.List})");
        script.Append($"document.getElementById('current-action-map-{Name}').value = null; ");
        script.AppendLine("document.forms[0].submit(); ");

        var goBackAction = new ScriptAction
        {
            Name = "go-back-action",
            Icon = IconType.Backward,
            ShowAsButton = true,
            Text = StringLocalizer["Back"],
            OnClientClick = script.ToString()
        };

        if (PageState == PageState.View)
        {
            var html = await AuditLogView.GetLogDetailsHtmlAsync(actionMap?.PkFieldValues);

            if (actionMap?.PkFieldValues != null)
                html.AppendComponent(await GetAuditLogBottomBar());

            PageState = PageState.AuditLog;
            return new ContentComponentResult(html);
        }

        AuditLogView.GridView.AddToolbarAction(goBackAction);
        AuditLogView.DataPanel = DataPanel;
        PageState = PageState.AuditLog;
        return await AuditLogView.GetResultAsync();
    }

    private async Task<ComponentResult> GetImportationResult()
    {
        var action = GridView.ImportAction;
        var formStateData = await GridView.GetFormStateDataAsync();
        bool isVisible = ExpressionsService.GetBoolValue(action.VisibleExpression, formStateData);
        if (!isVisible)
            throw new UnauthorizedAccessException(StringLocalizer["Import action not enabled"]);

        var html = new HtmlBuilder(HtmlTag.Div);

        if (ShowTitle)
            html.AppendComponent(GetTitle(new FormStateData(UserValues, PageState.Import)));

        PageState = PageState.Import;

        DataImportation.UserValues = UserValues;
        DataImportation.ProcessOptions = action.ProcessOptions;
        DataImportation.EnableAuditLog =ExpressionsService.GetBoolValue(
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

    private Task<ComponentResult> GetFormResult(FormContext formContext, bool autoReloadFormFields)
    {
        var (values, errors, pageState) = formContext;

        var visibleRelationships = GetVisibleRelationships(values, pageState);
        
        DataPanel.PageState = PanelState ?? pageState;
        DataPanel.Errors = errors;
        DataPanel.Values = values;
        DataPanel.AutoReloadFormFields = autoReloadFormFields;
        
        if (!visibleRelationships.Any() || visibleRelationships.All(r=>r.IsParent))
        {
            return GetDataPanelResult();
        }

        return GetRelationshipLayoutResult(visibleRelationships,values);
    }
    

    internal async Task<ComponentResult> GetRelationshipLayoutResult(
        List<FormElementRelationship> visibleRelationships,
        Dictionary<string, object?> values)
    {
        var html = new HtmlBuilder(HtmlTag.Div);
        if (ShowTitle)
            html.AppendComponent(GetTitle(new FormStateData(values, UserValues, PageState)));

        var layout = new FormViewRelationshipLayout(this, visibleRelationships);

        await ConfigureFormToolbar();

        var topActions = GetTopToolbarActions(FormElement).ToList();

        html.AppendComponent(await GetFormToolbarAsync(topActions));

        var relationshipsResult = await layout.GetRelationshipsResult();

        if (relationshipsResult is HtmlComponentResult htmlResult)
        {
            html.Append((HtmlBuilder?)htmlResult.HtmlBuilder);
            var toolbarActions = FormElement.Options.FormToolbarActions;

            var bottomActions = 
                toolbarActions.Where(a => a.Location is FormToolbarActionLocation.Bottom).ToList();
        
            toolbarActions.BackAction.SetVisible(true);
        
            html.AppendComponent(await GetFormToolbarAsync(bottomActions));

            return new RenderedComponentResult(html);
        }

        return relationshipsResult;
    }

    private async Task ConfigureFormToolbar()
    {
        var formToolbarActions = FormElement.Options.FormToolbarActions;
        if (PanelState is PageState.View)
        {
            formToolbarActions.CancelAction.SetVisible(false);
            formToolbarActions.SaveAction.SetVisible(false);
        }
        
        
        switch (PageState)
        {
            case PageState.Insert when PanelState is PageState.Insert && IsChildFormView:
                formToolbarActions.CancelAction.SetVisible(false);
                break;
            case PageState.Insert when IsInsertAtGridView:
                formToolbarActions.CancelAction.SetVisible(false);
                break;
            case PageState.Update when PanelState is PageState.View:
                formToolbarActions.FormEditAction.SetVisible(true);
                formToolbarActions.SaveAction.SetVisible(false);
                formToolbarActions.CancelAction.SetVisible(false);
                break;
            case PageState.Update:
                formToolbarActions.FormEditAction.SetVisible(false);
                if (IsChildFormView)
                    formToolbarActions.BackAction.SetVisible(false);
                break;
            case PageState.View:
            {
                if (IsChildFormView)
                    formToolbarActions.BackAction.SetVisible(false);
                
                formToolbarActions.AuditLogFormToolbarAction.SetVisible(await IsAuditLogEnabled());
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

    private List<FormElementRelationship> GetVisibleRelationships(Dictionary<string, object?> values,
        PageState pageState)
    {
        var visibleRelationships = FormElement
            .Relationships
            .Where(r => r.ViewType != RelationshipViewType.None || r.IsParent)
            .Where( r =>
                 ExpressionsService.GetBoolValue(r.Panel.VisibleExpression,
                    new FormStateData(values, pageState)))
            .ToList();
        return visibleRelationships;
    }

    private async Task<HtmlBuilder> GetDataPanelHtml()
    {
        var formHtml = new HtmlBuilder(HtmlTag.Div);
        
        await ConfigureFormToolbar();

        var topToolbarActions = GetTopToolbarActions(FormElement).ToList();

        formHtml.AppendComponent(await GetFormToolbarAsync(topToolbarActions));
        
        if(IsInsertAtGridView && DataPanel.Errors.Count != 0)
            DataPanel.Values = await DataPanel.GetFormValuesAsync();
            
        var parentPanelHtml = await DataPanel.GetPanelHtmlBuilderAsync();

        var panelAndBottomToolbarActions = GetPanelToolbarActions(FormElement).ToList();
        panelAndBottomToolbarActions.AddRange(GetBottomToolbarActions(FormElement));

        var toolbar = await GetFormToolbarAsync(panelAndBottomToolbarActions);

        formHtml.Append(parentPanelHtml);

        formHtml.AppendComponent(toolbar);

        if (DataPanel.Errors.Any())
            formHtml.AppendComponent(ComponentFactory.Html.ValidationSummary.Create(DataPanel.Errors));
        
        return formHtml;
    }

    internal async Task<HtmlBuilder> GetParentPanelHtmlAtRelationship(FormElementRelationship relationship)
    {
        var formHtml = new HtmlBuilder(HtmlTag.Div);

        if (PageState is not PageState.View)
        {
            PanelState ??= relationship.EditModeOpenByDefault ? PageState.Update : PageState.View;
            DataPanel.PageState = PanelState.Value;
        }
        else
        {
            PanelState = PageState.View;
            DataPanel.PageState = PanelState.Value;
        }

        
        var parentPanelHtml = await DataPanel.GetPanelHtmlBuilderAsync();

        await ConfigureFormToolbar();
        
        var panelToolbarActions = GetPanelToolbarActions(FormElement).ToList();

        var toolbar = await GetFormToolbarAsync(panelToolbarActions);

        formHtml.Append(parentPanelHtml);

        formHtml.AppendComponent(toolbar);

        if (DataPanel.Errors.Any())
            formHtml.AppendComponent(ComponentFactory.Html.ValidationSummary.Create(DataPanel.Errors));
        
        return formHtml;
    }

    private static IEnumerable<BasicAction> GetPanelToolbarActions(FormElement formElement)
    {
        var toolbarActions = formElement.Options.FormToolbarActions
            .Where(a => a.Location == FormToolbarActionLocation.Panel);

        return toolbarActions;
    }

    private static IEnumerable<BasicAction> GetTopToolbarActions(FormElement formElement)
    {
        var toolbarActions = formElement.Options.FormToolbarActions
            .Where(a => a.Location == FormToolbarActionLocation.Top);

        return toolbarActions;
    }

    private static IEnumerable<BasicAction> GetBottomToolbarActions(FormElement formElement)
    {
        var toolbarActions = formElement.Options.FormToolbarActions
            .Where(a => a.Location == FormToolbarActionLocation.Bottom);

        return toolbarActions;
    }

    private async Task<JJToolbar> GetAuditLogBottomBar()
    {
        var formStateData = await GetFormStateDataAsync();
        var hideAuditLogButton =
             ComponentFactory.ActionButton.CreateFormToolbarButton(
                FormElement.Options.FormToolbarActions.BackAction,formStateData, this);

        var toolbar = new JJToolbar
        {
            CssClass = "pb-3 mt-3"
        };
        toolbar.Items.Add(hideAuditLogButton.GetHtmlBuilder());
        return toolbar;
    }

    private async Task<JJToolbar> GetFormToolbarAsync(IList<BasicAction> actions)
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
            btnGroup.CaretText = StringLocalizer["More"];

            foreach (var groupedAction in actions.Where(a => a.IsGroup).ToList())
            {
                btnGroup.ShowAsButton = groupedAction.ShowAsButton;
                var factory = ComponentFactory.ActionButton;
                var linkButton = factory.CreateFormToolbarButton(groupedAction,formStateData, this);
                btnGroup.Actions.Add(linkButton);
            }

            toolbar.Items.Add(btnGroup.GetHtmlBuilder());
        }

        return toolbar;
    }

    private Task InsertSelectionOnRenderAction(object? sender, ActionEventArgs args)
    {
        if (sender is not JJGridView)
            return Task.CompletedTask;

        if (args.ActionName is not InsertSelectionAction.ActionName)
            return Task.CompletedTask;

        args.LinkButton.Tooltip = StringLocalizer["Select"];
        args.LinkButton.OnClientClick = Scripts.GetInsertSelectionScript(args.FieldValues);

        return Task.CompletedTask;
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
        var result = await FormService.InsertAsync(FormElement, values, dataContext, validateFields);
        UrlRedirect = result.UrlRedirect;
        return result.Errors;
    }

    /// <summary>
    /// Update the records in the database.
    /// </summary>
    /// <returns>The list of errors.</returns>
    public async Task<Dictionary<string, string>> UpdateFormValuesAsync(Dictionary<string, object?> values)
    {
        var result = await FormService.UpdateAsync(FormElement, values,
            new DataContext(CurrentContext.Request, DataContextSource.Form, UserId));
        UrlRedirect = result.UrlRedirect;
        return result.Errors;
    }

    public async Task<Dictionary<string, string>> DeleteFormValuesAsync(Dictionary<string, object?>? filter)
    {
        var values =
            await FieldValuesService.MergeWithExpressionValuesAsync(FormElement,  new FormStateData(filter!, UserValues, PageState.Delete), true);
        var result = await FormService.DeleteAsync(FormElement, values,
            new DataContext(CurrentContext.Request, DataContextSource.Form, UserId));
        UrlRedirect = result.UrlRedirect;
        return result.Errors;
    }


    public async Task<Dictionary<string, object?>> GetFormValuesAsync()
    {
        var values = await DataPanel.GetFormValuesAsync();

        if (!RelationValues.Any())
            return values;

        DataHelper.CopyIntoDictionary(values, RelationValues!, true);

        return values;
    }

    public Dictionary<string, string> ValidateFields(Dictionary<string, object> values,
        PageState pageState)
    {
        DataPanel.Values = values;
        var errors = DataPanel.ValidateFields(values, pageState);
        return errors;
    }

    private void ClearTempFiles()
    {
        var uploadFields = FormElement.Fields.ToList().FindAll(x => x.Component == FormComponent.File);
        foreach (var field in uploadFields)
        {
            string sessionName = $"{field.Name}-upload-view_jjfiles";
            if (CurrentContext?.Session[sessionName] != null)
                CurrentContext.Session[sessionName] = null;
        }
    }


    public async Task<FormStateData> GetFormStateDataAsync()
    {
        if (_formStateData != null)
            return _formStateData;

        var tempFormData = new FormStateData(new Dictionary<string, object?>(), UserValues, PageState);
        var autoReloadFormFields = CurrentContext.Request.Form.ContainsFormValues();
        var values = await GridView.FormValuesService.GetFormValuesWithMergedValuesAsync(FormElement, tempFormData, autoReloadFormFields);

        if (!values.Any())
            values = DataPanel.Values as Dictionary<string,object?>;

        _formStateData = new FormStateData(values ?? new Dictionary<string, object?>(), UserValues, PageState);
        return _formStateData;
    }

    public Dictionary<string, object> GetRelationValuesFromForm()
    {
        var encryptedRelationValues = CurrentContext.Request.Form[$"form-view-relation-values-{FormElement.Name}"];

        if (encryptedRelationValues is null)
            return new Dictionary<string, object>();

        return EncryptionService.DecryptDictionary(encryptedRelationValues);
    }

    private async Task<bool> IsAuditLogEnabled()
    {
        var auditLogAction = FormElement.Options.GridToolbarActions.AuditLogGridToolbarAction;
        var formStateData = await GetFormStateDataAsync();
        return ExpressionsService.GetBoolValue(auditLogAction.VisibleExpression, formStateData);
    }
    
    private bool ContainsGridAction() => !string.IsNullOrEmpty(CurrentContext.Request.Form[$"grid-view-action-map-{Name}"]);
    
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
        return ComponentFactory.Html.Title.Create(FormElement,formStateData);
    }

    private async Task<JJTitle> GetTitleAsync()
    {
        return ComponentFactory.Html.Title.Create(FormElement, await GetFormStateDataAsync());
    }
    
    #region "Legacy inherited GridView compatibility"

    [Obsolete("Please use GridView.GridActions")]
    public GridTableActionList GridActions => GridView.GridTableActions;

    [Obsolete("Please use GridView.ToolBarActions")]
    public GridToolbarActionList ToolBarActions => GridView.ToolbarActions;

    [Obsolete("Please use GridView.SetCurrentFilter")]
    public void SetCurrentFilter(string filterKey, object filterValue)
    {
        GridView.SetCurrentFilter(filterKey, filterValue);
    }

    [Obsolete("Please use GridView.GetSelectedGridValues")]
    public List<Dictionary<string, object>> GetSelectedGridValues() => GridView.GetSelectedGridValues();
    

    [Obsolete("Please use GridView.ClearSelectedGridValues")]
    public void ClearSelectedGridValues()
    {
        GridView.ClearSelectedGridValues();
    }

    [Obsolete("Please use GridView.EnableMultiSelect")]
    public bool EnableMultiSelect
    {
        get => GridView.EnableMultiSelect;
        set => GridView.EnableMultiSelect = value;
    }
    
    [Obsolete("Please use GridView.EnableEditMode")]
    public bool EnableEditMode
    {
        get => GridView.EnableEditMode;
        set => GridView.EnableEditMode = value;
    }
    
    [Obsolete("Please use GridView.CurrentSettings")]
    public GridSettings CurrentSettings
    {
        get => GridView.CurrentSettings;
        set => GridView.CurrentSettings = value;
    }
    
    [Obsolete("Please use GridView.EnableFilter")]
    public bool EnableFilter
    {
        get => GridView.EnableFilter;
        set => GridView.EnableFilter = value;
    }

    [Obsolete("Please use GridView.ShowToolbar")]
    public bool ShowToolbar
    {
        get => GridView.ShowToolbar;
        set => GridView.ShowToolbar = value;
    }

    [Obsolete("Please use GridView.ShowPagging")]
    public bool ShowPagging
    {
        get => GridView.ShowPagging;
        set => GridView.ShowPagging = value;
    }

    #if NETFRAMEWORK
    [Obsolete("Please use GridView.GetGridValuesAsync()")]
    public List<Dictionary<string,object?>> GetGridValues()
    {
        return AsyncHelper.RunSync(()=>GridView.GetGridValuesAsync()) ?? [];
    }
    #endif
    
    [Obsolete("Please use GridView.SetGridOptions()")]
    public void SetGridOptions(GridUI options)
    {
         GridView.SetGridOptions(options);
    }

    [Obsolete("Please use GridView.ExportAction")]
    public ExportAction ExportAction => GridView.ExportAction;
    
    #endregion

    public static implicit operator JJGridView(JJFormView formView) => formView.GridView;
    public static implicit operator JJDataPanel(JJFormView formView) => formView.DataPanel;
}