using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using JJMasterData.Commons.Data.Entity.Models;
using JJMasterData.Commons.Exceptions;
using JJMasterData.Commons.Util;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.DataManager.Models;
using JJMasterData.Core.UI.Html;

namespace JJMasterData.Core.DataManager.Services;

public class FieldFormattingService(DataItemService dataItemService, LookupService lookupService)
{
    public async ValueTask<string> FormatGridValueAsync(
        FormElementFieldSelector fieldSelector,
        FormStateData formStateData)
    {
        var field = fieldSelector.Field;

        formStateData.Values.TryGetValue(field.Name, out var value);

        if (value == null || value == DBNull.Value)
            return string.Empty;

        if (field.EncodeHtml)
            value = HttpUtility.HtmlEncode(value);

        string stringValue;
        switch (field.Component)
        {
            case FormComponent.Number:
            case FormComponent.Slider:
                stringValue = GetNumericValueAsString(field, value);
                break;
            case FormComponent.Currency:
                CultureInfo cultureInfo;
                if (field.Attributes.TryGetValue(FormElementField.CultureInfoAttribute, out var cultureInfoName)
                    && !string.IsNullOrEmpty(cultureInfoName?.ToString()))
                    cultureInfo = CultureInfo.GetCultureInfo(cultureInfoName.ToString());
                else
                    cultureInfo = CultureInfo.CurrentUICulture;

                if (double.TryParse(value?.ToString(), NumberStyles.Currency, cultureInfo, out var currencyValue))
                    stringValue = currencyValue.ToString($"C{field.NumberOfDecimalPlaces}", cultureInfo);
                else
                    stringValue = null;
                break;
            case FormComponent.Lookup
                when field.DataItem is { GridBehavior: not DataItemGridBehavior.Id }:
                var allowOnlyNumerics = field.DataType is FieldType.Int or FieldType.Float;
                stringValue = await lookupService.GetDescriptionAsync(field.DataItem.ElementMap!, formStateData,
                    value.ToString(), allowOnlyNumerics);
                break;
            case FormComponent.CheckBox:
                stringValue = StringManager.ParseBool(value) ? "Sim" : "Não";
                break;
            case FormComponent.Search or FormComponent.ComboBox or FormComponent.RadioButtonGroup
                when field.DataItem is { GridBehavior: not DataItemGridBehavior.Id }:

                var dataQuery = new DataQuery(formStateData, fieldSelector.FormElement.ConnectionId)
                {
                    SearchId = value?.ToString()
                };

                var searchBoxValues = await dataItemService.GetValuesAsync(field.DataItem, dataQuery);
                var rowValue = searchBoxValues.FirstOrDefault(v =>
                    string.Equals(v.Id, value?.ToString()!.Trim(), StringComparison.InvariantCultureIgnoreCase));
                return rowValue?.Description ?? rowValue?.Id ?? string.Empty;
            case FormComponent.Email:
                stringValue = GetEmailLink(value?.ToString());
                break;
            default:
                stringValue = FormatValue(field, value);
                break;
        }

        return stringValue ?? string.Empty;
    }

    private static string GetEmailLink(string value)
    {
        if (string.IsNullOrEmpty(value))
            return string.Empty;

        var a = new A();
        a.WithAttribute("href", $"mailto:{value}");
        a.AppendText(value);

        return a.ToString();
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
            if (double.TryParse(value.ToString(), out var doubleValue))
                stringValue = doubleValue.ToString($"N{field.NumberOfDecimalPlaces}");
        }
        else if (field.DataType == FieldType.Int)
        {
            if (int.TryParse(value.ToString(), out var intVal))
                stringValue = intVal.ToString("0");
        }
        else
        {
            throw new JJMasterDataException($"Invalid FieldType for numeric component [{field.Name}]");
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
                            stringValue =
                                dateValue.ToString(
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