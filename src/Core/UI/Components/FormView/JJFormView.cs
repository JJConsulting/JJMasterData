#nullable enable

// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable EventNeverSubscribedTo.Global

using JJMasterData.Commons.Cryptography;
using JJMasterData.Commons.Data.Entity;
using JJMasterData.Commons.Data.Entity.Abstractions;
using JJMasterData.Commons.Localization;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataDictionary.Actions;
using JJMasterData.Core.DataDictionary.Actions.Abstractions;
using JJMasterData.Core.DataDictionary.Actions.FormToolbar;
using JJMasterData.Core.DataDictionary.Actions.GridTable;
using JJMasterData.Core.DataDictionary.Actions.GridToolbar;
using JJMasterData.Core.DataDictionary.Actions.UserCreated;
using JJMasterData.Core.DataManager;
using JJMasterData.Core.Extensions;
using JJMasterData.Core.FormEvents.Args;
using JJMasterData.Core.Web.Factories;
using JJMasterData.Core.Web.Html;
using JJMasterData.Core.Web.Http.Abstractions;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JJMasterData.Commons.Tasks;
using JJMasterData.Core.DataDictionary.Repository.Abstractions;
using JJMasterData.Core.DataManager.Expressions.Abstractions;
using JJMasterData.Core.DataManager.Models;
using JJMasterData.Core.UI.Components;


#if NET48
using JJMasterData.Commons.Configuration;
#endif

namespace JJMasterData.Core.Web.Components;

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

    public event EventHandler<FormBeforeActionEventArgs>? OnBeforeInsert;
    public event EventHandler<FormBeforeActionEventArgs>? OnBeforeUpdate;
    public event EventHandler<FormBeforeActionEventArgs>? OnBeforeDelete;
    public event EventHandler<FormAfterActionEventArgs>? OnAfterInsert;
    public event EventHandler<FormAfterActionEventArgs>? OnAfterUpdate;
    public event EventHandler<FormAfterActionEventArgs>? OnAfterDelete;


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
    private ActionMap? _currentActionMap;
    private JJAuditLogView? _auditLogView;
    private JJDataImportation? _dataImportation;
    private string? _userId;
    private bool? _showTitle;
    private PageState? _pageState;
    private IDictionary<string, object> _relationValues = new Dictionary<string, object>();

    #endregion

    #region "Properties"

    private JJAuditLogView AuditLogView =>
        _auditLogView ??= ComponentFactory.AuditLog.Create(FormElement);

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
    private JJDataImportation DataImportation
    {
        get
        {
            if (_dataImportation != null)
                return _dataImportation;

            _dataImportation = GridView.DataImportation;
            _dataImportation.IsExternalRoute = IsExternalRoute;
            _dataImportation.OnAfterDelete += OnAfterDelete;
            _dataImportation.OnAfterInsert += OnAfterInsert;
            _dataImportation.OnAfterUpdate += OnAfterUpdate;

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
            _dataPanel.Name = "data-panel-" + FormElement.Name.ToLower();
            _dataPanel.FormUI = FormElement.Options.Form;
            _dataPanel.UserValues = UserValues;
            _dataPanel.RenderPanelGroup = true;
            _dataPanel.IsExternalRoute = IsExternalRoute;
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
                var encryptedRelationValues = CurrentContext.Request.GetFormValue($"form-view-relation-values-{Name}");

                if (encryptedRelationValues is null)
                    return _relationValues;

                _relationValues = EncryptionService.DecryptDictionary(encryptedRelationValues);
                GridView.RelationValues = _relationValues;
            }

            return _relationValues;
        }
        set
        {
            GridView.RelationValues = value;
            _relationValues = value;
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
            _gridView.Name = Name.ToLower();
            _gridView.FormElement = FormElement;
            _gridView.UserValues = UserValues;
            _gridView.IsExternalRoute = IsExternalRoute;
            _gridView.ShowTitle = true;

            _gridView.ToolBarActions.Add(new DeleteSelectedRowsAction());
            _gridView.ToolBarActions.Add(new LogAction());

            return _gridView;
        }
    }

    public PageState PageState
    {
        get
        {
            if (CurrentContext.Request["form-view-page-state-" + Name] != null && _pageState is null)
                _pageState = (PageState)int.Parse(CurrentContext.Request["form-view-page-state-" + Name]);

            return _pageState ?? PageState.List;
        }
        internal set => _pageState = value;
    }

    private ActionMap? CurrentActionMap
    {
        get
        {
            if (_currentActionMap != null)
                return _currentActionMap;

            string encryptedActionMap = CurrentContext.Request["form-view-action-map-" + Name.ToLower()];
            if (string.IsNullOrEmpty(encryptedActionMap))
                return null;

            _currentActionMap = EncryptionService.DecryptActionMap(encryptedActionMap);
            return _currentActionMap;
        }
    }
    
    public bool ShowTitle
    {
        get
        {
            _showTitle ??= FormElement.Options.Grid.ShowTitle;
            return _showTitle.Value;
        }
        set => _showTitle = value;
    }

    internal IHttpContext CurrentContext { get; }
    internal IEntityRepository EntityRepository { get; }
    internal IFieldValuesService FieldValuesService { get; }
    internal IExpressionsService ExpressionsService { get; }
    private IStringLocalizer<JJMasterDataResources> StringLocalizer { get; }
    internal IDataDictionaryRepository DataDictionaryRepository { get; }
    internal IFormService FormService { get; }
    internal IComponentFactory ComponentFactory { get; }

    #endregion

    #region "Constructors"

