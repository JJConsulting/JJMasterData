#nullable enable

using JJMasterData.Commons.Cryptography;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataManager;
using JJMasterData.Core.DataManager.Services.Abstractions;
using JJMasterData.Core.Extensions;
using JJMasterData.Core.FormEvents.Args;
using JJMasterData.Core.UI.Components.Abstractions;
using JJMasterData.Core.Web.Html;
using JJMasterData.Core.Web.Http.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JJMasterData.Core.UI.Components;


namespace JJMasterData.Core.Web.Components;

/// <summary>
/// Represents a searchable combobox.
/// </summary>
public class JJSearchBox : AsyncControl
{
    
    
    #region "Events"

    /// <summary>
    /// Evento disparado para recuperar os registros com parte do texto digitado
    /// </summary>  
    public event EventHandler<SearchBoxQueryEventArgs>? OnSearchQuery;

    /// <summary>
    /// Evento disparado para recuperar a descrição com base no Id
    /// </summary>
    public event EventHandler<SearchBoxItemEventArgs>? OnSearchId;

    #endregion

    #region "Fields"
    private IEnumerable<DataItemValue>? _values;
    private string? _selectedValue;
    private string? _text;
    private string? _fieldName;
    private string? _htmlId;
    private ComponentContext? _componentContext;
    #endregion
    
    #region "Properties"

    private const string NumberOfItemsAttribute = "numberofitems";
    private const string ScrollbarAttribute = "scrollbar";
    private const string TriggerLengthAttribute = "triggerlength";

    internal string FieldName
    {
        get => _fieldName ?? Name;
        set => _fieldName = value;
    }

    internal string? ElementName { get; set; }

    public string HtmlId
    {
        get => _htmlId ??= Name;
        set => _htmlId = value;
    }

    public new string? Text
    {
        get
        {
            if (AutoReloadFormFields && _text == null && CurrentContext.Request.IsPost)
            {
                _text = CurrentContext.Request[Name];
            }

            return _text;
        }
        set => _text = value;
    }

    /// <summary>
    /// Quantidade minima de caracteres digitado para disparar a pesquisa
    /// (Default = 1)
    /// </summary>
    public int TriggerLength
    {
        get
        {
            if (Attributes.TryGetValue(TriggerLengthAttribute, out var attribute))
                return int.Parse(attribute);
            return 0;
        }
        set => Attributes[TriggerLengthAttribute] = value.ToString();
    }

    /// <summary>
    /// Numero máximo de itens que será exibido na lista de pesquisa
    /// (Default = 10)
    /// </summary>
    public int NumberOfItems
    {
        get
        {
            if (Attributes.TryGetValue(NumberOfItemsAttribute, out var attribute))
                return int.Parse(attribute);
            return 0;
        }
        set => Attributes[NumberOfItemsAttribute] = value.ToString();
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
        get =>
            Attributes.ContainsKey(ScrollbarAttribute) &&
            Attributes[ScrollbarAttribute].Equals("true");
        set
        {
            string booleanString = value ? "true" : "false";
            Attributes[ScrollbarAttribute] = booleanString;
        }
    }

    /// <summary>
    /// Id correspondente ao texto pesquisado
    /// </summary>
    public async Task<string?> GetSelectedValueAsync()
    {
        if (AutoReloadFormFields && string.IsNullOrEmpty(_selectedValue) && CurrentContext.Request.IsPost)
        {
            _selectedValue = CurrentContext.Request[Name];
        }

        if (string.IsNullOrEmpty(_selectedValue) && !string.IsNullOrEmpty(Text))
        {
            var values =await  GetValuesAsync(Text);
            var item = values.FirstOrDefault();
            if (item == null)
                return null;

            _selectedValue = item.Id;
        }

        return _selectedValue;
    }

    
    public FormElementDataItem DataItem { get; set; }
    

    /// <summary>
    /// Ao recarregar o painel, manter os valores digitados no formulário
    /// (Default=True)
    /// </summary>
    public bool AutoReloadFormFields { get; set; }

    public IDataItemService DataItemService { get; }
    private IEncryptionService EncryptionService { get; }
    private JJMasterDataUrlHelper UrlHelper { get; }
    public FormStateData FormStateData { get; internal set; }

    public string SelectedValue
    {
        set => _selectedValue = value;
    }
    
    internal ComponentContext ComponentContext
    {
        get
        {
            if (_componentContext != null)
                return _componentContext.Value;
            
            var resolver = new ComponentContextResolver(this);
            _componentContext = resolver.GetContext();

            return _componentContext.Value;
        }
    }

    #endregion

    #region "Constructors"

    public JJSearchBox(
        IHttpContext httpContext,
        IEncryptionService encryptionService,
        IDataItemService dataItemService,
        JJMasterDataUrlHelper urlHelper) : base(httpContext)
    {
        HtmlId = Name;
        EncryptionService = encryptionService;
        UrlHelper = urlHelper;
        DataItemService = dataItemService;
        Enabled = true;
        TriggerLength = 1;
        PlaceHolder = "Search...";
        NumberOfItems = 10;
        ScrollBar = false;
        AutoReloadFormFields = true;
        Name = "jjsearchbox1";
        DataItem = new FormElementDataItem();
        var defaultValues = new Dictionary<string, object?>();
        FormStateData = new(defaultValues, UserValues, PageState.List);
    }

