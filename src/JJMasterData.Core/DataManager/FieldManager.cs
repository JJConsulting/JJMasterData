using JJMasterData.Commons.Util;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataDictionary.Action;
using System;
using System.Collections;
using System.Globalization;
using JJMasterData.Commons.Data.Entity;
using JJMasterData.Core.Web.Components;
using JJMasterData.Core.Web.Factories;

namespace JJMasterData.Core.DataManager;

public class FieldManager
{
    #region "Properties"

    private string Name { get; set; }
    
    /// <summary>
    /// Configurações pré-definidas do formulário
    /// </summary>
    public FormElement FormElement { get; set; }

    /// <summary>
    /// Objeto responsável por parsear expressoões
    /// </summary>
    public ExpressionManager Expression { get; private set; }

    #endregion

    #region "Constructors"

    public FieldManager(FormElement formElement, ExpressionManager expression)
    {
        FormElement = formElement ?? throw new ArgumentNullException(nameof(formElement));
        Expression = expression ?? throw new ArgumentNullException(nameof(expression));
        Name = "jjpainel_" + formElement.Name.ToLower();
    }

    #endregion

    public bool IsVisible(BasicAction action, PageState state, Hashtable formValues)
    {
        if (action == null)
            throw new ArgumentNullException(nameof(action), "action can not be null");

        return Expression.GetBoolValue(action.VisibleExpression, action.Name, state, formValues);
    }


    public bool IsVisible(FormElementField field, PageState state, Hashtable formValues)
    {
        if (field == null)
            throw new ArgumentNullException(nameof(field), "FormElementField can not be null");

        return Expression.GetBoolValue(field.VisibleExpression, field.Name, state, formValues);
    }

    public bool IsEnable(FormElementField field, PageState state, Hashtable formValues)
    {
        if (state == PageState.View)
            return false;

        if (field == null)
            throw new ArgumentNullException(nameof(field), "FormElementField can not be null");

        return Expression.GetBoolValue(field.EnableExpression, field.Name, state, formValues);
    }

    /// <summary>
    /// Formata os valores exibidos na Grid
    /// </summary>
    public string ParseVal(FormElementField field, Hashtable values)
    {
        if (values == null)
            return "";

        if (field == null)
            throw new ArgumentNullException(nameof(field), "FormElementField can not be null");

        object value = null;
        if (values.Contains(field.Name))
            value = values[field.Name];

        if (value == null)
            return "";

        string sVal;
        if (field.Component == FormComponent.ComboBox
            && field.DataItem != null
            && (field.DataItem.ReplaceTextOnGrid || field.DataItem.ShowImageLegend))
        {
            var cbo = (JJComboBox)GetField(field, PageState.List, values, value);
            sVal = cbo.GetDescription() ?? value.ToString();
        }
        else if (field.Component == FormComponent.Lookup
                 && field.DataItem is { ReplaceTextOnGrid: true })
        {
            var lookup = (JJLookup)GetField(field, PageState.List, values, value);
            sVal = lookup.GetDescription() ?? value.ToString();
        }
        else if (field.Component == FormComponent.CheckBox)
        {
            sVal = ExpressionManager.ParseBool(value) ? "Sim" : "Não";
        }
        else if (field.Component == FormComponent.Search
                 && field.DataItem is { ReplaceTextOnGrid: true })
        {
            var search = (JJSearchBox)GetField(field, PageState.List, values, value);
            search.AutoReloadFormFields = false;
            sVal = search.GetDescription(value.ToString()) ?? value.ToString();
        }
        else
        {
            sVal = FormatValue(field, value);
        }

        return sVal ?? "";
    }


    /// <summary>
    /// Formata os valores exibidos no Panel
    /// </summary>
    public string FormatValue(FormElementField field, object value)
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
                if (double.TryParse(sVal, out var nCurrency))
                    sVal = nCurrency.ToString("C" + field.NumberOfDecimalPlaces);
                break;
            case FormComponent.Date:
            case FormComponent.DateTime:
            case FormComponent.Text:
                if (type == FieldType.Date)
                {
                    var dVal = DateTime.Parse(sVal);
                    sVal = dVal == DateTime.MinValue ? "" : dVal.ToString(DateTimeFormatInfo.CurrentInfo.ShortDatePattern);
                }
                else if (type is FieldType.DateTime or FieldType.DateTime2)
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

    public JJBaseControl GetField(FormElementField f, PageState pageState, Hashtable formValues, object value = null)
    {
        if (pageState == PageState.Filter & f.Filter.Type == FilterMode.Range)
        {
            return JJTextRange.GetInstance(f, formValues);
        }
        
        var expOptions = new ExpressionOptions(Expression.UserValues, formValues, pageState, Expression.EntityRepository);
        var controlFactory = new WebControlFactory(FormElement, expOptions, Name);

        return controlFactory.CreateControl(f, value);
    }

    public bool IsRange(FormElementField field, PageState pageState)
    {
        return pageState == PageState.Filter & field.Filter.Type == FilterMode.Range;
    }
}
