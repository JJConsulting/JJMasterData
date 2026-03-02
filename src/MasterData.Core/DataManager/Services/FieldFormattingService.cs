using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Threading.Tasks;
using System.Web;
using JJMasterData.Commons.Data.Entity.Models;
using JJMasterData.Commons.Exceptions;
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
                stringValue = GetNumericValueAsString(field, value, $"N{field.NumberOfDecimalPlaces}");
                if (!string.IsNullOrEmpty(stringValue))
                {
                    stringValue += "%";
                }
                
                break;
            case FormComponent.Number:
            case FormComponent.Slider:
                stringValue = GetNumericValueAsString(field, value,$"N{field.NumberOfDecimalPlaces}");
                break;
            case FormComponent.Currency:
                stringValue = GetNumericValueAsString(field, value,$"C{field.NumberOfDecimalPlaces}");
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
                return await dataItemService.GetDescriptionAsync(fieldSelector.FormElement, field, formStateData, value);
            default:
                stringValue = FormatValue(field, value);
                break;
        }
        
        if (field.EncodeHtml)
            stringValue = HttpUtility.HtmlEncode(stringValue);
        
        return stringValue ?? string.Empty;
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
            case FormComponent.Slider:
            case FormComponent.Number:
                switch (type)
                {
                    case FieldType.Int when !field.IsPk:
                    case FieldType.Float:
                    {
                        return GetNumericValueAsString(field, value, $"N{field.NumberOfDecimalPlaces}");
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

    private static string GetNumericValueAsString(FormElementField field, object value, [StringSyntax("NumericFormat")] string decimalFormat)
    {
        CultureInfo cultureInfo;
        if (field.Attributes.TryGetValue(FormElementField.CultureInfoAttribute, out var cultureInfoName)
            && !string.IsNullOrEmpty(cultureInfoName?.ToString()))
            cultureInfo = CultureInfo.GetCultureInfo(cultureInfoName.ToString()!);
        else
            cultureInfo = CultureInfo.CurrentUICulture;
        
        string stringValue = null;
        switch (field.DataType)
        {
            case FieldType.Float:
            {
                if (value is double doubleValue || double.TryParse(value.ToString(), out doubleValue))
                    stringValue = doubleValue.ToString(decimalFormat, cultureInfo);
                break;
            }
            case FieldType.Int:
            {
                if (value is int intValue || int.TryParse(value.ToString(), out intValue))
                    stringValue = intValue.ToString("0", cultureInfo);
                break;
            }
            case FieldType.Decimal:
            {
                if (value is decimal decimalValue || decimal.TryParse(value.ToString(), out decimalValue))
                    stringValue = decimalValue.ToString(decimalFormat, cultureInfo);
                break;
            }
            default:
                throw new JJMasterDataException($"Invalid FieldType for numeric component [{field.Name}]");
        }

        return stringValue;
    }
}