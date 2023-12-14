#nullable enable

// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable EventNeverSubscribedTo.Global

using System;
using System.Collections.Generic;
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
using JJMasterData.Core.Configuration.Options;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.DataDictionary.Models.Actions;
using JJMasterData.Core.DataDictionary.Repository.Abstractions;
using JJMasterData.Core.DataManager;
using JJMasterData.Core.DataManager.Expressions;
using JJMasterData.Core.DataManager.Models;
using JJMasterData.Core.DataManager.Services;
using JJMasterData.Core.Events.Args;
using JJMasterData.Core.Extensions;
using JJMasterData.Core.Http.Abstractions;
using JJMasterData.Core.UI.Events.Args;
using JJMasterData.Core.UI.Html;
using JJMasterData.Core.UI.Routing;
using Microsoft.Extensions.Localization;
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
    private IDictionary<string, object> _relationValues = new Dictionary<string, object>();
    private RouteContext? _routeContext;
    private FormStateData? _formStateData;

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
    /// Url a ser direcionada após os eventos de Update/Delete/Save
    /// </summary>
    private string? UrlRedirect { get; set; }


    /// <summary>
    /// Id do usuário Atual
    /// </summary>
    /// <remarks>
    /// Se a variavel não for atribuida diretamente,
    /// o sistema tenta recuperar em UserValues ou nas variaveis de Sessão
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
            _dataImportation.OnAfterDeleteAsync += OnAfterDeleteAsync;
            _dataImportation.OnAfterInsertAsync += OnAfterInsertAsync;
            _dataImportation.OnAfterUpdateAsync += OnAfterUpdateAsync;

            return _dataImportation;
        }
    }

    /// <summary>
    /// Configuração do painel com os campos do formulário
    /// </summary>
    public JJDataPanel DataPanel
    {
        get
        {
            _dataPanel ??= ComponentFactory.DataPanel.Create(FormElement);
            _dataPanel.ParentComponentName = Name;
            _dataPanel.FormUI = FormElement.Options.Form;
            _dataPanel.UserValues = UserValues;
            _dataPanel.RenderPanelGroup = true;
            _dataPanel.PageState = PageState;

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
    public IDictionary<string, object> RelationValues
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

            _gridView.ToolBarActions.Add(new DeleteSelectedRowsAction());

            return _gridView;
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
    internal PageState PanelState
    {
        get
        {
            if (CurrentContext.Request.Form[$"form-view-panel-state-{Name}"] != null && _panelState is null)
                _panelState = (PageState)int.Parse(CurrentContext.Request.Form[$"form-view-panel-state-{Name}"]);

            return _panelState ?? PageState.View;
        }
        set => _panelState = value;
    }

    private ActionMap? CurrentActionMap
    {
        get
        {
            if (_currentActionMap != null)
                return _currentActionMap;

            string encryptedActionMap = CurrentContext.Request.Form[$"form-view-action-map-{Name}"];
            if (string.IsNullOrEmpty(encryptedActionMap))
                return null;

            _currentActionMap = EncryptionService.DecryptActionMap(encryptedActionMap);
            return _currentActionMap;
        }
    }

    private BasicAction? CurrentAction
    {
        get
        {
            if (_currentAction != null)
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
    
    internal IHttpContext CurrentContext { get; }
    internal IFormValues FormValues => CurrentContext.Request.Form;
    public IQueryString QueryString => CurrentContext.Request.QueryString;
    internal IEntityRepository EntityRepository { get; }
    internal FieldValuesService FieldValuesService { get; }
    internal ExpressionsService ExpressionsService { get; }
    private IEnumerable<IPluginHandler> PluginHandlers { get; }
    private IOptions<MasterDataCoreOptions> Options { get; }
    private IStringLocalizer<MasterDataResources> StringLocalizer { get; }
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
        childFormView.FormElement.ParentName = RouteContext.ParentElementName;
        childFormView.UserValues = UserValues;
        childFormView.RelationValues = childFormView.GetRelationValuesFromForm();
        childFormView.DataPanel.FieldNamePrefix = $"{childFormView.DataPanel.Name}_";
        
        var isInsertSelection = PageState is PageState.Insert &&
                                GridView.ToolBarActions.InsertAction.ElementNameToSelect ==
                                childFormView.FormElement.Name;
        
        childFormView.ShowTitle = isInsertSelection;
        
        if (PageState is PageState.View)
            childFormView.DisableActionsAtViewMode();

        if (!isInsertSelection)
            return childFormView;

        childFormView.GridView.GridActions.Add(new InsertSelectionAction());
        childFormView.GridView.ToolBarActions.Add(GetInsertSelectionBackAction());
        
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
                return ComponentFactory.Downloader.Create().GetDirectDownloadFromUrl();
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
        IDictionary<string, object?>? values;
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
        var values = await GetFormValuesAsync();
        
        var errors = PageState is PageState.Insert
            ? await InsertFormValuesAsync(values)
            : await UpdateFormValuesAsync(values);

        if (errors.Count != 0)
            return await GetFormResult(new FormContext(values, errors, PageState), true);

        if (!string.IsNullOrEmpty(UrlRedirect))
            return new RedirectComponentResult(UrlRedirect!);
        

        if (PageState is PageState.Insert && GridView.ToolBarActions.InsertAction.ReopenForm)
        {
            var formResult = await GetFormResult(new FormContext(RelationValues!, PageState.Insert), false);

            if (formResult is HtmlComponentResult htmlComponent)
            {
                AppendInsertSuccessAlert(htmlComponent.HtmlBuilder);

                return htmlComponent;
            }

            return formResult;
        }

        if (ContainsRelationships())
        {
            PanelState = PageState.View;
            return await GetFormResult(new FormContext(values, PageState), false);
        }

        PageState = PageState.List;

        if (ComponentContext is ComponentContext.Modal)
        {
            return new JsonComponentResult(new { closeModal = true });
        }

        return await GridView.GetResultAsync();
    }

    internal bool ContainsRelationships()
    {
        return CurrentContext.Request.Form[$"form-view-panel-state-{Name}"] != null;
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

        if (result is HtmlComponentResult htmlComponent && ComponentContext is not ComponentContext.Modal)
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
        html.AppendHiddenInput($"form-view-page-state-{Name}", ((int)PageState).ToString());
        html.AppendHiddenInput($"form-view-action-map-{Name}",
            EncryptionService.EncryptActionMap(CurrentActionMap));
        html.AppendHiddenInput($"form-view-relation-values-{FormElement.Name}",
            EncryptionService.EncryptDictionary(RelationValues));
    }

    private async Task<ComponentResult> GetSqlCommandActionResult()
    {
        JJMessageBox? messageBox = null;
        var sqlAction = CurrentActionMap!.GetAction<SqlCommandAction>(FormElement);
        try
        {
            var sqlCommand = ExpressionsService.ReplaceExpressionWithParsedValues(sqlAction.SqlCommand, await GetFormStateDataAsync());

            await EntityRepository.SetCommandAsync(new DataAccessCommand(sqlCommand!));
        }
        catch (Exception ex)
        {
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
            result = PluginActionResult.Error(StringLocalizer["Error"], exception.Message);
        }
        
        var formResult = await GetDefaultResult(formValues);

        if (formResult is HtmlComponentResult htmlComponentResult)
        {
            htmlComponentResult.HtmlBuilder.AppendScriptIf(!string.IsNullOrEmpty(result.JsCallback),result.JsCallback!);
        }

        return formResult;
    }

    private Task<PluginActionResult> GetPluginActionResult(IDictionary<string, object?> formValues)
    {
        var pluginAction = (PluginAction)CurrentAction!;

        return GetPluginActionResult(pluginAction, formValues, CurrentActionMap!.FieldName);
    }

    private Task<PluginActionResult> GetPluginActionResult(PluginAction pluginAction,
        IDictionary<string, object?> values, string? fieldName)
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

    private async Task<ComponentResult> GetUpdateResult()
    {
        bool autoReloadFields;
        IDictionary<string, object?>? values;
        if (PageState is PageState.Update && ComponentContext is not ComponentContext.Modal)
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

    private async Task<ComponentResult> GetDefaultResult(IDictionary<string, object?>? formValues = null)
    {
        switch (PageState)
        {
            case PageState.Insert:
                return await GetFormResult(new FormContext((IDictionary<string, object?>)RelationValues, PageState),
                    false);
            case PageState.Update:
                formValues ??= await GetFormValuesAsync();
                return await GetFormResult(new FormContext(formValues, PageState), true);
            default:
                return await GetGridViewResult();
        }
    }

    private async Task<ComponentResult> GetInsertResult()
    {
        var insertAction = GridView.ToolBarActions.InsertAction;
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
            return await GetFormResult(new FormContext(RelationValues!, PageState.Insert), false);

        return await GetInsertSelectionListResult();
    }

    private async Task<ComponentResult> GetInsertSelectionListResult()
    {
        var insertAction = GridView.ToolBarActions.InsertAction;
        var html = new HtmlBuilder(HtmlTag.Div);
        html.AppendHiddenInput($"form-view-insert-selection-values-{Name}");
        var formElement = await DataDictionaryRepository.GetFormElementAsync(insertAction.ElementNameToSelect);
        formElement.ParentName = FormElement.Name;

        var formView = ComponentFactory.FormView.Create(formElement);
        formView.UserValues = UserValues;
        formView.GridView.OnRenderActionAsync += InsertSelectionOnRenderAction;
        
        formView.GridView.ToolBarActions.Add(GetInsertSelectionBackAction());

        formView.GridView.GridActions.Add(new InsertSelectionAction());

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
        
        var childElementName = GridView.ToolBarActions.InsertAction.ElementNameToSelect;
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
        script.Append($"document.getElementById('form-view-page-state-{Name}').value = '{(int)PageState.List}'; ");
        script.Append($"document.getElementById('form-view-action-map-{Name}').value = null; ");
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

        AuditLogView.GridView.AddToolBarAction(goBackAction);
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
            html.AppendComponent(GridView.GetTitle(UserValues));

        PageState = PageState.Import;

        DataImportation.UserValues = UserValues;
        DataImportation.BackButton.OnClientClick = "DataImportationModal.getInstance().hide()";
        DataImportation.ProcessOptions = action.ProcessOptions;
        DataImportation.EnableAuditLog =ExpressionsService.GetBoolValue(
                GridView.ToolBarActions.AuditLogGridToolbarAction.VisibleExpression, formStateData);

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

        var parentPanel = DataPanel;
        parentPanel.PageState = pageState;
        parentPanel.Errors = errors;
        parentPanel.Values = values;
        parentPanel.AutoReloadFormFields = autoReloadFormFields;

        if (!visibleRelationships.Any() || visibleRelationships.Count == 1)
        {
            return GetParentPanelResult(parentPanel, values);
        }

        return GetRelationshipLayoutResult(visibleRelationships, values);
    }

    private async Task<ComponentResult> GetRelationshipLayoutResult(List<FormElementRelationship> visibleRelationships,
        IDictionary<string, object?> values)
    {
        var html = new HtmlBuilder(HtmlTag.Div);
        if (ShowTitle)
            html.AppendComponent(GridView.GetTitle(values));

        var layout = new FormViewRelationshipLayout(this);

        var formToolbarActions = FormElement.Options.FormToolbarActions;

        if (PageState is PageState.Update)
        {
            if (PanelState is PageState.View)
            {
                formToolbarActions.FormEditAction.SetVisible(true);
                formToolbarActions.RemoveAll(a => a is SaveAction or CancelAction);
            }
            else
            {
                formToolbarActions.FormEditAction.SetVisible(false);
            }
        }
        else if (PageState is PageState.View)
        {
            FormElement.Options.FormToolbarActions.AuditLogFormToolbarAction.SetVisible(await IsAuditLogEnabled());
        }

        FormElement.Options.FormToolbarActions.BackAction.SetVisible(true);

        var topActions = GetTopToolbarActions(FormElement);

        html.AppendComponent(await GetFormToolbarAsync(topActions));

        var relationshipsResult = await layout.GetRelationshipsResult(visibleRelationships);

        if (relationshipsResult is RenderedComponentResult renderedComponentResult)
        {
            html.Append((HtmlBuilder?)renderedComponentResult.HtmlBuilder);
        }

        var bottomActions = FormElement.Options.FormToolbarActions
            .Where(a => a.Location is FormToolbarActionLocation.Bottom).ToList();

        html.AppendComponent(await GetFormToolbarAsync(bottomActions));

        if (ComponentContext is ComponentContext.Modal)
        {
            html.AppendScript($"document.getElementById('form-view-page-state-{Name}').value={(int)PageState}");
            return new ContentComponentResult(html);
        }

        return new RenderedComponentResult(html);
    }

    private async Task<ComponentResult> GetParentPanelResult(JJDataPanel parentPanel,
        IDictionary<string, object?> values)
    {
        FormElement.Options.FormToolbarActions.FormEditAction.SetVisible(false);
        
        var panelHtml = await GetParentPanelHtml(parentPanel);
        panelHtml.AppendScript($"document.getElementById('form-view-page-state-{Name}').value={(int)PageState}");
        
        if (ComponentContext is ComponentContext.Modal)
            return new ContentComponentResult(panelHtml);

        if (ShowTitle)
            panelHtml.Prepend(GridView.GetTitle(values).GetHtmlBuilder());

        return new RenderedComponentResult(panelHtml);
    }

    private List<FormElementRelationship> GetVisibleRelationships(IDictionary<string, object?> values,
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

    internal async Task<HtmlBuilder> GetParentPanelHtml(JJDataPanel panel)
    {
        var formHtml = new HtmlBuilder(HtmlTag.Div);

        if (PageState is PageState.View)
        {
            FormElement.Options.FormToolbarActions.AuditLogFormToolbarAction.SetVisible(await IsAuditLogEnabled());
        }

        var topToolbarActions = GetTopToolbarActions(FormElement);

        formHtml.AppendComponent(await GetFormToolbarAsync(topToolbarActions));

        panel.Values = await panel.GetFormValuesAsync();
        
        var parentPanelHtml = await panel.GetPanelHtmlBuilderAsync();

        var panelAndBottomToolbarActions = GetPanelToolbarActions(FormElement);
        panelAndBottomToolbarActions.AddRange(GetBottomToolbarActions(FormElement));

        var toolbar = await GetFormToolbarAsync(panelAndBottomToolbarActions);

        formHtml.Append(parentPanelHtml);

        formHtml.AppendComponent(toolbar);

        if (panel.Errors.Any())
            formHtml.AppendComponent(ComponentFactory.Html.ValidationSummary.Create(panel.Errors));

        return formHtml;
    }

    internal async Task<HtmlBuilder> GetRelationshipParentPanelHtml(JJDataPanel panel)
    {
        var formHtml = new HtmlBuilder(HtmlTag.Div);

        panel.PageState = PanelState;

        var parentPanelHtml = await panel.GetPanelHtmlBuilderAsync();

        var panelToolbarActions = GetPanelToolbarActions(panel.FormElement);

        var toolbar = await GetFormToolbarAsync(panelToolbarActions);

        formHtml.Append(parentPanelHtml);

        formHtml.AppendComponent(toolbar);

        if (panel.Errors.Any())
            formHtml.AppendComponent(ComponentFactory.Html.ValidationSummary.Create(panel.Errors));

        formHtml.AppendHiddenInput($"form-view-panel-state-{Name}", ((int)PanelState).ToString());

        return formHtml;
    }

    private static List<BasicAction> GetPanelToolbarActions(FormElement formElement)
    {
        var toolbarActions = formElement.Options.FormToolbarActions
            .Where(a => a.Location == FormToolbarActionLocation.Panel);

        return toolbarActions.ToList();
    }

    private static List<BasicAction> GetTopToolbarActions(FormElement formElement)
    {
        var toolbarActions = formElement.Options.FormToolbarActions
            .Where(a => a.Location == FormToolbarActionLocation.Top);

        return toolbarActions.ToList();
    }

    private static List<BasicAction> GetBottomToolbarActions(FormElement formElement)
    {
        var toolbarActions = formElement.Options.FormToolbarActions
            .Where(a => a.Location == FormToolbarActionLocation.Bottom);

        return toolbarActions.ToList();
    }

    private async Task<JJToolbar> GetAuditLogBottomBar()
    {
        var hideAuditLogButton =
            await ComponentFactory.ActionButton.CreateFormToolbarButton(
                FormElement.Options.FormToolbarActions.BackAction, this);

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

        foreach (var action in actions.Where(a => !a.IsGroup))
        {
            if (action is SaveAction saveAction)
            {
                saveAction.EnterKeyBehavior = DataPanel.FormUI.EnterKey;
            }

            var factory = ComponentFactory.ActionButton;


            var linkButton = await factory.CreateFormToolbarButton(action, this);
            toolbar.Items.Add(linkButton.GetHtmlBuilder());
        }

        if (actions.Any(a => a.IsGroup))
        {
            var btnGroup = new JJLinkButtonGroup
            {
                CaretText = StringLocalizer["More"]
            };

            foreach (var groupedAction in actions.Where(a => a.IsGroup).ToList())
            {
                btnGroup.ShowAsButton = groupedAction.ShowAsButton;
                var factory = ComponentFactory.ActionButton;
                var linkButton = await factory.CreateFormToolbarButton(groupedAction, this);
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
    public async Task<IDictionary<string, string>> InsertFormValuesAsync(
        IDictionary<string, object?> values,
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
    public async Task<IDictionary<string, string>> UpdateFormValuesAsync(IDictionary<string, object?> values)
    {
        var result = await FormService.UpdateAsync(FormElement, values,
            new DataContext(CurrentContext.Request, DataContextSource.Form, UserId));
        UrlRedirect = result.UrlRedirect;
        return result.Errors;
    }

    public async Task<IDictionary<string, string>> DeleteFormValuesAsync(IDictionary<string, object?>? filter)
    {
        var values =
            await FieldValuesService.MergeWithExpressionValuesAsync(FormElement,  new FormStateData(filter!, UserValues, PageState.Delete), true);
        var result = await FormService.DeleteAsync(FormElement, values,
            new DataContext(CurrentContext.Request, DataContextSource.Form, UserId));
        UrlRedirect = result.UrlRedirect;
        return result.Errors;
    }


    public async Task<IDictionary<string, object?>> GetFormValuesAsync()
    {
        var panel = DataPanel;
        var values = await panel.GetFormValuesAsync();

        if (!RelationValues.Any())
            return values;

        DataHelper.CopyIntoDictionary(values, RelationValues!, true);

        return values;
    }

    public IDictionary<string, string> ValidateFields(IDictionary<string, object> values,
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
        {
            values = DataPanel.Values as Dictionary<string,object?>;
        }

        _formStateData = new FormStateData(values ?? new Dictionary<string, object?>(), UserValues, PageState);
        return _formStateData;
    }

    public IDictionary<string, object> GetRelationValuesFromForm()
    {
        var encryptedRelationValues = CurrentContext.Request.Form[$"form-view-relation-values-{FormElement.Name}"];

        if (encryptedRelationValues is null)
            return new Dictionary<string, object>();

        return EncryptionService.DecryptDictionary(encryptedRelationValues);
    }

    public void SetRelationshipPageState(RelationshipViewType relationshipViewType)
    {
        var relationshipPageState =
            relationshipViewType == RelationshipViewType.List ? PageState.List : PageState.Update;

        if (CurrentContext.Request.Form.ContainsFormValues())
        {
            var pageState = CurrentContext.Request.Form[$"form-view-page-state-{Name}"];
            PageState = pageState != null ? (PageState)int.Parse(pageState) : relationshipPageState;
        }
        else
        {
            PageState = relationshipPageState;
        }
    }

    private async Task<bool> IsAuditLogEnabled()
    {
        var auditLogAction = FormElement.Options.GridToolbarActions.AuditLogGridToolbarAction;
        var formStateData = await GetFormStateDataAsync();
        return ExpressionsService.GetBoolValue(auditLogAction.VisibleExpression, formStateData);
    }

    internal void DisableActionsAtViewMode()
    {
        foreach (var action in FormElement.Options.GridTableActions)
        {
            if (action.GetType() != typeof(ViewAction))
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
            IsModal = ComponentContext is ComponentContext.Modal,
            ParentComponentName = Name
        };
    }
    
    public async Task<ActionContext> GetActionContextAsync(BasicAction action)
    {
        return new ActionContext
        {
            Action = action,
            FormElement = FormElement,
            FormStateData = await GetFormStateDataAsync(),
            IsModal = ComponentContext is ComponentContext.Modal,
            ParentComponentName = Name
        };
    }
    
#if NETFRAMEWORK
    [Obsolete("Please use GetCurrentFilterAsync")]
    public IDictionary<string, object?> CurrentFilter
    {
        get
        {
            return AsyncHelper.RunSync(()=>GridView.GetCurrentFilterAsync());
        }
    }
#endif
    
    
    #region "Legacy inherited GridView compatibility"

    [Obsolete("Please use GridView.GridActions")]
    public GridTableActionList GridActions => GridView.GridActions;

    [Obsolete("Please use GridView.ToolBarActions")]
    public GridToolbarActionList ToolBarActions => GridView.ToolBarActions;

    [Obsolete("Please use GridView.SetCurrentFilter")]
    public void SetCurrentFilter(string filterKey, object filterValue)
    {
        GridView.SetCurrentFilter(filterKey, filterValue);
    }

    [Obsolete("Please use GridView.GetSelectedGridValues")]
    public List<Dictionary<string, object>> GetSelectedGridValues() => GridView.GetSelectedGridValues();

    [Obsolete("Please use GridView.AddToolBarAction")]
    public void AddToolBarAction(UserCreatedAction userCreatedAction)
    {
        switch (userCreatedAction)
        {
            case UrlRedirectAction urlRedirectAction:
                GridView.AddToolBarAction(urlRedirectAction);
                break;
            case SqlCommandAction sqlCommandAction:
                GridView.AddToolBarAction(sqlCommandAction);
                break;
            case ScriptAction scriptAction:
                GridView.AddToolBarAction(scriptAction);
                break;
            case InternalAction internalAction:
                GridView.AddToolBarAction(internalAction);
                break;
        }
    }

    [Obsolete("Please use GridView.AddGridAction")]
    public void AddGridAction(UserCreatedAction userCreatedAction)
    {
        switch (userCreatedAction)
        {
            case UrlRedirectAction urlRedirectAction:
                GridView.AddGridAction(urlRedirectAction);
                break;
            case SqlCommandAction sqlCommandAction:
                GridView.AddGridAction(sqlCommandAction);
                break;
            case ScriptAction scriptAction:
                GridView.AddGridAction(scriptAction);
                break;
            case InternalAction internalAction:
                GridView.AddGridAction(internalAction);
                break;
        }
    }

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
        return AsyncHelper.RunSync(()=>GridView.GetGridValuesAsync()) ?? new List<Dictionary<string, object?>>();
    }
    #endif
    
    [Obsolete("Please use GridView.SetGridOptions()")]
    public void SetGridOptions(GridUI options)
    {
         GridView.SetGridOptions(options);
    }
    


    [Obsolete("Please use GridView.AddGridAction()")]
    public void AddGridAction(BasicAction action)
    {
        switch (action)
        {
            case SqlCommandAction sqlCommandAction:
                GridView.AddGridAction(sqlCommandAction);
                break;
            case UrlRedirectAction url:
                GridView.AddGridAction(url);
                break;
            case InternalAction internalAction:
                GridView.AddGridAction(internalAction);
                break;
            case ScriptAction scriptAction:
                GridView.AddGridAction(scriptAction);
                break;
        }
    }
    
    [Obsolete("Please use GridView.AddToolBarAction()")]
    public void AddToolBarAction(BasicAction action)
    {
        switch (action)
        {
            case SqlCommandAction sqlCommandAction:
                GridView.AddToolBarAction(sqlCommandAction);
                break;
            case UrlRedirectAction url:
                GridView.AddToolBarAction(url);
                break;
            case InternalAction internalAction:
                GridView.AddToolBarAction(internalAction);
                break;
            case ScriptAction scriptAction:
                GridView.AddToolBarAction(scriptAction);
                break;
        }
    }


    [Obsolete("Please use GridView.ExportAction")]
    public ExportAction ExportAction
    {
        get
        {
            return GridView.ExportAction;
        }
    }
    
    [Obsolete("Please use GridView.GridTableActions.EditAction")]
    public EditAction EditAction
    {
        get
        {
            return FormElement.Options.GridTableActions.EditAction;
        }
    }
    
    [Obsolete("Please use GridView.GridTableActions.ViewAction")]
    public ViewAction ViewAction
    {
        get
        {
            return FormElement.Options.GridTableActions.ViewAction;
        }
    }
    
    [Obsolete("Please use GridView.GridTableActions.DeleteAction")]
    public DeleteAction DeleteAction
    {
        get
        {
            return FormElement.Options.GridTableActions.DeleteAction;
        }
    }
    [Obsolete("Please use GridView.GridToolbarActions.InsertAction")]
    public InsertAction InsertAction
    {
        get
        {
            return FormElement.Options.GridToolbarActions.InsertAction;
        }
    }
    
    [Obsolete("Please use GridView.IsExportPost")]
    public bool IsExportPost()
    {
        return GridView.IsExportPost();
    }

    [Obsolete("Please use GridView.DataSource")]
    public IList<Dictionary<string, object?>>? DataSource
    {
        get => GridView.DataSource;
        set => GridView.DataSource = value;
    }
    #endregion

    public static implicit operator JJGridView(JJFormView formView) => formView.GridView;
    public static implicit operator JJDataPanel(JJFormView formView) => formView.DataPanel;
}