using System.Globalization;
using JJMasterData.BlazorClient.Services.Abstractions;
using JJMasterData.Commons.Data.Entity;
using JJMasterData.Commons.Util;
using JJMasterData.Core.DataDictionary;

namespace JJMasterData.BlazorClient.Services;

public class FieldFormattingService : IFieldFormattingService
{
    private ISearchBoxService SearchBoxService { get; }

    public FieldFormattingService(ISearchBoxService searchBoxService)
    {
        SearchBoxService = searchBoxService;
    }
    
    public async Task<string> FormatGridValue(FormElementField field, IDictionary<string,dynamic?> values)
    {
        object? fieldValue = null;
        if (values.TryGetValue(field.Name, out var value))
            fieldValue = value;

        if (fieldValue == null || fieldValue == DBNull.Value)
            return string.Empty;

        switch (field.Component)
        {
            case FormComponent.Search 
                when field.DataItem is { ReplaceTextOnGrid: true }:
            {
                var searchBoxValues = await SearchBoxService.GetValues(field.DataItem, null, new SearchBoxContext(null, values,null, PageState.List));

                return searchBoxValues.FirstOrDefault(v => v.Id == fieldValue?.ToString())?.Description ?? string.Empty;
            }
            case FormComponent.ComboBox 
                when field.DataItem!.ReplaceTextOnGrid || field.DataItem.ShowImageLegend:
            {
                return "Not implemented";
            }
            case FormComponent.Lookup 
                when field.DataItem is { ReplaceTextOnGrid: true }:
            {
                return "Not implemented";
            }
            case FormComponent.CheckBox:
                return "Not implemented";
            default:
                return FormatValue(field, value);
       
        }
    }
    
    private static string FormatValue(FormElementField field, object? value)
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
}