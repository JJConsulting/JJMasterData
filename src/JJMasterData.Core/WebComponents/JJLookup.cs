using JJMasterData.Commons.Dao;
using JJMasterData.Commons.Dao.Entity;
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
using JJMasterData.Core.Facades;
using JJMasterData.Core.Http;
using JJMasterData.Core.Http.Abstractions;
using Newtonsoft.Json;

namespace JJMasterData.Core.WebComponents;

//Represents a field with a value from another Data Dictionary accessed via popup.
public class JJLookup : JJBaseControl
{
    private readonly CoreServicesFacade _coreServicesFacade;

    #region "Properties"

    private string _selectedValue;
    private string _text;
    private FormElementDataItem _dataItem;
    private ExpressionManager _expressionManager;

    internal IEntityRepository EntityRepository { get; private set; }
    internal ExpressionManager ExpressionManager
    {
        get => _expressionManager ??= new ExpressionManager(UserValues, EntityRepository, HttpContext);
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
            if (AutoReloadFormFields && _text == null && HttpContext.IsPost)
            {
                _text = HttpContext.Request[Name];
            }

            return _text;
        }
        set => _text = value;
    }

    public string SelectedValue
    {
        get
        {
            if (AutoReloadFormFields && string.IsNullOrEmpty(_selectedValue) && HttpContext.IsPost)
            {
                _selectedValue = HttpContext.Request["id_" + Name];
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

    public IDataDictionaryRepository DataDictionaryRepository { get; }
    
    public JJLookup(IHttpContext httpContext,IDataDictionaryRepository dataDictionaryRepository, CoreServicesFacade coreServicesFacade) : base(httpContext)
    {
        _coreServicesFacade = coreServicesFacade;
        DataDictionaryRepository = dataDictionaryRepository;
        Enabled = true;
        AutoReloadFormFields = true;
        Name = "jjlookup1";
        PageState = PageState.List;
        PopSize = PopupSize.Full;
        PopTitle = "Search";
    }

    internal static JJLookup GetInstance(FormElementField f,IHttpContext httpContext,IDataDictionaryRepository repository, CoreServicesFacade coreServicesFacade,  ExpressionOptions expOptions, object value, string panelName)
    {
        var search = new JJLookup(httpContext, repository, coreServicesFacade);
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
    
    #region "DTOs"
    private record LookupUrlDto(string Url)
    {
        [JsonProperty("url")]
        public string Url { get; } = Url;
        public string ToJson() => JsonConvert.SerializeObject(this);
    }
    private record LookupDescriptionDto(string Description)
    {
        [JsonProperty("description")]
        public string Description { get; } = Description;
        public string ToJson() => JsonConvert.SerializeObject(this);
    }
    #endregion
    
    internal override HtmlBuilder RenderHtml()
    {
        if (!IsLookupRoute())
            return GetLookupHtmlElement();

        if (IsAjaxGetDescription())
            SendDescription();
        else
            SendUrl();

        return null;
    }

    private HtmlBuilder GetLookupHtmlElement()
    {
        string inputValue = SelectedValue;

        string description = Text;

        if (string.IsNullOrEmpty(description) && !string.IsNullOrEmpty(inputValue))
            description = GetDescription(inputValue);

        var div = new HtmlBuilder(HtmlTag.Div);

        var textGroup = new JJTextGroup(HttpContext)
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

    public void SendUrl()
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
        
        string url = $"{MasterDataUrlHelper.GetUrl(_coreServicesFacade.Options.Value.JJMasterDataUrl)}Lookup?p={Cript.EnigmaEncryptRP(@params.ToString())}";

        var dto = new LookupUrlDto(url);
        
        HttpContext.Response.SendResponse(dto.ToJson(), "application/json");
    }
    
    private void SendDescription()
    {
        LookupDescriptionDto dto = null;
        try
        {
            string searchId = HttpContext.Request["lkid"];
            string description = GetDescription(searchId);
            dto = new LookupDescriptionDto(description);
        }
        catch (Exception ex)
        {
            Log.AddError(ex, ex.Message);
        }

        HttpContext.Response.SendResponse(dto?.ToJson(), "application/json");
    }

    /// <summary>
    /// Recovers the description based on the selected value
    /// </summary>
    /// <returns>Returns the description of the id</returns>
    public string GetDescription() => GetDescription(SelectedValue);

    private string GetDescription(string searchId)
    {
        if (string.IsNullOrEmpty(searchId))
            return null;

        if (DataItem.ElementMap.Filters == null)
            return null;

        if (OnlyNumbers)
        {
            bool isNumeric = int.TryParse(searchId, out _);
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

        filters.Add(DataItem.ElementMap.FieldKey, StringManager.ClearText(searchId));

        Hashtable fields;
        try
        {
            IDataDictionaryRepository repository = DataDictionaryRepository;
            var dictionary = repository.GetMetadata(DataItem.ElementMap.ElementName);
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
        string lkaction = HttpContext.Request.QueryString("lkaction");
        return "ajax".Equals(lkaction);
    }

    private bool IsLookupRoute()
    {
        string pnlName = string.Empty;
        if (Attributes.ContainsKey("pnlname"))
            pnlName = Attributes["pnlname"]?.ToString();

        string lookupRoute = HttpContext.Request.QueryString("jjlookup_" + pnlName);
        return Name.Equals(lookupRoute);
    }

    public static bool IsLookupRoute(IHttpContext httpContext, JJBaseView baseView)
    {
        string dataPanelName = string.Empty;
        if (baseView is JJFormView formView)
            dataPanelName = formView.DataPanel.Name;
        else if (baseView is JJDataPanel dataPanel)
            dataPanelName = dataPanel.Name;

        string lookupRoute = httpContext.Request.QueryString("jjlookup_" + dataPanelName);
        return !string.IsNullOrEmpty(lookupRoute);
    }

    public static HtmlBuilder ResponseRoute(JJDataPanel view)
    {
        string lookupRoute = view.HttpContext.Request.QueryString("jjlookup_" + view.Name);

        if (string.IsNullOrEmpty(lookupRoute)) return null;

        var field = view.FormElement.Fields.ToList().Find(x => x.Name.Equals(lookupRoute));

        if (field == null) return null;

        var lookup = view.FieldManager.GetField(field, view.PageState, null, view.Values);
        return lookup.GetHtmlBuilder();

    }
}