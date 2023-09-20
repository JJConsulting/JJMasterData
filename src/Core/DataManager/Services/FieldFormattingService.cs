using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using JJMasterData.Commons.Data.Entity;
using JJMasterData.Commons.Exceptions;
using JJMasterData.Commons.Util;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataManager.Services.Abstractions;

namespace JJMasterData.Core.DataManager.Services;


public class FieldFormattingService : IFieldFormattingService
{
    private IDataItemService DataItemService { get; }
    private ILookupService LookupService { get; }

    public FieldFormattingService(IDataItemService dataItemService, ILookupService lookupService)
    {
        DataItemService = dataItemService;
        LookupService = lookupService;
    }

    public async Task<string> FormatGridValueAsync(FormElementField field, IDictionary<string, object> values, IDictionary<string, object> userValues)
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
                stringValue = GetNumericValueAsString(field, value);
                break;
            case FormComponent.Lookup
                 when field.DataItem is { ReplaceTextOnGrid: true }:
                var allowOnlyNumerics = field.DataType is FieldType.Int or FieldType.Float;
                var formData = new FormStateData(values, PageState.List);
                stringValue = await LookupService.GetDescriptionAsync(field.DataItem.ElementMap, formData, value.ToString(), allowOnlyNumerics);
                break;
            case FormComponent.CheckBox:
                stringValue = StringManager.ParseBool(value) ? "Sim" : "Não";
                break;
            case FormComponent.Search or FormComponent.ComboBox
                 when field.DataItem is { ReplaceTextOnGrid: true }:
                var searchFormData = new FormStateData(values, userValues, PageState.List);
                var searchBoxValues = DataItemService.GetValuesAsync(field.DataItem, searchFormData, null, null);
                return (await searchBoxValues.FirstOrDefaultAsync(v => v.Id == fieldValue?.ToString()))?.Description ?? string.Empty;
            default:
                stringValue = FormatValue(field, value);
                break;
        }

        return stringValue ?? string.Empty;
    }

    private static string GetNumericValueAsString(FormElementField field, object value)
    {
        string stringValue = null;
        if (field.DataType == FieldType.Float)
        {
            if (double.TryParse(value.ToString(), NumberStyles.Any, CultureInfo.CurrentCulture,
                    out var floatValue))
                stringValue = floatValue.ToString($"N{field.NumberOfDecimalPlaces}");
        }
        
        else if (field.DataType == FieldType.Int)
        {
            if (int.TryParse(value.ToString(), out var intVal))
                stringValue = intVal.ToString("0");
        }
        else
        {
            throw new JJMasterDataException("Invalid FieldType for numeric component");
        }

        return stringValue;
    }

    public string FormatValue(FormElementField field, object value)
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
                stringValue = Format.FormatCnpj_Cpf(stringValue);
                break;
            case FormComponent.Slider:
            case FormComponent.Number:

                switch (type)
                {
                    case FieldType.Float:
                        {
                            if (double.TryParse(stringValue, out double doubleValue))
                                stringValue = doubleValue.ToString($"N{field.NumberOfDecimalPlaces}");
                            break;
                        }
                    case FieldType.Int when !field.IsPk:
                        {
                            if (int.TryParse(stringValue, out int intVal))
                                stringValue = intVal.ToString();
                            break;
                        }
                }
                break;
            case FormComponent.Currency:
                if (double.TryParse(stringValue, out var currencyValue))
                {
                    var cultureInfo = CultureInfo.CurrentCulture;
                    var numberFormatInfo = (NumberFormatInfo)cultureInfo.NumberFormat.Clone();
                    stringValue = currencyValue.ToString($"C{field.NumberOfDecimalPlaces}", numberFormatInfo);
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