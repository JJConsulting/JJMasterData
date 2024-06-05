using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JetBrains.Annotations;
using JJMasterData.Commons.Data.Entity.Repository.Abstractions;
using JJMasterData.Commons.Exceptions;
using JJMasterData.Commons.Localization;
using JJMasterData.Commons.Security.Cryptography.Abstractions;
using JJMasterData.Commons.Util;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.DataManager;
using JJMasterData.Core.DataManager.Expressions;
using JJMasterData.Core.DataManager.Models;
using JJMasterData.Core.DataManager.Services;
using JJMasterData.Core.Extensions;
using JJMasterData.Core.Http.Abstractions;
using JJMasterData.Core.Tasks;
using JJMasterData.Core.UI.Html;
using JJMasterData.Core.UI.Routing;
using Microsoft.Extensions.Localization;
#if NET48
using JJMasterData.Commons.Configuration;
#endif

namespace JJMasterData.Core.UI.Components;

/// <summary>
/// Render panels with fields
/// </summary>
public class JJDataPanel : AsyncComponent
{
    #region "Fields"
    private RouteContext _routeContext;
    private PageState? _pageState;
    private bool _isAtModal;
    private FormUI _formUI;
    #endregion
    #region "Properties"


    /// <summary>
    /// Layout form settings
    /// </summary>
    public FormUI FormUI
    {
        get => _formUI ??= FormElement.Options.Form;
        internal set => _formUI = value;
    }

    /// <summary>
    /// Predefined form settings
    /// </summary>
    public FormElement FormElement { get; set; }

    /// <summary>
    /// Current state of the form
    /// </summary>
    public PageState PageState
    {
        get
        {
            if (ContainsPanelState() && _pageState is null)
                _pageState = (PageState)int.Parse(CurrentContext.Request.Form[$"data-panel-state-{Name}"]);

            return _pageState ?? PageState.View;
        }
        set
        {
            _pageState = value;
            HasCustomPanelState = true;
        }
    }

    internal bool ContainsPanelState() => CurrentContext.Request.Form[$"data-panel-state-{Name}"] != null;

    internal bool HasCustomPanelState { get; set; }
    
    /// <summary>
    /// Fields with error.
    /// Key=Field Name, Value=Error Description
    /// </summary>
    public Dictionary<string, string> Errors { get; set; }

    /// <summary>
    /// Field Values.
    /// Key=Field Name, Value=Field Value
    /// </summary>
    public Dictionary<string, object> Values { get; set; }

    /// <summary>
    /// When reloading the panel, keep the values entered in the form
    /// (Default=True)
    /// </summary>
    public bool AutoReloadFormFields { get; set; }
    
    [CanBeNull] 
    public string ParentComponentName { get; set; }

    /// <summary>
    /// Render field grouping
    /// </summary>
    internal bool RenderPanelGroup { get; set; }

    private bool AppendPkValues { get; set; } = true;
    
    public string FieldNamePrefix { get; set; }

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

    public bool IsAtModal
    {
        get
        {
            if (CurrentContext.Request.Form[$"data-panel-is-at-modal-{Name}"] != null)
                _isAtModal = StringManager.ParseBool(CurrentContext.Request.Form[$"data-panel-is-at-modal-{Name}"]);

            return _isAtModal;
        }
        set => _isAtModal = value;
    }

    public IEntityRepository EntityRepository { get; }
    internal IHttpContext CurrentContext { get; }
    internal IEncryptionService EncryptionService { get; }
    internal FieldsService FieldsService { get; }
    internal FormValuesService FormValuesService { get; }
    internal ExpressionsService ExpressionsService { get; }
    private UrlRedirectService UrlRedirectService { get; }
    internal IComponentFactory ComponentFactory { get; }
    internal IStringLocalizer<MasterDataResources> StringLocalizer { get; }

    #endregion

    #region "Constructors"