    #endregion
    
    protected override async Task<ComponentResult> BuildResultAsync()
    {
        var fieldName = CurrentContext.Request.QueryString("fieldName");
        if (ComponentContext is ComponentContext.SearchBox && FieldName == fieldName)
        {
            return new JsonComponentResult(await GetSearchBoxItemsAsync());
        }

        var html = await GetSearchBoxHtml();

        return new RenderedComponentResult(html);
    }
    
    private async Task<HtmlBuilder> GetSearchBoxHtml()
    {
        if (DataItem == null)
            throw new ArgumentException("[DataItem] property not set");

        var selectedValue = await GetSelectedValueAsync();

        var div = new HtmlBuilder(HtmlTag.Div);
        await div.AppendAsync(HtmlTag.Input, async input =>
        {
            input.WithAttribute("id", HtmlId + "_text");
            input.WithAttribute("name", HtmlId + "_text");
            input.WithAttribute("hidden-input-id", HtmlId);
            input.WithAttribute("type", "text");
            input.WithAttribute("urltypehead", GetUrl());
            input.WithAttribute("autocomplete", "off");
            input.WithAttributeIf(MaxLength > 0, "maxlength", MaxLength.ToString());
            input.WithAttributeIf(DataItem.ShowImageLegend, "showimagelegend", "true");
            input.WithAttributeIf(ReadOnly, "readonly", "readonly");
            input.WithAttributeIf(!Enabled, "disabled", "disabled");
            input.WithAttributes(Attributes);
            input.WithToolTip(ToolTip);
            input.WithCssClass("form-control jjsearchbox");
            input.WithCssClassIf(string.IsNullOrEmpty(selectedValue), "jj-icon-search");
            input.WithCssClassIf(!string.IsNullOrEmpty(selectedValue), "jj-icon-success");
            input.WithCssClass(CssClass);

            string? description = Text;
            if (string.IsNullOrEmpty(description) && selectedValue != null && !string.IsNullOrEmpty(selectedValue))
                description = await GetDescriptionAsync(selectedValue);

            input.WithAttributeIfNotEmpty("value", description);

        });
        div.Append(HtmlTag.Input, input =>
        {
            input.WithAttribute("hidden", "hidden");
            input.WithAttribute("id", HtmlId);
            input.WithAttribute("name", Name);
            input.WithAttributeIfNotEmpty("value",selectedValue);
        });

        return div;
    }

    private string GetUrl()
    {
        var url = new StringBuilder();
        if (IsExternalRoute)
        {
            string dictionaryNameEncrypted = EncryptionService.EncryptStringWithUrlEscape(ElementName);
            url.Append(UrlHelper.GetUrl("GetItems","Search", "MasterData", new
            {
                dictionaryName = dictionaryNameEncrypted,
                fieldName = FieldName,
                fieldSearchName = Name + "_text",
                pageState = (int)FormStateData.PageState,
                Area = "MasterData"
            }));
        }
        else
        {
            url.Append("context=searchBox");
            url.Append($"&dictionaryName={ElementName}");
            url.Append($"&fieldName={FieldName}");
            url.Append($"&fieldSearchName={Name + "_text"}");
            url.Append($"&pageState={(int)FormStateData.PageState}");
        }



        return url.ToString();
    }

    /// <summary>
    /// Recupera descrição com base no id
    /// </summary>
    /// <param name="idSearch">Id a ser pesquisado</param>
    /// <returns>Retorna descrição referente ao id</returns>
    public async Task<string?> GetDescriptionAsync(string searchId)
    {
        string? description = null;
        if (OnSearchQuery != null)
        {
            var args = new SearchBoxItemEventArgs(searchId);
            OnSearchId?.Invoke(this, args);
            description = args.ResultText;
        }
        else
        {
            _values ??= await DataItemService.GetValuesAsync(DataItem, FormStateData, null, searchId).ToListAsync();
        }

        var item = _values?.ToList().Find(x => x.Id.Equals(searchId));

        if (item != null)
            description = item.Description;

        return description;
    }

    /// <summary>
    /// Recover values from the given text.
    /// </summary>
    public async Task<List<DataItemValue>>GetValuesAsync(string? searchText)
    {
        var list = new List<DataItemValue>();
        if (OnSearchQuery != null)
        {
            var args = new SearchBoxQueryEventArgs(searchText);
            OnSearchQuery.Invoke(this, args);
            foreach (var value in args.Values)
            {
                list.Add(value);
            }
        }
        else
        {
            await foreach (var value in DataItemService.GetValuesAsync(DataItem, FormStateData, searchText, null))
            {
                list.Add(value);
            }
        }

        return list;
    }
    

    public async Task<List<DataItemResult>> GetSearchBoxItemsAsync()
    {
        string componentName = CurrentContext.Request.QueryString("fieldName");
        string textSearch = CurrentContext.Request.GetFormValue(componentName);

        var values = await GetValuesAsync(textSearch);
        var items = DataItemService.GetItems(DataItem, values);

        return items.ToList();
    }


}