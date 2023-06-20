﻿using JJMasterData.Commons.DI;
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

    private const string NumberOfItemsAttribute = "numberofitems";
    private const string ScrollbarAttribute = "scrollbar";
    private const string TriggerLengthAttribute = "triggerlength";

    private IList<DataItemValue> _values;
    private FormElementDataItem _dataItem;
    private string _selectedValue;
    private string _text;


    internal ExpressionOptions ExpressionOptions { get; }


    internal string Id
    {
        get => Name.Replace(".", "_").Replace("[", "_").Replace("]", "_");
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
            if (Attributes.ContainsKey(NumberOfItemsAttribute))
                return int.Parse(Attributes[NumberOfItemsAttribute].ToString());
            return 0;
        }
        set
        {
            if (Attributes.ContainsKey(NumberOfItemsAttribute))
                Attributes[NumberOfItemsAttribute] = value;
            else
                Attributes.Add(NumberOfItemsAttribute, value);
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
            Attributes.ContainsKey(ScrollbarAttribute) &&
            Attributes[ScrollbarAttribute].ToString().Equals("true");
        set
        {
            string v = value ? "true" : "false";
            if (Attributes.ContainsKey(ScrollbarAttribute))
                Attributes[ScrollbarAttribute] = v;
            else
                Attributes.Add(ScrollbarAttribute, v);
        }
    }

    /// <summary>
    /// Id correspondente ao texto pesquisado
    /// </summary>
    public string SelectedValue
    {
        get
        {
            if (AutoReloadFormFields && string.IsNullOrEmpty(_selectedValue) && CurrentContext.IsPost)
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
        if (IsSearchBoxRoute(this))
        {
            ResponseJson();
            return null;
        }

        return GetSearchBoxHtml();
    }


    public static bool IsSearchBoxRoute(JJBaseView view)
    {
        string requestType = view.CurrentContext.Request.QueryString("t");
        return "jjsearchbox".Equals(requestType);
    }

    public static HtmlBuilder ResponseJson(JJDataPanel view)
    {
        string dictionaryName = view.CurrentContext.Request.QueryString("dictionaryName");
        string fieldName = view.CurrentContext.Request.QueryString("objname");
        var pageState = (PageState)int.Parse(view.CurrentContext.Request.QueryString("pageState"));

        if (!IsSearchBoxRoute(view))
            return null;

        if (string.IsNullOrEmpty(dictionaryName))
            return null;

        FormElement element;
        IDictionary formValues = null;
        if (view.FormElement.Name.Equals(dictionaryName))
        {
            element = view.FormElement;
            formValues = view.Values;
        }
        else
        {
            element = view.DataDictionaryRepository.GetMetadata(dictionaryName);
            var dataItem = element.Fields[fieldName].DataItem;
            if (dataItem == null)
                throw new ArgumentNullException(nameof(dataItem));

            if (dataItem.HasSqlExpression())
            {
                var expression = new ExpressionManager(view.UserValues, view.EntityRepository);
                var fieldManager = new FieldManager(element, expression);
                var formRequest = new FormValues(fieldManager);
                var dbValues = formRequest.GetDatabaseValuesFromPk(element);
                formValues = formRequest.GetFormValues(pageState, dbValues, true);
            }
        }

        var field = element.Fields[fieldName];
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
            _values ??= searchData.GetValues(null, idSearch);
        }

        var item = _values?.ToList().Find(x => x.Id.Equals(idSearch));

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
            _values = args.Values;
        }
        else
        {
            var searchData = new SearchBoxData(DataItem, ExpressionOptions);
            _values ??= searchData.GetValues(searchText, null);
        }

        if (_values != null && searchText != null)
            return _values.ToList().FindAll(x => x.Description.ToLower().Contains(searchText.ToLower()));
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

    private string GetJsonValues(string textSearch)
    {
        var listValue = GetValues(textSearch);
        var listItem = new List<SearchBoxItem>();
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