using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using JJMasterData.Commons.Data.Entity.Models;
using JJMasterData.Commons.Exceptions;
using JJMasterData.Commons.Util;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.DataManager.Models;

namespace JJMasterData.Core.DataManager.Services;


public class FieldFormattingService(DataItemService dataItemService, LookupService lookupService)
{
    private DataItemService DataItemService { get; } = dataItemService;
    private LookupService LookupService { get; } = lookupService;

    public async Task<string> FormatGridValueAsync(FormElementField field, Dictionary<string, object> values, Dictionary<string, object> userValues)
    {
        object fieldValue = null;
        if (values.TryGetValue(field.Name, out var value))
            fieldValue = value;

        if (fieldValue == null || fieldValue == DBNull.Value)
            return string.Empty;

        if (field == null)
            throw new ArgumentNullException(nameof(field), "FormElementField cannot be null");

        string stringValue;
        switch (field.Component)
        {
            case FormComponent.Number:
            case FormComponent.Slider:
                stringValue = GetCurrencyValueAsString(field, value);
                break;
            case FormComponent.Currency:
                CultureInfo cultureInfo;
                if (field.Attributes.TryGetValue(FormElementField.CultureInfoAttribute, out var cultureInfoName) && !string.IsNullOrEmpty(cultureInfoName?.ToString()))
                    cultureInfo = CultureInfo.GetCultureInfo(cultureInfoName.ToString());
                else
                    cultureInfo = CultureInfo.CurrentUICulture;
                if (float.TryParse(value?.ToString(),NumberStyles.Currency,cultureInfo, out var currencyValue))
                {
                    stringValue = currencyValue.ToString($"C{field.NumberOfDecimalPlaces}", cultureInfo);
                }
                else
                    stringValue = null;
                break;
            case FormComponent.Lookup
                 when field.DataItem is { GridBehavior: not DataItemGridBehavior.Id}:
                var allowOnlyNumerics = field.DataType is FieldType.Int or FieldType.Float;
                var formData = new FormStateData(values, PageState.List);
                stringValue = await LookupService.GetDescriptionAsync(field.DataItem.ElementMap, formData, value.ToString(), allowOnlyNumerics);
                break;
            case FormComponent.CheckBox:
                stringValue = StringManager.ParseBool(value) ? "Sim" : "Não";
                break;
            case FormComponent.Search or FormComponent.ComboBox or FormComponent.RadioButtonGroup
                 when field.DataItem is { GridBehavior: not DataItemGridBehavior.Id }:
                var searchFormData = new FormStateData(values, userValues, PageState.List);

                values.TryGetValue(field.Name, out var searchId);
                
                var searchBoxValues = await DataItemService.GetValuesAsync(field.DataItem, searchFormData, null, searchId?.ToString());

                var rowValue = searchBoxValues.FirstOrDefault(v => v.Id == fieldValue?.ToString());
                
                return rowValue?.Description ?? rowValue?.Id ?? string.Empty;
            default:
                stringValue = FormatValue(field, value);
                break;
        }

        return stringValue ?? string.Empty;
    }

    private static string GetCurrencyValueAsString(FormElementField field, object value)
    {
        CultureInfo cultureInfo;
        if (field.Attributes.TryGetValue(FormElementField.CultureInfoAttribute, out var cultureInfoName) && !string.IsNullOrEmpty(cultureInfoName?.ToString()))
            cultureInfo = CultureInfo.GetCultureInfo(cultureInfoName.ToString());
        else
            cultureInfo = CultureInfo.CurrentUICulture;

        string stringValue = null;
        if (field.DataType == FieldType.Float)
        {
            if (float.TryParse(value.ToString(), NumberStyles.Currency, cultureInfo,
                    out var floatValue))
                stringValue = floatValue.ToString($"N{field.NumberOfDecimalPlaces}", cultureInfo);
        }
        
        else if (field.DataType == FieldType.Int)
        {
            if (int.TryParse(value.ToString(), NumberStyles.Currency, cultureInfo,out var intVal))
                stringValue = intVal.ToString("0", cultureInfo);
        }
        else
        {
            throw new JJMasterDataException("Invalid FieldType for numeric component");
        }

        return stringValue;
    }

    public static string FormatValue(FormElementField field, object value)
    {
        if (value == null)
            return string.Empty;

        var stringValue = value.ToString()!;
        if (string.IsNullOrEmpty(stringValue))
            return string.Empty;

        var type = field.DataType;
        switch (field.Component)
        {
            case FormComponent.Cnpj:
            case FormComponent.Cpf:
            case FormComponent.CnpjCpf:
                stringValue = Format.FormatCnpjCpf(stringValue);
                break;
            case FormComponent.Currency:
                switch (type)
                {
                    case FieldType.Int when !field.IsPk:
                    case FieldType.Float:
                    {
                        return GetCurrencyValueAsString(field, value);
                    }
                }
                break;
            case FormComponent.Slider:
            case FormComponent.Number:
                switch (type)
                {
                    case FieldType.Int when !field.IsPk:
                    case FieldType.Float:
                    {
                        return GetCurrencyValueAsString(field, value);
                    }
                }
                break;
            case FormComponent.Hour:
                if (TimeSpan.TryParse(stringValue, out var timeSpan))
                    stringValue = timeSpan.ToString(@"hh\:mm");
                break;
            case FormComponent.Date:
            case FormComponent.DateTime:
            case FormComponent.Text:
                switch (type)
                {
                    case FieldType.Date:
                        {
                            var dVal = DateTime.Parse(stringValue);
                            stringValue = dVal == DateTime.MinValue ? string.Empty : dVal.ToString(DateTimeFormatInfo.CurrentInfo.ShortDatePattern);
                            break;
                        }
                    case FieldType.DateTime or FieldType.DateTime2:
                        {
                            var dateValue = DateTime.Parse(stringValue);
                            stringValue = dateValue == DateTime.MinValue
                                ? string.Empty
                                : dateValue.ToString(
                                    $"{DateTimeFormatInfo.CurrentInfo.ShortDatePattern} {DateTimeFormatInfo.CurrentInfo.ShortTimePattern}");
                            break;
                        }
                }

                break;
            case FormComponent.Tel:
                stringValue = Format.FormatPhone(stringValue);
                break;
        }

        return stringValue;
    }
}