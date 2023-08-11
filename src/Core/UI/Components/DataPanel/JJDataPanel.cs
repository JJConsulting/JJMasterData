using JJMasterData.Commons.Cryptography;
using JJMasterData.Commons.Data.Entity.Abstractions;
using JJMasterData.Commons.Util;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataDictionary.Repository.Abstractions;
using JJMasterData.Core.DataManager;
using JJMasterData.Core.DataManager.Services.Abstractions;
using JJMasterData.Core.Extensions;
using JJMasterData.Core.FormEvents.Args;
using JJMasterData.Core.Web.Factories;
using JJMasterData.Core.Web.Html;
using JJMasterData.Core.Web.Http.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JJMasterData.Core.DataManager.Services;
#if NET48
using JJMasterData.Commons.Configuration;
#endif
namespace JJMasterData.Core.Web.Components;

/// <summary>
/// Render panels with fields
/// </summary>
public class JJDataPanel : JJAsyncComponentBase
{
    #region "Events"

    public event EventHandler<ActionEventArgs> OnRenderAction;

    #endregion

    #region "Properties"

    private FormUI _formUI;

    /// <summary>
    /// Layout form settings
    /// </summary>
    public FormUI FormUI
    {
        get => _formUI ??= FormElement.Options.Form ?? new FormUI();
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
    public IDictionary<string, dynamic> Errors { get; set; }

    /// <summary>
    /// Field Values.
    /// Key=Field Name, Value=Field Value
    /// </summary>
    public IDictionary<string, dynamic> Values { get; set; }

    /// <summary>
    /// When reloading the panel, keep the values entered in the form
    /// (Default=True)
    /// </summary>
    public bool AutoReloadFormFields { get; set; }

    /// <summary>
    /// Render field grouping
    /// </summary>
    internal bool RenderPanelGroup { get; set; }

    public IEntityRepository EntityRepository { get; }

    public IDataDictionaryRepository DataDictionaryRepository { get; }

    internal IHttpContext CurrentContext { get; }
    internal JJMasterDataUrlHelper UrlHelper { get; }
    internal JJMasterDataEncryptionService EncryptionService { get; }
    internal IFieldsService FieldsService { get; }
    internal IFormValuesService FormValuesService { get; }
    internal IExpressionsService ExpressionsService { get; }
    internal ControlFactory ControlFactory { get; }

    #endregion

    #region "Constructors"
#if NET48
    public JJDataPanel()
    {
        ControlFactory = StaticServiceLocator.Provider.GetScopedDependentService<ControlFactory>();
        EntityRepository =  StaticServiceLocator.Provider.GetScopedDependentService<IEntityRepository>();
        DataDictionaryRepository = StaticServiceLocator.Provider.GetScopedDependentService<IDataDictionaryRepository>();
        CurrentContext =  StaticServiceLocator.Provider.GetScopedDependentService<IHttpContext>();
        EncryptionService = StaticServiceLocator.Provider.GetScopedDependentService<JJMasterDataEncryptionService>();
        FieldsService = StaticServiceLocator.Provider.GetScopedDependentService<IFieldsService>();
        FormValuesService = StaticServiceLocator.Provider.GetScopedDependentService<IFormValuesService>();
        ExpressionsService = StaticServiceLocator.Provider.GetScopedDependentService<IExpressionsService>();
        UrlHelper = StaticServiceLocator.Provider.GetScopedDependentService<JJMasterDataUrlHelper>();
        EncryptionService = StaticServiceLocator.Provider.GetScopedDependentService<JJMasterDataEncryptionService>();

        Values = new Dictionary<string,dynamic>();
        Errors =  new Dictionary<string,dynamic>();
        AutoReloadFormFields = true;
        PageState = PageState.View;
    }
    
    [Obsolete("This constructor uses a static service locator, and have business logic inside it. This an anti pattern. Please use ComponentsFactory.")]
    public JJDataPanel(string elementName): this()
    {
        Name = "pnl_" + elementName;
        FormElement = StaticServiceLocator.Provider.GetScopedDependentService<IDataDictionaryRepository>()
            .GetMetadata(elementName);
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
        IDataDictionaryRepository dataDictionaryRepository,
        IHttpContext currentContext,
        JJMasterDataEncryptionService encryptionService,
        JJMasterDataUrlHelper urlHelper,
        IFieldsService fieldsService,
        IFormValuesService formValuesService,
        IExpressionsService expressionsService,
        ControlFactory controlFactory
    )
    {
        EntityRepository = entityRepository;
        DataDictionaryRepository = dataDictionaryRepository;
        CurrentContext = currentContext;
        EncryptionService = encryptionService;
        UrlHelper = urlHelper;
        FieldsService = fieldsService;
        FormValuesService = formValuesService;
        ExpressionsService = expressionsService;
        ControlFactory = controlFactory;
        Values = new Dictionary<string, dynamic>();
        Errors = new Dictionary<string, dynamic>();
        AutoReloadFormFields = true;
        PageState = PageState.View;
    }

    public JJDataPanel(
        FormElement formElement,
        IEntityRepository entityRepository,
        IDataDictionaryRepository dataDictionaryRepository,
        IHttpContext currentContext,
        JJMasterDataEncryptionService encryptionService,
        JJMasterDataUrlHelper urlHelper,
        IFieldsService fieldsService,
        IFormValuesService formValuesService,
        IExpressionsService expressionsService,
        ControlFactory controlFactory
    ) : this(entityRepository, dataDictionaryRepository, currentContext, encryptionService, urlHelper, fieldsService, formValuesService, expressionsService, controlFactory)
    {
        Name = "pnl_" + formElement.Name.ToLower();
        FormElement = formElement;
        RenderPanelGroup = formElement.Panels.Count > 0;
    }

    #endregion
    
    protected override async Task<HtmlBuilder> RenderHtmlAsync()
    {
        Values ??= await GetFormValuesAsync();
        string requestType = CurrentContext.Request.QueryString("t");
        string panelName = CurrentContext.Request.QueryString("pnlname");
        
        if (JJLookup.IsLookupRoute(this, CurrentContext))
            return JJLookup.ResponseRoute(this);
        
        if (JJTextFile.IsUploadViewRoute(this, CurrentContext))
            return JJTextFile.ResponseRoute(this);
        
        if (JJFileDownloader.IsDownloadRoute(CurrentContext))
            //TODO GUSTAVO INJETAR FILEDOWNLOADER FACTORY OU ARRUMAR
            return JJFileDownloader.ResponseRoute(CurrentContext, EncryptionService, null);

        if (JJSearchBox.IsSearchBoxRoute(this, CurrentContext))
            return JJSearchBox.ResponseJson(this, CurrentContext);

        if ("reloadPanel".Equals(requestType) && Name.Equals(panelName))
        {
            var html = await GetPanelHtmlAsync();
            CurrentContext.Response.SendResponse(html.ToString());
            return null;
        }

        if ("geturlaction".Equals(requestType))
        {
            var encryptedActionMap = CurrentContext.Request["current-form-action-" + Name.ToLower()];
            await SendUrlRedirect(EncryptionService.DecryptActionMap(encryptedActionMap));
            return null;
        }

        return await GetPanelHtmlAsync();
    }

    internal async Task<HtmlBuilder> GetPanelHtmlAsync()
    {
        var html = new HtmlBuilder(HtmlTag.Div)
            .WithAttributes(Attributes)
            .WithNameAndId(Name)
            .WithCssClass(CssClass);

        if (PageState == PageState.Update)
        {
            html.AppendHiddenInput($"jjform_pkval_{FormElement.Name}", GetPkHiddenInput());
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
    public async Task<IDictionary<string, dynamic>> GetFormValuesAsync()
    {
        return await FormValuesService.GetFormValuesWithMergedValuesAsync(FormElement, PageState, AutoReloadFormFields);
    }

    [Obsolete($"{SynchronousMethodObsolete.Message}Please use LoadValuesFromPkAsync")]
    public void LoadValuesFromPK(IDictionary<string, dynamic> pks)
    {
        Values = EntityRepository.GetDictionaryAsync(FormElement, pks).GetAwaiter().GetResult();
    }

    public async Task LoadValuesFromPkAsync(IDictionary<string, dynamic> pks)
    {
        Values = await EntityRepository.GetDictionaryAsync(FormElement, pks);
    }

    /// <summary>
    /// Validate form fields and return a list with errors
    ///  </summary>
    /// <returns>
    /// Key = Field Name
    /// Valor = Error message
    /// </returns>
    public async Task<IDictionary<string, dynamic>> ValidateFieldsAsync(IDictionary<string, dynamic> values, PageState pageState)
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
    public async Task<IDictionary<string, dynamic>> ValidateFieldsAsync(IDictionary<string, dynamic> values, PageState pageState, bool enableErrorLink)
    {
        return await FieldsService.ValidateFieldsAsync(FormElement, values, pageState, enableErrorLink);
    }
    
    internal async Task SendUrlRedirect(ActionMap actionMap)
    {
        var model = await new UrlRedirectService(FormValuesService, ExpressionsService).GetUrlRedirectAsync(FormElement,actionMap, PageState);
        
        CurrentContext.Response.SendResponse(model.ToJson(), "application/json");
    }
}
