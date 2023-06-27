using JJMasterData.Commons.Data.Entity;
using JJMasterData.Commons.Util;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataDictionary.Actions.Abstractions;
using JJMasterData.Core.Web.Components;
using JJMasterData.Core.Web.Factories;
using System;
using System.Collections;
using System.Globalization;
using System.Threading;
using JJMasterData.Commons.Data.Entity;
using JJMasterData.Commons.Exceptions;
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
    public ExpressionManager ExpressionManager { get; private set; }

    #endregion

    #region "Constructors"

    public FieldManager(FormElement formElement, ExpressionManager expression)
    {
        FormElement = formElement ?? throw new ArgumentNullException(nameof(formElement));
        ExpressionManager = expression ?? throw new ArgumentNullException(nameof(expression));
        Name = "jjpainel_" + formElement.Name.ToLower();
    }
    
    public FieldManager(string name, FormElement formElement, ExpressionManager expressionManager) : this(formElement, expressionManager)
    {
        Name = name;
    }

    #endregion

    public bool IsVisible(BasicAction action, PageState state, IDictionary formValues)
    {
        if (action == null)
            throw new ArgumentNullException(nameof(action), "action can not be null");

        return ExpressionManager.GetBoolValue(action.VisibleExpression, action.Name, state, formValues);
    }

    public bool IsVisible(FormElementField field, PageState state, IDictionary formValues)
    {
        if (field == null)
            throw new ArgumentNullException(nameof(field), "FormElementField can not be null");

        return ExpressionManager.GetBoolValue(field.VisibleExpression, field.Name, state, formValues);
    }
    

    public bool IsEnabled(FormElementField field, PageState state, IDictionary formValues)
    {
        if (state == PageState.View)
            return false;

        if (field == null)
            throw new ArgumentNullException(nameof(field), "FormElementField can not be null");

        return ExpressionManager.GetBoolValue(field.EnableExpression, field.Name, state, formValues);
    }

    /// <summary>
    /// Formata os valores exibidos na Grid
    /// </summary>
    public string ParseVal(FormElementField field, IDictionary values)
    {
        if (values == null)
            return string.Empty;

        if (field == null)
            throw new ArgumentNullException(nameof(field), "FormElementField can not be null");

        object value = null;
        if (values.Contains(field.Name))
            value = values[field.Name];

        if (value == null || value == DBNull.Value)
            return string.Empty;

        string stringValue = null;
        switch (field.Component)
        {
            case FormComponent.Slider:
                switch (field.DataType)
                {
                    case FieldType.Float:
                    {
                        if (double.TryParse(value.ToString(),NumberStyles.Any,CultureInfo.CurrentCulture, out var floatValue))
                            stringValue = floatValue.ToString("N" + field.NumberOfDecimalPlaces);
                        break;
                    }
                    case FieldType.Int:
                    {
                        if (int.TryParse(value.ToString(), out int intVal))
                            stringValue = intVal.ToString("N0");
                        break;
                    }
                    default:
                        throw new JJMasterDataException("Invalid FieldType for Slider component");
                }
                break;
            case FormComponent.ComboBox 
            when field.DataItem!.ReplaceTextOnGrid || field.DataItem.ShowImageLegend:
            {
                var comboBox = (JJComboBox)GetField(field, PageState.List, values, value);
                stringValue = comboBox.GetDescription() ?? value.ToString();
                break;
            }
            case FormComponent.Lookup 
                 when field.DataItem is { ReplaceTextOnGrid: true }:
            {
                var lookup = (JJLookup)GetField(field, PageState.List, values, value);
                stringValue = lookup.GetDescription() ?? value.ToString();
                break;
            }
            case FormComponent.CheckBox:
                stringValue = ExpressionManager.ParseBool(value) ? "Sim" : "Não";
                break;
            case FormComponent.Search 
                 when field.DataItem is { ReplaceTextOnGrid: true }:
            {
                var search = (JJSearchBox)GetField(field, PageState.List, values, value);
                search.AutoReloadFormFields = false;
                stringValue = search.GetDescription(value.ToString()) ?? value.ToString();
                break;
            }
            default:
                stringValue = FormatValue(field, value);
                break;
        }

        return stringValue ?? string.Empty;
    }


    /// <summary>
    /// Formata os valores exibidos no Panel
    /// </summary>
    public string FormatValue(FormElementField field, object value)
    {
        if (value == null)
            return string.Empty;

        if (field == null)
            throw new ArgumentNullException(nameof(field), "FormElementField can not be null");

        string stringValue = value.ToString();
        if (string.IsNullOrEmpty(stringValue))
            return string.Empty;

        FieldType type = field.DataType;
        switch (field.Component)
        {
            case FormComponent.Cnpj:
            case FormComponent.Cpf:
            case FormComponent.CnpjCpf:
                stringValue = Format.FormatCnpj_Cpf(stringValue);
                break;
            case FormComponent.Slider:
            case FormComponent.Number:

                switch (type)
                {
                    case FieldType.Float:
                    {
                        if (double.TryParse(stringValue, out double doubleValue))
                            stringValue = doubleValue.ToString("N" + field.NumberOfDecimalPlaces);
                        break;
                    }
                    case FieldType.Int when !field.IsPk:
                    {
                        if (int.TryParse(stringValue, out int intVal))
                            stringValue = intVal.ToString("N0");
                        break;
                    }
                }
                break;
            case FormComponent.Currency:
                if (double.TryParse(stringValue, out var currencyValue))
                {
                    var cultureInfo = CultureInfo.CurrentCulture;
                    var numberFormatInfo = (NumberFormatInfo)cultureInfo.NumberFormat.Clone();
                    stringValue = currencyValue.ToString("C" + field.NumberOfDecimalPlaces, numberFormatInfo);
                }
                    
                break;
            case FormComponent.Date:
            case FormComponent.DateTime:
            case FormComponent.Text:
                switch (type)
                {
                    case FieldType.Date:
                    {
                        var dVal = DateTime.Parse(stringValue);
                        stringValue = dVal == DateTime.MinValue ? "" : dVal.ToString(DateTimeFormatInfo.CurrentInfo.ShortDatePattern);
                        break;
                    }
                    case FieldType.DateTime or FieldType.DateTime2:
                    {
                        var dateValue = DateTime.Parse(stringValue);
                        stringValue = dateValue == DateTime.MinValue
                            ? ""
                            : dateValue.ToString(
                                $"{DateTimeFormatInfo.CurrentInfo.ShortDatePattern} " +
                                $"{DateTimeFormatInfo.CurrentInfo.ShortTimePattern}");
                        break;
                    }
                }

                break;
            case FormComponent.Tel:
                stringValue = Format.FormatTel(stringValue);
                break;
        }

        return stringValue;
    }

    public JJBaseControl GetField(FormElementField field, PageState pageState, IDictionary formValues, object value = null)
    {
        if (pageState == PageState.Filter && field.Filter.Type == FilterMode.Range)
        {
            return JJTextRange.GetInstance(field, formValues);
        }
        
        var expOptions = new ExpressionOptions(ExpressionManager.UserValues, formValues, pageState, ExpressionManager.EntityRepository);
        var controlFactory = new WebControlFactory(FormElement, expOptions, Name);
        var control = controlFactory.CreateControl(field, value);

        control.Enabled = IsEnabled(field, pageState, formValues);

        return control;
    }

    public bool IsRange(FormElementField field, PageState pageState)
    {
        return pageState == PageState.Filter & field.Filter.Type == FilterMode.Range;
    }
}
