using JJMasterData.Commons.Util;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataDictionary.Action;
using System;
using System.Collections;
using System.Globalization;
using System.Threading;
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

        string valueString;
        switch (field.Component)
        {
            case FormComponent.ComboBox 
            when field.DataItem!.ReplaceTextOnGrid || field.DataItem.ShowImageLegend:
            {
                var comboBox = (JJComboBox)GetField(field, PageState.List, values, value);
                valueString = comboBox.GetDescription() ?? value.ToString();
                break;
            }
            case FormComponent.Lookup 
                 when field.DataItem is { ReplaceTextOnGrid: true }:
            {
                var lookup = (JJLookup)GetField(field, PageState.List, values, value);
                valueString = lookup.GetDescription() ?? value.ToString();
                break;
            }
            case FormComponent.CheckBox:
                valueString = ExpressionManager.ParseBool(value) ? "Sim" : "Não";
                break;
            case FormComponent.Search 
                 when field.DataItem is { ReplaceTextOnGrid: true }:
            {
                var search = (JJSearchBox)GetField(field, PageState.List, values, value);
                search.AutoReloadFormFields = false;
                valueString = search.GetDescription(value.ToString()) ?? value.ToString();
                break;
            }
            default:
                valueString = FormatValue(field, value);
                break;
        }

        return valueString ?? "";
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

        string valueString = value.ToString();
        if (string.IsNullOrEmpty(valueString))
            return string.Empty;

        FieldType type = field.DataType;
        switch (field.Component)
        {
            case FormComponent.Cnpj:
            case FormComponent.Cpf:
            case FormComponent.CnpjCpf:
                valueString = Format.FormatCnpj_Cpf(valueString);
                break;
            case FormComponent.Number:

                switch (type)
                {
                    case FieldType.Float:
                    {
                        if (double.TryParse(valueString, out double nVal))
                            valueString = nVal.ToString("N" + field.NumberOfDecimalPlaces);
                        break;
                    }
                    case FieldType.Int when !field.IsPk:
                    {
                        if (int.TryParse(valueString, out int intVal))
                            valueString = intVal.ToString("N0");
                        break;
                    }
                }
                break;
            case FormComponent.Currency:
                if (double.TryParse(valueString, out var currencyValue))
                {
                    var cultureInfo = CultureInfo.CurrentCulture;
                    var numberFormatInfo = (NumberFormatInfo)cultureInfo.NumberFormat.Clone();
                    valueString = currencyValue.ToString("C" + field.NumberOfDecimalPlaces, numberFormatInfo);
                }
                    
                break;
            case FormComponent.Date:
            case FormComponent.DateTime:
            case FormComponent.Text:
                switch (type)
                {
                    case FieldType.Date:
                    {
                        var dVal = DateTime.Parse(valueString);
                        valueString = dVal == DateTime.MinValue ? "" : dVal.ToString(DateTimeFormatInfo.CurrentInfo.ShortDatePattern);
                        break;
                    }
                    case FieldType.DateTime or FieldType.DateTime2:
                    {
                        var dateValue = DateTime.Parse(valueString);
                        valueString = dateValue == DateTime.MinValue
                            ? ""
                            : dateValue.ToString(
                                $"{DateTimeFormatInfo.CurrentInfo.ShortDatePattern} " +
                                $"{DateTimeFormatInfo.CurrentInfo.ShortTimePattern}");
                        break;
                    }
                }

                break;
            case FormComponent.Tel:
                valueString = Format.FormatTel(valueString);
                break;
        }

        return valueString;
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
