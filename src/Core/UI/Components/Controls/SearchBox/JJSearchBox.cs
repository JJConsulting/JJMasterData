#nullable enable

using JJMasterData.Commons.Cryptography;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataManager;
using JJMasterData.Core.DataManager.Services.Abstractions;
using JJMasterData.Core.Extensions;
using JJMasterData.Core.FormEvents.Args;
using JJMasterData.Core.UI.Components.Abstractions;
using JJMasterData.Core.Web.Factories;
using JJMasterData.Core.Web.Html;
using JJMasterData.Core.Web.Http.Abstractions;
using Newtonsoft.Json;
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
    private JJMasterDataEncryptionService EncryptionService { get; }
    private JJMasterDataUrlHelper UrlHelper { get; }

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

    #region "Properties"

    private const string NumberOfItemsAttribute = "numberofitems";
    private const string ScrollbarAttribute = "scrollbar";
    private const string TriggerLengthAttribute = "triggerlength";

    private IEnumerable<DataItemValue>? _values;
    private string? _selectedValue;
    private string? _text;
    private string? _fieldName;

    internal string FieldName
    {
        get
        {
            if (_fieldName == null)
                return Name;

            return _fieldName;
        }
        set => _fieldName = value;
    }

    internal string DictionaryName { get; set; }

    internal string Id => Name.Replace(".", "_").Replace("[", "_").Replace("]", "_");


    public new string? Text
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

    /// <summary>
    /// Quantidade minima de caracteres digitado para disparar a pesquisa
    /// (Default = 1)
    /// </summary>
    public int TriggerLength
    {
        get
        {
            if (Attributes.TryGetValue(TriggerLengthAttribute, out var attribute))
                return int.Parse(attribute.ToString());
            return 0;
        }
        set => Attributes[TriggerLengthAttribute] = value;
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
                return int.Parse(attribute.ToString());
            return 0;
        }
        set => Attributes[NumberOfItemsAttribute] = value;
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
            Attributes[ScrollbarAttribute].ToString().Equals("true");
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
        if (AutoReloadFormFields && string.IsNullOrEmpty(_selectedValue) && CurrentContext.IsPost)
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

    public FormStateData FormStateData { get; internal set; }

    public string SelectedValue
    {
        set => _selectedValue = value;
    }

    #endregion

    #region "Constructors"

    public JJSearchBox(
        IHttpContext httpContext,
        JJMasterDataEncryptionService encryptionService,
        IDataItemService dataItemService,
        JJMasterDataUrlHelper urlHelper) : base(httpContext)
    {
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
        var defaultValues = new Dictionary<string, dynamic>();
        FormStateData = new(defaultValues, UserValues, PageState.List);
    }

    #endregion
    
    protected override async Task<ComponentResult> BuildResultAsync()
    {
        if (IsSearchBoxRoute(DictionaryName, CurrentContext))
        {
            if (!FieldName.Equals(CurrentContext.Request.QueryString("fieldName")))
                return ComponentResult.Empty;

            return new JsonComponentResult(await GetSearchBoxItemsAsync());
        }

        var html = await GetSearchBoxHtml();

        return new RenderedComponentResult(html);
    }
    
    public static bool IsSearchBoxRoute(string? dictionaryName, IHttpContext httpContext)
    {
        string requestType = httpContext.Request.QueryString("t");
        string requestedDictionaryName = httpContext.Request.QueryString("dictionaryName");
        return "jjsearchbox".Equals(requestType) && (requestedDictionaryName == dictionaryName || dictionaryName is null);
    }

    public static async Task<ComponentResult> GetResultFromPanel(JJDataPanel view, IHttpContext httpContext)
    {
        return await GetResultFromComponent(view, view.FormElement, view.Values, httpContext, view.ComponentFactory.Controls.GetFactory<IControlFactory<JJSearchBox>>());
    }

    internal static async Task<ComponentResult> GetResultFromComponent(
        ComponentBase view,
        FormElement formElement,
        IDictionary<string, dynamic> formValues,
        IHttpContext httpContext,
        IControlFactory<JJSearchBox> searchBoxFactory
        )
    {
        string dictionaryName = httpContext.Request.QueryString("dictionaryName");
        string fieldName = httpContext.Request.QueryString("fieldName");
        var pageState = (PageState)int.Parse(httpContext.Request.QueryString("pageState"));

        if (!formElement.Name.Equals(dictionaryName))
            return ComponentResult.Empty;

        var field = formElement.Fields[fieldName];
        var expOptions = new FormStateData(formValues, view.UserValues, pageState);

        var searchBox = searchBoxFactory.Create(formElement, field, new(expOptions, view.Name, dictionaryName));
        return await searchBox.GetResultAsync();
    }


    private async Task<HtmlBuilder> GetSearchBoxHtml()
    {
        if (DataItem == null)
            throw new ArgumentException("[DataItem] property not set");

        var selectedValue = await GetSelectedValueAsync();

        var div = new HtmlBuilder(HtmlTag.Div);
        await div.AppendAsync(HtmlTag.Input, async input =>
        {
            input.WithAttribute("id", Id + "_text");
            input.WithAttribute("name", Name + "_text");
            input.WithAttribute("jjid", Name);
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
            if (string.IsNullOrEmpty(description) && !string.IsNullOrEmpty(selectedValue))
                description = await GetDescriptionAsync(selectedValue!);

            input.WithAttribute("value", description);

        });
        div.Append(HtmlTag.Input, input =>
        {
            input.WithAttribute("hidden", "hidden");
            input.WithAttribute("id", Id);
            input.WithAttribute("name", Name);
            input.WithValue(selectedValue);
        });

        return div;
    }

    private string GetUrl()
    {
        var url = new StringBuilder();
        if (IsExternalRoute)
        {
            string dictionaryNameEncrypted = EncryptionService.EncryptStringWithUrlEscape(DictionaryName);
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
            url.Append("t=jjsearchbox");
            url.Append($"&dictionaryName={DictionaryName}");
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
    public async Task<string?> GetDescriptionAsync(string idSearch)
    {
        string? description = null;
        if (OnSearchQuery != null)
        {
            var args = new SearchBoxItemEventArgs(idSearch);
            OnSearchId?.Invoke(this, args);
            description = args.ResultText;
        }
        else
        {
            _values ??= await DataItemService.GetValuesAsync(DataItem, FormStateData, null, idSearch).ToListAsync();
        }

        var item = _values?.ToList().Find(x => x.Id.Equals(idSearch));

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
        string textSearch = CurrentContext.Request.Form(componentName);

        var values = await GetValuesAsync(textSearch);
        var items = DataItemService.GetItems(DataItem, values);

        return items.ToList();
    }


}