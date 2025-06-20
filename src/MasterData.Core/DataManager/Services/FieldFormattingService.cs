using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using JJMasterData.Commons.Data.Entity.Models;
using JJMasterData.Commons.Exceptions;
using JJMasterData.Commons.Localization;
using JJMasterData.Commons.Util;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.DataManager.Models;
using Microsoft.Extensions.Localization;

namespace JJMasterData.Core.DataManager.Services;

public class FieldFormattingService(
    DataItemService dataItemService,
    LookupService lookupService,
    IStringLocalizer<MasterDataResources> stringLocalizer
    )
{
    public async ValueTask<string> FormatGridValueAsync(
        FormElementFieldSelector fieldSelector,
        FormStateData formStateData)
    {
        var field = fieldSelector.Field;

        formStateData.Values.TryGetValue(field.Name, out var value);

        if (value == null || value == DBNull.Value)
            return string.Empty;
        
        string stringValue;
        switch (field.Component)
        {
            case FormComponent.Percentage:
                stringValue = GetNumericValueAsString(field, value);
                if (!string.IsNullOrEmpty(stringValue))
                {
                    stringValue += "%";
                }
                
                break;
            case FormComponent.Number:
            case FormComponent.Slider:
                stringValue = GetNumericValueAsString(field, value);
                break;
            case FormComponent.Currency:
                CultureInfo cultureInfo;
                if (field.Attributes.TryGetValue(FormElementField.CultureInfoAttribute, out var cultureInfoName)
                    && !string.IsNullOrEmpty(cultureInfoName?.ToString()))
                    cultureInfo = CultureInfo.GetCultureInfo(cultureInfoName.ToString()!);
                else
                    cultureInfo = CultureInfo.CurrentUICulture;

                if (value is double doubleValue || double.TryParse(value.ToString(), NumberStyles.Currency, cultureInfo, out doubleValue))
                    stringValue = doubleValue.ToString($"C{field.NumberOfDecimalPlaces}", cultureInfo);
                else if (value is decimal decimalValue || decimal.TryParse(value.ToString(), NumberStyles.Currency, cultureInfo, out decimalValue))
                    stringValue = decimalValue.ToString($"C{field.NumberOfDecimalPlaces}", cultureInfo);
                else
                    stringValue = null;
                break;
            case FormComponent.Lookup
                when field.DataItem is { GridBehavior: not DataItemGridBehavior.Id }:
                var allowOnlyNumerics = field.DataType is FieldType.Int or FieldType.Float or FieldType.Decimal;
                stringValue = await lookupService.GetDescriptionAsync(field.DataItem.ElementMap!, formStateData,
                    value.ToString(), allowOnlyNumerics);
                break;
            case FormComponent.CheckBox:
                stringValue = StringManager.ParseBool(value) ? stringLocalizer["Yes"] : stringLocalizer["No"];
                break;
            case FormComponent.Search or FormComponent.ComboBox or FormComponent.RadioButtonGroup
                when field.DataItem is { GridBehavior: not DataItemGridBehavior.Id }:

                var searchId = value.ToString()?.Trim();

                var dataQuery = new DataQuery(formStateData, fieldSelector.FormElement.ConnectionId)
                {
                    SearchId = searchId
                };

                var searchBoxValues = await dataItemService.GetValuesAsync(field.DataItem, dataQuery);

                if (field.DataItem.EnableMultiSelect)
                {
                    var searchIds = searchId.Split(',').Select(id => id.Trim());
                    var rowValues = searchBoxValues
                        .Where(v => searchIds.Contains(v.Id.Trim(), StringComparer.InvariantCultureIgnoreCase))
                        .Select(v => v.Description ?? v.Id);
                    
                    return string.Join(", ", rowValues);
                }

                
                var rowValue = searchBoxValues.Find(v =>
                    string.Equals(v.Id.Trim(), searchId, StringComparison.InvariantCultureIgnoreCase));
                return rowValue?.Description ?? rowValue?.Id ?? string.Empty;
            default:
                stringValue = FormatValue(field, value);
                break;
        }
        
        if (field.EncodeHtml)
            stringValue = HttpUtility.HtmlEncode(stringValue);
        
        return stringValue ?? string.Empty;
    }
    

    private static string GetCurrencyValueAsString(FormElementField field, object value)
    {
        CultureInfo cultureInfo;
        if (field.Attributes.TryGetValue(FormElementField.CultureInfoAttribute, out var cultureInfoName) &&
            !string.IsNullOrEmpty(cultureInfoName?.ToString()))
            cultureInfo = CultureInfo.GetCultureInfo(cultureInfoName.ToString());
        else
            cultureInfo = CultureInfo.CurrentUICulture;

        string stringValue = null;
        if (field.DataType == FieldType.Float)
        {
            if (double.TryParse(value.ToString(), NumberStyles.Currency, cultureInfo, out var floatValue))
                stringValue = floatValue.ToString($"N{field.NumberOfDecimalPlaces}", cultureInfo);
        }
        else if (field.DataType == FieldType.Int)
        {
            if (int.TryParse(value.ToString(), NumberStyles.Currency, cultureInfo, out var intVal))
                stringValue = intVal.ToString("0", cultureInfo);
        }
        else
        {
            throw new JJMasterDataException($"Invalid FieldType for currency component [{field.Name}]");
        }

        return stringValue;
    }

    private static string GetNumericValueAsString(FormElementField field, object value)
    {
        string stringValue = null;
        if (field.DataType == FieldType.Float)
        {
            if (value is double doubleValue || double.TryParse(value.ToString(), out doubleValue))
                stringValue = doubleValue.ToString($"N{field.NumberOfDecimalPlaces}");
        }
        else if (field.DataType == FieldType.Int)
        {
            if (value is int intValue || int.TryParse(value.ToString(), out intValue))
                stringValue = intValue.ToString("0");
        }
        else if (field.DataType == FieldType.Decimal)
        {
            if (value is decimal decimalValue || decimal.TryParse(value.ToString(), out decimalValue))
                stringValue = decimalValue.ToString($"N{field.NumberOfDecimalPlaces}");
        }
        else
        {
            throw new JJMasterDataException($"Invalid FieldType for numeric component [{field.Name}]");
        }

        return stringValue;
    }

    public static string FormatValue(FormElementField field, object value)
    {
        var stringValue = value?.ToString();
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
                        return GetNumericValueAsString(field, value);
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
                        if (DateTime.TryParse(stringValue, out var dateValue))
                            stringValue = dateValue.ToString(DateTimeFormatInfo.CurrentInfo.ShortDatePattern);
                        break;
                    }
                    case FieldType.DateTime or FieldType.DateTime2:
                    {
                        if (DateTime.TryParse(stringValue, out var dateValue))
                        {
                            stringValue =
                                dateValue.ToString(
                                    $"{DateTimeFormatInfo.CurrentInfo.ShortDatePattern} {DateTimeFormatInfo.CurrentInfo.ShortTimePattern}");
                        }

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