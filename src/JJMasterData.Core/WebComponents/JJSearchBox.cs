using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using JJMasterData.Commons.Dao;
using JJMasterData.Commons.Dao.Entity;
using JJMasterData.Commons.Language;
using JJMasterData.Commons.Util;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataManager;
using JJMasterData.Core.FormEvents.Args;
using JJMasterData.Core.Html;
using Newtonsoft.Json;

namespace JJMasterData.Core.WebComponents;

/// <summary>
/// Representa uma caixa de texto capaz de localizar itens
/// </summary>
/// <example>
/// Exemplo da pagina
/// <code lang="html">
/// <![CDATA[
///    <form id="form1" runat="server">
///        <div class="container-fluid" id="tab-demo2">
///            <h3>Demo JJSearchBox</h3>
///            <div class="well">
///                <label>Título</label>
///                <%=jjview.GetHtml() %>
///            </div>
///
///            <asp:Button ID="btnOk" runat="server" Text="Submit" CssClass="btn" OnClick="BtnOk_Click" />
///            <br /><br />
///            Valor: <asp:Label ID="lblValue" runat="server" Text=""></asp:Label>
///            <br />
///            Descrição: <asp:Label ID="lblDescri" runat="server" Text=""></asp:Label>
///
///        </div>
///    </form>
///]]>
/// </code>
/// Exemplo objeto
/// <code lang="c#">
/// <![CDATA[
///using JJMasterData.WebForm;
///using System;
///using System.Web.UI;
///using System.Collections.Generic;
///
///namespace WebAppExample
///{
///    public partial class WebForm2 : Page
///    {
///        public JJSearchBox jjview;
///
///        protected void Page_Load(object sender, EventArgs e)
///        {
///            jjview = SetupSearchBox();
///        }
///        
///        //Configurando componente
///        private JJSearchBox SetupSearchBox()
///        {
///            //Nova instancia do componente
///            var search = new JJSearchBox();
///            //Id do Componente
///            search.Name = "id1";
///            //Evento disparado ao pesquisar, retornar id e descrição correspondente 
///            search.OnSearchQuery += JJView_OnSearchQuery;
///            //Evento disparado ao recuperar a descrição a partir de um id
///            search.OnSearchId += JJView_OnSearchId;
///
///            return search;
///        }
///
///        private Dictionary<string, string> JJView_OnSearchQuery(string textSearch)
///        {
///            var values = GetValues();
///            var ret = new Dictionary<string, string>();
///            foreach (KeyValuePair<string, string> val in values)
///            {
///                if (val.Value.ToLower().Contains(textSearch.ToLower()))
///                    ret.Add(val.Key, val.Value);
///            }
///            return ret;
///        }
///
///        private string JJView_OnSearchId(string id)
///        {
///            string descr = null;
///            var values = GetValues();
///            if (values.ContainsKey(id))
///                descr = values[id];
///
///            return descr;
///        }
///
///        private Dictionary<string, string> GetValues()
///        {
///            var list = new Dictionary<string, string>();
///            list.Add("1", "test1");
///            list.Add("2", "test2");
///            list.Add("3", "test3");
///            list.Add("4", "test4");
///            list.Add("5", "foo1");
///            list.Add("6", "foo2");
///
///            return list;
///        }
///
///        protected void BtnOk_Click(object sender, EventArgs e)
///        {
///            lblValue.Text = jjview.SelectedValue;
///            lblDescri.Text = jjview.Text;
///        }
///    }
///}
///]]>
/// </code> 
/// </example>
public class JJSearchBox : JJBaseControl
{
    #region "Events"

    /// <summary>
    /// Evento disparado para recuperar os registros com parte do texto digitado
    /// </summary>  
    public event EventHandler<SearchBoxQueryEventArgs> OnSearchQuery;

    /// <summary>
    /// Evento disparado para recuperar a descrição com base no Id
    /// </summary>
    public event EventHandler<SearchBoxItemEventArgs> OnSearchId;

    #endregion

    #region "Properties"

    private const string NUMBEROFITEMS = "numberofitems";
    private const string SCROLLBAR = "scrollbar";
    private const string TRIGGERLENGTH = "triggerlength";
    

    private List<DataItemValue> _values;
    private string _selectedValue;
    private string _text { get; set; }
    private FormElementDataItem _DataItem;

    /// <summary>
    /// Conteudo da caixa de texto 
    /// </summary>
    public new string Text
    {
        get
        {
            if (AutoReloadFormFields && _text == null && IsPostBack)
            {
                _text = CurrentContext.Request[Name];
            }

            return _text;
        }
        set
        {
            _text = value;
        }
    }

