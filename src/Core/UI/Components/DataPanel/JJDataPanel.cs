using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JJMasterData.Commons.Configuration;
using JJMasterData.Commons.Cryptography;
using JJMasterData.Commons.Data.Entity.Abstractions;
using JJMasterData.Commons.DI;
using JJMasterData.Commons.Util;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataDictionary.Actions.UserCreated;
using JJMasterData.Core.DataManager;
using JJMasterData.Core.FormEvents.Args;
using JJMasterData.Core.Web.Factories;
using JJMasterData.Core.DataDictionary.Repository.Abstractions;
using JJMasterData.Core.DataManager.Services;
using JJMasterData.Core.DataManager.Services.Abstractions;
using JJMasterData.Core.DI;
using JJMasterData.Core.Extensions;
using JJMasterData.Core.Web.Html;
using Newtonsoft.Json;

namespace JJMasterData.Core.Web.Components;

/// <summary>
/// Render panels with fields
/// </summary>
public class JJDataPanel : JJBaseView
{
    #region "Events"

    public EventHandler<ActionEventArgs> OnRenderAction;

    #endregion

    #region "Properties"

    private FieldManager _fieldManager;
    private FormUI _formUI;
    private IEntityRepository _entityRepository;
    private IDataDictionaryRepository _dataDictionaryRepository;
    
    public IEntityRepository EntityRepository
    {
        get => _entityRepository ??= JJService.EntityRepository;
        set => _entityRepository = value;
    }

    public IDataDictionaryRepository DataDictionaryRepository
    {
        get => _dataDictionaryRepository ??= JJServiceCore.DataDictionaryRepository;
    }

    public FieldManager FieldManager
    {
        get
        {
            if (_fieldManager == null)
            {
                _fieldManager = new FieldManager(Name, FormElement);
            }
            return _fieldManager;
        }
    }


    /// <summary>
    /// Layout form settings
    /// </summary>
    public FormUI FormUI
    {
        get => _formUI ??= new FormUI();
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
    public IDictionary<string,dynamic> Errors { get; set; }

    /// <summary>
    /// Field Values.
    /// Key=Field Name, Value=Field Value
    /// </summary>
    public IDictionary<string,dynamic> Values { get; set; }

    /// <summary>
    /// When reloading the panel, keep the values ​​entered in the form
    /// (Default=True)
    /// </summary>
    public bool AutoReloadFormFields { get; set; }

    /// <summary>
    /// Render field grouping
    /// </summary>
    internal bool RenderPanelGroup { get; set; }

    public IFieldFormattingService FieldFormattingService { get; } =
        JJService.Provider.GetScopedDependentService<IFieldFormattingService>();

    private JJMasterDataEncryptionService EncryptionService { get; } =
        JJService.Provider.GetScopedDependentService<JJMasterDataEncryptionService>();
    
    public IFieldEvaluationService FieldEvaluationService { get; } = JJService.Provider.GetScopedDependentService<IFieldEvaluationService>();
    public IFormFieldsService FormFieldsService { get; } = JJService.Provider.GetScopedDependentService<IFormFieldsService>();
    public IFormValuesService FormValuesService { get; } = JJService.Provider.GetScopedDependentService<IFormValuesService>();

    
    #endregion

    #region "Constructors"

    internal JJDataPanel()
    {
        Values = new Dictionary<string,dynamic>();
        Errors =  new Dictionary<string,dynamic>();
        AutoReloadFormFields = true;
        PageState = PageState.View;
    }

    public JJDataPanel(string elementName) : this()
    {
        DataPanelFactory.SetDataPanelParams(this, elementName);
    }

    public JJDataPanel(FormElement formElement) : this()
    {
        DataPanelFactory.SetDataPanelParams(this, formElement);   
    }

    public JJDataPanel(FormElement formElement, IDictionary<string,dynamic>values, IDictionary<string,dynamic>errors, PageState pageState) : this(formElement)
    {
        Values = values;
        Errors = errors;
        PageState = pageState;
    }

    #endregion

    internal override HtmlBuilder RenderHtml()
    {
        Values ??= GetFormValues().GetAwaiter().GetResult();
        string requestType = CurrentContext.Request.QueryString("t");
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

        if (JJSearchBox.IsSearchBoxRoute(this))
            return JJSearchBox.ResponseJson(this);

        if ("reloadpainel".Equals(requestType) && Name.Equals(pnlname))
        {
            CurrentContext.Response.SendResponse(GetHtmlPanel().ToString());
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
            html.AppendHiddenInput($"jjform_pkval_{FormElement.Name}", GetPkHiddenInput());
        }

        var panelGroup = new DataPanelLayout(this);
        html.AppendRange(panelGroup.GetHtmlPanelList());
        html.AppendScript(GetHtmlFormScript());

        return html;
    }

    private string GetPkHiddenInput()
    {
        string pkval = DataHelper.ParsePkValues(FormElement, Values, '|');
        return EncryptionService.EncryptStringWithUrlEncode(pkval);
    }

    private string GetHtmlFormScript()
    {
        var script = new StringBuilder();
        script.AppendLine("");
        script.AppendLine("$(document).ready(function () { ");

        if (FormUI.EnterKey == FormEnterKey.Tab)
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
    public async Task<IDictionary<string,dynamic>> GetFormValues()
    {
        return await FormValuesService.GetFormValuesWithMergedValues(FormElement,PageState, AutoReloadFormFields);
    }

    /// <summary>
    /// Load values from database
    /// </summary>
    [Obsolete("Make this async")]
    public void LoadValuesFromPK(IDictionary<string,dynamic>pks)
    {
        var entityRepository = JJService.EntityRepository;
        Values = entityRepository.GetDictionaryAsync(FormElement, pks).GetAwaiter().GetResult();
    }

    /// <summary>
    /// Validate form fields and return a list with errors
    ///  </summary>
    /// <returns>
    /// Key = Field Name
    /// Valor = Error message
    /// </returns>
    public IDictionary<string,dynamic>ValidateFields(IDictionary<string,dynamic>values, PageState pageState)
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
    public IDictionary<string,dynamic>ValidateFields(IDictionary<string,dynamic>values, PageState pageState, bool enableErrorLink)
    {
        return FormFieldsService.ValidateFields(FormElement,values, pageState, enableErrorLink);
    }

    [Obsolete("Must be async")]
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
        var values = GetFormValues().GetAwaiter().GetResult();

        if (action is UrlRedirectAction urlAction)
        {
            string parsedUrl = FieldManager.ExpressionManager.ParseExpression(urlAction.UrlRedirect, PageState,  false,values);
            var result = new Hashtable
            {
                { "UrlAsPopUp", urlAction.UrlAsPopUp },
                { "TitlePopUp", urlAction.TitlePopUp },
                { "UrlRedirect", parsedUrl }
            };

            CurrentContext.Response.SendResponse(JsonConvert.SerializeObject(result), "application/json");
        }
    }

}
