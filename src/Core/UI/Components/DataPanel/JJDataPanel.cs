using JJMasterData.Commons.Cryptography;
using JJMasterData.Commons.Data.Entity.Abstractions;
using JJMasterData.Commons.Util;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataDictionary.Repository.Abstractions;
using JJMasterData.Core.DataManager;
using JJMasterData.Core.DataManager.Services.Abstractions;
using JJMasterData.Core.Extensions;
using JJMasterData.Core.Web.Factories;
using JJMasterData.Core.Web.Html;
using JJMasterData.Core.Web.Http.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JJMasterData.Core.DataManager.Expressions.Abstractions;
using JJMasterData.Core.DataManager.Services;
using JJMasterData.Core.UI.Components;
#if NET48
using JJMasterData.Commons.Configuration;
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
    
    public IEntityRepository EntityRepository { get; }
    internal IHttpContext CurrentContext { get; }
    internal JJMasterDataUrlHelper UrlHelper { get; }
    internal IFieldsService FieldsService { get; }
    internal IFormValuesService FormValuesService { get; }
    internal IExpressionsService ExpressionsService { get; }
    internal IComponentFactory ComponentFactory { get; }

    #endregion

    #region "Constructors"
#if NET48
    public JJDataPanel() : base(StaticServiceLocator.Provider.GetScopedDependentService<IQueryString>(), StaticServiceLocator.Provider.GetScopedDependentService<IEncryptionService>())
    {
        ComponentFactory = StaticServiceLocator.Provider.GetScopedDependentService<IComponentFactory>();
        EntityRepository =  StaticServiceLocator.Provider.GetScopedDependentService<IEntityRepository>();
        CurrentContext =  StaticServiceLocator.Provider.GetScopedDependentService<IHttpContext>();
        FieldsService = StaticServiceLocator.Provider.GetScopedDependentService<IFieldsService>();
        FormValuesService = StaticServiceLocator.Provider.GetScopedDependentService<IFormValuesService>();
        ExpressionsService = StaticServiceLocator.Provider.GetScopedDependentService<IExpressionsService>();
        UrlHelper = StaticServiceLocator.Provider.GetScopedDependentService<JJMasterDataUrlHelper>();

        Values = new Dictionary<string, object>();
        Errors =  new Dictionary<string, string>();
        AutoReloadFormFields = true;
        PageState = PageState.View;
    }
    
    [Obsolete("This constructor uses a static service locator, and have business logic inside it. This an anti pattern. Please use ComponentsFactory.")]
    public JJDataPanel(string elementName): this()
    {
        Name = "pnl_" + elementName;
        FormElement = StaticServiceLocator.Provider.GetScopedDependentService<IDataDictionaryRepository>()
            .GetMetadataAsync(elementName).GetAwaiter().GetResult();
        RenderPanelGroup = FormElement.Panels.Count > 0;
    }
    
    [Obsolete("This constructor uses a static service locator. This an anti pattern. Please use ComponentsFactory.")]
    public JJDataPanel(
        FormElement formElement) : this()
    {
        Name = "pnl_" + formElement.Name.ToLower();
        FormElement = formElement;
        RenderPanelGroup = formElement.Panels.Count > 0;
    }
#endif

    public JJDataPanel(
        IEntityRepository entityRepository,
        IHttpContext currentContext,
        IEncryptionService encryptionService,
        JJMasterDataUrlHelper urlHelper,
        IFieldsService fieldsService,
        IFormValuesService formValuesService,
        IExpressionsService expressionsService,
        IComponentFactory componentFactory
    ) : base(currentContext.Request.QueryString, encryptionService)
    {
        EntityRepository = entityRepository;
        CurrentContext = currentContext;
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
        IFieldsService fieldsService,
        IFormValuesService formValuesService,
        IExpressionsService expressionsService,
        IComponentFactory componentFactory
    ) : this(entityRepository,  currentContext, encryptionService, urlHelper, fieldsService, formValuesService, expressionsService, componentFactory)
    {
        Name = "pnl_" + formElement.Name.ToLower();
        FormElement = formElement;
        RenderPanelGroup = formElement.Panels.Count > 0;
    }

    #endregion
    
    protected override async Task<ComponentResult> BuildResultAsync()
    {
        Values ??= await GetFormValuesAsync();
        
        if (ComponentContext is ComponentContext.FileUpload)
            return await JJTextFile.GetResultFromPanel(this);
        
        if (ComponentContext is ComponentContext.DownloadFile)
            return ComponentFactory.Downloader.Create().GetDirectDownloadFromUrl();

        if (ComponentContext is ComponentContext.SearchBox)
        {
            var formStateData = new FormStateData(Values, UserValues,PageState);
            
            var fieldName = QueryString["fieldName"];

            var field = FormElement.Fields[fieldName];
            
            var searchBox = ComponentFactory.Controls.Create<JJSearchBox>(FormElement,field,new ControlContext(formStateData));
            return await searchBox.GetResultAsync();
        }

        if (ComponentContext is ComponentContext.PanelReload)
        {
            var html = await GetPanelHtmlAsync();
            var panelHtml = html.ToString();
            return new HtmlComponentResult(panelHtml);
        }


        return new RenderedComponentResult(await GetPanelHtmlAsync());
    }

    internal async Task<HtmlBuilder> GetPanelHtmlAsync()
    {
        var html = new HtmlBuilder(HtmlTag.Div)
            .WithAttributes(Attributes)
            .WithNameAndId(Name)
            .WithCssClass(CssClass);

        if (PageState == PageState.Update)
        {
            html.AppendHiddenInput($"data-panel-pk-values-{FormElement.Name}", GetPkHiddenInput());
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
