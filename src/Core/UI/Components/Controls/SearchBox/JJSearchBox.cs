#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JJMasterData.Commons.Security.Cryptography.Abstractions;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.DataManager.Models;
using JJMasterData.Core.DataManager.Services;
using JJMasterData.Core.Events.Args;
using JJMasterData.Core.Extensions;
using JJMasterData.Core.Http.Abstractions;
using JJMasterData.Core.UI.Html;
using JJMasterData.Core.UI.Routing;

namespace JJMasterData.Core.UI.Components;

/// <summary>
/// Represents a searchable combobox.
/// </summary>
public class JJSearchBox : ControlBase, IDataItemControl
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

    #endregion

    #region "Properties"

    private const string NumberOfItemsAttribute = "number-of-items";
    private const string ScrollbarAttribute = "scrollbar";
    private const string TriggerLengthAttribute = "trigger-length";

    internal string? ParentElementName { get; set; }

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
            if (AutoReloadFormFields && _text == null && Request.Form.ContainsFormValues())
            {
                _text = Request.Form[$"{Name}_text"];
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
            return 1;
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
            return 10;
        }
        set => Attributes[NumberOfItemsAttribute] = value.ToString();
    }

    /// <summary>
    /// Exibir barra de rolagem na lista de pesquisa
    /// (Default = false)
    /// </summary>
    /// <remarks>
    /// Used to show scrollBar when there are too many match records and it's set to True.
    /// If this option is set to true,the HTML menu will be set to:
    /// <code lang="html">
    /// <ul class="typeahead dropdown-menu" style="max-height:220px;overflow:auto;"></ul>
    /// </code>
    /// </remarks>
    public bool ScrollBar
    {
        get
        {
            if (!Attributes.ContainsKey(ScrollbarAttribute))
                return true;
            
            return Attributes[ScrollbarAttribute].Equals("true");
        }
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
        if (!string.IsNullOrEmpty(_selectedValue))
        {
            return _selectedValue;
        }
        
        if (AutoReloadFormFields && Request.Form.ContainsFormValues())
        {
            _selectedValue = Request.Form[Name];
        }

        if (!string.IsNullOrEmpty(Text))
        {
            var values = await GetValuesAsync(Text);
            var item = values.FirstOrDefault();
            if (item == null)
                return null;

            _selectedValue = item.Id;
        }

        return _selectedValue;
    }
    
    /// <summary>
    /// Ao recarregar o painel, manter os valores digitados no formulário
    /// (Default=True)
    /// </summary>
    public bool AutoReloadFormFields { get; set; }

    private IHttpRequest Request { get; }
    private IEncryptionService EncryptionService { get; }
    private DataItemService DataItemService { get; }
    
    public Guid? ConnectionId { get; set; }
    public FormElementDataItem DataItem { get; set; }
    public FormStateData FormStateData { get; set; }

    public string SelectedValue
    {
#if NETFRAMEWORK
        [Obsolete("Please use GetSelectedValueAsync()")]
        get => JJMasterData.Core.Tasks.AsyncHelper.RunSync(GetSelectedValueAsync) ?? string.Empty;
#endif

        set => _selectedValue = value;
    }
    
    private RouteContext? _routeContext;
    
    protected RouteContext RouteContext
    {
        get
        {
            if (_routeContext != null)
                return _routeContext;

            var factory = new RouteContextFactory(Request.QueryString, EncryptionService);
            _routeContext = factory.Create();
            
            return _routeContext;
        }
    }
    
    internal ComponentContext ComponentContext => RouteContext.ComponentContext;


    #endregion

    #region "Constructors"

    public JJSearchBox(
        IHttpRequest request,
        IEncryptionService encryptionService,
        DataItemService dataItemService) : base(request.Form)
    {
        HtmlId = Name;
        Request = request;
        EncryptionService = encryptionService;
        DataItemService = dataItemService;
        Enabled = true;
        TriggerLength = 1;
        PlaceHolder = "Search...";
        NumberOfItems = 10;
        ScrollBar = true;
        AutoReloadFormFields = true;
        Name = "jjsearchbox1";
        DataItem = new FormElementDataItem();
        var defaultValues = new Dictionary<string, object?>();
        FormStateData = new(defaultValues, UserValues, PageState.List);
    }

    #endregion

    protected override Task<ComponentResult> BuildResultAsync()
    {
        var fieldName = Request.QueryString["fieldName"];
        
        if (ComponentContext is ComponentContext.SearchBox or ComponentContext.SearchBoxFilter && FieldName == fieldName)
        {
            return GetItemsResult();
        }

        return GetRenderedComponentResult();
    }

    internal async Task<ComponentResult> GetRenderedComponentResult()
    {
        var html = await GetSearchBoxHtml();

        return new RenderedComponentResult(html);
    }

    internal async Task<ComponentResult> GetItemsResult()
    {
        return new JsonComponentResult(await GetSearchBoxItemsAsync());
    }

    private async Task<HtmlBuilder> GetSearchBoxHtml()
    {
        if (DataItem == null)
            throw new ArgumentException("[DataItem] property not set");

        var selectedValue = await GetSelectedValueAsync();

        var div = new Div();
        await div.AppendAsync(HtmlTag.Input, async input =>
        {
            input.WithAttribute("id", $"{HtmlId}_text");
            input.WithAttribute("name", $"{HtmlId}_text");
            input.WithAttribute("hidden-input-id", HtmlId);
            input.WithAttribute("type", "text");
            input.WithAttribute("query-string", GetQueryString());
            input.WithAttribute("autocomplete", "off");
            input.WithAttributeIf(MaxLength > 0, "maxlength", MaxLength.ToString());
            input.WithAttributeIf(ReadOnly, "readonly", "readonly");
            input.WithAttributeIf(!Enabled, "disabled", "disabled");
            input.WithAttributes(Attributes);
            input.WithAttribute(TriggerLengthAttribute, TriggerLength);
            input.WithAttribute(NumberOfItemsAttribute, NumberOfItems);
            input.WithToolTip(Tooltip);
            input.WithCssClass("form-control jj-search-box");
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
            input.WithAttributeIfNotEmpty("value", selectedValue);
        });

        if (ScrollBar)
            div.WithCssClass("scrollable-dropdown-menu");
        
        return div;
    }

    private string GetQueryString()
    {
        var url = new StringBuilder();

        var componentContext = FormStateData.PageState is PageState.Filter
            ? ComponentContext.SearchBoxFilter
            : ComponentContext.SearchBox;
        
        var context = new RouteContext(ElementName, ParentElementName, componentContext);
        
        var encryptedRoute = EncryptionService.EncryptRouteContext(context);

        url.Append($"routeContext={encryptedRoute}");
        url.Append($"&fieldName={FieldName}");

        return url.ToString();
    }

    /// <summary>
    /// Recupera descrição com base no id
    /// </summary>
    /// <param name="searchId">Id a ser pesquisado</param>
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
            var dataQuery = new DataQuery(FormStateData, ConnectionId)
            {
                SearchId = searchId
            };
            _values ??= await DataItemService.GetValuesAsync(DataItem, dataQuery);
        }

        var item = _values?.First(x => x.Id.Equals(searchId));

        if (item != null)
            description = item.Description;

        return description;
    }

    /// <summary>
    /// Recover values from the given text.
    /// </summary>
    private async Task<List<DataItemValue>> GetValuesAsync(string? searchText)
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
            var dataQuery = new DataQuery(FormStateData, ConnectionId)
            {
                SearchText = searchText
            };
            list.AddRange(await DataItemService.GetValuesAsync(DataItem, dataQuery));
        }

        return list;
    }


    private async Task<List<DataItemResult>> GetSearchBoxItemsAsync()
    {
        var searchText = Request.Form[Name + "_text"];
        var values = await GetValuesAsync(searchText);
        return values.ConvertAll(v=>new DataItemResult()
        {
            Id = v.Id,
            Description = v.Description,
            IconCssClass = DataItem.ShowIcon ? v.Icon.GetCssClass() : null,
            IconColor =  DataItem.ShowIcon ? v.IconColor : null
        });
    }
}