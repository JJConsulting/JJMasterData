using JJMasterData.Commons.Dao;
using JJMasterData.Commons.DI;
using JJMasterData.Commons.Language;
using JJMasterData.Commons.Util;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataManager;
using JJMasterData.Core.Html;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace JJMasterData.Core.WebComponents;

public class JJComboBox : JJBaseControl
{
    private IList<DataItemValue> _values;
    private string _selectedValue;
    private FormElementDataItem _dataItem;
    private IEntityRepository _entityRepository;

    internal IEntityRepository EntityRepository
    {
        get => _entityRepository ??= JJService.EntityRepository;
        private set => _entityRepository = value;
    }

    internal Hashtable FormValues { get; private set; }

    internal PageState PageState { get; set; }

    public bool EnableSearch { get; set; }

    /// <summary>
    /// If the filter is MULTVALUES_EQUALS, enable multiselect.
    /// </summary>
    public bool MultiSelect { get; set; }

    public FormElementDataItem DataItem
    {
        get => _dataItem ??= new FormElementDataItem();
        set => _dataItem = value;
    }

    public string SelectedValue
    {
        get
        {
            if (_selectedValue == null && CurrentContext.IsPostBack)
            {
                _selectedValue = CurrentContext.Request[Name];
            }

            return _selectedValue;
        }
        set => _selectedValue = value;
    }

    public JJComboBox()
    {
        Enabled = true;
        MultiSelect = false;
    }


    internal static JJComboBox GetInstance(FormElementField f, ExpressionOptions expOptions, object value)
    {
        var cbo = new JJComboBox
        {
            Name = f.Name,
            Visible = true,
            DataItem = f.DataItem,
            FormValues = expOptions.FormValues,
            PageState = expOptions.PageState,
            UserValues = expOptions.UserValues,
            EntityRepository = expOptions.EntityRepository
        };
        if (value != null)
            cbo.SelectedValue = value.ToString();

        return cbo;
    }

    internal override HtmlBuilder RenderHtml()
    {
        if (DataItem == null)
            throw new ArgumentException(Translate.Key("[DataItem] properties not defined for combo"), Name);

        var values = GetValues();

        if (values == null)
            throw new ArgumentException(Translate.Key("Data source not defined for combo"), Name);



        if (ReadOnly)
        {
            var combobox = new HtmlBuilder(HtmlTag.Div);
            combobox.AppendRange(GetReadOnlyInputs(values));
            return combobox;
        }
        else
        {
            return GetSelectElement(values);
        }


    }

    private HtmlBuilder GetSelectElement(IEnumerable<DataItemValue> values)
    {
        var select = new HtmlBuilder(HtmlTag.Select)
            .WithCssClass("form-control ")
            .WithCssClass(MultiSelect || DataItem.ShowImageLegend ? "selectpicker" : "form-select")
            .WithNameAndId(Name)
            .WithAttributeIf(MultiSelect, "title", Translate.Key("All"))
            .WithAttributeIf(MultiSelect, "data-live-search", "true")
            .WithAttributeIf(MultiSelect, "multiselect", "multiselect")
            .WithAttributeIf(!Enabled, "disabled", "disabled")
            .WithAttribute("data-style", "form-control")
            .WithAttributes(Attributes)
            .AppendRange(GetOptions(values));

        return select;
    }

    private IEnumerable<HtmlBuilder> GetOptions(IEnumerable<DataItemValue> values)
    {
        var options = new List<HtmlBuilder>();

        var firstOption = new HtmlBuilder(HtmlTag.Option)
            .WithValue(string.Empty)
            .AppendTextIf(DataItem.FirstOption == FirstOptionMode.All, Translate.Key("(All)"))
            .AppendTextIf(DataItem.FirstOption == FirstOptionMode.Choose, Translate.Key("(Choose)"));

        if (DataItem.FirstOption != FirstOptionMode.None)
            options.Add(firstOption);

        foreach (var value in values)
        {
            var label = IsManualValues() ? Translate.Key(value.Description) : value.Description;

            var option = new HtmlBuilder(HtmlTag.Option)
                .WithValue(value.Id)
                .WithAttributeIf(SelectedValue != null && SelectedValue.Equals(value.Id), "selected", "selected")
                .WithAttributeIf(DataItem.ShowImageLegend, "data-icon", value.Icon.GetCssClass())
                .AppendText(label);

            options.Add(option);
        }

        return options;
    }

