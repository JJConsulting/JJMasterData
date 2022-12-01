using JJMasterData.Commons.Dao;
using JJMasterData.Commons.Dao.Entity;
using JJMasterData.Commons.DI;
using JJMasterData.Commons.Logging;
using JJMasterData.Commons.Util;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataDictionary.Repository;
using JJMasterData.Core.DataManager;
using JJMasterData.Core.Html;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JJMasterData.Core.WebComponents;

//Represents a field with a value from another Data Dictionary accessed via popup.
public class JJLookup : JJBaseControl
{
    #region "Properties"

    private string _selectedValue;
    private string _text;
    private FormElementDataItem _dataItem;
    private ExpressionManager _expressionManager;
    private IEntityRepository _entityRepository;

    internal IEntityRepository EntityRepository
    {
        get => _entityRepository ??= JJService.EntityRepository;
        private set => _entityRepository = value;
    }

    internal ExpressionManager ExpressionManager
    {
        get => _expressionManager ??= new ExpressionManager(UserValues, EntityRepository);
        private set => _expressionManager = value;
    }

    internal Hashtable FormValues { get; private set; }

    internal PageState PageState { get; set; }

    public bool AutoReloadFormFields { get; set; }

    public bool OnlyNumbers { get; set; }

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

    public string PopTitle
    {
        get => GetAttr("popuptitle");
        set => SetAttr("popuptitle", value);
    }

    public new string Text
    {
        get
        {
            if (AutoReloadFormFields && _text == null && CurrentContext.IsPostBack)
            {
                _text = CurrentContext.Request[Name];
            }

            return _text;
        }
        set => _text = value;
    }

    public string SelectedValue
    {
        get
        {
            if (AutoReloadFormFields && string.IsNullOrEmpty(_selectedValue) && CurrentContext.IsPostBack)
            {
                _selectedValue = CurrentContext.Request["id_" + Name];
            }

            return _selectedValue;
        }
        set => _selectedValue = value;
    }


    public FormElementDataItem DataItem
    {
        get => _dataItem ??= new FormElementDataItem();
        set => _dataItem = value;
    }

    #endregion

    #region "Constructors"

    public JJLookup()
    {
        Enabled = true;
        AutoReloadFormFields = true;
        Name = "jjlookup1";
        PageState = PageState.List;
        PopSize = PopupSize.Full;
        PopTitle = "Search";
    }

    internal static JJLookup GetInstance(FormElementField f, ExpressionOptions expOptions, object value, string panelName)
    {
        var search = new JJLookup();
        search.SetAttr(f.Attributes);
        search.Name = f.Name;
        search.SelectedValue = value?.ToString();
        search.Visible = true;
        search.DataItem = f.DataItem;
        search.AutoReloadFormFields = false;
        search.Attributes.Add("pnlname", panelName);
        search.FormValues = expOptions.FormValues;
        search.PageState = expOptions.PageState;
        search.UserValues = expOptions.UserValues;
        search.EntityRepository = expOptions.EntityRepository;

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

    internal override HtmlBuilder RenderHtml()
    {
        if (!IsLookupRoute())
            return GetLookupHtmlElement();

        if (IsAjaxGetDescription())
            ResponseAjax();
        else
            ResponseParams();

        return null;
    }

    private HtmlBuilder GetLookupHtmlElement()
    {
        string inputValue = SelectedValue;

        string description = Text;

        if (string.IsNullOrEmpty(description) && !string.IsNullOrEmpty(inputValue))
            description = GetDescription(inputValue);

        var div = new HtmlBuilder(HtmlTag.Div);

        var textGroup = new JJTextGroup
        {
            Name = Name,
            CssClass = $"form-control jjlookup {GetFeedbackIcon(inputValue, description)} {CssClass}",
            InputType = OnlyNumbers ? InputType.Number : InputType.Text,
            MaxLength = MaxLength,
            Text = inputValue,
            Attributes = Attributes,
            ToolTip = ToolTip,
            ReadOnly = ReadOnly | (Enabled & !string.IsNullOrEmpty(description)),
            Enabled = Enabled,
            Actions = new List<JJLinkButton>
            {
                new()
                {
                    Name = $"btn_{Name}",
                    Enabled = Enabled,
                    ShowAsButton = true,
                    IconClass = "fa fa-search"
                }
            }
        };

        div.AppendElement(textGroup);
        div.AppendHiddenInput($"id_{Name}", SelectedValue);
        return div;
    }

    private string GetFeedbackIcon(string value, string description)
    {
        if (!string.IsNullOrEmpty(value) & !string.IsNullOrEmpty(description))
            return " fa fa-check ";
        if (!string.IsNullOrEmpty(value) & string.IsNullOrEmpty(description))
            return " fa fa-exclamation-triangle";
        return null;
    }

    public void ResponseParams()
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
            foreach (DictionaryEntry filter in elementMap.Filters)
            {
                string filterParsed = ExpressionManager.ParseExpression(filter.Value.ToString(), PageState, false, FormValues);
                @params.Append('&');
                @params.Append(filter.Key);
                @params.Append('=');
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

        if (DataItem.ElementMap.Filters.Count > 0)
        {
            foreach (DictionaryEntry filter in DataItem.ElementMap.Filters)
            {
                string filterParsed = ExpressionManager.ParseExpression(filter.Value?.ToString(), PageState, false, FormValues);
                filters.Add(filter.Key, StringManager.ClearText(filterParsed));
            }
        }

        filters.Add(DataItem.ElementMap.FieldKey, StringManager.ClearText(idSearch));

        var dicDao = DictionaryRepositoryFactory.GetInstance();
        Hashtable fields;
        try
        {
            var dictionary = dicDao.GetMetadata(DataItem.ElementMap.ElementName);
            var entityRepository = ExpressionManager.EntityRepository;
            fields = entityRepository.GetFields(dictionary.Table, filters);
        }
        catch
        {
            fields = null;
        }

        if (fields == null)
            return null;

        if (string.IsNullOrEmpty(DataItem.ElementMap.FieldDescription))
            return fields[DataItem.ElementMap.FieldKey]?.ToString();

        return fields[DataItem.ElementMap.FieldDescription]?.ToString();
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
            pnlName = Attributes["pnlname"]?.ToString();

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

    public static HtmlBuilder ResponseRoute(JJDataPanel view)
    {
        string lookupRoute = view.CurrentContext.Request.QueryString("jjlookup_" + view.Name);

        if (string.IsNullOrEmpty(lookupRoute)) return null;

        var field = view.FormElement.FormFields.ToList().Find(x => x.Name.Equals(lookupRoute));

        if (field == null) return null;

        var lookup = view.FieldManager.GetField(field, view.PageState, null, view.Values);
        return lookup.GetHtmlBuilder();

    }
}