    /// <summary>
    /// Quantidade minima de caracteres digitado para disparar a pesquisa
    /// (Default = 1)
    /// </summary>
    public int TriggerLength 
    {
        get
        {
            if (Attributes.ContainsKey(TRIGGERLENGTH))
                return int.Parse(Attributes[TRIGGERLENGTH].ToString());
            return 0;
        }
        set
        {
            if (Attributes.ContainsKey(TRIGGERLENGTH))
                Attributes[TRIGGERLENGTH] = value;
            else
                Attributes.Add(TRIGGERLENGTH, value);
        }
    }

    /// <summary>
    /// Numero máximo de itens que será exibido na lista de pesquisa
    /// (Default = 10)
    /// </summary>
    public int NumberOfItems 
    {
        get
        {
            if (Attributes.ContainsKey(NUMBEROFITEMS))
                return int.Parse(Attributes[NUMBEROFITEMS].ToString());
            return 0;
        }
        set
        {
            if (Attributes.ContainsKey(NUMBEROFITEMS))
                Attributes[NUMBEROFITEMS] = value;
            else
                Attributes.Add(NUMBEROFITEMS, value);
        }
    }

    /// <summary>
    /// Exibir barra de rolagem na lista de pesquisa
    /// (Default = false)
    /// </summary>
    /// <remarks>
    /// Used to show scrollBar when there are too many match records and it's set to True.
    /// If this option is set to true, the NumberOfItems value will be 100 and the HTML render menu will be set to:
    /// <code lang="html">
    /// <ul class="typeahead dropdown-menu" style="max-height:220px;overflow:auto;"></ul>
    /// </code>
    /// </remarks>
    public bool ScrollBar 
    {
        get
        {
            if (Attributes.ContainsKey(SCROLLBAR))
                return Attributes[SCROLLBAR].ToString().Equals("true");
            return false;
        }
        set
        {
            string v = value ? "true" : "false";
            if (Attributes.ContainsKey(SCROLLBAR))
                Attributes[SCROLLBAR] = v;
            else
                Attributes.Add(SCROLLBAR, v);
        }
    }

    /// <summary>
    /// Id correspondente ao texto pesquisado
    /// </summary>
    public string SelectedValue
    {
        get
        {
            if (AutoReloadFormFields && string.IsNullOrEmpty(_selectedValue) && IsPostBack)
            {
                _selectedValue = CurrentContext.Request[Name];
            }

            if (string.IsNullOrEmpty(_selectedValue) && !string.IsNullOrEmpty(Text))
            {
                var list = GetValues(Text);
                if (list.Count == 1)
                    _selectedValue = list.ToList().First().Id;
            }

            return _selectedValue;
        }
        set
        {
            _selectedValue = value;
        }
    }

    /// <summary>
    /// Origem dos dados
    /// </summary>
    public FormElementDataItem DataItem
    {
        get
        {
            if (_DataItem == null)
            {
                _DataItem = new FormElementDataItem();
            }
            return _DataItem;
        }
        set
        {
            _DataItem = value;
        }
    }

    /// <summary>
    /// Ao recarregar o painel, manter os valores digitados no formuário
    /// (Default=True)
    /// </summary>
    public bool AutoReloadFormFields { get; set; }

    internal PageState PageState { get; set; }

    private Hashtable FormValues { get; set; }

    private string Id => Name.Replace(".", "_").Replace("[", "_").Replace("]", "_");

    #endregion

    #region "Constructors"

    public JJSearchBox()
    {
        Enable = true;
        TriggerLength = 1;
        PlaceHolder = Translate.Key("Search...");
        NumberOfItems = 10;
        ScrollBar = false;
        AutoReloadFormFields = true;
        Name = "jjsearchbox1";
        PageState = PageState.List;
    }

    public JJSearchBox(IDataAccess dataAccess) : this()
    {
        DataAccess = dataAccess;
    }


    internal static JJSearchBox GetInstance(FormElementField f,
                                  PageState pagestate,
                                  object value,
                                  Hashtable formValues,
                                  bool enable,
                                  string name)
    {
        JJSearchBox search = new JJSearchBox();
        search.Name = (name == null ? f.Name : name);
        search.SelectedValue = (string)value;
        search.Visible = true;
        search.DataItem = f.DataItem;
        search.Enable = enable;
        search.ReadOnly = f.DataBehavior == FieldBehavior.ViewOnly && pagestate != PageState.Filter;
        search.AutoReloadFormFields = false;
        search.FormValues = formValues;
        search.PageState = pagestate;

        return search;
    }

    #endregion

    internal override HtmlElement GetHtmlElement()
    {
        var html = new HtmlElement();
        if ("jjsearchbox".Equals(CurrentContext.Request.QueryString("t")))
        {
            if (Id.Equals(CurrentContext.Request.QueryString("objname")))
            {
                string textSearch = CurrentContext.Request[Name + "_text"];
                string json = GetJsonValues(textSearch);
                
                CurrentContext.Response.SendResponse(json, "application/json");
            }
        }
        else
        {
            html = GetSearchBoxHtmlElement();
        }

        return html;
    }


