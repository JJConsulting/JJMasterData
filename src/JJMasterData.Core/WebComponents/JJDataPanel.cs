using JJMasterData.Commons.Language;
using JJMasterData.Commons.Util;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataDictionary.Action;
using JJMasterData.Core.DataManager;
using JJMasterData.Core.FormEvents.Args;
using JJMasterData.Core.Html;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Linq;
using System.Text;

namespace JJMasterData.Core.WebComponents;

public class JJDataPanel : JJBaseView
{
    #region "Events"

    public EventHandler<ActionEventArgs> OnRenderAction;

    #endregion

    #region "Properties"

    private FieldManager _fieldManager;
    private DataDictionaryManager _dataDictionaryManager;
    private UIForm _uiFormSettings;

    private DataDictionaryManager DataDictionaryManager => _dataDictionaryManager ??= new DataDictionaryManager(FormElement);

    /// <summary>
    /// Useful functions for manipulating fields on the form
    /// </summary>
    public FieldManager FieldManager => _fieldManager ??= new FieldManager(this, FormElement);

    /// <summary>
    /// Layout form settings
    /// </summary>
    public UIForm UISettings
    {
        get
        {
            if (_uiFormSettings == null)
                _uiFormSettings = new UIForm();

            return _uiFormSettings;
        }
        internal set
        {
            _uiFormSettings = value;
        }
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
    public Hashtable Erros { get; set; }

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

    internal JJDataPanel()
    {
        Values = new Hashtable();
        Erros = new Hashtable();
        AutoReloadFormFields = true;
        PageState = PageState.View;
    }

    public JJDataPanel(string elementName) : this()
    {
        WebComponentFactory.SetDataPanelParams(this, elementName);
    }

    public JJDataPanel(FormElement formElement) : this()
    {
        if (formElement == null)
            throw new ArgumentNullException(nameof(formElement));

        FormElement = formElement;
        Name = "pnl_" + formElement.Name;
        RenderPanelGroup = FormElement.Panels.Count > 0;
        UISettings.IsVerticalLayout = true;
        UISettings.EnterKey = FormEnterKey.Disabled;
    }

    public JJDataPanel(FormElement formElement, Hashtable values, Hashtable erros, PageState pageState) : this(formElement)
    {
        Values = values;
        Erros = erros;
        PageState = pageState;
    }

    #endregion

    internal override HtmlElement RenderHtmlElement()
    {
        Values = GetFormValues();
        string requestType = CurrentContext.Request.QueryString("t");
        string objname = CurrentContext.Request.QueryString("objname");
        string pnlname = CurrentContext.Request.QueryString("pnlname");

        //Lookup Route
        if (JJLookup.IsLookupRoute(this))
        {
            CurrentContext.Response.SendResponse(JJLookup.ResponseRoute(this));
            return null;
        }

        //FormUpload Route
        if (JJTextFile.IsFormUploadRoute(this))
        {
            CurrentContext.Response.SendResponse(JJTextFile.ResponseRoute(this));
            return null;
        }

        //DownloadFile Route
        if (JJDownloadFile.IsDownloadRoute(this))
        {
            CurrentContext.Response.SendResponse(JJDownloadFile.ResponseRoute(this));
            return null;
        }

        if ("reloadpainel".Equals(requestType) && Name.Equals(pnlname))
        {
            CurrentContext.Response.SendResponse(GetHtmlPanel().GetElementHtml());
            return null;
        }

        if ("jjsearchbox".Equals(requestType))
        {
            if (Name.Equals(pnlname))
            {
                var f = FormElement.Fields.ToList().Find(x => x.Name.Equals(objname));
                if (f != null)
                {
                    var jjSearchBox = FieldManager.GetField(f, PageState, Values, null);
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

    internal HtmlElement GetHtmlPanel()
    {
        var html = new HtmlElement(HtmlTag.Div)
            .WithAttributes(Attributes)
            .WithNameAndId(Name)
            .WithCssClass(CssClass);

        if (PageState == PageState.Update)
        {
            html.AppendHiddenInput($"jjform_pkval_{Name}", GetPkInputHidden());
        }

        var panelGroup = new DataPanelGroup(this);
        html.AppendRange(panelGroup.GetListHtmlPanel());
        html.AppendScript(GetHtmlFormScript());

        return html;
    }

    private string GetPkInputHidden()
    {
        var sHtml = new StringBuilder();
        var pkFields = FormElement.Fields.ToList().FindAll(x => x.IsPk);
        string pkval = "";
        foreach (var pkField in pkFields)
        {
            if (!Values.ContainsKey(pkField.Name))
                throw new Exception(Translate.Key("Primary key {0} value not found", pkField.Name));

            if (pkval.Length > 0)
                pkval += "|";

            pkval += Values[pkField.Name].ToString();
        }

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
    /// Recupera os dados do Form, aplicando o valor padrão e as triggers
    /// </summary>
    public Hashtable GetFormValues()
    {
        Hashtable tempvalues = null;

        if (CurrentContext.HasContext())
        {
            string t = CurrentContext.Request.QueryString("t");
            if (!string.IsNullOrEmpty(CurrentContext.Request["jjform_pkval_" + Name]))
            {
                var filters = new Hashtable();
                string pkval = Cript.Descript64(CurrentContext.Request["jjform_pkval_" + Name]);
                var pkvalues = pkval.Split('|');
                var pkFields = FormElement.Fields.ToList().FindAll(x => x.IsPk);
                for (int i = 0; i < pkvalues.Length; i++)
                {
                    filters.Add(pkFields[i].Name, pkvalues[i]);
                }
                tempvalues = DataDictionaryManager.GetHashtable(filters).Result;
            }
        }

        tempvalues ??= new Hashtable();

        if (Values != null && FormElement.Fields.Any(f => Values.Contains(f.Name)))
        {
            foreach (var f in FormElement.Fields)
            {
                if (Values.Contains(f.Name))
                {
                    if (tempvalues.Contains(f.Name))
                        tempvalues[f.Name] = Values[f.Name];
                    else
                        tempvalues.Add(f.Name, Values[f.Name]);
                }
            }
        }

        Hashtable newvalues = FieldManager.GetFormValues("", FormElement, PageState, tempvalues, AutoReloadFormFields);
        return newvalues;
    }

    /// <summary>
    /// Carregar os valores do banco de dados de todos os campos do formulário 
    /// e atribui na propriedade Values
    /// </summary>
    public void LoadValuesFromPK(Hashtable pks)
    {
        Values = DataDictionaryManager.GetHashtable(pks).Result;
    }

    /// <summary>
    /// Valida os campos do formulário e 
    /// retorna uma lista com erros encontrados
    /// </summary>
    /// <param name="values">Dados do Formulário</param>
    /// <param name="pageState">Estado atual da pagina</param>
    /// <returns>
    /// Chave = Nome do Campo
    /// Valor = Mensagem de erro
    /// </returns>
    public Hashtable ValidateFields(Hashtable values, PageState pageState)
    {
        return ValidateFields(values, pageState, true);
    }

    /// <summary>
    /// Valida os campos do formulário e 
    /// retorna uma lista com erros encontrados
    /// </summary>
    /// <param name="values">Dados do Formulário</param>
    /// <param name="pageState">Estado atual da pagina</param>
    /// <param name="enableErrorLink">Inclui link nos campos de errro</param>
    /// <returns>
    /// Chave = Nome do Campo
    /// Valor = Mensagem de erro
    /// </returns>
    public Hashtable ValidateFields(Hashtable values, PageState pageState, bool enableErrorLink)
    {
        var formManager = new FormManager(FormElement, UserValues, DataAccess);
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

        var action = FormElement.Fields[parms.FieldName].Actions.Get(parms.ActionName);
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
