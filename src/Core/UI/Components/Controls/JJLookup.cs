using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JJMasterData.Commons.Configuration;
using JJMasterData.Commons.Data.Entity;
using JJMasterData.Commons.Data.Entity.Abstractions;
using JJMasterData.Commons.DI;
using JJMasterData.Commons.Logging;
using JJMasterData.Commons.Util;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataDictionary.Repository.Abstractions;
using JJMasterData.Core.DataManager;
using JJMasterData.Core.DataManager.Services.Abstractions;
using JJMasterData.Core.Web.Factories;
using JJMasterData.Core.Web.Html;
using JJMasterData.Core.Web.Http.Abstractions;
using Newtonsoft.Json;

namespace JJMasterData.Core.Web.Components;

//Represents a field with a value from another Data Dictionary accessed via popup.
public class JJLookup : JJBaseControl
{
    private TextGroupFactory TextGroupFactory { get; }

    #region "Properties"

    private string _selectedValue;
    private string _text;
    private FormElementDataItem _dataItem;
    private IExpressionsService _expressionManager;
    private IEntityRepository _entityRepository;

    internal IEntityRepository EntityRepository
    {
        get => _entityRepository ??= JJService.EntityRepository;
        private set => _entityRepository = value;
    }

    internal IExpressionsService ExpressionManager
    {
        get => _expressionManager ??= JJService.Provider.GetScopedDependentService<IExpressionsService>();
        private set => _expressionManager = value;
    }

    internal IDictionary<string,dynamic>FormValues { get; set; }

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
            if (AutoReloadFormFields && _text == null && CurrentContext.IsPost)
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
            if (AutoReloadFormFields && string.IsNullOrEmpty(_selectedValue) && CurrentContext.IsPost)
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

    public JJLookup(IHttpContext httpContext, TextGroupFactory textGroupFactory) : base(httpContext)
    {
        TextGroupFactory = textGroupFactory;
        Enabled = true;
        AutoReloadFormFields = true;
        Name = "jjlookup1";
        PageState = PageState.List;
        PopSize = PopupSize.Full;
        PopTitle = "Search";
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

        var textGroup = TextGroupFactory.CreateTextGroup();
        textGroup.Name = Name;
        textGroup.CssClass = $"form-control jjlookup {GetFeedbackIcon(inputValue, description)} {CssClass}";
        textGroup.InputType = OnlyNumbers ? InputType.Number : InputType.Text;
        textGroup.MaxLength = MaxLength;
        textGroup.Text = inputValue;
        textGroup.Attributes = Attributes;
        textGroup.ToolTip = ToolTip;
        textGroup.ReadOnly = ReadOnly | (Enabled & !string.IsNullOrEmpty(description));
        textGroup.Enabled = Enabled;
        textGroup.Actions = new List<JJLinkButton>
        {
            new()
            {
                Name = $"btn_{Name}",
                Enabled = Enabled,
                ShowAsButton = true,
                IconClass = "fa fa-search"
            }
        };

        div.AppendElement(textGroup);
        div.AppendHiddenInput($"id_{Name}", SelectedValue);
        return div;
    }

    private static string GetFeedbackIcon(string value, string description)
    {
        if (!string.IsNullOrEmpty(value) & !string.IsNullOrEmpty(description))
            return " jj-icon-success ";
        if (!string.IsNullOrEmpty(value) & string.IsNullOrEmpty(description))
            return " jj-icon-warning";
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
            foreach (var filter in elementMap.Filters)
            {
                string filterParsed = ExpressionManager.ParseExpression(filter.Value.ToString(), PageState, false, FormValues);
                @params.Append('&');
                @params.Append(filter.Key);
                @params.Append('=');
                @params.Append(filterParsed);
            }
        }
        var urlHelper = JJMasterDataUrlHelper.GetInstance();
        string url = $"{urlHelper.GetUrl("Index","Lookup", new {p=Cript.EnigmaEncryptRP(@params.ToString()), Area="MasterData"})}";

        var dto = new LookupUrlDto(url);
        
        CurrentContext.Response.SendResponse(dto.ToJson(), "application/json");
    }
    
    private void SendDescription()
    {
        LookupDescriptionDto dto = null;
        try
        {
            string searchId = CurrentContext.Request["lkid"];
            string description = GetDescription(searchId);
            dto = new LookupDescriptionDto(description);
        }
        catch (Exception ex)
        {
            Log.AddError(ex, ex.Message);
        }

        CurrentContext.Response.SendResponse(dto?.ToJson(), "application/json");
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

        var filters = new Dictionary<string, dynamic>();

        if (DataItem.ElementMap.Filters.Count > 0)
        {
            foreach (var filter in DataItem.ElementMap.Filters)
            {
                string filterParsed = ExpressionManager.ParseExpression(filter.Value?.ToString(), PageState, false, FormValues);
                filters.Add(filter.Key, StringManager.ClearText(filterParsed));
            }
        }

        filters.Add(DataItem.ElementMap.FieldKey, StringManager.ClearText(searchId));

        var dicDao = JJService.Provider.GetScopedDependentService<IDataDictionaryRepository>();
        IDictionary<string,dynamic> fields;
        try
        {
            var dictionary = dicDao.GetMetadata(DataItem.ElementMap.ElementName);
            var entityRepository = EntityRepository;
            fields = entityRepository.GetDictionaryAsync(dictionary, filters).GetAwaiter().GetResult();
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

    public static bool IsLookupRoute(JJBaseView view, IHttpContext context)
    {
        string dataPanelName = string.Empty;
        if (view is JJFormView formView)
            dataPanelName = formView.DataPanel.Name;
        else if (view is JJDataPanel dataPanel)
            dataPanelName = dataPanel.Name;

        string lookupRoute = context.Request.QueryString("jjlookup_" + dataPanelName);
        return !string.IsNullOrEmpty(lookupRoute);
    }

    public static HtmlBuilder ResponseRoute(JJDataPanel view)
    {
        string lookupRoute = view.CurrentContext.Request.QueryString("jjlookup_" + view.Name);
        if (string.IsNullOrEmpty(lookupRoute)) 
            return null;

        var field = view.FormElement.Fields.ToList().Find(x => x.Name.Equals(lookupRoute));
        if (field == null) 
            return null;
        var lookup = view.FieldControlFactory.CreateControl(view.FormElement,view.Name,field, view.PageState, null, view.Values);
        return lookup.GetHtmlBuilder();

    }
}