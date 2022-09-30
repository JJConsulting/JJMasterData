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
using Newtonsoft.Json;

namespace JJMasterData.Core.WebComponents;

/// <summary>
/// Responsável por renderizar os campos do formulário
/// </summary>
public class JJDataPanel : JJBaseView
{

    #region "Events"

    public event EventHandler<ActionEventArgs> OnRenderAction;

    #endregion

    #region "Properties"

    private FieldManager _fieldManager;
    private ActionManager _actionManager;
    private DataDictionaryManager _dataDictionaryManager;

    private DataDictionaryManager DataDictionaryManager => _dataDictionaryManager ??= new DataDictionaryManager(FormElement);
    
    /// <summary>
    /// Funções úteis para manipular campos no formulário
    /// </summary>
    public FieldManager FieldManager => _fieldManager ??= new FieldManager(this, FormElement);

    /// <summary>
    /// Funções úteis para manipular ações
    /// </summary>
    private ActionManager ActionManager => _actionManager ??= new ActionManager(this, FormElement);

    /// <summary>
    /// Configurações pré-definidas do formulário
    /// </summary>
    public FormElement FormElement { get; set; }

    /// <summary>
    /// Número de colunas onde os campos serão renderizados
    /// Default 1
    /// </summary>
    /// <remarks>
    /// Somente multiplos de 12
    /// </remarks>
    public int FormCols { get; set; }

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
    /// Tipo de layot para renderizar os campos
    /// </summary>
    public DataPanelLayout Layout { get; set; }

    /// <summary>
    /// Quando o painel estiver no modo de visualização
    /// remover as bordas dos campos exibindo como texto.
    /// </summary>
    /// <remarks>
    /// Valor padrão falso
    /// </remarks>
    public bool ShowViewModeAsStatic { get; set; }

    /// <summary>
    /// Ao recarregar o painel, manter os valores digitados no formuário
    /// (Default=True)
    /// </summary>
    public bool AutoReloadFormFields { get; set; }

    /// <summary>
    /// Comportamento da tecla enter no formulário
    /// Default = DISABLED
    /// </summary>
    public FormEnterKey EnterKey { get; set; }

    /// <summary>
    /// Renderiza agrupamento de campo
    /// </summary>
    internal bool RenderPanelGroup { get; set; }

    #endregion

    #region "Constructors"

    public JJDataPanel(string elementName)
    {
        if (string.IsNullOrEmpty(elementName))
            throw new ArgumentNullException(nameof(elementName));

        var dicParser = GetDictionary(elementName);
        FormElement = dicParser.GetFormElement();
        Name = "pnl_" + elementName.ToLower();
        Values = new Hashtable();
        Erros = new Hashtable();
        AutoReloadFormFields = true;
        RenderPanelGroup = FormElement.Panels.Count > 0;
        SetOptions(dicParser.UIOptions.Form);
    }

    public JJDataPanel(FormElement formElement)
    {
        if (formElement == null)
            throw new ArgumentNullException(nameof(formElement));

        FormElement = formElement;
        Name = "pnl_" + formElement.Name;
        Values = new Hashtable();
        Erros = new Hashtable();
        PageState = PageState.View;
        Layout = DataPanelLayout.Vertical;
        AutoReloadFormFields = true;
        EnterKey = FormEnterKey.Disabled;
        RenderPanelGroup = FormElement.Panels.Count > 0;
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
            CurrentContext.Response.SendResponse(GetHtmlPanel());
            return null;
        }

