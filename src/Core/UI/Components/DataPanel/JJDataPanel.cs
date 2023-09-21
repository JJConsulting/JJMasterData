using JJMasterData.Commons.Cryptography;
using JJMasterData.Commons.Data.Entity.Abstractions;
using JJMasterData.Commons.Util;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataManager;
using JJMasterData.Core.DataManager.Services;
using JJMasterData.Core.Extensions;
using JJMasterData.Core.Web.Factories;
using JJMasterData.Core.Web.Html;
using JJMasterData.Core.Web.Http.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JJMasterData.Core.UI.Components;

#if NET48
using JJMasterData.Commons.Configuration;
using JJMasterData.Core.DataDictionary.Repository.Abstractions;
#endif

namespace JJMasterData.Core.Web.Components;

/// <summary>
/// Render panels with fields
/// </summary>
public class JJDataPanel : AsyncComponent
{
    #region "Properties"

    private FormUI _formUI;

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
    public PageState PageState { get; set; }

    /// <summary>
    /// Fields with error.
    /// Key=Field Name, Value=Error Description
    /// </summary>
    public IDictionary<string, string> Errors { get; set; }

    /// <summary>
    /// Field Values.
    /// Key=Field Name, Value=Field Value
    /// </summary>
    public IDictionary<string, object> Values { get; set; }

    /// <summary>
    /// When reloading the panel, keep the values entered in the form
    /// (Default=True)
    /// </summary>
    public bool AutoReloadFormFields { get; set; }

    /// <summary>
    /// Render field grouping
    /// </summary>
    internal bool RenderPanelGroup { get; set; }

    public string FieldNamePrefix { get; set; }
    
    
    private RouteContext _routeContext;
    protected RouteContext RouteContext
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
    
    
    public IEntityRepository EntityRepository { get; }
    internal IHttpContext CurrentContext { get; }
    internal IEncryptionService EncryptionService { get; }
    internal JJMasterDataUrlHelper UrlHelper { get; }
    internal FieldsService FieldsService { get; }
    internal FormValuesService FormValuesService { get; }
    internal ExpressionsService ExpressionsService { get; }
    internal IComponentFactory ComponentFactory { get; }

    #endregion

    #region "Constructors"
#if NET48
    public JJDataPanel() 
    {
        ComponentFactory = StaticServiceLocator.Provider.GetScopedDependentService<IComponentFactory>();
        EntityRepository =  StaticServiceLocator.Provider.GetScopedDependentService<IEntityRepository>();
        CurrentContext =  StaticServiceLocator.Provider.GetScopedDependentService<IHttpContext>();
        FieldsService = StaticServiceLocator.Provider.GetScopedDependentService<FieldsService>();
        FormValuesService = StaticServiceLocator.Provider.GetScopedDependentService<FormValuesService>();
        ExpressionsService = StaticServiceLocator.Provider.GetScopedDependentService<ExpressionsService>();
        UrlHelper = StaticServiceLocator.Provider.GetScopedDependentService<JJMasterDataUrlHelper>();

        Values = new Dictionary<string, object>();
        Errors =  new Dictionary<string, string>();
        AutoReloadFormFields = true;
        PageState = PageState.View;
    }
    
    [Obsolete("This constructor uses a static service locator, and have business logic inside it. This an anti pattern. Please use ComponentsFactory.")]
    public JJDataPanel(string elementName): this()
    {
        Name = $"{ComponentNameGenerator.Create(elementName)}-data-panel";
        FormElement = StaticServiceLocator.Provider.GetScopedDependentService<IDataDictionaryRepository>()
            .GetMetadataAsync(elementName).GetAwaiter().GetResult();
        RenderPanelGroup = FormElement.Panels.Count > 0;
    }
    
    [Obsolete("This constructor uses a static service locator. This an anti pattern. Please use ComponentsFactory.")]
    public JJDataPanel(
        FormElement formElement) : this()
    {
        Name = $"{ComponentNameGenerator.Create(formElement.Name)}-data-panel";
        FormElement = formElement;
        RenderPanelGroup = formElement.Panels.Count > 0;
    }
#endif

    public JJDataPanel(
        IEntityRepository entityRepository,
        IHttpContext currentContext,
        IEncryptionService encryptionService,
        JJMasterDataUrlHelper urlHelper,
        FieldsService fieldsService,
        FormValuesService formValuesService,
        ExpressionsService expressionsService,
        IComponentFactory componentFactory
    ) 
    {
        EntityRepository = entityRepository;
        CurrentContext = currentContext;
        EncryptionService = encryptionService;
        UrlHelper = urlHelper;
        FieldsService = fieldsService;
        FormValuesService = formValuesService;
        ExpressionsService = expressionsService;
        ComponentFactory = componentFactory;
        Values = new Dictionary<string, object>();
        Errors = new Dictionary<string, string>();
        AutoReloadFormFields = true;
        PageState = PageState.View;
    }

    public JJDataPanel(
        FormElement formElement,
        IEntityRepository entityRepository,
        IHttpContext currentContext,
        IEncryptionService encryptionService,
        JJMasterDataUrlHelper urlHelper,
        FieldsService fieldsService,
        FormValuesService formValuesService,
        ExpressionsService expressionsService,
        IComponentFactory componentFactory
    ) : this(entityRepository,  currentContext, encryptionService, urlHelper, fieldsService, formValuesService, expressionsService, componentFactory)
    {
        Name = $"{ComponentNameGenerator.Create(formElement.Name)}-data-panel";
        FormElement = formElement;
        RenderPanelGroup = formElement.Panels.Count > 0;
    }

