using System;
using System.Collections;
using System.Linq;
using System.Text;
using JJMasterData.Commons.Dao;
using JJMasterData.Commons.Dao.Entity;
using JJMasterData.Commons.Language;
using JJMasterData.Commons.Logging;
using JJMasterData.Commons.Util;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataManager;

namespace JJMasterData.Core.WebComponents;

public class JJLookup : JJBaseControl
{
    #region "Properties"

    private string _selectedValue;
    private string _text;
    private FormElementDataItem _dataItem;
    private Hashtable FormValues { get; set; }

    /// <summary>
    /// Estado atual da pagina
    /// </summary>
    internal PageState PageState { get; set; }

    /// <summary>
    /// Ao recarregar o painel, manter os valores digitados no formuário
    /// (Default=True)
    /// </summary>
    public bool AutoReloadFormFields { get; set; }

    /// <summary>
    /// Permite somente números
    /// </summary>
    public bool OnlyNumbers { get; set; }

    /// <summary>
    /// Tamanho da modal que será aberta para selecionar os itens
    /// </summary>
    public PopupSize PopSize
    {
        get
        {
            string val = GetAttr("popupsize");
            if (string.IsNullOrEmpty(val))
                return PopupSize.Full;
            return (PopupSize)int.Parse(val);
        }
        set => SetAttr("popupsize", value);
    }

