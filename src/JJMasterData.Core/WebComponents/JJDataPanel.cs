using System;
using System.Collections;
using System.Linq;
using System.Text;
using JJMasterData.Commons.Dao;
using JJMasterData.Commons.DI;
using JJMasterData.Commons.Util;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataDictionary.Action;
using JJMasterData.Core.DataDictionary.Repository;
using JJMasterData.Core.DataManager;
using JJMasterData.Core.Facades;
using JJMasterData.Core.FormEvents.Args;
using JJMasterData.Core.Html;
using JJMasterData.Core.WebComponents.Factories;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace JJMasterData.Core.WebComponents;

/// <summary>
/// Render panels with fields
/// </summary>
public class JJDataPanel : JJBaseView
{
    private readonly RepositoryServicesFacade _repositoryServicesFacade;
    private readonly CoreServicesFacade _coreServicesFacade;

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

            var expression = new ExpressionManager(UserValues, EntityRepository);

            _fieldManager = new FieldManager(FormElement, _repositoryServicesFacade, _coreServicesFacade, expression);
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

    #endregion

    #region "Constructors"

    internal JJDataPanel(RepositoryServicesFacade repositoryServicesFacade, CoreServicesFacade coreServicesFacade)
    {
        EntityRepository = repositoryServicesFacade.EntityRepository;
        DataDictionaryRepository = repositoryServicesFacade.DataDictionaryRepository;
        Values = new Hashtable();
        Errors = new Hashtable();
        AutoReloadFormFields = true;
        PageState = PageState.View;
        _repositoryServicesFacade = repositoryServicesFacade;
        _coreServicesFacade = coreServicesFacade;
    }

    [Obsolete("Please use DataPanelFactory by constructor injection.")]
    internal JJDataPanel()
    {
        EntityRepository = JJService.Provider.GetRequiredService<IEntityRepository>();
        DataDictionaryRepository = JJService.Provider.GetRequiredService<IDataDictionaryRepository>();
        Values = new Hashtable();
        Errors = new Hashtable();
        AutoReloadFormFields = true;
        PageState = PageState.View;
        _repositoryServicesFacade = JJService.Provider.GetRequiredService<RepositoryServicesFacade>();
        _coreServicesFacade = JJService.Provider.GetRequiredService<CoreServicesFacade>();
    }

    [Obsolete("Please use DataPanelFactory by constructor injection.")]
    public JJDataPanel(string elementName) : this()
    {
        var factory = JJService.Provider.GetRequiredService<DataPanelFactory>();
        factory.SetDataPanelParams(this, elementName);
    }

    public JJDataPanel(FormElement formElement, RepositoryServicesFacade repositoryServicesFacade,
        CoreServicesFacade coreServicesFacade) : this(repositoryServicesFacade, coreServicesFacade)
    {
        var factory = new DataPanelFactory(repositoryServicesFacade, coreServicesFacade);
        factory.SetDataPanelParams(this, formElement);
    }

    public JJDataPanel(FormElement formElement, Hashtable values, Hashtable errors, PageState pageState,
        RepositoryServicesFacade repositoryServicesFacade, CoreServicesFacade coreServicesFacade) : this(formElement,
        repositoryServicesFacade, coreServicesFacade)
    {
        Values = values;
        Errors = errors;
        PageState = pageState;
    }

    #endregion

    internal override HtmlBuilder RenderHtml()
    {
        Values = GetFormValues();
        string requestType = CurrentContext.Request.QueryString("t");
        string objname = CurrentContext.Request.QueryString("objname");
        string pnlname = CurrentContext.Request.QueryString("pnlname");

        //Lookup Route
        if (JJLookup.IsLookupRoute(this))
            return JJLookup.ResponseRoute(this);

        //FormUpload Route
        if (JJTextFile.IsFormUploadRoute(this))
            return JJTextFile.ResponseRoute(this);

        //DownloadFile Route
        if (JJDownloadFile.IsDownloadRoute(this))
            return JJDownloadFile.ResponseRoute(this);

        if ("reloadpainel".Equals(requestType) && Name.Equals(pnlname))
        {
            CurrentContext.Response.SendResponse(GetHtmlPanel().ToString());
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
        return Cript.Cript64(pkval);
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

        if (CurrentContext.HasContext())
        {
            string criptPkval = CurrentContext.Request["jjform_pkval_" + Name];
            if (!string.IsNullOrEmpty(criptPkval))
            {
                string parsedPkval = Cript.Descript64(criptPkval);
                var filters = DataHelper.GetPkValues(FormElement, parsedPkval, '|');
                var entityRepository = FieldManager.Expression.EntityRepository;
                tempvalues = entityRepository.GetFields(FormElement, filters);
            }
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
    ///  </summary>
    /// <returns>
    /// Key = Field Name
    /// Valor = Error message
    /// </returns>
    public Hashtable ValidateFields(Hashtable values, PageState pageState)
    {
        return ValidateFields(values, pageState, true);
    }

    /// <summary>
    /// Validate form fields and return a list with errors
    /// </summary>
    /// <returns>
    /// Key = Field Name
    /// Valor = Error message
    /// </returns>
    public Hashtable ValidateFields(Hashtable values, PageState pageState, bool enableErrorLink)
    {
        var formManager = new FormManager(FormElement, FieldManager.Expression);
        return formManager.ValidateFields(values, pageState, enableErrorLink);
    }

    internal void ResponseUrlAction()
    {
        if (!Name.Equals(CurrentContext.Request["objname"]))
            return;

        string criptMap = CurrentContext.Request["criptid"];
        if (string.IsNullOrEmpty(criptMap))
            return;

        string jsonMap = Cript.Descript64(criptMap);
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

            CurrentContext.Response.SendResponse(JsonConvert.SerializeObject(result), "application/json");
        }
    }
}