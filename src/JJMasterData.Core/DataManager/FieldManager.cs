using System;
using System.Collections;
using System.Globalization;
using JJMasterData.Commons.Dao;
using JJMasterData.Commons.Dao.Entity;
using JJMasterData.Commons.DI;
using JJMasterData.Commons.Extensions;
using JJMasterData.Commons.Util;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataDictionary.Action;
using JJMasterData.Core.Http;
using JJMasterData.Core.WebComponents;

namespace JJMasterData.Core.DataManager;

public class FieldManager
{
    #region "Properties"

    private ExpressionManager _expression;
    private Hashtable _userValues;

    private string Name { get; set; }

    /// <summary>
    /// Valores espeçificos do usuário.
    /// Utilizado para substituir os valores em tempo de execução nos métodos que suportam expression.
    /// </summary>
    internal Hashtable UserValues
    {
        get
        {
            if (_userValues == null)
                _userValues = new Hashtable();

            return _userValues;
        }
        set
        {
            _expression = null;
            _userValues = value;
        }
    }

    /// <summary>
    /// Informações sobre o request HTTP
    /// </summary>
    internal JJHttpContext CurrentContext { get; set; }

    /// <summary>
    /// Object responsible for Database communications.
    /// </summary>
    public IDataAccess DataAccess { get; set; }


    /// <summary>
    /// Configurações pré-definidas do formulário
    /// </summary>
    public FormElement FormElement { get; set; }

    /// <summary>
    /// Indica se a pagina esta sendo renderizada pela primeira vez
    /// </summary>
    private bool IsPostBack => CurrentContext.Request.HttpMethod.Equals("POST");

    /// <summary>
    /// Objeto responsável por parsear expressoões
    /// </summary>
    public ExpressionManager Expression
    {
        get
        {
            if (_expression == null)
                _expression = new ExpressionManager(UserValues, DataAccess);

            return _expression;
        }
    }

    #endregion

    #region "Constructors"


    public FieldManager(FormElement formElement)
    {
        FormElement = formElement ?? throw new ArgumentNullException(nameof(formElement));
        DataAccess = JJService.DataAccess;
        UserValues = new Hashtable();
        Name = "pnl_" + formElement.Name.ToLower();

    }
    public FieldManager(JJBaseView baseView, FormElement formElement) : this(formElement)
    {
        if (baseView == null)
            throw new ArgumentNullException(nameof(baseView));

        CurrentContext = baseView.CurrentContext;
        UserValues = baseView.UserValues;
        DataAccess = baseView.DataAccess;
        Name = baseView.Name;
    }

    #endregion

    public bool IsVisible(BasicAction action, PageState state, Hashtable formValues)
    {
        if (action == null)
            throw new ArgumentNullException(nameof(action), "action can not be null");

        return Expression.GetBoolValue(action.VisibleExpression, action.Name, state, formValues);
    }


    public bool IsVisible(FormElementField f, PageState state, Hashtable formValues)
    {
        if (f == null)
            throw new ArgumentNullException(nameof(f), "FormElementField can not be null");

        return Expression.GetBoolValue(f.VisibleExpression, f.Name, state, formValues);
    }

    public bool IsEnable(FormElementField f, PageState state, Hashtable formValues)
    {
        if (state == PageState.View)
            return false;

        if (f == null)
            throw new ArgumentNullException(nameof(f), "FormElementField can not be null");

        return Expression.GetBoolValue(f.EnableExpression, f.Name, state, formValues);
    }

    /// <summary>
    /// Formata os valores exibidos na Grid
    /// </summary>
    public string ParseVal(Hashtable values, FormElementField f)
    {
        if (values == null)
            return "";

        if (f == null)
            throw new ArgumentNullException(nameof(f), "FormElementField can not be null");

        object value = null;
        if (values.Contains(f.Name))
            value = values[f.Name];

        if (value == null)
            return "";

        string sVal;
        if (f.Component == FormComponent.ComboBox
            && f.DataItem != null
            && (f.DataItem.ReplaceTextOnGrid || f.DataItem.ShowImageLegend))
        {
            var cbo = (JJComboBox)GetField(f, PageState.List, value, values);
            sVal = cbo.GetDescription();
            if (sVal == null)
                sVal = value.ToString();
        }
        else if (f.Component == FormComponent.Lookup
           && f.DataItem != null
           && f.DataItem.ReplaceTextOnGrid)
        {
            var lookup = (JJLookup)GetField(f, PageState.List, value, values);
            sVal = lookup.GetDescription();
            if (sVal == null)
                sVal = value.ToString();
        }
        else if (f.Component == FormComponent.CheckBox)
        {
            if (Expression.ParseBool(value))
                sVal = "Sim";
            else
                sVal = "Não";
        }
        else if (f.Component == FormComponent.Search
            && f.DataItem != null
            && f.DataItem.ReplaceTextOnGrid)
        {
            var search = (JJSearchBox)GetField(f, PageState.Filter, null, values);
            search.AutoReloadFormFields = false;
            sVal = search.GetDescription(value.ToString());
            if (sVal == null)
                sVal = value.ToString();
        }
        else
        {
            sVal = FormatVal(value, f);
        }

        if (sVal == null)
            return "";

        return sVal;
    }


