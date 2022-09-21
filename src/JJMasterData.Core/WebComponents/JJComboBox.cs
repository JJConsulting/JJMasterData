using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using JJMasterData.Commons.Dao;
using JJMasterData.Commons.Dao.Entity;
using JJMasterData.Commons.Language;
using JJMasterData.Commons.Util;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataManager;

namespace JJMasterData.Core.WebComponents;
public class JJComboBox : JJBaseView
{

    private List<DataItemValue> _values;
    private string _selectedValue;
    private FormElementDataItem _DataItem;

    internal PageState PageState { get; set; }
    public bool ReadOnly { get; set; }

    public bool Enable { get; set; }

    public bool EnableSearch { get; set; }

    /// <summary>
    /// Caso o filtro do formulário seja MULTVALUES_EQUALS, habilita uma combobox de multiseleção.
    /// </summary>
    public bool MultiSelect { get; set; }

    private Hashtable FormValues { get; set; }

    public FormElementDataItem DataItem
    {
        get
        {
            if (_DataItem == null)
            {
                _DataItem = new FormElementDataItem();
            }
            return _DataItem;
        }
        set
        {
            _DataItem = value;
        }
    }

    public string SelectedValue
    {
        get
        {
            if (_selectedValue == null && IsPostBack)
            {
                _selectedValue = CurrentContext.Request[Name];
            }
            return _selectedValue;
        }
        set
        {
            _selectedValue = value;
        }
    }

    public JJComboBox()
    {
        Enable = true;
        MultiSelect = false;
    }

    public JJComboBox(IDataAccess dataAccess) : base(dataAccess)
    {
        Enable = true;
    }


    internal static JJComboBox GetInstance(FormElementField f,
                                  PageState pagestate,
                                  object value,
                                  Hashtable formValues,
                                  bool enable,
                                  string name)
    {
        JJComboBox cbo = new JJComboBox
        {
            Name = name ?? f.Name,
            Visible = true,
            DataItem = f.DataItem,
            Enable = enable,
            ReadOnly = f.DataBehavior == FieldBehavior.ViewOnly && pagestate != PageState.Filter,
            FormValues = formValues,
            PageState = pagestate
        };
        if (value != null)
            cbo.SelectedValue = value.ToString();

        return cbo;
    }


    protected override string RenderHtml()
    {
        if (DataItem == null)
            throw new ArgumentException("Propriedades [DataItem] não definidas para combo", Name);

        var list = GetValues();
        if (list == null)
            throw new ArgumentException("Origem de dados não definida para combo", Name);

        string cssClass = "form-control form-select";
        if (!string.IsNullOrEmpty(CssClass))
            cssClass += " " + CssClass;

        StringBuilder html = new StringBuilder();
        if (ReadOnly)
        {
            html.Append("<input type=\"hidden\" id=\"");
            html.Append(Name);
            html.Append("\" name=\"");
            html.Append(Name);
            html.Append("\" value=\"");
            html.Append(SelectedValue);
            html.Append("\"/>");

            string text = "";
            if (SelectedValue != null)
            {
                foreach (DataItemValue item in list)
                {
                    if (SelectedValue.Equals(item.Id))
                    {
                        text = item.Description;

                        if (IsManualValues())
                            text = Translate.Key(text);

                        break;
                    }
                }
            }

            html.Append("<input type=\"text\" id=\"cboview_");
            html.Append(Name);
            html.Append("\" name=\"cboview_");
            html.Append(Name);
            html.Append("\" value=\"");
            html.Append(text);
            html.Append("\" class=\"");
            html.Append(cssClass);
            html.Append("\" readonly");

            foreach (DictionaryEntry attr in Attributes)
            {
                html.Append(" ");
                html.Append(attr.Key);
                if (attr.Value != null)
                {
                    html.Append("=\"");
                    html.Append(attr.Value);
                    html.Append("\"");
                }
            }

            html.Append("/>");
        }
        else
        {
            if (MultiSelect)
                cssClass += " selectpicker";
              
            html.Append("<select class=\"");
            html.Append(cssClass);
            html.Append("\" id=\"");
            html.Append(Name);
            html.Append("\" name=\"");
            html.Append(Name);
            html.Append("\"");

            if (MultiSelect)
                html.Append("title=\"(Todos)\" data-live-search=\"true\" multiple");

            if (!Enable)
                html.Append(" disabled");


            foreach (DictionaryEntry attr in Attributes)
            {
                html.Append(" ");
                html.Append(attr.Key);
                if (attr.Value != null)
                {
                    html.Append("=\"");
                    html.Append(attr.Value);
                    html.Append("\"");
                }
            }

            html.Append(" data-style=\"form-control\">");

            if (DataItem.FirstOption == FirstOptionMode.All)
                html.Append("<option value=\"\">(Todos)</option>");
            else if (DataItem.FirstOption == FirstOptionMode.Choose)
                html.Append("<option value=\"\">(Selecione)</option>");


            foreach (DataItemValue item in list)
            {
                string label;
                if (IsManualValues())
                    label = Translate.Key(item.Description);
                else
                    label = item.Description;


                html.Append("<option value=\"");
                html.Append(item.Id);
                html.Append("\"");
                if (SelectedValue != null && SelectedValue.Equals(item.Id))
                {
                    html.Append(" selected");
                }

                if (DataItem.ShowImageLegend)
                {
                    html.Append(" data-content=\"");
                    var icon = new JJIcon(item.Icon, item.ImageColor)
                    {
                        CssClass = "fa-lg fa-fw"
                    };
                    html.Append(icon.GetHtml().Replace("\"", "'"));
                    html.AppendFormat("&nbsp;{0}\"", label);    
                }

                html.Append(">");
                html.Append(label);
                html.Append("</option>");
            }

            html.Append("</select>");
        }
        return html.ToString();
    }