    #endregion
    
    protected override async Task<ComponentResult> BuildResultAsync()
    {
        Values ??= await GetFormValuesAsync();

        switch (ComponentContext)
        {
            case ComponentContext.TextFileUploadView:
            case ComponentContext.TextFileFileUpload:
                return await GetFieldResultAsync<JJTextFile>();
            case ComponentContext.SearchBox:
                return await GetFieldResultAsync<JJSearchBox>();
            case ComponentContext.DownloadFile:
                return ComponentFactory.Downloader.Create().GetDirectDownloadFromUrl();
            case ComponentContext.DataPanelReload:
            {
                var html = await GetPanelHtmlBuilderAsync();
                return new ContentComponentResult(html);
            }
            default:
                return new RenderedComponentResult(await GetPanelHtmlBuilderAsync());
        }
    }

    private async Task<ComponentResult> GetFieldResultAsync<TControl>() where TControl : ControlBase
    {
        var fieldName = CurrentContext.Request.QueryString["fieldName"];
        var formStateData = new FormStateData(await GetFormValuesAsync(), UserValues, PageState);
        var controlContext = new ControlContext(formStateData);

        if (!FormElement.Fields.TryGetField(fieldName, out var field))
            return new EmptyComponentResult();
        
        var control = ComponentFactory.Controls.Create<TControl>(FormElement, field, controlContext);
        control.Name = FieldNamePrefix + fieldName;
        
        return await control.GetResultAsync();
    }

    internal async Task<HtmlBuilder> GetPanelHtmlBuilderAsync()
    {
        var html = new HtmlBuilder(HtmlTag.Div)
            .WithAttributes(Attributes)
            .WithNameAndId(Name)
            .WithCssClass(CssClass);

        if (PageState == PageState.Update)
        {
            html.AppendHiddenInput($"data-panel-pk-values-{ComponentNameGenerator.Create(FormElement.Name)}", GetPkHiddenInput());
        }

        var panelGroup = new DataPanelLayout(this);
        await html.AppendRangeAsync(panelGroup.GetHtmlPanelList());
        html.AppendScript(await GetHtmlFormScript());

        return html;
    }

    private string GetPkHiddenInput()
    {
        string pkval = DataHelper.ParsePkValues(FormElement, Values, '|');
        return EncryptionService.EncryptStringWithUrlEscape(pkval);
    }

    private async Task<string> GetHtmlFormScript()
    {
        var script = new StringBuilder();
        script.AppendLine("$(document).ready(function () { ");

        if (FormUI.EnterKey == FormEnterKey.Tab)
        {
            script.AppendLine($"\tjjutil.replaceEntertoTab('{Name}');");
        }

        var listField = FormElement.Fields.ToList();
        if (!listField.Exists(x => x.AutoPostBack))
        {
            var dataPanelScript = new DataPanelExpressionScripts(this);
            script.AppendLine(await dataPanelScript.GetHtmlFormScript());
        }

        script.AppendLine("});");
        return script.ToString();
    }
    
    /// <summary>
    /// Load form data with default values and triggers
    /// </summary>
    public async Task<IDictionary<string, object>> GetFormValuesAsync()
    {
        return await FormValuesService.GetFormValuesWithMergedValuesAsync(FormElement, PageState, AutoReloadFormFields, FieldNamePrefix);
    }

    [Obsolete($"{SynchronousMethodObsolete.Message}Please use LoadValuesFromPkAsync")]
    public void LoadValuesFromPK(IDictionary<string, object> pks)
    {
        Values = EntityRepository.GetFieldsAsync(FormElement, pks).GetAwaiter().GetResult();
    }

    public async Task LoadValuesFromPkAsync(IDictionary<string, object> pks)
    {
        Values = await EntityRepository.GetFieldsAsync(FormElement, pks);
    }

    /// <summary>
    /// Validate form fields and return a list with errors
    ///  </summary>
    /// <returns>
    /// Key = Field Name
    /// Valor = Error message
    /// </returns>
    public async Task<IDictionary<string, string>> ValidateFieldsAsync(IDictionary<string, object> values, PageState pageState)
    {
        return await ValidateFieldsAsync(values, pageState, true);
    }

    /// <summary>
    /// Validate form fields and return a list with errors
    /// </summary>
    /// <returns>
    /// Key = Field Name
    /// Valor = Error message
    /// </returns>
    public async Task<IDictionary<string, string>> ValidateFieldsAsync(IDictionary<string, object> values, PageState pageState, bool enableErrorLink)
    {
        return await FieldsService.ValidateFieldsAsync(FormElement, values, pageState, enableErrorLink);
    }
    
    internal async Task<JsonComponentResult> GetUrlRedirectResult(ActionMap actionMap)
    {
        var model = await new UrlRedirectService(FormValuesService, ExpressionsService).GetUrlRedirectAsync(FormElement,actionMap, PageState);

        return new JsonComponentResult(model);
    }
}
