#nullable enable

// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedMember.Local

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
using JJMasterData.Core.DataManager.Services.Abstractions;
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

    #endregion

    
    #region "Fields"
    private JJDataPanel? _dataPanel;
    private JJGridView? _gridView;
    private ActionMap? _currentActionMap;
    private JJAuditLogView? _auditLogView;
    private JJDataImportation? _dataImportation;
    private string? _userId;
    private bool? _showTitle;
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
            _dataPanel.Name = "jjpanel_" + FormElement.Name.ToLower();
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
    public IDictionary<string, object> RelationValues { get; set; } = new Dictionary<string, object>();
    
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

    /// <summary>
    /// Estado atual da pagina
    /// </summary>
    private PageState? _pageState;

    public PageState PageState
    {
        get
        {
            if (CurrentContext.Request["current-page-state-" + Name] != null && _pageState is null)
                _pageState = (PageState)int.Parse(CurrentContext.Request["current-page-state-" + Name]);

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

            string encryptedActionMap = CurrentContext.Request["current-form-action-" + Name.ToLower()];
            if (string.IsNullOrEmpty(encryptedActionMap))
                return null;

            _currentActionMap = EncryptionService.DecryptActionMap(encryptedActionMap);
            return _currentActionMap;
        }
    }


    public DeleteSelectedRowsAction DeleteSelectedRowsAction
        => (DeleteSelectedRowsAction)GridView.ToolBarActions.First(x => x is DeleteSelectedRowsAction);

    public InsertAction InsertAction => GridView.ToolBarActions.InsertAction;

    public EditAction EditAction => GridView.GridActions.EditAction;

    public DeleteAction DeleteAction => GridView.GridActions.DeleteAction;

    public ViewAction ViewAction => GridView.GridActions.ViewAction;

    public LogAction LogAction => GridView.ToolBarActions.LogAction;


    public bool ShowTitle
    {
        get
        {
            _showTitle ??= FormElement.Options.Grid.ShowTitle;
            return _showTitle.Value;
        }
        set => _showTitle = value;
    }

    internal bool IsModal { get; set; }

    internal IHttpContext CurrentContext { get; }
    internal IEntityRepository EntityRepository { get; }
    internal JJMasterDataEncryptionService EncryptionService { get; }
    internal IFieldValuesService FieldValuesService { get; }
    internal IExpressionsService ExpressionsService { get; }
    private IStringLocalizer<JJMasterDataResources> StringLocalizer { get; }
    internal IDataDictionaryRepository DataDictionaryRepository { get; }
    internal IFormService FormService { get; }
    internal ComponentFactory ComponentFactory { get; }

    #endregion

    #region "Constructors"

