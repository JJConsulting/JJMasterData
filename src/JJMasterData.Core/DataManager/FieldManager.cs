using JJMasterData.Commons.Dao.Entity;
using JJMasterData.Commons.Util;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataDictionary.Action;
using JJMasterData.Core.WebComponents;
using System;
using System.Collections;
using System.Globalization;
using JJMasterData.Core.DataDictionary.Repository;
using JJMasterData.Core.Facades;
using JJMasterData.Core.Http.Abstractions;
using JJMasterData.Core.WebComponents.Factories;

namespace JJMasterData.Core.DataManager;

public class FieldManager
{


    #region "Properties"

    private string Name { get; set; }
    
    private readonly RepositoryServicesFacade _repositoryServicesFacade;
    private readonly CoreServicesFacade _coreServicesFacade;
    

    /// <summary>
    /// Configurações pré-definidas do formulário
    /// </summary>
    public FormElement FormElement { get; set; }

    public IDataDictionaryRepository DataDictionaryRepository { get; }

    /// <summary>
    /// Objeto responsável por parsear expressoões
    /// </summary>
    public ExpressionManager Expression { get; private set; }
    
    internal IHttpContext HttpContext { get; }


    #endregion

    #region "Constructors"

    public FieldManager(
        FormElement formElement, 
        IHttpContext httpContext,
        RepositoryServicesFacade repositoryServicesFacade,
        CoreServicesFacade coreServicesFacade,
        ExpressionManager expression)
    {
        FormElement = formElement;
        DataDictionaryRepository = repositoryServicesFacade.DataDictionaryRepository;
        _repositoryServicesFacade = repositoryServicesFacade;
        _coreServicesFacade = coreServicesFacade;
        Expression = expression ?? throw new ArgumentNullException(nameof(expression));
        HttpContext = httpContext;
        Name = "jjpainel_" + (formElement?.Name?.ToLower() ?? "1");
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
        switch (field.Component)
        {
            case FormComponent.ComboBox 
            when field.DataItem != null 
                 && (field.DataItem.ReplaceTextOnGrid || field.DataItem.ShowImageLegend):
            {
                var cbo = (JJComboBox)GetField(field, PageState.List, values, value);
                sVal = cbo.GetDescription() ?? value.ToString();
                break;
            }
            case FormComponent.Lookup 
                 when field.DataItem is { ReplaceTextOnGrid: true }:
            {
                var lookup = (JJLookup)GetField(field, PageState.List, values, value);
                sVal = lookup.GetDescription() ?? value.ToString();
                break;
            }
            case FormComponent.CheckBox:
                sVal = ExpressionManager.ParseBool(value) ? "Sim" : "Não";
                break;
            case FormComponent.Search 
                 when field.DataItem is { ReplaceTextOnGrid: true }:
            {
                var search = (JJSearchBox)GetField(field, PageState.List, values, value);
                search.AutoReloadFormFields = false;
                sVal = search.GetDescription(value.ToString()) ?? value.ToString();
                break;
            }
            default:
                sVal = FormatValue(field, value);
                break;
        }

        return sVal ?? "";
    }


    /// <summary>
    /// Format the values for exibition.
    /// </summary>
    public static string FormatValue(FormElementField field, object value)
    {
        if (value == null)
            return "";

        if (field == null)
            throw new ArgumentNullException(nameof(field), "FormElementField can not be null");

        string valueResult = value.ToString();
        if (string.IsNullOrEmpty(valueResult))
            return string.Empty;

        var type = field.DataType;
        switch (field.Component)
        {
            case FormComponent.Cnpj:
            case FormComponent.Cpf:
            case FormComponent.CnpjCpf:
                valueResult = Format.FormatCnpj_Cpf(valueResult);
                break;
            case FormComponent.Number:

                switch (type)
                {
                    case FieldType.Float:
                    {
                        if (double.TryParse(valueResult, out double nVal))
                            valueResult = nVal.ToString("N" + field.NumberOfDecimalPlaces);
                        break;
                    }
                    case FieldType.Int when !field.IsPk:
                    {
                        if (int.TryParse(valueResult, out int intVal))
                            valueResult = intVal.ToString("N0");
                        break;
                    }
                }
                break;
            case FormComponent.Currency:
                if (double.TryParse(valueResult, out var nCurrency))
                    valueResult = nCurrency.ToString("C" + field.NumberOfDecimalPlaces);
                break;
            case FormComponent.Date:
            case FormComponent.DateTime:
            case FormComponent.Text:
                switch (type)
                {
                    case FieldType.Date:
                    {
                        var dateValue = DateTime.Parse(valueResult);
                        valueResult = dateValue == DateTime.MinValue ? "" : dateValue.ToString(DateTimeFormatInfo.CurrentInfo.ShortDatePattern);
                        break;
                    }
                    case FieldType.DateTime or FieldType.DateTime2:
                    {
                        var dateValue = DateTime.Parse(valueResult);
                        valueResult = dateValue == DateTime.MinValue
                            ? ""
                            : dateValue.ToString(
                                $"{DateTimeFormatInfo.CurrentInfo.ShortDatePattern} " +
                                $"{DateTimeFormatInfo.CurrentInfo.ShortTimePattern}");
                        break;
                    }
                }

                break;
            case FormComponent.Tel:
                valueResult = Format.FormatTel(valueResult);
                break;
        }

        return valueResult;
    }

    public JJBaseControl GetField(FormElementField f, PageState pageState, Hashtable formValues, object value = null)
    {
        if (pageState == PageState.Filter & f.Filter.Type == FilterMode.Range)
        {
            return JJTextRange.GetInstance(f, formValues, HttpContext);
        }
        
        var expOptions = new ExpressionOptions(Expression.UserValues, formValues, pageState, Expression.EntityRepository);
        var controlFactory = new FormElementControlFactory(FormElement, HttpContext, _repositoryServicesFacade,_coreServicesFacade, Expression, expOptions, Name);

        return controlFactory.CreateControl(f, value);
    }


    public static bool IsRange(FormElementField field, PageState pageState)
    {
        return pageState == PageState.Filter & field.Filter.Type == FilterMode.Range;
    }
}