#if NET48
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    private JJFormView() : base(StaticServiceLocator.Provider.GetScopedDependentService<IQueryString>(), StaticServiceLocator.Provider.GetScopedDependentService<IEncryptionService>())
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    {
        CurrentContext = StaticServiceLocator.Provider.GetScopedDependentService<IHttpContext>();
        EntityRepository = StaticServiceLocator.Provider.GetScopedDependentService<IEntityRepository>();
        ComponentFactory = StaticServiceLocator.Provider.GetScopedDependentService<IComponentFactory>();
        FormService = StaticServiceLocator.Provider.GetScopedDependentService<IFormService>();
        FieldValuesService = StaticServiceLocator.Provider.GetScopedDependentService<IFieldValuesService>();
        ExpressionsService = StaticServiceLocator.Provider.GetScopedDependentService<IExpressionsService>();
        StringLocalizer = StaticServiceLocator.Provider.GetScopedDependentService<IStringLocalizer<JJMasterDataResources>>();
        DataDictionaryRepository = StaticServiceLocator.Provider.GetScopedDependentService<IDataDictionaryRepository>();
    }

    public JJFormView(string elementName) : this()
    {
        var dataDictionaryRepository = StaticServiceLocator.Provider.GetScopedDependentService<IDataDictionaryRepository>();
        var factory = StaticServiceLocator.Provider.GetScopedDependentService<FormViewFactory>();
        FormElement = dataDictionaryRepository.GetMetadataAsync(elementName).GetAwaiter().GetResult();
        IsExternalRoute = false;
        factory.SetFormViewParamsAsync(this, FormElement).GetAwaiter().GetResult();
    }

    public JJFormView(FormElement formElement) : this()
    {
        IsExternalRoute = false;
        FormElement = formElement;
    }