    /// <summary>
    /// Formata os valores exibidos no Panel
    /// </summary>
    public string FormatVal(object value, FormElementField field)
    {
        if (value == null)
            return "";

        if (field == null)
            throw new ArgumentNullException(nameof(field), "FormElementField can not be null");

        string sVal = value.ToString();
        if (string.IsNullOrEmpty(sVal))
            return "";

        FieldType type = field.DataType;
        switch (field.Component)
        {
            case FormComponent.Cnpj:
            case FormComponent.Cpf:
            case FormComponent.CnpjCpf:
                sVal = Format.FormatCnpj_Cpf(sVal);
                break;
            case FormComponent.Number:

                if (type == FieldType.Float)
                {
                    if (double.TryParse(sVal, out double nVal))
                        sVal = nVal.ToString("N" + field.NumberOfDecimalPlaces);
                }
                else if (type == FieldType.Int && !field.IsPk)
                {
                    if (int.TryParse(sVal, out int intVal))
                        sVal = intVal.ToString("N0");
                }
                break;
            case FormComponent.Currency:
                double nCurrency;
                if (double.TryParse(sVal, out nCurrency))
                    sVal = nCurrency.ToString("C" + field.NumberOfDecimalPlaces);             
                break;
            case FormComponent.Date:
            case FormComponent.DateTime:
            case FormComponent.Text:
                if (type == FieldType.Date)
                {
                    DateTime dVal = DateTime.Parse(sVal);
                    sVal = dVal == DateTime.MinValue ? "" : dVal.ToString(DateTimeFormatInfo.CurrentInfo.ShortDatePattern);
                }
                else if (type == FieldType.DateTime)
                {
                    DateTime dVal = DateTime.Parse(sVal);
                    sVal = dVal == DateTime.MinValue
                        ? ""
                        : dVal.ToString(
                            $"{DateTimeFormatInfo.CurrentInfo.ShortDatePattern} " +
                            $"{DateTimeFormatInfo.CurrentInfo.ShortTimePattern}");
                }

                break;
            case FormComponent.Tel:
                sVal = Format.FormatTel(sVal);
                break;
        }

        return sVal;
    }

    public JJBaseView GetField(FormElementField f, PageState pagestate, object value, Hashtable formValues)
    {
        return GetField(f, pagestate, value, formValues, null);
    }