    public JJDataPanel(
        IEntityRepository entityRepository,
        IHttpContext currentContext,
        IEncryptionService encryptionService,
        FieldsService fieldsService,
        FormValuesService formValuesService,
        ExpressionsService expressionsService,
        UrlRedirectService urlRedirectService,
        IStringLocalizer<MasterDataResources> stringLocalizer,
        IComponentFactory componentFactory
    ) 
    {
        EntityRepository = entityRepository;
        CurrentContext = currentContext;
        EncryptionService = encryptionService;
        FieldsService = fieldsService;
        FormValuesService = formValuesService;
        ExpressionsService = expressionsService;
        UrlRedirectService = urlRedirectService;
        StringLocalizer = stringLocalizer;
        ComponentFactory = componentFactory;
        Values = new Dictionary<string, object>(StringComparer.InvariantCultureIgnoreCase);
        Errors = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
        AutoReloadFormFields = true;
    }

    public JJDataPanel(
        FormElement formElement,
        IEntityRepository entityRepository,
        IHttpContext currentContext,
        IEncryptionService encryptionService,
        FieldsService fieldsService,
        FormValuesService formValuesService,
        ExpressionsService expressionsService,
        UrlRedirectService urlRedirectService,
        IStringLocalizer<MasterDataResources> stringLocalizer,
        IComponentFactory componentFactory
    ) : this(entityRepository,  currentContext, encryptionService, fieldsService, formValuesService, expressionsService, urlRedirectService,stringLocalizer,componentFactory)
    {
        Name = $"{ComponentNameGenerator.Create(formElement.Name)}-data-panel";
        FormElement = formElement;
        RenderPanelGroup = formElement.Panels.Count > 0;
    }

    #endregion
    
    protected override async Task<ComponentResult> BuildResultAsync()
    {
        if (!RouteContext.CanRender(FormElement))
            return new EmptyComponentResult();
        
        Values = await GetFormValuesAsync();

        switch (ComponentContext)
        {
            case ComponentContext.TextFileUploadView:
            case ComponentContext.TextFileFileUpload:
                return await GetFieldResultAsync<JJTextFile>();
            case ComponentContext.SearchBox:
            case ComponentContext.SearchBoxFilter:
                return await GetFieldResultAsync<JJSearchBox>();
            case ComponentContext.LookupDescription:
                return await GetFieldResultAsync<JJLookup>();
            case ComponentContext.DownloadFile:
                return ComponentFactory.Downloader.Create().GetDownloadResult();
            case ComponentContext.DataPanelReload:
            {
                var html = await GetPanelHtmlBuilderAsync();
                return new ContentComponentResult(html);
            }
            case ComponentContext.UrlRedirect:
            {
                string encryptedActionMap = CurrentContext.Request.Form[$"current-action-map-{Name}"];
                if (string.IsNullOrEmpty(encryptedActionMap))
                    return null;

                var actionMap = EncryptionService.DecryptActionMap(encryptedActionMap);
                
                return await GetUrlRedirectResult(actionMap);
            }
         
            default:
                return new RenderedComponentResult(await GetPanelHtmlBuilderAsync());
        }
    }

    private async Task<ComponentResult> GetFieldResultAsync<TControl>() where TControl : ControlBase
    {
        var fieldName = CurrentContext.Request.QueryString["fieldName"];
        var formStateData = new FormStateData(await GetFormValuesAsync(), UserValues, PageState);
        var controlContext = new ControlContext(formStateData, Name);

        if (!FormElement.Fields.TryGetField(fieldName, out var field))
            return new EmptyComponentResult();
        
        var control = ComponentFactory.Controls.Create<TControl>(FormElement, field, controlContext);
        control.Name = FieldNamePrefix + fieldName;
        
        if (control is JJTextFile textFile)
            textFile.ParentName = FormElement.Name;
        
        return await control.GetResultAsync();
    }

    internal async Task<HtmlBuilder> GetPanelHtmlBuilderAsync()
    {
        var html = new HtmlBuilder(HtmlTag.Div)
            .WithAttributes(Attributes)
            .WithNameAndId(Name)
            .WithCssClass(CssClass);

        AppendHiddenInputs(html);

        var panelGroup = new DataPanelLayout(this);
        html.AppendRange(await panelGroup.GetHtmlPanelList());
        html.AppendScript(GetHtmlFormScript());

        return html;
    }

