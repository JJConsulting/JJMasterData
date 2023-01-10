using System;
using System.Collections;
using System.Linq;
using System.Text;
using JJMasterData.Commons.Cryptography;
using JJMasterData.Commons.Dao.Entity.Abstractions;
using JJMasterData.Commons.DI;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataDictionary.Action;
using JJMasterData.Core.DataDictionary.Repository.Abstractions;
using JJMasterData.Core.DataManager;
using JJMasterData.Core.Facades;
using JJMasterData.Core.FormEvents.Args;
using JJMasterData.Core.Html;
using JJMasterData.Core.Http.Abstractions;
using JJMasterData.Core.Options;
using JJMasterData.Core.WebComponents.Factories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace JJMasterData.Core.WebComponents;

/// <summary>
/// Render panels with fields
/// </summary>
public class JJDataPanel : JJBaseView
{
    private readonly RepositoryServicesFacade _repositoryServicesFacade;


    #region "Events"

    public EventHandler<ActionEventArgs> OnRenderAction;

    #endregion

    #region "Properties"

    private FieldManager _fieldManager;
    private UIForm _uiFormSettings;
    public IEntityRepository EntityRepository { get; }
    public IDataDictionaryRepository DataDictionaryRepository { get; }

    internal FieldManager FieldManager
    {
        get
        {
            if (_fieldManager != null)
                return _fieldManager;

            var expression = new ExpressionManager(UserValues, EntityRepository, HttpContext, LoggerFactory);

            _fieldManager = new FieldManager(FormElement, HttpContext, _repositoryServicesFacade, expression,EncryptionService,Options,LoggerFactory);
            return _fieldManager;
        }
    }