        if ("jjsearchbox".Equals(requestType))
        {
            if (Name.Equals(pnlname))
            {
                var f = FormElement.Fields.ToList().Find(x => x.Name.Equals(objname));
                if (f != null)
                {
                    var jjSearchBox = FieldManager.GetField(f, PageState, null, Values);
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

    internal string GetHtmlPanel()
    {
        var html = new StringBuilder();
        html.AppendLine("<!-- Start Form -->");
        html.Append("<div id=\"");
        html.Append(Name);
        html.Append("\"");
        if (!string.IsNullOrEmpty(CssClass))
        {
            html.Append(" class=\"");
            html.Append(CssClass);
            html.Append("\"");
        }

        foreach (DictionaryEntry attr in Attributes)
        {
            html.Append(" ");
            html.Append(attr.Key);
            if (attr.Value != null)
            {
                html.Append("=\"");
                html.Append(attr.Value);
                html.Append("\"");
            }
        }

        html.AppendLine(">");

        if (PageState == PageState.Update)
        {
            html.AppendLine(GetPkInputHidden());
        }

        var tabs = FormElement.Panels.FindAll(x => x.Layout == PanelLayout.Tab);
        if (tabs.Count > 0)
        {
            var navTab = new JJTabNav
            {
                Name = "nav_" + Name
            };
            foreach (FormElementPanel panel in tabs)
            {
                string htmlContent = GetHtmlPanelGroup(panel);
                if (!string.IsNullOrEmpty(htmlContent))
                {
                    var tabContent = new NavContent
                    {
                        Title = panel.Title,
                        HtmlContent = htmlContent
                    };
                    navTab.ListTab.Add(tabContent);
                }
            }
            html.AppendLine(navTab.GetHtml());
        }

        //Render other layout types
        foreach (FormElementPanel panel in FormElement.Panels)
        {
            if (panel.Layout != PanelLayout.Tab)
                html.AppendLine(GetHtmlPanelGroup(panel));
        }

        //Render fields without panel
        if (FormElement.Fields.ToList().Exists(x => x.PanelId == 0 & !x.VisibleExpression.Equals("val:0")))
        {
            html.AppendLine(GetHtmlPanelGroup(null));
        }
        
        html.AppendLine("</div>");

        html.AppendLine(GetHtmlFormScript());
        html.AppendLine("<!-- End Form -->");

        return html.ToString();
    }

    private string GetHtmlPanelGroup(FormElementPanel panel)
    {
        var html = new StringBuilder();
        if (panel == null)
        {
            if (!RenderPanelGroup)
            {
                html.AppendLine("<div class=\"container-fluid\">");
                html.AppendLine(GetHtmlForm(null));
                html.AppendLine("</div>");
            }
            else
            {
                html.AppendLine($"<div class=\"{BootstrapHelper.Well}\">");
                if(BootstrapHelper.Version != 3)
                    html.AppendLine("<div class=\"card-body\">");

                html.AppendLine("<div class=\"container-fluid\">");
                html.AppendLine(GetHtmlForm(null));
                html.AppendLine("</div>");

                if (BootstrapHelper.Version != 3)
                    html.AppendLine("</div>");
                html.AppendLine("</div>");
            }

            return html.ToString();
        }

        bool visible = FieldManager.Expression.GetBoolValue(panel.VisibleExpression, "Panel " + panel.Title, PageState, Values);
        if (!visible)
            return string.Empty;

        if (panel.Layout == PanelLayout.Well)
        {
            html.AppendLine($"<div class=\"{BootstrapHelper.Well}\">");
            
            if (BootstrapHelper.Version != 3)
                html.Append("<div class=\"card-header\">");
            
            html.AppendLine(GetHtmlPanelTitle(panel));
            
            if (BootstrapHelper.Version != 3)
                html.Append("</div>");

            if (BootstrapHelper.Version != 3)
                html.Append("<div class=\"card-body\">");

            html.AppendLine("<div class=\"container-fluid\">");
            html.AppendLine(GetHtmlForm(panel));
            html.AppendLine("</div>");                

            if (BootstrapHelper.Version != 3)
                html.Append("</div>");
            html.AppendLine("</div>");
        }
        else if (panel.Layout == PanelLayout.Panel)
        {
            html.AppendFormat($"<div class=\"{BootstrapHelper.GetPanel(panel.Color.ToString().ToLower())}\">");
            html.AppendLine("");
            if (!string.IsNullOrEmpty(panel.Title))
            {
                html.AppendFormat($"<div class=\"{BootstrapHelper.GetPanelHeading(panel.Color.ToString().ToLower())}\">{panel.Title}</div>");
                html.AppendLine("");
            }

            html.AppendLine($"<div class=\"{BootstrapHelper.PanelBody}\">");
            html.AppendLine("<div class=\"container-fluid\">");

            if (!string.IsNullOrEmpty(panel.SubTitle))
            {
                html.AppendLine(GetHtmlPanelTitle(null, panel.SubTitle));
            }

            html.AppendLine(GetHtmlForm(panel));
            html.AppendLine("</div>");
            html.AppendLine("</div>");
            html.AppendLine("</div>");
        }
        else if (panel.Layout == PanelLayout.Collapse)
        {
            var sContent = new StringBuilder();
            sContent.AppendLine("<div class=\"container-fluid\">");
            if (!string.IsNullOrEmpty(panel.SubTitle))
                sContent.AppendLine(GetHtmlPanelTitle(null, panel.SubTitle));

            sContent.AppendLine(GetHtmlForm(panel));
            sContent.AppendLine("</div>");
            var collapse = new JJCollapsePanel();
            collapse.Title = panel.Title;
            collapse.Name = Name + "_panel" + panel.PanelId;
            collapse.CssClass = panel.CssClass;
            collapse.HtmlContent = sContent.ToString();
            collapse.ExpandedByDefault = panel.ExpandedByDefault;
            collapse.Color = panel.Color;

            html.AppendLine(collapse.GetHtml());
        }
        else if (panel.Layout == PanelLayout.Tab)
        {
            html.AppendLine("<div class=\"container-fluid\">");
            html.AppendLine(GetHtmlPanelTitle(null, panel.SubTitle));
            html.AppendLine(GetHtmlForm(panel));
            html.AppendLine("</div>");
        }
        else
        {
            html.AppendLine("<div class=\"container-fluid\">");
            html.AppendLine(GetHtmlPanelTitle(panel));
            html.AppendLine(GetHtmlForm(panel));
            html.AppendLine("</div>");
        }

        return html.ToString();
    }

    private string GetHtmlForm(FormElementPanel panel)
    {
        bool panelEnable = true;
        int panelId = 0;

        if (panel != null)
        {
            panelEnable = FieldManager.Expression.GetBoolValue(panel.EnableExpression, "Panel " + panel.Title, PageState, Values);
            panelId = panel.PanelId;
        }

        List<FormElementField> fields = FormElement.Fields.ToList()
                                 .FindAll(x => x.PanelId == panelId)
                                 .OrderBy(x => x.LineGroup)
                                 .ThenBy(x => x.Order)
                                 .ToList();

        if (fields.Count == 0)
            return string.Empty;

        if (Layout == DataPanelLayout.Vertical)
            return GetHtmlFormVertical(fields);
        return GetHtmlFormHorizontal(fields);
    }

    private string GetHtmlFormVertical(List<FormElementField> fields)
    {
        string colClass = "";
   

        int cols = FormCols;
        if (cols > 12)
            cols = 12;

        if (cols >= 1)
            colClass = string.Format(" col-sm-{0}", (12 / cols));

        var html = new StringBuilder();

        int linegroup = int.MinValue;
        bool isfirst = true;
        foreach (var f in fields)
        {
            //visible expression
            bool visible = FieldManager.IsVisible(f, PageState, Values);
            if (!visible)
            {
                continue;
            }

            //value
            object value = null;
            if (Values != null && Values.Contains(f.Name))
                value = FieldManager.FormatVal(Values[f.Name], f);

            if (linegroup != f.LineGroup)
            {
                if (isfirst)
                    isfirst = false;
                else
                    html.AppendLine("\t</div>");

                html.AppendLine("\t<div class=\"row\">");
                linegroup = f.LineGroup;
            }

            string fieldClass = BootstrapHelper.FormGroup;

            if (!string.IsNullOrEmpty(f.CssClass))
            {
                fieldClass += string.Format(" {0}", f.CssClass);
            }
            else
            {
                if (cols > 1 && f.Component == FormComponent.TextArea)
                    fieldClass += " col-sm-12";
                else
                    fieldClass += colClass;
            }

            if (Erros != null && Erros.Contains(f.Name))
                fieldClass += " " + BootstrapHelper.HasError;

            if (PageState == PageState.View && ShowViewModeAsStatic)
                fieldClass += " jjborder-static";

            html.AppendLine("\t\t<div class=\"" + fieldClass + "\">");

            if (f.Component != FormComponent.CheckBox)
            {
                html.Append("\t\t\t");
                html.AppendLine(new JJLabel(f).GetHtml());
            }

            html.Append("\t\t\t");

            if (PageState == PageState.View && ShowViewModeAsStatic)
            {
                html.Append("<p class=\"form-control-static\">");
                //TODO: recuperar valor do texto corretamente quando for combo, search, etc..
                html.Append(value);
                html.AppendLine("</p>");
            }
            else
            {
                html.AppendLine(GetHtmlField(f, value));
            }

            html.AppendLine("\t\t</div>");

        }

        html.AppendLine("\t</div>");

        return html.ToString();
    }

    private string GetHtmlFormHorizontal(List<FormElementField> fields)
    {
        string fldClass = "";
        string labelClass = "";
        string fieldClass = "";
        string fullClass = "";

        int cols = FormCols;
        if (cols == 1)
        {
            labelClass = "col-sm-2";
            fieldClass = "col-sm-9";
            fullClass = "col-sm-9";
        }
        else if (cols == 2)
        {
            labelClass = "col-sm-2";
            fieldClass = "col-sm-4";
            fullClass = "col-sm-10";
        }
        else if (cols == 3)
        {
            labelClass = "col-sm-2";
            fieldClass = "col-sm-2";
            fullClass = "col-sm-10";
        }
        else if (cols >= 4)
        {
            cols = 4;
            labelClass = "col-sm-1";
            fieldClass = "col-sm-2";
            fullClass = "col-sm-8";
        }

        StringBuilder html = new();
        html.Append($"<div class=\"{BootstrapHelper.FormHorizontal}\">");

        int colCount = 1;
        foreach (var f in fields)
        {
            //visible expression
            bool visible = FieldManager.IsVisible(f, PageState, Values);
            if (!visible)
            {
                continue;
            }

            //value
            object fieldValue = null;
            if (Values != null && Values.Contains(f.Name))
                fieldValue = FieldManager.FormatVal(Values[f.Name], f);

            var label = new JJLabel(f);
            label.CssClass = labelClass;

            fldClass += string.IsNullOrEmpty(f.CssClass) ? "" : string.Format(" {0}", f.CssClass);
            if (Erros != null && Erros.Contains(f.Name))
                fldClass += " " + BootstrapHelper.HasError;

            string bs4Row = BootstrapHelper.Version > 3 ? "row" : string.Empty;

            if (f.Component == FormComponent.TextArea)
            {
                if (colCount > 1)
                    html.AppendLine("\t</div>");

                html.AppendLine($"\t<div class=\"{BootstrapHelper.FormGroup} {bs4Row}\">");
                html.Append("\t\t");
                html.Append("<div class=\"");
                html.Append(fldClass);
                html.AppendLine("\">");
                html.Append("\t\t\t");
                html.AppendLine(label.GetHtml());
                html.AppendLine("\t\t\t<div class=\"" + fullClass + "\">");
                html.Append("\t\t\t\t");
                if (PageState == PageState.View && ShowViewModeAsStatic)
                {
                    html.Append("<p class=\"form-control-static\">");
                    html.Append(fieldValue);
                    html.AppendLine("</p>");
                }
                else
                {
                    html.AppendLine(FieldManager.GetField(f, PageState, fieldValue, Values).GetHtml());
                }
                html.AppendLine("\t\t\t</div>");
                html.AppendLine("\t\t</div>");
                html.AppendLine("\t</div>");

                colCount = 1;
            }
            else
            {
                if (colCount == 1)
                    html.AppendLine($"\t<div class=\"{BootstrapHelper.FormGroup}\">");

                html.Append("\t\t");
                html.Append("<div class=\"");
                html.Append($"{fldClass} {bs4Row}");
                html.AppendLine("\">");
                html.Append("\t\t\t");
                html.AppendLine(label.GetHtml());
                html.AppendLine("\t\t\t<div class=\"" + fieldClass + "\">");
                html.Append("\t\t\t\t");
                if (PageState == PageState.View && ShowViewModeAsStatic)
                {
                    html.Append("<p class=\"form-control-static\">");
                    html.Append(fieldValue);
                    html.AppendLine("</p>");
                }
                else
                {
                    html.AppendLine(GetHtmlField(f, fieldValue));
                }
                html.AppendLine("\t\t\t</div>");
                html.AppendLine("\t\t</div>");

                if (colCount >= cols)
                {
                    html.AppendLine("\t</div>");
                    colCount = 1;
                }
                else
                {
                    colCount++;
                }
            }

        }
        if (colCount > 1)
            html.AppendLine("\t</div>");

        html.AppendLine("</div>");
        return html.ToString();
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

        sHtml.Append("\t<input type=\"hidden\" ");
        sHtml.Append("id=\"jjform_pkval_");
        sHtml.Append(Name);
        sHtml.Append("\" ");
        sHtml.Append("name=\"jjform_pkval_");
        sHtml.Append(Name);
        sHtml.Append("\" ");
        sHtml.Append("value=\"");
        if (!string.IsNullOrEmpty(pkval))
            sHtml.Append(Cript.Cript64(pkval));
        sHtml.AppendLine("\"/>");

        return sHtml.ToString();
    }

    private string GetHtmlField(FormElementField f, object value)
    {
        var field = FieldManager.GetField(f, PageState, value, Values);

        if (f.Actions == null)
            return field.GetHtml();

        var actions = f.Actions.GetAll().FindAll(x => x.IsVisible);
        if (actions.Count == 0)
            return field.GetHtml();

        if (!(field is JJTextGroup))
            return field.GetHtml();

        //Actions
        var textBox = (JJTextGroup)field;
        foreach (BasicAction action in actions)
        {
            var link = ActionManager.GetLinkField(action, Values, PageState, field);
            var onRender = OnRenderAction;
            if (onRender != null)
            {
                var args = new ActionEventArgs(action, link, Values);
                onRender.Invoke(this, args);
            }
            if (link != null)
                textBox.Actions.Add(link);
        }

        return textBox.GetHtml();
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

        if (EnterKey == FormEnterKey.Tab)
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

    private string GetHtmlPanelTitle(FormElementPanel panel)
    {
        if (panel.HasTitle())
            return GetHtmlPanelTitle(panel.Title, panel.SubTitle);
        return string.Empty;
    }

    private string GetHtmlPanelTitle(string title, string subTitle)
    {
        if (string.IsNullOrEmpty(title) &&
            string.IsNullOrEmpty(subTitle))
            return string.Empty;

        var html = new StringBuilder();

        if(BootstrapHelper.Version != 3)
        {
            if (!string.IsNullOrEmpty(title))
                html.AppendFormat("<span>{0}</span>", Translate.Key(title));

            if (!string.IsNullOrEmpty(subTitle))
                html.AppendFormat("<footer>{0}</footer>", Translate.Key(subTitle));

            return html.ToString();
        }

        html.AppendLine("<div class=\"row\">");
        html.AppendLine("<blockquote>");
        if (!string.IsNullOrEmpty(title))
            html.AppendFormat("<p>{0}</p>", Translate.Key(title));

        if (!string.IsNullOrEmpty(subTitle))
            html.AppendFormat("<footer>{0}</footer>", Translate.Key(subTitle));

        html.AppendLine("</blockquote>");
        html.AppendLine("</div>");


        return html.ToString();
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

        if (Values != null && FormElement.Fields.Any(f=>Values.Contains(f.Name)))
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

    /// <summary>
    /// Atribui as configurações do usuário cadastrado no dicionário de dados
    /// </summary>
    /// <param name="o">
    /// Configurações do usuário
    /// </param>
    public void SetOptions(UIForm o)
    {
        if (o == null)
            return;

        FormCols = o.FormCols;
        Layout = o.IsVerticalLayout ? DataPanelLayout.Vertical : DataPanelLayout.Horizontal;
        ShowViewModeAsStatic = o.ShowViewModeAsStatic;
        EnterKey = o.EnterKey;
    }

}