    private void AppendHiddenInputs(HtmlBuilder html)
    {
        if (DataHelper.ContainsPkValues(FormElement, Values) && AppendPkValues)
        {
            html.AppendHiddenInput($"data-panel-pk-values-{FormElement.Name}", GetPkHiddenInput());
        }
        html.AppendHiddenInput($"data-panel-state-{Name}", ((int)PageState).ToString());
        html.AppendHiddenInput($"data-panel-is-at-modal-{Name}", IsAtModal.ToString());
    }

    private string GetPkHiddenInput()
    {
        string pkval = DataHelper.ParsePkValues(FormElement, Values, '|');
        return EncryptionService.EncryptStringWithUrlEscape(pkval);
    }

    private string GetHtmlFormScript()
    {
        var script = new StringBuilder();
        script.AppendLine("$(document).ready(function () { ");

        if (FormUI.EnterKey == FormEnterKey.Tab)
        {
            script.AppendLine($"\tjjutil.replaceEntertoTab('{Name}');");
        }
        
        if (!FormElement.Fields.Any(x => x.AutoPostBack))
        {
            var dataPanelScripts = new DataPanelExpressionScripts(this);
            script.AppendLine( dataPanelScripts.GetHtmlFormScript());
        }

        script.AppendLine("});");
        return script.ToString();
    }

    [Obsolete("Please use GetFormValuesAsync")]
    public Dictionary<string,object> GetFormValues()
    {
        return AsyncHelper.RunSync(GetFormValuesAsync);
    }

    /// <summary>
    /// Load form data with default values and triggers
    /// </summary>
    public async Task<Dictionary<string, object>> GetFormValuesAsync()
    {
        var formStateData = new FormStateData(Values, UserValues, PageState);
        var mergedValues = await FormValuesService.GetFormValuesWithMergedValuesAsync(FormElement, formStateData, AutoReloadFormFields, FieldNamePrefix);
        
        DataHelper.CopyIntoDictionary(Values, mergedValues, true);
        DataHelper.RemoveNullValues(Values);
        
        return Values;
    }
#if NETFRAMEWORK
    [Obsolete($"{SynchronousMethodObsolete.Message}Please use LoadValuesFromPkAsync")]
    public void LoadValuesFromPK(Dictionary<string, object> pks)
    {
        Values = AsyncHelper.RunSync(()=>EntityRepository.GetFieldsAsync(FormElement, pks));
    }
#endif
    public async Task LoadValuesFromPkAsync(Dictionary<string, object> pks)
    {
        Values = await EntityRepository.GetFieldsAsync(FormElement, pks);
    }

    public async Task LoadValuesFromPkAsync(params object[] pks)
    {
        var primaryKeys = FormElement.GetPrimaryKeys();
        if (primaryKeys.Count == 0)
            throw new JJMasterDataException("Primary key not found");

        if (pks.Length != primaryKeys.Count)
            throw new JJMasterDataException("Primary key not found");
        
        var filter = new Dictionary<string, object>();
        for (var index = 0; index < primaryKeys.Count; index++)
        {
            var field = primaryKeys[index];
            filter.Add(field.Name, pks[index]);
        }

        await LoadValuesFromPkAsync(filter);
    }
    
    
    /// <summary>
    /// Validate form fields and return a list with errors
    ///  </summary>
    /// <returns>
    /// Key = Field Name
    /// Valor = Error message
    /// </returns>
    public Dictionary<string, string> ValidateFields(Dictionary<string, object> values, PageState pageState)
    {
        return ValidateFields(values, pageState, true);
    }

    /// <inheritdoc cref="ValidateFields()"/>
    public Dictionary<string, string> ValidateFields(Dictionary<string, object> values)
    {
        return ValidateFields(values, PageState, true);
    }
    
    /// <summary>
    /// Validate form fields and return a list with errors
    /// </summary>
    /// <returns>
    /// Key = Field Name
    /// Valor = Error message
    /// </returns>
    public Dictionary<string, string> ValidateFields(Dictionary<string, object> values, PageState pageState, bool enableErrorLink)
    {
        return FieldsService.ValidateFields(FormElement, values, pageState, enableErrorLink);
    }
    
    internal Task<JsonComponentResult> GetUrlRedirectResult(ActionMap actionMap)
    {
        return UrlRedirectService.GetUrlRedirectResult(this, actionMap);
    }
}