    private IEnumerable<HtmlBuilder> GetReadOnlyInputs(IEnumerable<DataItemValue> values)
    {
        var inputs = new List<HtmlBuilder>();

        var hiddenInput = new HtmlBuilder(HtmlTag.Input)
            .WithAttribute("type", "hidden")
            .WithNameAndId(Name)
            .WithValue(SelectedValue);

        inputs.Add(hiddenInput);

        var selectedText = GetSelectedText(values);

        var readonlyInput = new HtmlBuilder(HtmlTag.Input)
            .WithNameAndId("cboview_" + Name)
            .WithCssClass("form-control form-select")
            .WithCssClass(CssClass)
            .WithValue(selectedText)
            .WithAttributes(Attributes)
            .WithAttribute("readonly", "readonly");

        inputs.Add(readonlyInput);

        return inputs;
    }

    private string GetSelectedText(IEnumerable<DataItemValue> list)
    {
        string selectedText = string.Empty;

        if (SelectedValue == null)
            return selectedText;

        foreach (var item in list.Where(item => SelectedValue.Equals(item.Id)))
        {
            selectedText = item.Description;

            if (IsManualValues())
                selectedText = Translate.Key(selectedText);

            break;
        }

        return selectedText;
    }

    public IList<DataItemValue> GetValues()
    {
        try
        {
            _values ??= GetValues(null);
        }
        catch (Exception ex)
        {
            var err = Translate.Key("Error loading data from JJComboBox {0}. Error Details: {1}", Name, ex.Message);
            throw new Exception(err);
        }

        return _values;
    }


    /// <summary>
    /// Recovers the description from the selected value;
    /// </summary>
    public string GetDescription()
    {
        string description;
        var item = GetValue(SelectedValue);
        if (item == null)
            return null;

        var label = IsManualValues() ? Translate.Key(item.Description) : item.Description;

        if (DataItem.ShowImageLegend)
        {
            var div = new HtmlBuilder(HtmlTag.Div);

            var icon = new JJIcon(item.Icon, item.ImageColor, item.Description)
            {
                CssClass = "fa-lg fa-fw"
            }.RenderHtml();

            div.AppendElement(icon);

            if (DataItem.ReplaceTextOnGrid)
            {
                div.AppendText("&nbsp;");
                div.AppendText(label);
            }
            description = div.ToString();
        }
        else
        {
            description = label;
        }

        return description;
    }


    public DataItemValue GetValue(string searchId)
    {
        if (searchId == null)
            return null;

        var listValues = _values ?? GetValues(searchId);


        return listValues?.ToList().Find(x => x.Id.Equals(searchId));
    }

    private IList<DataItemValue> GetValues(string searchId)
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
                else
                {
                    if (!UserValues.ContainsKey("search_id"))
                        UserValues.Add("search_id", null);
                }

                var exp = new ExpressionManager(UserValues, EntityRepository);
                sql = exp.ParseExpression(sql, PageState, false, FormValues);
            }


            DataTable dt = EntityRepository.GetDataTable(sql);
            foreach (DataRow row in dt.Rows)
            {
                var item = new DataItemValue
                {
                    Id = row[0].ToString(),
                    Description = row[1].ToString()?.Trim()
                };
                if (DataItem.ShowImageLegend)
                {
                    item.Icon = (IconType)int.Parse(row[2].ToString() ?? string.Empty);
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


    private bool IsManualValues()
    {
        if (DataItem?.Items == null)
            return false;

        return DataItem.Items.Count > 0;
    }
}