    /// <summary>
    /// Layout form settings
    /// </summary>
    public UIForm UISettings
    {
        get => _uiFormSettings ??= new UIForm();
        internal set => _uiFormSettings = value;
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
    public Hashtable Errors { get; set; }

    /// <summary>
    /// Field Values.
    /// Key=Field Name, Value=Error Description
    /// </summary>
    public Hashtable Values { get; set; }

    /// <summary>
    /// When reloading the panel, keep the values ​​entered in the form
    /// (Default=True)
    /// </summary>
    public bool AutoReloadFormFields { get; set; }

    /// <summary>
    /// Render field grouping
    /// </summary>
    internal bool RenderPanelGroup { get; set; }

    internal IHttpContext HttpContext { get; set; }

    internal ILoggerFactory LoggerFactory { get; }
    
    internal JJMasterDataEncryptionService EncryptionService { get; }

    internal IOptions<JJMasterDataCoreOptions> Options { get; }

    #endregion

    #region "Constructors"

    
    [Obsolete("Please use DataPanelFactory by constructor injection.")]
    internal JJDataPanel()
    {

        using var scope = JJService.Provider.CreateScope();
        _repositoryServicesFacade = scope.ServiceProvider.GetRequiredService<RepositoryServicesFacade>();

        EntityRepository = scope.ServiceProvider.GetRequiredService<IEntityRepository>();
        HttpContext = scope.ServiceProvider.GetRequiredService<IHttpContext>();
        DataDictionaryRepository = scope.ServiceProvider.GetRequiredService<IDataDictionaryRepository>();
        LoggerFactory = JJService.Provider.GetRequiredService<ILoggerFactory>();
        EncryptionService = scope.ServiceProvider.GetRequiredService<JJMasterDataEncryptionService>();

        DataPanelFactory.SetDataPanelParams(this);
    }

    [Obsolete("Please use DataPanelFactory by constructor injection.")]
    public JJDataPanel(string elementName) : this()
    {
        using var scope = JJService.Provider.CreateScope();
        var factory = scope.ServiceProvider.GetRequiredService<DataPanelFactory>();
        factory.SetDataPanelParams(this, elementName);
    }
    
    internal JJDataPanel(
        IHttpContext httpContext, 
        RepositoryServicesFacade repositoryServicesFacade, 
        JJMasterDataEncryptionService encryptionService,
        IOptions<JJMasterDataCoreOptions> options,
        ILoggerFactory loggerFactory)
    {
        EntityRepository = repositoryServicesFacade.EntityRepository;
        HttpContext = httpContext;
        DataDictionaryRepository = repositoryServicesFacade.DataDictionaryRepository;
        LoggerFactory = loggerFactory;
        EncryptionService = encryptionService;
        _repositoryServicesFacade = repositoryServicesFacade;
        Options = options;
        

        DataPanelFactory.SetDataPanelParams(this);
    }

    #endregion

    internal override HtmlBuilder RenderHtml()
    {
        Values = GetFormValues();
        string requestType = HttpContext.Request.QueryString("t");
        string objname = HttpContext.Request.QueryString("objname");
        string pnlname = HttpContext.Request.QueryString("pnlname");

        //Lookup Route
        if (JJLookup.IsLookupRoute(HttpContext, this))
            return JJLookup.ResponseRoute(this);

        //FormUpload Route
        if (JJTextFile.IsFormUploadRoute(HttpContext, this))
            return JJTextFile.ResponseRoute(this);

        //DownloadFile Route
        if (JJDownloadFile.IsDownloadRoute(HttpContext))
            return JJDownloadFile.ResponseRoute(HttpContext, EncryptionService, LoggerFactory);

        if ("reloadpainel".Equals(requestType) && Name.Equals(pnlname))
        {
            HttpContext.Response.SendResponse(GetHtmlPanel().ToString());
            return null;
        }

        if ("jjsearchbox".Equals(requestType))
        {
            if (Name.Equals(pnlname))
            {
                var f = FormElement.Fields.ToList().Find(x => x.Name.Equals(objname));
                if (f != null)
                {
                    var jjSearchBox = FieldManager.GetField(f, PageState, Values);
                    jjSearchBox.GetHtml();
                }
            }

            return null;
        }

        if ("geturlaction".Equals(requestType))
        {
            ResponseUrlAction();
            return null;
        }

        return GetHtmlPanel();
    }

    internal HtmlBuilder GetHtmlPanel()
    {
        var html = new HtmlBuilder(HtmlTag.Div)
            .WithAttributes(Attributes)
            .WithNameAndId(Name)
            .WithCssClass(CssClass);

        if (PageState == PageState.Update)
        {
            html.AppendHiddenInput($"jjform_pkval_{Name}", GetPkInputHidden());
        }

        var panelGroup = new DataPanelGroup(this);
        html.AppendRange(panelGroup.GetHtmlPanelList());
        html.AppendScript(GetHtmlFormScript());

        return html;
    }

    private string GetPkInputHidden()
    {
        string pkval = DataHelper.ParsePkValues(FormElement, Values, '|');
        return EncryptionService.EncryptString(pkval);
    }

    private string GetHtmlFormScript()
    {
        var script = new StringBuilder();
        script.AppendLine("");
        script.AppendLine("$(document).ready(function () { ");

        if (UISettings.EnterKey == FormEnterKey.Tab)
        {
            script.AppendLine($"\tjjutil.replaceEntertoTab('{Name}');");
        }

        var listField = FormElement.Fields.ToList();
        if (!listField.Exists(x => x.AutoPostBack))
        {
            script.AppendLine(new DataPanelScript(this).GetHtmlFormScript());
        }

        script.AppendLine("});");
        return script.ToString();
    }


    /// <summary>
    /// Load form data with default values and triggers
    /// </summary>
    public Hashtable GetFormValues()
    {
        Hashtable tempvalues = null;


        string criptPkval = HttpContext.Request["jjform_pkval_" + Name];
        if (!string.IsNullOrEmpty(criptPkval))
        {
            string parsedPkval = EncryptionService.DecryptString(criptPkval);
            var filters = DataHelper.GetPkValues(FormElement, parsedPkval, '|');
            var entityRepository = FieldManager.Expression.EntityRepository;
            tempvalues = entityRepository.GetFields(FormElement, filters);
        }


        tempvalues ??= new Hashtable();

        DataHelper.CopyIntoHash(ref tempvalues, Values, true);

        var formValues = new FormValues(FieldManager);
        return formValues.GetFormValues(PageState, tempvalues, AutoReloadFormFields);
    }

    /// <summary>
    /// Load values from database
    /// </summary>
    public void LoadValuesFromPK(Hashtable pks)
    {
        var entityRepository = FieldManager.Expression.EntityRepository;
        Values = entityRepository.GetFields(FormElement, pks);
    }

    /// <summary>
    /// Validate form fields and return a list with errors
    /// </summary>
    /// <returns>
    /// Key = Field Name
    /// Valor = Error message
    /// </returns>
    public Hashtable ValidateFields(Hashtable values, PageState pageState, bool enableErrorLink = true)
    {
        var formManager = new FormManager(FormElement, FieldManager.Expression);
        return formManager.ValidateFields(values, pageState, enableErrorLink);
    }

    internal void ResponseUrlAction()
    {
        if (!Name.Equals(HttpContext.Request["objname"]))
            return;

        string criptMap = HttpContext.Request["criptid"];
        if (string.IsNullOrEmpty(criptMap))
            return;

        string jsonMap = EncryptionService.DecryptString(criptMap);
        var parms = JsonConvert.DeserializeObject<ActionMap>(jsonMap);

        var action = FormElement.Fields[parms?.FieldName].Actions.Get(parms?.ActionName);
        var values = GetFormValues();

        if (action is UrlRedirectAction urlAction)
        {
            string parsedUrl = FieldManager.Expression.ParseExpression(urlAction.UrlRedirect, PageState, false, values);
            var result = new Hashtable();
            result.Add("UrlAsPopUp", urlAction.UrlAsPopUp);
            result.Add("TitlePopUp", urlAction.TitlePopUp);
            result.Add("UrlRedirect", parsedUrl);

            HttpContext.Response.SendResponse(JsonConvert.SerializeObject(result), "application/json");
        }
    }
}