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
public class JJDataPanel(
    IEntityRepository entityRepository,
    IHttpContext currentContext,
    IEncryptionService encryptionService,
    FieldFormattingService fieldFormattingService,
    FieldValidationService fieldValidationService,
    FormValuesService formValuesService,
    ExpressionsService expressionsService,
    UrlRedirectService urlRedirectService,
    IStringLocalizer<MasterDataResources> stringLocalizer,
    IComponentFactory componentFactory)
    : AsyncComponent
{
    #region "Fields"
    private RouteContext _routeContext;
    private PageState? _pageState;
    private bool _isAtModal;
    private FormUI _formUI;
    private Dictionary<string, object> _secretValues;

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
            if (_pageState is null && ContainsPanelState())
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

    internal bool HasCustomPanelState { get; private set; }
    
    /// <summary>
    /// Fields with error.
    /// Key=Field Name, Value=Error Description
    /// </summary>
    public Dictionary<string, string> Errors { get; set; } = new(StringComparer.InvariantCultureIgnoreCase);

    /// <summary>
    /// Field Values.
    /// Key=Field Name, Value=Field Value
    /// </summary>
    public Dictionary<string, object> Values { get; set; } = new(StringComparer.InvariantCultureIgnoreCase);


    /// <summary>
    /// Values not intended to be edited at the client. They are encrypted using <see cref="IEncryptionService"/>.
    /// </summary>
    [CanBeNull]
    public Dictionary<string, object> SecretValues
    {
        get
        {
            if (_secretValues == null && CurrentContext.Request.Form.TryGetValue($"data-panel-secret-values-{Name}", out var secretValues))
                _secretValues = EncryptionService.DecryptDictionary(secretValues);
            return _secretValues;
        }
        set => _secretValues = value;
    }

    /// <summary>
    /// When reloading the panel, keep the values entered in the form
    /// (Default=True)
    /// </summary>
    public bool AutoReloadFormFields { get; set; } = true;

    [CanBeNull] 
    public string ParentComponentName { get; set; }

    /// <summary>
    /// Render field grouping
    /// </summary>
    internal bool RenderPanelGroup { get; set; }

    private static bool AppendPkValues => true;

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
    
    internal IHttpContext CurrentContext { get; } = currentContext;
    internal IEncryptionService EncryptionService { get; } = encryptionService;
    internal FieldFormattingService FieldFormattingService { get; } = fieldFormattingService;
    internal ExpressionsService ExpressionsService { get; } = expressionsService;
    internal IComponentFactory ComponentFactory { get; } = componentFactory;
    internal IStringLocalizer<MasterDataResources> StringLocalizer { get; } = stringLocalizer;

    #endregion

    #region "Constructors"

    public JJDataPanel(
        FormElement formElement,
        IEntityRepository entityRepository,
        IHttpContext currentContext,
        IEncryptionService encryptionService,
        FieldFormattingService fieldFormattingService,
        FieldValidationService fieldValidationService,
        FormValuesService formValuesService,
        ExpressionsService expressionsService,
        UrlRedirectService urlRedirectService,
        IStringLocalizer<MasterDataResources> stringLocalizer,
        IComponentFactory componentFactory
    ) : this(
        entityRepository, 
        currentContext, 
        encryptionService, 
        fieldFormattingService, 
        fieldValidationService,
        formValuesService, 
        expressionsService, 
        urlRedirectService,
        stringLocalizer,
        componentFactory)
    {
        Name = $"{formElement.Name.ToLowerInvariant()}-data-panel";
        FormElement = formElement;
        RenderPanelGroup = formElement.Panels.Count > 0;
    }

    #endregion
    
    protected override async Task<ComponentResult> BuildResultAsync()
    {
        if (!RouteContext.CanRender(FormElement))
            return EmptyComponentResult.Value;
        
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
                var html = await GetHtmlBuilderAsync();
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
                return new RenderedComponentResult(await GetHtmlBuilderAsync());
        }
    }

    private async ValueTask<ComponentResult> GetFieldResultAsync<TControl>() where TControl : ControlBase
    {
        var fieldName = CurrentContext.Request.QueryString["fieldName"];
        var formStateData = new FormStateData(await GetFormValuesAsync(), UserValues, PageState);
        var controlContext = new ControlContext(formStateData, Name);

        if (!FormElement.Fields.TryGetField(fieldName, out var field))
            return EmptyComponentResult.Value;
        
        var control = ComponentFactory.Controls.Create<TControl>(FormElement, field, controlContext);
        control.Name = FieldNamePrefix + fieldName;
        
        if (control is JJTextFile textFile)
            textFile.ParentName = FormElement.Name;
        
        return await control.GetResultAsync();
    }

    public async Task<HtmlBuilder> GetHtmlBuilderAsync()
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

    internal void AppendHiddenInputs(HtmlBuilder html)
    {
        var containsPkErrors = FormElement.Fields
            .Where(f=>f.IsPk)
            .Any(field => Errors.ContainsKey(field.Name));
        
        if (DataHelper.ContainsPkValues(FormElement, Values) && AppendPkValues && !containsPkErrors)
            html.AppendHiddenInput($"data-panel-pk-values-{FormElement.Name}", GetPkHiddenInput());
        
        html.AppendHiddenInput($"data-panel-state-{Name}", ((int)PageState).ToString());
        html.AppendHiddenInput($"data-panel-is-at-modal-{Name}", IsAtModal.ToString());
        
        if (SecretValues?.Count > 0)
            html.AppendHiddenInput($"data-panel-secret-values-{Name}", EncryptionService.EncryptObject(SecretValues));
    }
    
    private string GetPkHiddenInput()
    {
        var pkValues = DataHelper.ParsePkValues(FormElement, Values, '|');
        return EncryptionService.EncryptStringWithUrlEscape(pkValues);
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
        var mergedValues = await formValuesService.GetFormValuesWithMergedValuesAsync(FormElement, formStateData, AutoReloadFormFields, FieldNamePrefix);

        if (SecretValues?.Count > 0)
        {
            DataHelper.RemoveNullValues(SecretValues);
            DataHelper.CopyIntoDictionary(mergedValues, SecretValues, true);
        }
        
        DataHelper.CopyIntoDictionary(Values, mergedValues, true);
        DataHelper.RemoveNullValues(Values);
        
        return Values;
    }