    /// <summary>
    /// Título que será exibido na modal
    /// </summary>
    public string PopTitle
    {
        get => GetAttr("popuptitle");
        set => SetAttr("popuptitle", value);
    }

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
        set { _text = value; }
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
                _selectedValue = CurrentContext.Request["id_" + Name];
            }

            return _selectedValue;
        }
        set => _selectedValue = value;
    }

    /// <summary>
    /// Origem dos dados
    /// </summary>
    public FormElementDataItem DataItem
    {
        get => _dataItem ??= new FormElementDataItem();
        set => _dataItem = value;
    }

    #endregion

    #region "Constructors"

    public JJLookup()
    {
        Enable = true;
        AutoReloadFormFields = true;
        Name = "jjlookup1";
        PageState = PageState.List;
        PopSize = PopupSize.Full;
        PopTitle = "Search";
    }

    public JJLookup(IDataAccess dataAccess) : this()
    {
        DataAccess = dataAccess;
    }

    internal static JJLookup GetInstance(FormElementField f,
        PageState pagestate,
        object value,
        Hashtable formValues,
        bool enable,
        string name)
    {
        var search = new JJLookup();
        search.SetAttr(f.Attributes);
        search.Name = name ?? f.Name;
        search.SelectedValue = value?.ToString();
        search.Visible = true;
        search.DataItem = f.DataItem;
        search.Enable = enable;
        search.ReadOnly = f.DataBehavior == FieldBehavior.ViewOnly && pagestate != PageState.Filter;
        search.AutoReloadFormFields = false;
        search.FormValues = formValues;
        search.PageState = pagestate;
        if (f.DataType == FieldType.Int)
        {
            search.OnlyNumbers = true;
            search.MaxLength = 11;
        }
        else
        {
            search.MaxLength = f.Size;
        }

        return search;
    }

    #endregion

    protected override string RenderHtml()
    {
        if (!IsLookupRoute()) return GetHtmlLookup();
        
        if (IsAjaxGetDescription())
            ResponseAjax();
        else
            ResponseParms();

        return null;

    }


    public void ResponseParms()
    {
        var elementMap = DataItem.ElementMap;

        var @params = new StringBuilder();
        @params.Append("elementName=");
        @params.Append(elementMap.ElementName);
        @params.Append("&fieldKey=");
        @params.Append(elementMap.FieldKey);
        @params.Append("&objid=");
        @params.Append(Name);
        @params.Append("&enableAction=");
        @params.Append(elementMap.EnableElementActions ? "1" : "0");

        //Filters
        if (DataItem.ElementMap.Filters is { Count: > 0 })
        {
            var exp = new ExpressionManager(UserValues, DataAccess);
            foreach (DictionaryEntry filter in elementMap.Filters)
            {
                string filterParsed = exp.ParseExpression(filter.Value.ToString(), PageState, false, FormValues);
                @params.Append("&");
                @params.Append(filter.Key);
                @params.Append("=");
                @params.Append(filterParsed);
            }
        }

        string url = $"{ConfigurationHelper.GetUrlMasterData()}Lookup?p={Cript.EnigmaEncryptRP(@params.ToString())}";

        string json = "{ \"url\": \"" + url + "\" }";
        CurrentContext.Response.SendResponse(json, "application/json");
    }

    private void ResponseAjax()
    {
        string json = string.Empty;
        try
        {
            string idSearch = CurrentContext.Request["lkid"];
            string textSearch = GetDescription(idSearch);
            json = "{ \"description\": \"" + textSearch + "\" }";
        }
        catch (Exception ex)
        {
            Log.AddError(ex.Message);
        }

        CurrentContext.Response.SendResponse(json, "application/json");
    }

    private string GetHtmlLookup()
    {
        if (DataItem == null)
            throw new ArgumentException(Translate.Key("[DataItem] property not set"), Name);

        var html = new StringBuilder();
        string cssClass = "form-control jjlookup ";
        cssClass += !string.IsNullOrEmpty(CssClass) ? CssClass : "";

        html.AppendLine("<!-- Start JJLookup -->");
        html.AppendLine($"<div class=\"{(BootstrapHelper.Version == 3 ? "input-group" : string.Empty)}\"> ");
        html.AppendLine("\t<div class=\"has-feedback\">");

        string value = SelectedValue;

        string description = Text;

        if (string.IsNullOrEmpty(description) && !string.IsNullOrEmpty(value))
            description = GetDescription(value);


        if (BootstrapHelper.Version == 3)
            html.Append(GetInputHtml(value, description, cssClass));

        html.AppendLine("</div>");
        html.Append($"\t<span class=\"{BootstrapHelper.InputGroupBtn}\"> ");
        
        if (BootstrapHelper.Version > 3)
            html.Append(GetInputHtml(value, description, cssClass));

        html.Append('\t', 2);

        html.Append("<button type=\"button\" ");

        html.Append(Enable ? $"id=\"btn_{Name}\" " : "disabled ");

        html.Append(
            $"class=\"{(BootstrapHelper.Version == 3 ? BootstrapHelper.DefaultButton : "input-group-text")}\">");
        html.AppendLine("\t\t\t\t<span class=\"fa fa-search\"></span> ");
        html.AppendLine("\t\t\t</button>");
        html.Append("\t</span> ");
        html.AppendLine("</div> ");

        html.AppendLine("<!-- End JJLookup -->");

        return html.ToString();
    }

    private string GetInputHtml(string value, string description, string cssClass)
    {
        var html = new StringBuilder();

        html.Append("\t\t<input ");
        html.Append($"id=\"{Name}\" ");
        html.Append($"name=\"{Name}\" ");
        html.Append($"class=\"{cssClass}\" ");
        html.Append("autocomplete =\"off\" ");
        html.Append("type=\"text\" ");

        if (OnlyNumbers)
            html.Append("onkeypress=\"return jjutil.justNumber(event);\" ");

        if (MaxLength > 0)
            html.Append($"maxlength =\"{MaxLength}\" ");

        if (!string.IsNullOrEmpty(description))
        {
            html.Append($"value =\"{description}\" ");
        }
        else if (!string.IsNullOrEmpty(value))
        {
            html.Append($"value =\"{value}\" ");
        }

        if (!string.IsNullOrEmpty(ToolTip))
        {
            html.Append($"{BootstrapHelper.DataToggle}=\"tooltip\" title=\"");
            html.Append(Translate.Key(ToolTip));
            html.Append("\" ");
        }

        if (ReadOnly | (Enable & !string.IsNullOrEmpty(description)))
            html.Append("readonly ");

        if (!Enable)
            html.Append("disabled ");

        foreach (DictionaryEntry attr in Attributes)
        {
            html.Append(attr.Key);
            if (attr.Value != null)
            {
                html.Append("=\"");
                html.Append(attr.Value);
                html.Append("\"");
            }

            html.Append(' ');
        }

        html.AppendLine("/>");


        html.Append("\t\t<span ");
        html.Append($" class=\"{(BootstrapHelper.Version > 3 ? " bg-transparent input-group-text" : string.Empty)} ");


        html.Append("\" aria-hidden=\"true\">");
        html.AppendLine($"<span id=\"st_{Name}\" class=\"form-control-feedback ");
        if (!string.IsNullOrEmpty(value) & !string.IsNullOrEmpty(description))
            html.Append(" fa fa-check ");
        else if (!string.IsNullOrEmpty(value) & string.IsNullOrEmpty(description))
            html.Append(" fa fa-exclamation-triangle");
        else if(BootstrapHelper.Version > 3)
            html.Append(" fa fa-ellipsis-h");
        html.AppendLine("\"></span>");
        html.AppendLine("</span>");


        html.Append("\t\t<input id=\"id_");
        html.Append(Name);
        html.Append("\" name=\"id_");
        html.Append(Name);
        html.Append("\" value=\"");
        html.Append(SelectedValue);
        html.AppendLine("\" type=\"hidden\"/>");

        return html.ToString();
    }

    /// <summary>
    /// Recovers the description based on the selected value
    /// </summary>
    /// <returns>Returns the description of the id</returns>
    public string GetDescription() => GetDescription(SelectedValue);

    private string GetDescription(string idSearch)
    {
        if (string.IsNullOrEmpty(idSearch))
            return null;

        if (DataItem.ElementMap.Filters == null)
            return null;

        if (OnlyNumbers)
        {
            bool isNumeric = int.TryParse(idSearch, out _);
            if (!isNumeric)
                return null;
        }

        var filters = new Hashtable();
        //Filters
        if (DataItem.ElementMap.Filters.Count > 0)
        {
            var exp = new ExpressionManager(UserValues, DataAccess);
            foreach (DictionaryEntry filter in DataItem.ElementMap.Filters)
            {
                string filterParsed = exp.ParseExpression(filter.Value.ToString(), PageState, false, FormValues);
                filters.Add(filter.Key, StringManager.ClearText(filterParsed));
            }
        }

        filters.Add(DataItem.ElementMap.FieldKey, StringManager.ClearText(idSearch));

        var dao = new Factory(DataAccess);
        Hashtable fields;
        try
        {
            fields = dao.GetFields(DataItem.ElementMap.ElementName, filters);
        }
        catch
        {
            fields = null;
        }

        if (fields == null)
            return null;

        if (string.IsNullOrEmpty(DataItem.ElementMap.FieldDescription))
            return fields[DataItem.ElementMap.FieldKey].ToString();

        return fields[DataItem.ElementMap.FieldDescription].ToString();
    }

    private bool IsAjaxGetDescription()
    {
        string lkaction = CurrentContext.Request.QueryString("lkaction");
        return "ajax".Equals(lkaction);
    }

    private bool IsLookupRoute()
    {
        string pnlName = string.Empty;
        if (Attributes.ContainsKey("pnlname"))
            pnlName = Attributes["pnlname"].ToString();

        string lookupRoute = CurrentContext.Request.QueryString("jjlookup_" + pnlName);
        return Name.Equals(lookupRoute);
    }

    public static bool IsLookupRoute(JJBaseView view)
    {
        string dataPanelName = string.Empty;
        if (view is JJFormView formView)
            dataPanelName = formView.DataPanel.Name;
        else if (view is JJDataPanel dataPanel)
            dataPanelName = dataPanel.Name;

        string lookupRoute = view.CurrentContext.Request.QueryString("jjlookup_" + dataPanelName);
        return !string.IsNullOrEmpty(lookupRoute);
    }

    public static string ResponseRoute(JJDataPanel view)
    {
        string lookupRoute = view.CurrentContext.Request.QueryString("jjlookup_" + view.Name);
        
        if (string.IsNullOrEmpty(lookupRoute)) return null;
        
        var field = view.FormElement.Fields.ToList().Find(x => x.Name.Equals(lookupRoute));
        
        if (field == null) return null;
        
        var lookup = view.FieldManager.GetField(field, view.PageState, null, view.Values);
        return lookup.GetHtml();

    }
}