#endif

    internal JJFormView(
        FormElement formElement,
        IHttpContext currentContext,
        IEntityRepository entityRepository,
        IDataDictionaryRepository dataDictionaryRepository,
        IFormService formService,
        IEncryptionService encryptionService,
        IFieldValuesService fieldValuesService,
        IExpressionsService expressionsService,
        IStringLocalizer<JJMasterDataResources> stringLocalizer,
        IComponentFactory componentFactory) : base(currentContext.Request.QueryString, encryptionService)
    {
        Name = "jj-" + formElement.Name.ToLower();
        FormElement = formElement;
        CurrentContext = currentContext;
        EntityRepository = entityRepository;
        FormService = formService;
        FieldValuesService = fieldValuesService;
        ExpressionsService = expressionsService;
        StringLocalizer = stringLocalizer;
        DataDictionaryRepository = dataDictionaryRepository;
        ComponentFactory = componentFactory;
    }

    #endregion

    protected override async Task<ComponentResult> BuildResultAsync()
    {
        if (!RouteContext.CanRender(FormElement.Name))
            return new EmptyComponentResult();

        if (RouteContext.IsCurrentFormElement(FormElement.Name))
            return await GetFormResultAsync();

        var formView = await ComponentFactory.FormView.CreateAsync(RouteContext.ElementName);

        return await formView.GetFormResultAsync();
    }

    
    internal async Task<ComponentResult> GetFormResultAsync()
    {
        if (ComponentContext is ComponentContext.GridViewReload)
            return await GridView.GetResultAsync();
        
        if (ComponentContext is ComponentContext.FileUpload)
            return await DataPanel.GetResultAsync();

        if (ComponentContext is ComponentContext.DownloadFile)
            return ComponentFactory.Downloader.Create().GetDirectDownloadFromUrl();

        if (ComponentContext is ComponentContext.SearchBox)
            return await DataPanel.GetResultAsync();

        if (ComponentContext is ComponentContext.GridViewFilterSearchBox)
            return await GridView.GetResultAsync();
        
        if (ComponentContext is ComponentContext.PanelReload)
        {
            return await GetReloadPanelResultAsync();
        }

        if (ComponentContext is ComponentContext.DataImportation or ComponentContext.FileUpload)
        {
            if (!DataImportation.Upload.Name.Equals("todo"))
                return new EmptyComponentResult();

            return await GetImportationResult();
        }

        if (ComponentContext is ComponentContext.UrlRedirect)
        {
            return await DataPanel.GetUrlRedirectResult(CurrentActionMap);
        }


        return await GetFormActionResult();
    }
    
    internal async Task<ComponentResult> GetReloadPanelResultAsync()
    {
        var filter = GridView.GetSelectedRowId();
        IDictionary<string, object?>? values;
        if (filter is { Count: > 0 })
            values = await EntityRepository.GetFieldsAsync(FormElement, filter);
        else
            values = await GetFormValuesAsync();

        DataPanel.Values = values;
        return await DataPanel.GetResultAsync();
    }


    private async Task<ComponentResult> GetSaveActionResult()
    {
        var values = await GetFormValuesAsync();
        var errors = PageState is PageState.Insert
            ? await InsertFormValuesAsync(values)
            : await UpdateFormValuesAsync(values);

        if (errors.Count == 0)
        {
            if (!string.IsNullOrEmpty(UrlRedirect))
            {
                return new RedirectComponentResult(UrlRedirect!);
            }

            if (GridView.ToolBarActions.InsertAction.ReopenForm)
            {
                PageState = PageState.Insert;

                var alert = new JJAlert
                {
                    Name = $"insert-message-panel{Name}",
                    Color = PanelColor.Success,
                    ShowIcon = true,
                    Icon = IconType.CheckCircleO
                };
                alert.Messages.Add(StringLocalizer["Record added successfully"]);
                var alertHtml = alert.GetHtmlBuilder();

                var formResult = await GetFormResult(new FormContext(RelationValues!, PageState.Insert), false);

                if (formResult is RenderedComponentResult renderedComponentResult)
                {
                    var htmlResult = renderedComponentResult.HtmlBuilder;
                    htmlResult.Append(HtmlTag.Div, div =>
                    {
                        div.WithAttribute("id", $"insert-panel{Name}")
                            .WithAttribute("style", "display:none")
                            .Append(alertHtml);
                    });
                    htmlResult.AppendScript($"JJViewHelper.showInsertSucess('{Name}');");

                    return new RenderedComponentResult(htmlResult);
                }

                return formResult;
            }

            PageState = PageState.List;

            if (ComponentContext is ComponentContext.Modal)
            {
                return new JsonComponentResult(new { closeModal = true });
            }

            return await GridView.GetResultAsync();
        }

        PageState = PageState.Insert;
        return await GetFormResult(new FormContext(values, errors, PageState), true);
    }

    private async Task<ComponentResult> GetCancelActionResult()
    {
        PageState = PageState.List;
        ClearTempFiles();
        return await GridView.GetResultAsync();
    }

    private async Task<ComponentResult> GetFormActionResult()
    {
        var currentAction = CurrentActionMap?.GetCurrentAction(FormElement);

        SetFormServiceEvents();

        var result = currentAction switch
        {
            ViewAction => await GetViewResult(),
            EditAction => await GetUpdateResult(),
            InsertAction => await GetInsertResult(),
            ImportAction => await GetImportationResult(),
            LogAction => await GetAuditLogResult(),
            DeleteAction => await GetDeleteResult(),
            DeleteSelectedRowsAction => await GetDeleteSelectedRowsResult(),
            SaveAction => await GetSaveActionResult(),
            CancelAction => await GetCancelActionResult(),
            _ => await GetDefaultResult()
        };

        if (result is not RenderedComponentResult renderedComponentResult)
            return result;

        var html = renderedComponentResult.HtmlBuilder;

        html.AppendHiddenInput($"form-view-page-state-{Name.ToLower()}", ((int)PageState).ToString());

        html.AppendHiddenInput($"form-view-action-map-{Name.ToLower()}", string.Empty);

        return new RenderedComponentResult(html);
    }


    private void SetFormServiceEvents()
    {
        FormService.OnBeforeInsert += OnBeforeInsert;
        FormService.OnBeforeDelete += OnBeforeDelete;
        FormService.OnBeforeUpdate += OnBeforeUpdate;

        FormService.OnAfterInsert += OnAfterInsert;
        FormService.OnAfterUpdate += OnAfterUpdate;
        FormService.OnAfterDelete += OnAfterDelete;

        FormService.OnBeforeInsertAsync += OnBeforeInsertAsync;
        FormService.OnBeforeDeleteAsync += OnBeforeDeleteAsync;
        FormService.OnBeforeUpdateAsync += OnBeforeUpdateAsync;

        FormService.OnAfterInsertAsync += OnAfterInsertAsync;
        FormService.OnAfterUpdateAsync += OnAfterUpdateAsync;
        FormService.OnAfterDeleteAsync += OnAfterDeleteAsync;
    }

    private async Task<ComponentResult> GetGridViewResult()
    {
        return await GridView.GetResultAsync();
    }

    private async Task<ComponentResult> GetUpdateResult()
    {
        bool autoReloadFields;
        IDictionary<string, object?>? values;
        if (PageState is PageState.Update)
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

    private async Task<ComponentResult> GetDefaultResult()
    {
        switch (PageState)
        {
            case PageState.Insert:
                return await GetFormResult(new FormContext((IDictionary<string, object?>)RelationValues, PageState),
                    false);
            case PageState.Update:
                var values = await GetFormValuesAsync();
                return await GetFormResult(new FormContext(values, PageState), true);
            default:
                return await GetGridViewResult();
        }
    }

    private async Task<ComponentResult> GetInsertResult()
    {
        var action = GridView.ToolBarActions.InsertAction;
        var formData = new FormStateData(RelationValues!, UserValues, PageState.List);
        
        bool isVisible = await ExpressionsService.GetBoolValueAsync(action.VisibleExpression, formData);
        if (!isVisible)
            throw new UnauthorizedAccessException(StringLocalizer["Insert action is not enabled"]);

        //todo: criar no dicionario as ações ELEMENTSEL e ELEMENTLIST 
        // if (formAction.Equals("ELEMENTSEL"))
        // {
        //     return await GetInsertSelectionResult();
        // }
        //
        // if (formAction.Equals("ELEMENTLIST"))
        // {
        //     PageState = PageState.Insert;
        //     return await GetInsertSelectionResult(action);
        // }

        if (PageState == PageState.Insert)
        {
            var formValues = await GetFormValuesAsync();
            return await GetFormResult(new FormContext(formValues, PageState), true);
        }

        PageState = PageState.Insert;

        if (string.IsNullOrEmpty(action.ElementNameToSelect))
            return await GetFormResult(new FormContext(RelationValues!, PageState.Insert), false);
        return await GetInsertSelectionResult(action);
    }

    private async Task<ComponentResult> GetInsertSelectionResult(InsertAction action)
    {
        var html = new HtmlBuilder(HtmlTag.Div);
        html.AppendHiddenInput($"form-view-current-action-{Name}", "ELEMENTLIST");
        html.AppendHiddenInput($"form-view-select-action-values{Name}", "");

        var formElement = await DataDictionaryRepository.GetMetadataAsync(action.ElementNameToSelect);
        var selectedForm = ComponentFactory.FormView.Create(formElement);
        selectedForm.UserValues = UserValues;
        selectedForm.Name = action.ElementNameToSelect;
        selectedForm.SetOptions(formElement.Options);

        var goBackScript = new StringBuilder();
        goBackScript.Append($"$('#form-view-page-state-{Name}').val('{((int)PageState.List).ToString()}'); ");
        goBackScript.AppendLine("$('form:first').submit(); ");

        var goBackAction = new ScriptAction
        {
            Name = "_jjgobacktion",
            Icon = IconType.ArrowLeft,
            Text = "Back",
            ShowAsButton = true,
            OnClientClick = goBackScript.ToString(),
            IsDefaultOption = true
        };
        selectedForm.GridView.AddToolBarAction(goBackAction);

        var selAction = new ScriptAction
        {
            Name = "_jjselaction",
            Icon = IconType.CaretRight,
            ToolTip = "Select",
            IsDefaultOption = true
        };
        selectedForm.GridView.AddGridAction(selAction);

        var result = await selectedForm.GetResultAsync();

        if (result is RenderedComponentResult renderedComponentResult)
        {
            html.Append(renderedComponentResult.HtmlBuilder);
        }
        else
        {
            return result;
        }

        return new RenderedComponentResult(html);
    }

    private async Task<ComponentResult> GetInsertSelectionResult()
    {
        string encryptedActionMap = CurrentContext.Request.GetFormValue("form-view-select-action-values" + Name);
        var actionMap = EncryptionService.DecryptActionMap(encryptedActionMap);
        var html = new HtmlBuilder(HtmlTag.Div);
        var formElement =
            await DataDictionaryRepository.GetMetadataAsync(GridView.ToolBarActions.InsertAction.ElementNameToSelect);
        var selValues = await EntityRepository.GetFieldsAsync(formElement, actionMap.PkFieldValues);
        var values =
            await FieldValuesService.MergeWithExpressionValuesAsync(formElement, selValues, PageState.Insert, true);
        var erros = await InsertFormValuesAsync(values, false);

        if (erros.Count > 0)
        {
            var sMsg = new StringBuilder();
            foreach (string err in erros.Values)
            {
                sMsg.Append(" - ");
                sMsg.Append(err);
                sMsg.Append("<br>");
            }

            html.AppendComponent(new JJMessageBox(sMsg.ToString(), MessageIcon.Warning));

            var insertSelectionResult = await GetInsertSelectionResult(GridView.ToolBarActions.InsertAction);

            if (insertSelectionResult is RenderedComponentResult renderedComponentResult)
            {
                html.Append(renderedComponentResult.HtmlBuilder);
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
                html.Append(renderedComponentResult.HtmlBuilder);
            }
            else
            {
                return result;
            }
        }

        return new RenderedComponentResult(html);
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
        try
        {
            var filter = CurrentActionMap?.PkFieldValues;

            var errors = await DeleteFormValuesAsync(filter);

            if (errors is { Count: > 0 })
            {
                var errorMessage = new StringBuilder();
                foreach (var err in errors)
                {
                    errorMessage.Append("- ");
                    errorMessage.Append(err.Value);
                    errorMessage.AppendLine("<br>");
                }

                html.AppendComponent(new JJMessageBox(errorMessage.ToString(), MessageIcon.Warning));
            }
            else
            {
                if (GridView.EnableMultiSelect)
                    GridView.ClearSelectedGridValues();
            }
        }
        catch (Exception ex)
        {
            html.AppendComponent(new JJMessageBox(ex.Message, MessageIcon.Error));
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
        var errorMessage = new StringBuilder();
        int errorCount = 0;
        int successCount = 0;

        try
        {
            var rows = GridView.GetSelectedGridValues();

            foreach (var row in rows)
            {
                var errors = await DeleteFormValuesAsync(row);

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

                html.AppendComponent(new JJMessageBox(message.ToString(), icon));

                GridView.ClearSelectedGridValues();
            }
        }
        catch (Exception ex)
        {
            html.AppendComponent(new JJMessageBox(ex.Message, MessageIcon.Error));
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
        script.Append($"$('#form-view-page-state-{Name}').val('{(int)PageState.List}'); ");
        script.AppendLine("$('form:first').submit(); ");

        var goBackAction = new ScriptAction
        {
            Name = "goBackAction",
            Icon = IconType.Backward,
            ShowAsButton = true,
            Text = "Back",
            OnClientClick = script.ToString()
        };

        if (PageState == PageState.View)
        {
            var html = await AuditLogView.GetLogDetailsHtmlAsync(actionMap?.PkFieldValues);

            if (actionMap?.PkFieldValues != null)
                html.AppendComponent(GetFormLogBottomBar(actionMap.PkFieldValues!));

            PageState = PageState.AuditLog;
            return HtmlComponentResult.FromHtmlBuilder(html);
        }

        AuditLogView.GridView.AddToolBarAction(goBackAction);
        AuditLogView.DataPanel = DataPanel;
        PageState = PageState.AuditLog;
        return await AuditLogView.GetResultAsync();
    }

    private async Task<ComponentResult> GetImportationResult()
    {
        var action = GridView.ImportAction;
        var formData = await GridView.GetFormDataAsync();
        bool isVisible = await ExpressionsService.GetBoolValueAsync(action.VisibleExpression, formData);
        if (!isVisible)
            throw new UnauthorizedAccessException(StringLocalizer["Import action not enabled"]);

        var html = new HtmlBuilder(HtmlTag.Div);

        if (ShowTitle)
            html.AppendComponent(GridView.GetTitle(UserValues));

        PageState = PageState.Import;
        var importationScript = new StringBuilder();
        importationScript.Append($"$('#form-view-page-state-{Name}').val('{(int)PageState.List}'); ");
        importationScript.AppendLine("$('form:first').submit(); ");

        DataImportation.UserValues = UserValues;
        DataImportation.BackButton.OnClientClick = importationScript.ToString();
        DataImportation.ProcessOptions = action.ProcessOptions;
        DataImportation.EnableAuditLog = GridView.ToolBarActions.LogAction.IsVisible;

        var result = await DataImportation.GetResultAsync();

        if (result is RenderedComponentResult renderedComponentResult)
        {
            html.Append(renderedComponentResult.HtmlBuilder);
        }
        else
        {
            return result;
        }


        return new RenderedComponentResult(html);
    }


    private async Task<ComponentResult> GetFormResult(FormContext formContext, bool autoReloadFormFields)
    {
        var (values, errors, pageState) = formContext;

        var visibleRelationships = await FormElement
            .Relationships
            .ToAsyncEnumerable()
            .Where(r => r.ViewType != RelationshipViewType.None || r.IsParent)
            .WhereAwait(async r =>
                await ExpressionsService.GetBoolValueAsync(r.Panel.VisibleExpression,
                    new FormStateData(values, pageState)))
            .ToListAsync();

        var parentPanel = DataPanel;
        parentPanel.PageState = pageState;
        parentPanel.Errors = errors;
        parentPanel.Values = values;
        parentPanel.IsExternalRoute = IsExternalRoute;
        parentPanel.AutoReloadFormFields = autoReloadFormFields;

        if (!visibleRelationships.Any() || visibleRelationships.Count == 1)
        {
            var panelHtml = await GetHtmlFromPanel(parentPanel, true);

            panelHtml.AppendHiddenInput($"form-view-relation-values-{Name}",
                EncryptionService.EncryptDictionary(RelationValues));

            if (ComponentContext is ComponentContext.Modal)
                return HtmlComponentResult.FromHtmlBuilder(panelHtml);

            if (ShowTitle)
                panelHtml.Prepend(GridView.GetTitle(values).GetHtmlBuilder());


            return new RenderedComponentResult(panelHtml);
        }

        var html = new HtmlBuilder(HtmlTag.Div);
        if (ShowTitle)
            html.AppendComponent(GridView.GetTitle(values));

        var layout = new FormViewRelationshipLayout(this);

        var topActions = FormElement.Options.FormToolbarActions
            .GetAllSorted()
            .Where(a => a.FormToolbarActionLocation is FormToolbarActionLocation.Top).ToList();

        html.AppendComponent(await GetFormToolbarAsync(topActions));

        var relationshipsResult = await layout.GetRelationshipsResult(visibleRelationships);

        if (relationshipsResult is RenderedComponentResult renderedComponentResult)
        {
            html.Append(renderedComponentResult.HtmlBuilder);
        }
        else
        {
            return relationshipsResult;
        }

        var bottomActions = FormElement.Options.FormToolbarActions
            .GetAllSorted()
            .Where(a => a.FormToolbarActionLocation is FormToolbarActionLocation.Bottom).ToList();

        html.AppendComponent(await GetFormToolbarAsync(bottomActions));

        return new RenderedComponentResult(html);
    }

    internal async Task<HtmlBuilder> GetHtmlFromPanel(JJDataPanel panel, bool isParent = false)
    {
        var formHtml = new HtmlBuilder(HtmlTag.Div);
        formHtml.WithNameAndId(Name);

        if (panel.Errors.Any())
            formHtml.AppendComponent(new JJValidationSummary(panel.Errors));

        var parentPanelHtml = await panel.GetPanelHtmlAsync();

        var panelActions = panel.FormElement.Options.FormToolbarActions
            .Where(a => a.FormToolbarActionLocation == FormToolbarActionLocation.Panel || isParent).ToList();

        var toolbar = await GetFormToolbarAsync(panelActions);

        formHtml.Append(parentPanelHtml);
        formHtml.AppendComponent(toolbar);
        formHtml.AppendHiddenInput($"form-view-current-action-{Name}");
        return formHtml;
    }

    private JJToolbar GetFormLogBottomBar(IDictionary<string, object?> values)
    {
        var backScript = new StringBuilder();
        backScript.Append($"$('#form-view-page-state-{Name}').val('{(int)PageState.List}'); ");
        backScript.AppendLine("$('form:first').submit(); ");

        var btnBack = GetBackButton();
        btnBack.OnClientClick = backScript.ToString();

        var btnHideLog = GetButtonHideLog(values);

        var toolbar = new JJToolbar
        {
            CssClass = "pb-3 mt-3"
        };
        toolbar.Items.Add(btnBack.GetHtmlBuilder());
        toolbar.Items.Add(btnHideLog.GetHtmlBuilder());
        return toolbar;
    }

    private async Task<JJToolbar> GetFormToolbarAsync(IList<BasicAction> actions)
    {
        var toolbar = new JJToolbar
        {
            CssClass = "pb-3 mt-3"
        };

        foreach (var action in actions.Where(a => !a.IsGroup))
        {
            if (action is SaveAction saveAction)
            {
                saveAction.EnterKeyBehavior = DataPanel.FormUI.EnterKey;
            }

            var factory = ComponentFactory.LinkButton;


            var linkButton = await factory.CreateFormToolbarButtonAsync(action, this);
            toolbar.Items.Add(linkButton.GetHtmlBuilder());
        }

        if (actions.Any(a => a.IsGroup))
        {
            var btnGroup = new JJLinkButtonGroup
            {
                CaretText = "More"
            };

            foreach (var groupedAction in actions.Where(a => a.IsGroup).ToList())
            {
                btnGroup.ShowAsButton = groupedAction.ShowAsButton;
                var factory = ComponentFactory.LinkButton;
                var linkButton = await factory.CreateFormToolbarButtonAsync(groupedAction, this);
                btnGroup.Actions.Add(linkButton);
            }

            toolbar.Items.Add(btnGroup.GetHtmlBuilder());
        }


        if (PageState == PageState.View)
        {
            if (GridView.ToolBarActions.LogAction.IsVisible)
            {
                var values = await GetFormValuesAsync();
                toolbar.Items.Add(GetButtonViewLog(values).GetHtmlBuilder());
            }
        }

        return toolbar;
    }

    private void FormSelectedOnRenderAction(object sender, ActionEventArgs e)
    {
        if (!e.Action.Name.Equals("_jjselaction")) return;

        if (sender is not JJGridView grid)
            return;

        var map = new ActionMap(ActionSource.GridTable, grid.FormElement, e.FieldValues, e.Action.Name);
        string encryptedActionMap = EncryptionService.EncryptActionMap(map);
        e.LinkButton.OnClientClick = $"JJViewHelper.openSelectElementInsert('{Name}','{encryptedActionMap}');";
    }

    /// <summary>
    /// Insert the records in the database.
    /// </summary>
    /// <returns>The list of errors.</returns>
    public async Task<IDictionary<string, string>> InsertFormValuesAsync(IDictionary<string, object?> values,
        bool validateFields = true)
    {
        var result = await FormService.InsertAsync(FormElement, values,
            new DataContext(CurrentContext.Request, DataContextSource.Form, UserId),
            validateFields);
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

    public async Task<IDictionary<string, string>> DeleteFormValuesAsync(IDictionary<string, object>? filter)
    {
        var values =
            await FieldValuesService.MergeWithExpressionValuesAsync(FormElement, filter, PageState.Delete, true);
        var result = await FormService.DeleteAsync(FormElement, values,
            new DataContext(CurrentContext.Request, DataContextSource.Form, UserId));
        UrlRedirect = result.UrlRedirect;
        return result.Errors;
    }


    public async Task<IDictionary<string, object?>> GetFormValuesAsync()
    {
        var painel = DataPanel;
        var values = await painel.GetFormValuesAsync();

        if (!RelationValues.Any())
            return values;

        DataHelper.CopyIntoDictionary(values, RelationValues!, true);

        return values;
    }

    public async Task<IDictionary<string, string>> ValidateFieldsAsync(IDictionary<string, object> values,
        PageState pageState)
    {
        DataPanel.Values = values;
        var errors = await DataPanel.ValidateFieldsAsync(values, pageState);
        return errors;
    }

    private void ClearTempFiles()
    {
        var uploadFields = FormElement.Fields.ToList().FindAll(x => x.Component == FormComponent.File);
        foreach (var field in uploadFields)
        {
            string sessionName = $"{field.Name}_uploadview_jjfiles";
            if (CurrentContext?.Session[sessionName] != null)
                CurrentContext.Session[sessionName] = null;
        }
    }

    public void SetOptions(FormElementOptions options)
    {
        FormElement.Options = options;
    }

    private JJLinkButton GetBackButton()
    {
        var btn = new JJLinkButton
        {
            Type = LinkButtonType.Button,
            CssClass = $"{BootstrapHelper.DefaultButton} btn-small",
            OnClientClick = $"JJViewHelper.doPainelAction('{Name}','CANCEL');",
            IconClass = IconType.Times.GetCssClass(),
            Text = "Cancel"
        };
        btn.IconClass = IconType.ArrowLeft.GetCssClass();
        btn.Text = "Back";
        return btn;
    }

    private JJLinkButton GetButtonHideLog(IDictionary<string, object?> values)
    {
        var context = new ActionContext
        {
            FormElement = FormElement,
            FormStateData = new FormStateData(values, UserValues, PageState),
            ParentComponentName = Name,
            IsExternalRoute = IsExternalRoute
        };
        string scriptAction =
            GridView.ActionsScripts.GetFormActionScript(GridView.GridActions.ViewAction, context,
                ActionSource.GridTable);
        var btn = new JJLinkButton
        {
            Type = LinkButtonType.Button,
            Text = "Hide Log",
            IconClass = IconType.Film.GetCssClass(),
            CssClass = "btn btn-primary btn-small",
            OnClientClick = $"$('#form-view-page-state-{Name}').val('{(int)PageState.List}');{scriptAction}"
        };
        return btn;
    }

    private JJLinkButton GetButtonViewLog(IDictionary<string, object?> values)
    {
        var context = new ActionContext
        {
            FormElement = FormElement,
            FormStateData = new FormStateData(values, UserValues, PageState),
            ParentComponentName = Name,
            IsExternalRoute = IsExternalRoute
        };
        string scriptAction =
            GridView.ActionsScripts.GetFormActionScript(GridView.ToolBarActions.LogAction, context,
                ActionSource.GridToolbar);
        var btn = new JJLinkButton
        {
            Type = LinkButtonType.Button,
            Text = "View Log",
            IconClass = IconType.Film.GetCssClass(),
            CssClass = BootstrapHelper.DefaultButton + " btn-small",
            OnClientClick = scriptAction
        };
        return btn;
    }

    public async Task<FormStateData> GetFormStateDataAsync()
    {
        var values =
            await GridView.FormValuesService.GetFormValuesWithMergedValuesAsync(FormElement, PageState,
                CurrentContext.Request.IsPost);

        return new FormStateData(values, UserValues, PageState);
    }

    #region "Legacy inherited GridView compatibility"

    [Obsolete("Please use GridView.GridActions")]
    public GridTableActionList GridActions => GridView.GridActions;

    [Obsolete("Please use GridView.ToolBarActions")]
    public GridToolbarActionList ToolBarActions => GridView.ToolBarActions;

    [Obsolete("Please use GridView.SetCurrentFilterAsync")]
    public void SetCurrentFilter(string filterKey, string filterValue)
    {
        GridView.SetCurrentFilterAsync(filterKey, filterValue).GetAwaiter().GetResult();
    }

    [Obsolete("Please use GridView.GetSelectedGridValues")]
    public List<IDictionary<string, object>> GetSelectedGridValues() => GridView.GetSelectedGridValues();

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
    public bool EnableMultSelect
    {
        get => GridView.EnableMultiSelect;
        set => GridView.EnableMultiSelect = value;
    }


    #endregion

    public static implicit operator JJGridView(JJFormView formView) => formView.GridView;
    public static implicit operator JJDataPanel(JJFormView formView) => formView.DataPanel;
}