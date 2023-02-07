using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.Serialization;
using JJMasterData.Commons.Data.Entity.Abstractions;
using JJMasterData.Commons.DI;
using JJMasterData.Commons.Localization;
using JJMasterData.Commons.Util;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataManager;
using JJMasterData.Core.FormEvents.Args;
using JJMasterData.Core.Web.Html;
using Newtonsoft.Json;

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

    private IEntityRepository _entityRepository;
    private IList<DataItemValue> _values;
    private FormElementDataItem _dataItem;
    private string _selectedValue;
    private string _text;

    internal Hashtable FormValues { get; private set; }

    internal PageState PageState { get; set; }

    internal string Id
    {
        get => Name.Replace(".", "_").Replace("[", "_").Replace("]", "_");
    }

    internal IEntityRepository EntityRepository
    {
        get => _entityRepository ??= JJService.EntityRepository;
        private set => _entityRepository = value;
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

    /// <summary>
    /// Quantidade minima de caracteres digitado para disparar a pesquisa
    /// (Default = 1)
    /// </summary>
    public int TriggerLength
    {
        get
        {
            if (Attributes.ContainsKey(TriggerLengthAttribute))
                return int.Parse(Attributes[TriggerLengthAttribute].ToString());
            return 0;
        }
        set
        {
            if (Attributes.ContainsKey(TriggerLengthAttribute))
                Attributes[TriggerLengthAttribute] = value;
            else
                Attributes.Add(TriggerLengthAttribute, value);
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
            if (AutoReloadFormFields && string.IsNullOrEmpty(_selectedValue) && CurrentContext.IsPostBack)
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
        PageState = PageState.List;
    }

    public JJSearchBox(IEntityRepository entityRepository) : this()
    {
        EntityRepository = entityRepository;
    }

    internal static JJSearchBox GetInstance(FormElementField f, ExpressionOptions expOptions, object value, string panelName)
    {
        var search = new JJSearchBox
        {
            Name = f.Name,
            SelectedValue = value?.ToString(),
            Visible = true,
            DataItem = f.DataItem,
            AutoReloadFormFields = false,
            FormValues = expOptions.FormValues,
            PageState = expOptions.PageState,
            EntityRepository = expOptions.EntityRepository,
            UserValues = expOptions.UserValues
        };

        search.Attributes.Add("pnlname", panelName);

        return search;
    }

    #endregion

    internal override HtmlBuilder RenderHtml()
    {
        var html = new HtmlBuilder();
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

    private HtmlBuilder GetSearchBoxHtmlElement()
    {
        if (DataItem == null)
            throw new ArgumentException(Translate.Key("[DataItem] property not set"), Name);

        var div = new HtmlBuilder(HtmlTag.Div)
            .AppendElement(HtmlTag.Input, input =>
            {
                input.WithAttribute("id", Id + "_text");
                input.WithAttribute("name", Name + "_text");
                input.WithAttribute("jjid", Id);
                input.WithAttribute("type", "text");
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
    /// <returns>Retorna descricão referente ao id</returns>
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
            _values ??= GetValues(null, idSearch);
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
            _values ??= GetValues(searchText, null);
        }

        if (_values != null && searchText != null)
            return _values.ToList().FindAll(x => x.Description.ToLower().Contains(searchText.ToLower()));
        return null;
    }

    private IList<DataItemValue> GetValues(string searchText, string searchId)
    {
        if (DataItem == null)
            return null;

        IList<DataItemValue> values = new List<DataItemValue>();
        if (DataItem.Command != null && !string.IsNullOrEmpty(DataItem.Command.Sql))
        {

            string sql = DataItem.Command.Sql;
            if (sql.Contains("{"))
            {
                if (searchId != null)
                {
                    if (!UserValues.ContainsKey("search_id"))
                        UserValues.Add("search_id", StringManager.ClearText(searchId));
                }

                if (searchText != null)
                {
                    if (!UserValues.ContainsKey("search_text"))
                        UserValues.Add("search_text", StringManager.ClearText(searchText));
                }

                var exp = new ExpressionManager(UserValues, EntityRepository);
                sql = exp.ParseExpression(sql, PageState, false, FormValues);
            }


            DataTable dt = EntityRepository.GetDataTable(sql);
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
            values = DataItem.Items;
        }


        return values;
    }

    private string GetJsonValues(string textSearch)
    {
        var listValue = GetValues(textSearch);
        var listItem = new List<JJSearchBoxItem>();

        string description;
        foreach (var i in listValue)
        {
            if (DataItem.ShowImageLegend)
                description = $"{i.Description}|{i.Icon.GetCssClass()}|{i.ImageColor}";
            else
                description = i.Description;

            listItem.Add(new JJSearchBoxItem(i.Id, description));
        }

        return JsonConvert.SerializeObject(listItem.ToArray());
    }

    /// <summary>
    /// Record used to send data to the client.
    /// </summary>
    [Serializable]
    [DataContract]
    private record JJSearchBoxItem(string Id, string Name)
    {
        [DataMember(Name = "id")]
        public string Id { get; set; } = Id;

        [DataMember(Name = "name")]
        public string Name { get; set; } = Name;
    }
}