    private HtmlElement GetSearchBoxHtmlElement()
    {
        if (DataItem == null)
            throw new ArgumentException(Translate.Key("[DataItem] property not set"), Name);

        var div = new HtmlElement(HtmlTag.Div)
            .AppendElement(HtmlTag.Input, input =>
            {
                input.WithAttribute("id",Id + "_text");
                input.WithAttribute("name",Name + "_text");
                input.WithAttribute("jjid",Id );
                input.WithAttribute("type", "text");
                input.WithAttribute("autocomplete", "off");
                input.WithAttributeIf(MaxLength > 0, "maxlength", MaxLength.ToString());
                input.WithAttributeIf(DataItem.ShowImageLegend, "showimagelegend", "true");
                input.WithAttributeIf(ReadOnly, "readonly", "readonly");
                input.WithAttributeIf(!Enable, "disabled", "disabled");
                input.WithAttributes(Attributes);
                input.WithToolTip(ToolTip);
                input.WithCssClass("form-control jjsearchbox");
                input.WithCssClassIf(string.IsNullOrEmpty(SelectedValue), "jj-icon-search");
                input.WithCssClassIf(!string.IsNullOrEmpty(SelectedValue), "jj-icon-success");
                input.WithCssClass(CssClass);
                
                string description = Text;
                if (string.IsNullOrEmpty(description) && !string.IsNullOrEmpty(SelectedValue))
                    description = GetDescription(SelectedValue);

                input.WithAttribute("value", description);
                
            })
            .AppendHiddenInput(Id, Name, SelectedValue);

        return div;
    }
    
    private string GetHtmlTextSearch()
    {
        if (DataItem == null)
            throw new ArgumentException(Translate.Key("[DataItem] property not set"), Name);

        StringBuilder html = new StringBuilder();
        string cssClass = "form-control jjsearchbox ";
        cssClass += !string.IsNullOrEmpty(CssClass) ? CssClass : "";

        html.AppendLine("<!-- Start JJSearchBox -->");
        html.AppendLine($"<div class=\"{(BootstrapHelper.Version == 3 ? string.Empty : "input-group")} has-feedback\">");

        html.Append("\t<input ");
        html.Append($"id=\"{Id}_text\" ");
        html.Append($"name=\"{Name}_text\" ");
        html.Append($"jjid=\"{Id}\" ");
        html.Append("autocomplete=\"off\" ");

        html.Append("type=\"text\" ");
        if (MaxLength > 0)
        {
            html.Append("maxlength=\"");
            html.Append(MaxLength);
            html.Append("\" ");
        }

        if (DataItem.ShowImageLegend)
        {
            html.Append(" showimagelegend=\"true\" ");
        }

        html.Append($"class=\"{(BootstrapHelper.Version == 3 ? string.Empty : "border-right-0 border ")}");
        html.Append(cssClass);
        html.Append("\" ");

        string descr = Text;
        if (string.IsNullOrEmpty(descr) && !string.IsNullOrEmpty(SelectedValue))
            descr = GetDescription(SelectedValue);

        if (!string.IsNullOrEmpty(descr))
        {
            html.Append("value =\"");
            html.Append(descr);
            html.Append("\" ");
        }

        if (!string.IsNullOrEmpty(ToolTip))
        {
            html.Append("data-toggle=\"tooltip\" title=\"");
            html.Append(Translate.Key(ToolTip));
            html.Append("\" ");
        }

        if (ReadOnly)
            html.Append("readonly ");

        if (!Enable)
            html.Append("disabled ");

        if (BootstrapHelper.Version > 3)
        {
            Attributes.Add("aria-describedby",Name + "-addon");
        }
        
        foreach (DictionaryEntry attr in Attributes)
        {
            html.Append(attr.Key);
            if (attr.Value != null)
            {
                html.Append("=\"");
                html.Append(attr.Value);
                html.Append("\"");
            }
            html.Append(" ");
        }
        html.AppendLine("/>");

        if(BootstrapHelper.Version == 3)
        {
            html.Append("\t<span id=\"st_");
            html.Append(Id);
            html.Append("\" class=\"form-control-feedback fa ");
            html.Append(string.IsNullOrEmpty(SelectedValue) ? "fa-search" : " fa-check");
            html.Append("\" aria-hidden=\"true\">");
            html.AppendLine("</span>");
        }
        else
        {
            html.Append("<span class=\"input-group-text\">");
            html.Append($"<span id=\"{Name}-addon\" class=\"fa {(string.IsNullOrEmpty(SelectedValue) ? "fa-search" : "fa-check")}\"></span>");
            html.Append("</span>");
        }


        html.Append("\t<input id=\"");
        html.Append(Id);
        html.Append("\" name=\"");
        html.Append(Name);
        html.Append("\" value=\"");
        html.Append(SelectedValue);
        html.AppendLine("\" type=\"hidden\"/>");
        html.AppendLine("</div>");


        html.AppendLine("<!-- End JJSearchBox -->");

        return html.ToString();
    }