    public List<DataItemValue> GetValues()
    {
        try
        {
            if (_values == null)
                _values = GetValues(null);
        }
        catch (Exception ex)
        {
            string err = string.Format("Erro carregar os dados da JJComboBox {0}. Detalhes do Erro: {1}", Name, ex.Message);
            throw new Exception(err);
        }

        return _values;
    }


    /// <summary>
    /// Recupera descrição com base no valor selecionado
    /// </summary>
    /// <returns>Retorna descricão referente ao id</returns>
    public string GetDescription()
    {
        string sRet = null;
        var item = GetValue(SelectedValue);
        if (item == null)
            return sRet;

        string label;
        if (IsManualValues())
            label = Translate.Key(item.Description);
        else
            label = item.Description;

        if (DataItem.ShowImageLegend)
        {
            StringBuilder sHtml = new StringBuilder();
            var ic = new JJIcon(item.Icon, item.ImageColor, item.Description);
            ic.CssClass = "fa-lg fa-fw";
            sHtml.Append(ic.GetHtml());

            if (DataItem.ReplaceTextOnGrid)
            {
                sHtml.Append("&nbsp;");
                sHtml.Append(label);
            }
            sRet = sHtml.ToString();
        }
        else
        {
            sRet = label;
        }

        return sRet;
    }


    public DataItemValue GetValue(string searchId)
    {
        if (searchId == null)
            return null;

        List<DataItemValue> listValues;
        if (_values == null)
            listValues = GetValues(searchId);
        else
            listValues = _values;


        if (listValues == null)
            return null;


        return listValues.ToList().Find(x => x.Id.Equals(searchId));
    }

    private List<DataItemValue> GetValues(string searchId)
    {
        if (DataItem == null)
            return null;

        var values = new List<DataItemValue>();
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

                var exp = new ExpressionManager(UserValues, DataAccess);
                sql = exp.ParseExpression(sql, PageState, false, FormValues);
            }


            DataTable dt = DataAccess.GetDataTable(sql);
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
            values = DataItem.Itens;
        }


        return values;
    }


    private bool IsManualValues()
    {
        if (DataItem == null)
            return false;

        if (DataItem.Itens == null)
            return false;

        if (DataItem.Itens.Count > 0)
            return true;

        return false;
    }

}
