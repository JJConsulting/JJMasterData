using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JJMasterData.Commons.Language;
using JJMasterData.Commons.Util;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataDictionary.Action;
using JJMasterData.Core.DataManager;
using JJMasterData.Core.FormEvents.Args;
using JJMasterData.Core.Html;
using Newtonsoft.Json;

namespace JJMasterData.Core.WebComponents;

/// <summary>
/// Responsável por renderizar os campos do formulário
/// </summary>
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
    /// Funções úteis para manipular campos no formulário
    /// </summary>
    public FieldManager FieldManager => _fieldManager ??= new FieldManager(this, FormElement);

    /// <summary>
    /// Configurações pré-definidas do formulário
    /// </summary>
    public FormElement FormElement { get; set; }

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
        set
        {
            _uiFormSettings = value;
        }
    }

    /// <summary>
    /// Estado atual da pagina
    /// </summary>
    public PageState PageState { get; set; }

    /// <summary>
    /// Campos com erro.
    /// Key=Nome do campo, Value=Descricão do erro
    /// </summary>
    public Hashtable Erros { get; set; }

    /// <summary>
    /// Conteúdo dos campos.
    /// Key=Nome do campo, Value=Contúdo do campo
    /// </summary>
    public Hashtable Values { get; set; }

    /// <summary>
    /// Ao recarregar o painel, manter os valores digitados no formuário
    /// (Default=True)
    /// </summary>
    public bool AutoReloadFormFields { get; set; }


    /// <summary>
    /// Renderiza agrupamento de campo
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

    protected override string RenderHtml()
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

        return GetHtmlPanel().GetElementHtml();

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

        //html.AppendLine(GetHtmlFormScript());

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
        StringBuilder sHtml = new StringBuilder();
        var listFieldsPost = FormElement.Fields.ToList().FindAll(x => x.AutoPostBack);
        var listFieldsExp = FormElement.Fields.ToList().FindAll(x => x.EnableExpression.StartsWith("exp:") && !x.AutoPostBack);

        if (listFieldsPost.Count == 0 &&
            listFieldsExp.Count == 0)
        {
            return "";
        }

        string functionname = string.Format("do_reload_{0}", Name);
        sHtml.AppendLine(" ");
        sHtml.AppendLine("<script type=\"text/javascript\">");

        if (listFieldsPost.Count > 0)
        {
            sHtml.Append("\tfunction ");
            sHtml.Append(functionname);
            sHtml.AppendLine("(objid) { ");
            sHtml.AppendLine("\t\tvar frm = $('form'); ");
            sHtml.AppendLine("\t\tvar surl = frm.attr('action'); ");
            sHtml.AppendLine("\t\tif (surl.includes('?'))");
            sHtml.AppendLine("\t\t\tsurl += '&t=reloadpainel&pnlname=" + Name + "&objname=' + objid;");
            sHtml.AppendLine("\t\telse");
            sHtml.AppendLine("\t\t\tsurl += '?t=reloadpainel&pnlname=" + Name + "&objname=' + objid;");
            sHtml.AppendLine("");
            sHtml.AppendLine("\t\t$.ajax({ ");
            sHtml.AppendLine("\t\tasync: false,");
            sHtml.AppendLine("\t\t\ttype: frm.attr('method'), ");
            sHtml.AppendLine("\t\t\turl: surl, ");
            sHtml.AppendLine("\t\t\tdata: frm.serialize(), ");
            sHtml.AppendLine("\t\t\tsuccess: function (data) { ");
            sHtml.AppendLine("\t\t\t\t$(\"#" + Name + "\").html(data); ");
            sHtml.AppendLine("\t\t\t\tjjloadform(); ");
            sHtml.AppendLine("\t\t\t\tjjutil.gotoNextFocus(objid); ");
            sHtml.AppendLine("\t\t\t}, ");
            sHtml.AppendLine("\t\t\terror: function (jqXHR, textStatus, errorThrown) { ");
            sHtml.AppendLine("\t\t\t\tconsole.log(errorThrown); ");
            sHtml.AppendLine("\t\t\t\tconsole.log(textStatus); ");
            sHtml.AppendLine("\t\t\t\tconsole.log(jqXHR); ");
            sHtml.AppendLine("\t\t\t} ");
            sHtml.AppendLine("\t\t}); ");
            sHtml.AppendLine("\t} ");
        }

        sHtml.AppendLine("\t$(document).ready(function () {");

        if (UISettings.EnterKey == FormEnterKey.Tab)
        {
            sHtml.AppendLine($"\t\tjjutil.replaceEntertoTab(\"{Name}\");");
            sHtml.AppendLine("");
        }

        if (listFieldsPost.Count > 0)
        {
            foreach (FormElementField f in listFieldsPost)
            {
                //WorkArroud para gatilhar o select do search
                if (f.Component == FormComponent.Search)
                {
                    sHtml.Append("\t\t$(\"");
                    sHtml.Append($"#{f.Name}_text");
                    sHtml.AppendLine("\").change(function () {");
                    sHtml.AppendLine("\t\t\tsetTimeout(function() {");
                    sHtml.Append("\t\t\t\t");
                    sHtml.Append(functionname);
                    sHtml.Append("('");
                    sHtml.Append(f.Name);
                    sHtml.AppendLine("');");
                    sHtml.AppendLine("\t\t\t},200);");
                    sHtml.AppendLine("\t\t});");
                    sHtml.AppendLine("");
                }
                else
                {
                    sHtml.Append("\t\t$(\"");
                    sHtml.Append("#");
                    sHtml.Append(f.Name);
                    sHtml.AppendLine("\").change(function () {");
                    sHtml.Append("\t\t\t");
                    sHtml.Append(functionname);
                    sHtml.AppendLine("($(this).attr('id'));");
                    sHtml.AppendLine("\t\t});");
                    sHtml.AppendLine("");
                }

            }
        }

        foreach (var f in listFieldsExp)
        {
            string exp = f.EnableExpression.Replace("exp:", "");
            exp = exp.Replace("{pagestate}", string.Format("'{0}'", PageState.ToString()));
            exp = exp.Replace("{PAGESTATE}", string.Format("'{0}'", PageState.ToString()));
            exp = exp
                .Replace(" and ", " && ")
                .Replace(" or ", " || ")
                .Replace(" AND ", " && ")
                .Replace(" OR ", " || ")
                .Replace("=", " == ")
                .Replace("<>", " != ");

            List<string> list = StringManager.FindValuesByInterval(exp, '{', '}');
            if (list.Count > 0)
            {
                foreach (string field in list)
                {
                    string val = null;
                    if (UserValues.Contains(field))
                    {
                        //Valor customizado pelo usuário
                        val = string.Format("'{0}'", UserValues[field]);
                    }
                    else if (CurrentContext.Session[field] != null)
                    {
                        //Valor da Sessão
                        val = string.Format("'{0}'", CurrentContext.Session[field]);
                    }
                    else
                    {
                        //Campos ocultos
                        if (Values.Contains(field))
                        {
                            var fTemp = FormElement.Fields.ToList().Find(x => x.Name.Equals(field));
                            if (fTemp != null)
                            {
                                bool visible = FieldManager.IsVisible(fTemp, PageState, Values);
                                if (!visible)
                                {
                                    val = string.Format("'{0}'", Values[field]);
                                }
                            }
                        }
                    }

                    if (val != null)
                    {
                        // Note: Use "{{" to denote a single "{" 
                        exp = exp.Replace(string.Format("{{{0}}}", field), val);
                    }
                }

                sHtml.Append("\t\t$(\"");
                for (int i = 0; i < list.Count; i++)
                {
                    if (i > 0)
                        sHtml.Append(",");

                    sHtml.Append("#");
                    sHtml.Append(list[i]);
                }

                sHtml.AppendLine("\").change(function () {");
                sHtml.Append("\t\t\tvar exp = \"");
                sHtml.Append(exp);
                sHtml.AppendLine("\";");

                for (int i = 0; i < list.Count; i++)
                {
                    sHtml.Append("\t\t\texp = exp.replace(\"");
                    sHtml.Append("{");
                    sHtml.Append(list[i]);
                    sHtml.Append("}\", \"'\" + $(\"#");
                    sHtml.Append(list[i]);
                    sHtml.AppendLine("\").val() + \"'\"); ");
                }

                sHtml.AppendLine("\t\t\tvar enable = eval(exp);");
                sHtml.AppendLine("\t\t\tif (enable)");
                sHtml.Append("\t\t\t\t$(\"#");
                sHtml.Append(f.Name);
                sHtml.AppendLine("\").removeAttr(\"readonly\").removeAttr(\"disabled\");");
                sHtml.AppendLine("\t\t\telse");
                sHtml.Append("\t\t\t\t$(\"#");
                sHtml.Append(f.Name);

                //Se alterar para disabled o valor não voltará no post e vai zuar a rotina GetFormValues() qd exisir exp EnabledExpression
                sHtml.AppendLine("\").attr(\"readonly\",\"readonly\").val(\"\");");
                sHtml.AppendLine("\t\t});");
            }
        }

        sHtml.AppendLine("\t});");
        sHtml.AppendLine("</script>");

        return sHtml.ToString();
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