    /// <summary>
    /// Recupera descrição com base no id
    /// </summary>
    /// <param name="idSearch">Id a ser pesquisado</param>
    /// <returns>Retorna descricão referente ao id</returns>
    public string GetDescription(string idSearch)
    {
        string sRet = null;
        if (OnSearchQuery != null)
        {
            var args = new SearchBoxItemEventArgs(idSearch);
            OnSearchId.Invoke(this, args);
            sRet = args.ResultText;
        }
        else
        {
            if (_values == null)
            {
                _values = GetValues(null, idSearch);
            }
        }

        if (_values != null)
        {
            var item = _values.ToList().Find(x => x.Id.Equals(idSearch));
            if (item != null)
                sRet = item.Description;
        }

        return sRet;
    }

    /// <summary>
    /// Recupera registros com base no texto a ser pesquisado
    /// </summary>
    /// <param name="textSearch">Texto a ser pesquisado</param>
    /// <returns>Retorna dicionario de dados onde o chave é o id do registro</returns>
    public List<DataItemValue> GetValues(string textSearch)
    {
        if (OnSearchQuery != null)
        {
            var args = new SearchBoxQueryEventArgs(textSearch);
            OnSearchQuery.Invoke(this, args);
            _values = args.Values;
        }
        else
        {
            if (_values == null)
            {
                _values = GetValues(textSearch, null);
            }
        }
        
        if (_values != null && textSearch != null)
            return _values.ToList().FindAll(x => x.Description.ToLower().Contains(textSearch.ToLower()));
        return null;
    }

    /// <summary>
    /// Recupera registros
    /// </summary>
    /// <summary>
    /// Recupera registros
    /// </summary>
    public List<DataItemValue> GetValues()
    {
        return GetValues(null, null);
    }

    private List<DataItemValue> GetValues(string textSearch, string idSearch)
    {
        if (DataItem == null)
            return null;

        var values = new List<DataItemValue>();
        if (DataItem.Command != null && !string.IsNullOrEmpty(DataItem.Command.Sql))
        {

            string sql = DataItem.Command.Sql;
            if (sql.Contains("{"))
            {
                if (idSearch != null)
                {
                    if (!UserValues.ContainsKey("search_id"))
                        UserValues.Add("search_id", StringManager.ClearText(idSearch));
                }
                    
                if (textSearch != null)
                {
                    if (!UserValues.ContainsKey("search_text"))
                        UserValues.Add("search_text", StringManager.ClearText(textSearch));
                }

                var exp = new ExpressionManager(UserValues, DataAccess);
                sql = exp.ParseExpression(sql, PageState, false, FormValues);
            }


            DataTable dt = DataAccess.GetDataTable(sql);
            foreach (DataRow row in dt.Rows)
            {
                var item = new DataItemValue();
                item.Id = row[0].ToString();
                item.Description = row[1].ToString().Trim();
                if (DataItem.ShowImageLegend)
                {
                    item.Icon = (IconType)int.Parse(row[2].ToString());
                    item.ImageColor = row[3].ToString();
                }
                values.Add(item);
            }
        }
        else
        {
            values = DataItem.Itens;
        }


        return values;
    }

    /// <summary>
    /// Recupera registros com base no texto a ser pesquisado
    /// </summary>
    /// <param name="textSearch">Texto a ser pesquisado</param>
    /// <returns>Retorna texto em JSON</returns>
    private string GetJsonValues(string textSearch)
    {
        var listValue = GetValues(textSearch);
        var listItem = new List<JJSearchBoxItem>();

        foreach (var i in listValue)
        {
            string descri;
            if (DataItem.ShowImageLegend)
                descri = string.Format("{0}|{1}|{2}", 
                    i.Description, IconHelper.GetClassName(i.Icon), i.ImageColor);
            else
                descri = i.Description;

            listItem.Add(new JJSearchBoxItem(i.Id, descri));
        }

        string json = JsonConvert.SerializeObject(listItem.ToArray());
        listValue = null;
        listItem = null;

        return json;
    }

    /// <summary>
    /// DTO Item
    /// </summary>
    [Serializable]
    [DataContract]
    private class JJSearchBoxItem
    {
        [DataMember(Name = "id")]
        public string Id { get; set; }

        [DataMember(Name = "name")]
        public string Name { get; set; }

        public JJSearchBoxItem(string id, string name)
        {
            Id = id;
            Name = name;
        }

    }

}




