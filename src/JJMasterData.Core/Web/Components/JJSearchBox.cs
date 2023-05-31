using JJMasterData.Commons.DI;
using JJMasterData.Commons.Localization;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataManager;
using JJMasterData.Core.FormEvents.Args;
using JJMasterData.Core.Web.Html;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JJMasterData.Commons.Util;
using JJMasterData.Core.Web.Http;

namespace JJMasterData.Core.Web.Components;


/// <summary>
/// Represents a searchable combobox.
/// </summary>
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

    private const string NUMBER_OF_ITEMS_ATTRIBUTE = "numberofitems";
    private const string SCROLLBAR_ATTRIBUTE = "scrollbar";
    private const string TRIGGER_LENGTH_ATTRIBUTE = "triggerlength";

    private IList<DataItemValue> values;
    private FormElementDataItem dataItem;
    private string selectedValue;
    private string text;


    internal ExpressionOptions ExpressionOptions { get; }


    internal string Id
    {
        get => Name.Replace(".", "_").Replace("[", "_").Replace("]", "_");
    }


    public new string Text
    {
        get
        {
            if (AutoReloadFormFields && text == null && CurrentContext.IsPostBack)
            {
                text = CurrentContext.Request[Name];
            }

            return text;
        }
        set => text = value;
    }

    /// <summary>
    /// Quantidade minima de caracteres digitado para disparar a pesquisa
    /// (Default = 1)
    /// </summary>
    public int TriggerLength
    {
        get
        {
            if (Attributes.TryGetValue(TRIGGER_LENGTH_ATTRIBUTE, out var attribute))
                return int.Parse(attribute.ToString());
            return 0;
        }
        set => Attributes[TRIGGER_LENGTH_ATTRIBUTE] = value;
    }

    /// <summary>
    /// Numero máximo de itens que será exibido na lista de pesquisa
    /// (Default = 10)
    /// </summary>
    public int NumberOfItems
    {
        get
        {
            if (Attributes.ContainsKey(NUMBER_OF_ITEMS_ATTRIBUTE))
                return int.Parse(Attributes[NUMBER_OF_ITEMS_ATTRIBUTE].ToString());
            return 0;
        }
        set
        {
            if (Attributes.ContainsKey(NUMBER_OF_ITEMS_ATTRIBUTE))
                Attributes[NUMBER_OF_ITEMS_ATTRIBUTE] = value;
            else
                Attributes.Add(NUMBER_OF_ITEMS_ATTRIBUTE, value);
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
        get =>
            Attributes.ContainsKey(SCROLLBAR_ATTRIBUTE) &&
            Attributes[SCROLLBAR_ATTRIBUTE].ToString().Equals("true");
        set
        {
            string v = value ? "true" : "false";
            if (Attributes.ContainsKey(SCROLLBAR_ATTRIBUTE))
                Attributes[SCROLLBAR_ATTRIBUTE] = v;
            else
                Attributes.Add(SCROLLBAR_ATTRIBUTE, v);
        }
    }

    /// <summary>
    /// Id correspondente ao texto pesquisado
    /// </summary>
    public string SelectedValue
    {
        get
        {
            if (AutoReloadFormFields && string.IsNullOrEmpty(selectedValue) && CurrentContext.IsPostBack)
            {
                selectedValue = CurrentContext.Request[Name];
            }

            if (string.IsNullOrEmpty(selectedValue) && !string.IsNullOrEmpty(Text))
            {
                var list = GetValues(Text);
                if (list.Count == 1)
                    selectedValue = list.ToList().First().Id;
            }

            return selectedValue;
        }
        set => selectedValue = value;
    }

    /// <summary>
    /// Origem dos dados
    /// </summary>
    public FormElementDataItem DataItem
    {
        get => dataItem ??= new FormElementDataItem();
        set => dataItem = value;
    }

    /// <summary>
    /// Ao recarregar o painel, manter os valores digitados no formuário
    /// (Default=True)
    /// </summary>
    public bool AutoReloadFormFields { get; set; }


    #endregion

    #region "Constructors"

    public JJSearchBox()
    {
        Enabled = true;
        TriggerLength = 1;
        PlaceHolder = Translate.Key("Search...");
        NumberOfItems = 10;
        ScrollBar = false;
        AutoReloadFormFields = true;
        Name = "jjsearchbox1";
        ExpressionOptions = new ExpressionOptions(UserValues, null, PageState.List, JJService.EntityRepository);
    }

    public JJSearchBox(ExpressionOptions expOptions) : this()
    {
        ExpressionOptions = expOptions;
        UserValues = expOptions.UserValues;
    }

    internal static JJSearchBox GetInstance(FormElementField f, ExpressionOptions expOptions, object value, string dictionaryName)
    {
        var search = new JJSearchBox(expOptions)
        {
            Name = f.Name,
            SelectedValue = value?.ToString(),
            Visible = true,
            AutoReloadFormFields = false,
            DataItem = f.DataItem
        };

        search.Attributes.Add("dictionaryName", dictionaryName);
        return search;
    }

    #endregion

    internal override HtmlBuilder RenderHtml()
    {
        if (IsSearchBoxRoute(CurrentContext))
        {
            ResponseJson();
            return null;
        }

        return GetSearchBoxHtml();
    }


    public static bool IsSearchBoxRoute(JJHttpContext context)
    {
        string requestType = context.Request.QueryString("t");
        return "jjsearchbox".Equals(requestType);
    }

    public static HtmlBuilder ResponseRoute(JJDataPanel view)
    {
        string dictionaryName = view.CurrentContext.Request.QueryString("dictionaryName");
        string fieldName = view.CurrentContext.Request.QueryString("objname");
        var pageState = (PageState)int.Parse(view.CurrentContext.Request.QueryString("pageState"));

        if (!IsSearchBoxRoute(view.CurrentContext))
            return null;

        if (string.IsNullOrEmpty(dictionaryName))
            return null;

        FormElement element = view.FormElement.Name.Equals(dictionaryName)
            ? view.FormElement
            : view.DataDictionaryRepository.GetMetadata(dictionaryName);

        var field = element.Fields[fieldName];
        var dataItem = field.DataItem;

        if (dataItem == null)
            throw new ArgumentNullException(nameof(dataItem));

        IDictionary formValues = null;
        string sql = dataItem.Command.Sql;
        sql = sql.Replace("{search_id}", string.Empty);
        sql = sql.Replace("{search_text}", string.Empty);
        if (sql.Contains("{"))
        {
            var formRequest = new FormValues(view.FieldManager);
            var dbValues = formRequest.GetDatabaseValuesFromPk(element);
            formValues = formRequest.GetFormValues(pageState, dbValues, true);
        }

        var expOptions = new ExpressionOptions(view.UserValues, formValues, pageState, view.EntityRepository);
        var searchBox = JJSearchBox.GetInstance(field, expOptions, null, dictionaryName);
        searchBox.ResponseJson();

        return null;
    }

    private HtmlBuilder GetSearchBoxHtml()
    {
        if (DataItem == null)
            throw new ArgumentException(Translate.Key("[DataItem] property not set"), Name);

        var div = new HtmlBuilder(HtmlTag.Div)
            .AppendElement(HtmlTag.Input, input =>
            {
                input.WithAttribute("id", Id + "_text");
                input.WithAttribute("name", Name + "_text");
                input.WithAttribute("jjid", Name);
                input.WithAttribute("type", "text");
                input.WithAttribute("pageState", (int)ExpressionOptions.PageState);
                input.WithAttribute("autocomplete", "off");
                input.WithAttributeIf(MaxLength > 0, "maxlength", MaxLength.ToString());
                input.WithAttributeIf(DataItem.ShowImageLegend, "showimagelegend", "true");
                input.WithAttributeIf(ReadOnly, "readonly", "readonly");
                input.WithAttributeIf(!Enabled, "disabled", "disabled");
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
            .AppendElement(HtmlTag.Input, input =>
            {
                input.WithAttribute("hidden", "hidden");
                input.WithAttribute("id", Id);
                input.WithAttribute("name", Name);
                input.WithValue(SelectedValue);
            });

        return div;
    }

    /// <summary>
    /// Recupera descrição com base no id
    /// </summary>
    /// <param name="idSearch">Id a ser pesquisado</param>
    /// <returns>Retorna descrição referente ao id</returns>
    public string GetDescription(string idSearch)
    {
        string description = null;
        if (OnSearchQuery != null)
        {
            var args = new SearchBoxItemEventArgs(idSearch);
            OnSearchId?.Invoke(this, args);
            description = args.ResultText;
        }
        else
        {
            var searchData = new SearchBoxData(DataItem, ExpressionOptions);
            values ??= searchData.GetValues(null, idSearch);
        }

        var item = values?.ToList().Find(x => x.Id.Equals(idSearch));

        if (item != null)
            description = item.Description;

        return description;
    }

    /// <summary>
    /// Recover values from the given text.
    /// </summary>
    public List<DataItemValue> GetValues(string searchText)
    {
        if (OnSearchQuery != null)
        {
            var args = new SearchBoxQueryEventArgs(searchText);
            OnSearchQuery.Invoke(this, args);
            values = args.Values;
        }
        else
        {
            var searchData = new SearchBoxData(DataItem, ExpressionOptions);
            values ??= searchData.GetValues(searchText, null);
        }

        if (values != null && searchText != null)
            return values.ToList().FindAll(x => x.Description.ToLower().Contains(searchText.ToLower()));
        return null;
    }

    public void ResponseJson()
    {
        if (!Name.Equals(CurrentContext.Request.QueryString("objname")))
            return;

        string textSearch = CurrentContext.Request[Name + "_text"];
        string json = GetJsonValues(textSearch);
        CurrentContext.Response.SendResponse(json, "application/json");
    }


    public string GetJsonValues(string textSearch)
    {
        var listValue = GetValues(textSearch);
        var listItem = new List<SearchBoxItem>();
        //TODO: use DaItemValue insteadof SearchBoxItem
        string description;
        foreach (var i in listValue)
        {
            if (DataItem.ShowImageLegend)
                description = $"{i.Description}|{i.Icon.GetCssClass()}|{i.ImageColor}";
            else
                description = i.Description;

            listItem.Add(new SearchBoxItem(i.Id, description));
        }

        return JsonConvert.SerializeObject(listItem.ToArray());
    }


}