    public JJBaseView GetField(FormElementField f,
                                  PageState pagestate,
                                  object value,
                                  Hashtable formValues,
                                  string fieldName)
    {
        if (f == null)
            throw new ArgumentNullException(nameof(f), "FormElementField can not be null");

        bool viewOnly = f.DataBehavior == FieldBehavior.ViewOnly && pagestate != PageState.Filter;
        bool enable = IsEnable(f, pagestate, formValues);
        JJBaseView baseView;
        switch (f.Component)
        {
            case FormComponent.ComboBox:
                baseView = JJComboBox.GetInstance(f, pagestate, value, formValues, enable, fieldName);
                baseView.DataAccess = DataAccess;
                baseView.UserValues = UserValues.DeepCopy();
                break;
            case FormComponent.Search:
                var search = JJSearchBox.GetInstance(f, pagestate, value, formValues, enable, fieldName);
                search.Attributes.Add("pnlname", Name);
                search.DataAccess = DataAccess;
                search.UserValues = UserValues.DeepCopy();
                baseView = search;
                break;
            case FormComponent.Lookup:
                var lookup = JJLookup.GetInstance(f, pagestate, value, formValues, enable, fieldName);
                lookup.Attributes.Add("pnlname", Name);
                lookup.DataAccess = DataAccess;
                lookup.UserValues = UserValues.DeepCopy();
                baseView = lookup;
                break;
            case FormComponent.CheckBox:
                bool isChecked = Expression.ParseBool(value);
                baseView = JJCheckBox.GetInstance(f, pagestate, isChecked, enable, fieldName);
                break;
            case FormComponent.Hour:
                baseView = JJTextHour.GetInstance(f, value, enable, viewOnly, fieldName);
                break;
            case FormComponent.Date:
                baseView = JJTextDate.GetInstance(f, value, enable, viewOnly, fieldName);
                break;
            case FormComponent.DateTime:
                baseView = JJTextDateTime.GetInstance(f, value, enable, viewOnly, fieldName);
                break;
            case FormComponent.TextArea:
                baseView = JJTextArea.GetInstance(f, value, enable, viewOnly, fieldName);
                break;
            case FormComponent.Tel:
                baseView = JJTextTel.GetInstance(f, value, enable, viewOnly, fieldName);
                break;
            case FormComponent.Slider:
                baseView = JJSlider.GetInstance(f, value, enable, viewOnly, fieldName);
                break;
            case FormComponent.File:
                if (pagestate == PageState.Filter)
                {
                    baseView = JJTextBox.GetInstance(f, value, enable, viewOnly, fieldName);
                }
                else
                {
                    var textFile = JJTextFile.GetInstance(f, pagestate, value, formValues, enable, fieldName);
                    textFile.Attributes.Add("pnlname", Name);
                    textFile.DataAccess = DataAccess;
                    textFile.UserValues = UserValues.DeepCopy();
                    textFile.FormElement = FormElement;
                    baseView = textFile;
                }
                break;
            default:
                baseView = JJTextBox.GetInstance(f, value, enable, viewOnly, fieldName);
                break;
        }

        return baseView;
    }


    /// <summary>
    /// Recupera os dados do Form, aplicando o valor padrão e as triggers
    /// </summary> 
    public Hashtable GetFormValues(string prefix, FormElement formElement, PageState state, Hashtable values, bool autoReloadFormFields)
    {
        if (formElement == null)
            throw new ArgumentNullException(nameof(formElement));

        var newvalues = new Hashtable();
        if (values != null)
        {
            foreach (DictionaryEntry v in values)
            {
                newvalues.Add(v.Key, v.Value);
            }
        }

        if (CurrentContext != null && IsPostBack && autoReloadFormFields)
        {
            string t = CurrentContext.Request.QueryString("t");
            string objname;
            object val;
            foreach (var f in formElement.Fields)
            {
                if (!IsEnable(f, state, newvalues) || !IsVisible(f, state, newvalues))
                {
                    if (!newvalues.Contains(f.Name))
                    {
                        val = Expression.GetDefaultValue(f, state, newvalues);
                        if (val != null)
                            newvalues.Add(f.Name, val);
                    }
                }

                objname = prefix + f.Name;
                val = f.ValidateRequest ? CurrentContext.Request.Form(objname) : CurrentContext.Request.GetUnvalidated(objname);

                switch (f.Component)
                {
                    case FormComponent.Search:
                        {
                            var search = (JJSearchBox)GetField(f, state, null, newvalues);
                            search.AutoReloadFormFields = true;
                            val = search.SelectedValue;
                            break;
                        }
                    case FormComponent.Lookup:
                        {
                            var lookup = (JJLookup)GetField(f, state, null, newvalues);
                            lookup.AutoReloadFormFields = true;
                            val = lookup.SelectedValue;
                            break;
                        }
                    case FormComponent.Number:
                    case FormComponent.Currency:
                        {
                            //Quando o post é executado via ajax com a função serialize()
                            //Não sei porque é alterado a virgula decimal para ponto.
                            //WorkAround Serialize()
                            if (val != null && "reloadpainel".Equals(t) | "tablerow".Equals(t) | "ajax".Equals(t))
                            {
                                string sVal = val.ToString().Replace(" ", "").Replace(".", ",");
                                if (double.TryParse(sVal, out var nVal))
                                    val = nVal;
                                else
                                    val = 0;
                            }

                            break;
                        }
                    case FormComponent.CheckBox:
                        {
                            val ??= CurrentContext.Request.Form(objname + "_hidden") ?? "0";

                            break;
                        }
                }

                if (val != null)
                {
                    if (!newvalues.Contains(f.Name))
                        newvalues.Add(f.Name, val);
                    else
                        newvalues[f.Name] = val;
                }

                if (newvalues[f.Name] == null || string.IsNullOrWhiteSpace(newvalues[f.Name].ToString()))
                    newvalues.Remove(f.Name);
            }
        }

        var formManager = new FormManager(formElement, UserValues, DataAccess);
        return formManager.GetTriggerValues(newvalues, state, !IsPostBack);
    }

}