#if NETFRAMEWORK
    [Obsolete("Please use LoadValuesFromPkAsync")]
    public void LoadValuesFromPK(Dictionary<string, object> pks)
    {
        Values = AsyncHelper.RunSync(()=>entityRepository.GetFieldsAsync(FormElement, pks));
    }
#endif
    public async Task LoadValuesFromPkAsync(Dictionary<string, object> pks)
    {
        Values = await entityRepository.GetFieldsAsync(FormElement, pks);
    }

    public Task LoadValuesFromPkAsync(params object[] pks)
    {
        var primaryKeys = FormElement.GetPrimaryKeys();
        if (primaryKeys.Count == 0)
            throw new JJMasterDataException("Primary key not found");

        if (pks.Length != primaryKeys.Count)
            throw new JJMasterDataException("Primary key not found");
        
        var filter = new Dictionary<string, object>(StringComparer.InvariantCultureIgnoreCase);
        for (var index = 0; index < primaryKeys.Count; index++)
        {
            var field = primaryKeys[index];
            filter.Add(field.Name, pks[index]);
        }

        return LoadValuesFromPkAsync(filter);
    }


    /// <inheritdoc cref="ValidateFields()"/>
    public Dictionary<string, string> ValidateFields(Dictionary<string, object> values)
    {
        return ValidateFields(values, PageState);
    }
    
    /// <summary>
    /// Validate form fields and return a list with errors
    /// </summary>
    /// <returns>
    /// Key = Field Name
    /// Valor = Error message
    /// </returns>
    public Dictionary<string, string> ValidateFields(Dictionary<string, object> values, PageState pageState, bool enableErrorLink = true)
    {
        return fieldValidationService.ValidateFields(FormElement, values, pageState, enableErrorLink);
    }
    
    internal Task<JsonComponentResult> GetUrlRedirectResult(ActionMap actionMap)
    {
        return urlRedirectService.GetUrlRedirectResult(this, actionMap);
    }
}