#if NET48
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    private JJFormView()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    {
        CurrentContext = StaticServiceLocator.Provider.GetScopedDependentService<IHttpContext>();
        EntityRepository = StaticServiceLocator.Provider.GetScopedDependentService<IEntityRepository>();
        ComponentFactory = StaticServiceLocator.Provider.GetScopedDependentService<ComponentFactory>();
        FormService = StaticServiceLocator.Provider.GetScopedDependentService<IFormService>();
        EncryptionService = StaticServiceLocator.Provider.GetScopedDependentService<JJMasterDataEncryptionService>();
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
        JJMasterDataEncryptionService encryptionService,
        IFieldValuesService fieldValuesService,
        IExpressionsService expressionsService,
        IStringLocalizer<JJMasterDataResources> stringLocalizer,
        ComponentFactory componentFactory)
    {
        Name = "jjview_" + formElement.Name.ToLower();
        FormElement = formElement;
        CurrentContext = currentContext;
        EntityRepository = entityRepository;
        FormService = formService;
        EncryptionService = encryptionService;
        FieldValuesService = fieldValuesService;
        ExpressionsService = expressionsService;
        StringLocalizer = stringLocalizer;
        DataDictionaryRepository = dataDictionaryRepository;
        ComponentFactory = componentFactory;
    }

    #endregion

     protected override async Task<ComponentResult> BuildResultAsync()
    {
        var requestType = CurrentContext.Request.QueryString("t");
        var objName = CurrentContext.Request.QueryString("objname");

        if (JJLookup.IsLookupRoute(this, CurrentContext))
            return await DataPanel.GetResultAsync();

        if (JJTextFile.IsUploadViewRoute(this, CurrentContext))
            return await DataPanel.GetResultAsync();

        if (JJFileDownloader.IsDownloadRoute(CurrentContext))
            return JJFileDownloader.GetDirectDownloadRedirect(CurrentContext, EncryptionService, ComponentFactory.Downloader);

        if (JJSearchBox.IsSearchBoxRoute(FormElement.Name, CurrentContext))
            return await JJSearchBox.GetResultFromPanel(DataPanel);

        if ("reloadPanel".Equals(requestType))
        {
            var panelHtml = await GetReloadPanelResultAsync();

            return new HtmlComponentResult(panelHtml!);
        }

        if ("jjupload".Equals(requestType) || "ajaxdataimp".Equals(requestType))
        {
            if (!DataImportation.Upload.Name.Equals(objName))
                return new EmptyComponentResult();

            return await GetImportationResult();
        }
        if ("geturlaction".Equals(requestType))
        {
            return await DataPanel.GetUrlRedirectResult(CurrentActionMap);
        }

        return await GetFormResult();
    }
    internal async Task<ComponentResult> GetReloadPanelResultAsync()
    {
        var filter = GridView.GetSelectedRowId();
        IDictionary<string, object> values;
        if (filter is { Count: > 0 })
            values = await EntityRepository.GetDictionaryAsync(FormElement, filter);
        else
            values = await GetFormValuesAsync();

        DataPanel.Values = values;

        var htmlPanel = await DataPanel.GetResultAsync();
        return htmlPanel;
    }

    private async Task<ComponentResult> GetFormResult()
    {
        HtmlBuilder? html;
        
        var currentAction = CurrentActionMap?.GetCurrentAction(FormElement);

        if (currentAction is EditAction || PageState is PageState.Update)
        {
            var updateResult = await GetUpdateResult();

            if (updateResult is RenderedComponentResult renderedComponentResult)
                html = renderedComponentResult.HtmlBuilder;
            else
            {
                return updateResult;
            }
        }
        else if (currentAction is InsertAction || PageState is PageState.Insert)
        {
            var insertResult = await GetInsertResult();

            if (insertResult is RenderedComponentResult renderedComponentResult)
                html = renderedComponentResult.HtmlBuilder;
            else
            {
                return insertResult;
            }
        }
        else if (currentAction is ImportAction || PageState is PageState.Import)
        {
            var importationResult = await GetImportationResult();

            if (importationResult is RenderedComponentResult renderedComponentResult)
                html = renderedComponentResult.HtmlBuilder;
            else
            {
                return importationResult;
            }
        }
        else if (currentAction is LogAction || PageState is PageState.AuditLog)
        {
            var auditLogResult = await GetAuditLogResult();

            if (auditLogResult is RenderedComponentResult renderedComponentResult)
                html = renderedComponentResult.HtmlBuilder;
            else
            {
                return auditLogResult;
            }
        }
        else if (currentAction is DeleteAction)
        {
            var deleteResult = await GetDeleteResult();

            if (deleteResult is RenderedComponentResult renderedComponentResult)
                html = renderedComponentResult.HtmlBuilder;
            else
            {
                return deleteResult;
            }
        }
        else if (currentAction is DeleteSelectedRowsAction)
        {
            html = await GetHtmlDeleteSelectedRows();
        }
        else if (currentAction is ViewAction || PageState is PageState.View)
        {
            var viewResult = await GetViewResult();

            if (viewResult is RenderedComponentResult renderedComponentResult)
                html = renderedComponentResult.HtmlBuilder;
            else
            {
                return viewResult;
            }
        }
        else
        {
            var gridViewResult = await GetGridViewResult();

            if (gridViewResult is RenderedComponentResult renderedComponentResult)
                html = renderedComponentResult.HtmlBuilder;
            else
            {
                return gridViewResult;
            }
        }
        
        html.AppendHiddenInput($"current-page-state-{Name.ToLower()}", ((int)PageState).ToString());
        html.AppendHiddenInput($"current-form-action-{Name.ToLower()}", "");
        
        return new RenderedComponentResult(html);
    }

    private async Task<ComponentResult> GetGridViewResult()
    {
        return await GridView.GetResultAsync();
    }

    private async Task<ComponentResult> GetUpdateResult()
    {
        string formAction = "";

        if (CurrentContext.Request["current-panel-action-" + Name] != null)
            formAction = CurrentContext.Request["current-panel-action-" + Name];

        if ("OK".Equals(formAction))
        {
            var values = await GetFormValuesAsync();
            var errors = await UpdateFormValuesAsync(values);

            if (errors.Count == 0)
            {
                if (!string.IsNullOrEmpty(UrlRedirect))
                {
                    return new RedirectComponentResult(UrlRedirect!);
                }

                PageState = PageState.List;
                return await GridView.GetResultAsync();
            }

            PageState = PageState.Update;
            return await GetFormResultAsync(new(values, errors, PageState), true);
        }

        if ("CANCEL".Equals(formAction))
        {
            PageState = PageState.List;
            return await GridView.GetResultAsync();
        }

        if ("REFRESH".Equals(formAction))
        {
            var values = await GetFormValuesAsync();
            return await GetFormResultAsync(new(values, null, PageState), true);
        }
        else
        {
            bool autoReloadFields;
            IDictionary<string, object> values;
            if (PageState is PageState.Update)
            {
                autoReloadFields = true;
                values = await GetFormValuesAsync();
            }
            else
            {
                autoReloadFields = false;
                values = await EntityRepository.GetDictionaryAsync(FormElement, CurrentActionMap?.PkFieldValues);
            }

            PageState = PageState.Update;
            return await GetFormResultAsync(new(values, null, PageState), autoReloadFields);
        }
    }

    private async Task<ComponentResult> GetInsertResult()
    {
        var action = InsertAction;
        var formData = new FormStateData(RelationValues, UserValues, PageState.List);
        bool isVisible = await ExpressionsService.GetBoolValueAsync(action.VisibleExpression, formData);
        if (!isVisible)
            throw new UnauthorizedAccessException(StringLocalizer["Insert action not enabled"]);

        string formAction = "";

        if (CurrentContext.Request["current-panel-action-" + Name] != null)
            formAction = CurrentContext.Request["current-panel-action-" + Name];

        if (formAction.Equals("OK"))
        {
            var values = await GetFormValuesAsync();
            var errors = await InsertFormValuesAsync(values);

            if (errors.Count == 0)
            {
                if (!string.IsNullOrEmpty(UrlRedirect))
                {
                    return new RedirectComponentResult(UrlRedirect!);
                }

                if (action.ReopenForm)
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

                    var formResult = await GetFormResultAsync(new(RelationValues, null, PageState.Insert), false);

                    if (formResult is RenderedComponentResult renderedComponentResult)
                    {
                        alertHtml.Append(HtmlTag.Div,  div =>
                        {
                            div.WithAttribute("id", $"insert-panel{Name}")
                                .WithAttribute("style", "display:none")
                                .Append(renderedComponentResult.HtmlBuilder);
                        });
                        alertHtml.AppendScript($"JJView.showInsertSucess('{Name}');");
                        return new RenderedComponentResult(alertHtml);
                    }
                    else
                    {
                        return formResult;
                    }
                }

                PageState = PageState.List;
                return await GridView.GetResultAsync();
            }

            PageState = PageState.Insert;
            return await GetFormResultAsync(new(values, errors, PageState), true);
        }

        if (formAction.Equals("CANCEL"))
        {
            PageState = PageState.List;
            ClearTempFiles();
            return await GridView.GetResultAsync();
        }

        if (formAction.Equals("ELEMENTSEL"))
        {
            return await GetInsertSelectionResult();
        }

        if (formAction.Equals("ELEMENTLIST"))
        {
            PageState = PageState.Insert;
            return await GetInsertSelectionResult(action);
        }

        if (PageState == PageState.Insert)
        {
            var formValues = await GetFormValuesAsync();
            return await GetFormResultAsync(new(formValues, null, PageState), true);
        }

        PageState = PageState.Insert;

        if (string.IsNullOrEmpty(action.ElementNameToSelect))
            return await GetFormResultAsync(new(RelationValues, null, PageState.Insert), false);
        return await GetInsertSelectionResult(action);
    }

    private async Task<ComponentResult> GetInsertSelectionResult(InsertAction action)
    {
        var html = new HtmlBuilder(HtmlTag.Div);
        html.AppendHiddenInput($"current-panel-action-{Name}", "ELEMENTLIST");
        html.AppendHiddenInput($"current-select-action-values{Name}", "");

        var formElement = await DataDictionaryRepository.GetMetadataAsync(action.ElementNameToSelect);
        var selectedForm = ComponentFactory.FormView.Create(formElement);
        selectedForm.UserValues = UserValues;
        selectedForm.Name = action.ElementNameToSelect;
        selectedForm.SetOptions(formElement.Options);

        var goBackScript = new StringBuilder();
        goBackScript.Append($"$('#current-page-state-{Name}').val('{((int)PageState.List).ToString()}'); ");
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
        string encryptedActionMap = CurrentContext.Request.Form("current-select-action-values" + Name);
        var actionMap = EncryptionService.DecryptActionMap(encryptedActionMap);
        var html = new HtmlBuilder(HtmlTag.Div);
        var formElement = await DataDictionaryRepository.GetMetadataAsync(InsertAction.ElementNameToSelect);
        var selValues = await EntityRepository.GetDictionaryAsync(formElement, actionMap.PkFieldValues);
        var values = await FieldValuesService.MergeWithExpressionValuesAsync(formElement, selValues, PageState.Insert, true);
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

            var insertSelectionResult = await GetInsertSelectionResult(InsertAction);

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

            var result = await GetFormResultAsync(new(values, null, PageState), false);

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
        var values = await EntityRepository.GetDictionaryAsync(FormElement, filter);
        return await GetFormResultAsync(new(values, null, PageState), false);
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


    private async Task<HtmlBuilder> GetHtmlDeleteSelectedRows()
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
        finally
        {
            var gridViewResult = await GetGridViewResult();

            if (gridViewResult is RenderedComponentResult)
            {
                html.Append(new HtmlBuilder(gridViewResult!));
            }
            
    
            PageState = PageState.List;
        }

        return html;
    }


    private async Task<ComponentResult> GetAuditLogResult()
    {
        var actionMap = _currentActionMap;
        var script = new StringBuilder();
        script.Append($"$('#current-page-state-{Name}').val('{(int)PageState.List}'); ");
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
                html.AppendComponent(GetFormLogBottomBar(actionMap.PkFieldValues));
            
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
        importationScript.Append($"$('#current-page-state-{Name}').val('{(int)PageState.List}'); ");
        importationScript.AppendLine("$('form:first').submit(); ");
        
        DataImportation.UserValues = UserValues;
        DataImportation.BackButton.OnClientClick = importationScript.ToString();
        DataImportation.ProcessOptions = action.ProcessOptions;
        DataImportation.EnableAuditLog = LogAction.IsVisible;

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
    
    
    private async Task<ComponentResult> GetFormResultAsync(FormContext formContext, bool autoReloadFormFields)
    {
        var relationships = FormElement
            .Relationships
            .Where(r => r.ViewType != RelationshipViewType.None || r.IsParent)
            .ToList();

        var (values, errors, pageState) = formContext;

        var parentPanel = DataPanel;
        parentPanel.PageState = pageState;
        parentPanel.Errors = errors;
        parentPanel.Values = values;
        parentPanel.IsExternalRoute = IsExternalRoute;
        parentPanel.AutoReloadFormFields = autoReloadFormFields;
        
        if (!relationships.Any())
        {
            return new RenderedComponentResult(await GetHtmlFromPanel(parentPanel));
        }

        var html = new HtmlBuilder(HtmlTag.Div);
        if (ShowTitle)
            html.AppendComponent(GridView.GetTitle(values));

        var layout = new FormViewRelationshipLayout(this);

        var topActions = FormElement.Options.FormToolbarActions
            .GetAllSorted()
            .Where(a => a.FormToolbarActionLocation is FormToolbarActionLocation.Top).ToList();

        html.AppendComponent(await GetFormToolbarAsync(topActions));

        var relationshipsResult = await layout.GetRelationshipsResult(parentPanel, relationships);

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

    internal async Task<HtmlBuilder> GetHtmlFromPanel(JJDataPanel panel)
    {
        var formHtml = new HtmlBuilder(HtmlTag.Div);
        formHtml.WithNameAndId(Name);

        if (panel.Errors != null)
            formHtml.AppendComponent(new JJValidationSummary(panel.Errors));

        var parentPanelHtml = await panel.GetPanelHtmlAsync();

        var panelActions = panel.FormElement.Options.FormToolbarActions
            .Where(a => a.FormToolbarActionLocation == FormToolbarActionLocation.Panel).ToList();

        var toolbar = await GetFormToolbarAsync(panelActions);

        formHtml.Append(parentPanelHtml);
        formHtml.AppendComponent(toolbar);
        formHtml.AppendHiddenInput($"current-panel-action-{Name}");
        return formHtml;
    }

    private JJToolbar GetFormLogBottomBar(IDictionary<string, object> values)
    {
        var backScript = new StringBuilder();
        backScript.Append($"$('#current-page-state-{Name}').val('{(int)PageState.List}'); ");
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
            if (LogAction.IsVisible)
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
        e.LinkButton.OnClientClick = $"JJView.openSelectElementInsert('{Name}','{encryptedActionMap}');";
    }

    /// <summary>
    /// Insert the records in the database.
    /// </summary>
    /// <returns>The list of errors.</returns>
    public async Task<IDictionary<string, object>> InsertFormValuesAsync(IDictionary<string, object> values,
        bool validateFields = true)
    {
        var result = await FormService.InsertAsync(FormElement, values, new DataContext(CurrentContext, DataContextSource.Form, UserId),
            validateFields);
        UrlRedirect = result.UrlRedirect;
        return result.Errors;
    }

    /// <summary>
    /// Update the records in the database.
    /// </summary>
    /// <returns>The list of errors.</returns>
    public async Task<IDictionary<string, object>> UpdateFormValuesAsync(IDictionary<string, object> values)
    {
        var result = await FormService.UpdateAsync(FormElement, values, new DataContext(CurrentContext, DataContextSource.Form, UserId));
        UrlRedirect = result.UrlRedirect;
        return result.Errors;
    }

    public async Task<IDictionary<string, object>> DeleteFormValuesAsync(IDictionary<string, object>? filter)
    {
        var values = await FieldValuesService.MergeWithExpressionValuesAsync(FormElement, filter, PageState.Delete, true);
        var result = await FormService.DeleteAsync(FormElement, values, new DataContext(CurrentContext, DataContextSource.Form, UserId));
        UrlRedirect = result.UrlRedirect;
        return result.Errors;
    }


    public async Task<IDictionary<string, object>> GetFormValuesAsync()
    {
        var painel = DataPanel;
        var values = await painel.GetFormValuesAsync();

        if (!RelationValues.Any())
            return values;

        DataHelper.CopyIntoDictionary(values, RelationValues, true);

        return values;
    }
    public async Task<IDictionary<string, object>> ValidateFieldsAsync(IDictionary<string, object> values, PageState pageState)
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
            OnClientClick = $"JJView.doPainelAction('{Name}','CANCEL');",
            IconClass = IconType.Times.GetCssClass(),
            Text = "Cancel"
        };
        btn.IconClass = IconType.ArrowLeft.GetCssClass();
        btn.Text = "Back";
        return btn;
    }

    private JJLinkButton GetButtonHideLog(IDictionary<string, object> values)
    {
        var context = new ActionContext
        {
            FormElement = FormElement,
            FormStateData = new FormStateData(values, UserValues, PageState),
            ParentComponentName = Name,
            IsExternalRoute = IsExternalRoute
        };
        string scriptAction = GridView.ActionsScripts.GetFormActionScript(ViewAction, context, ActionSource.GridTable);
        var btn = new JJLinkButton
        {
            Type = LinkButtonType.Button,
            Text = "Hide Log",
            IconClass = IconType.Film.GetCssClass(),
            CssClass = "btn btn-primary btn-small",
            OnClientClick = $"$('#current-page-state-{Name}').val('{(int)PageState.List}');{scriptAction}"
        };
        return btn;
    }

    private JJLinkButton GetButtonViewLog(IDictionary<string, object> values)
    {
        var context = new ActionContext
        {
            FormElement = FormElement,
            FormStateData = new FormStateData(values, UserValues, PageState),
            ParentComponentName = Name,
            IsExternalRoute = IsExternalRoute
        };
        string scriptAction = GridView.ActionsScripts.GetFormActionScript(LogAction, context, ActionSource.GridToolbar);
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
        var values = await GridView.FormValuesService.GetFormValuesWithMergedValuesAsync(FormElement, PageState, CurrentContext.IsPost);

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
    [Obsolete("Please use GridView.ImportAction")]
    public ImportAction ImportAction => GridView.ImportAction;

    [Obsolete("Please use GridView.FilterAction")]
    public FilterAction FilterAction => GridView.FilterAction;

    [Obsolete("Please use GridView.GetSelectedGridValues")]
    private List<IDictionary<string, object>> GetSelectedGridValues() => GridView.GetSelectedGridValues();

    [Obsolete("Please use GridView.AddToolBarAction")]
    private void AddToolBarAction(UserCreatedAction userCreatedAction)
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
    private void AddGridAction(UserCreatedAction userCreatedAction)
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
    private void ClearSelectedGridValues()
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