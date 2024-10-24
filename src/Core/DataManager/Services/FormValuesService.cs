#nullable enable

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using JJMasterData.Commons.Data.Entity.Models;
using JJMasterData.Commons.Data.Entity.Repository.Abstractions;
using JJMasterData.Commons.Security.Cryptography.Abstractions;
using JJMasterData.Commons.Util;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.DataManager.Exceptions;
using JJMasterData.Core.DataManager.Models;
using JJMasterData.Core.Extensions;
using JJMasterData.Core.Http.Abstractions;
using Microsoft.Extensions.Logging;

namespace JJMasterData.Core.DataManager.Services;

public class FormValuesService(
    IEntityRepository entityRepository,
    FieldValuesService fieldValuesService,
    IEncryptionService encryptionService,
    ILogger<FormValuesService> logger,
    IHttpRequest httpRequest)
{
    private Dictionary<string, object?> GetFormValues(FormElement formElement,
        string? fieldPrefix = null)
    {
        if (formElement == null)
            throw new ArgumentException(nameof(FormElement));

        var values = new Dictionary<string, object?>(StringComparer.InvariantCultureIgnoreCase);
        foreach (var field in formElement.Fields)
        {
            var fieldName = (fieldPrefix ?? string.Empty) + field.Name;

#if NET48
            var value = field.ValidateRequest
                ? httpRequest.Form[fieldName]
                : httpRequest.Form.GetUnvalidated(fieldName);
#else
            var value = httpRequest.Form[fieldName];
#endif
            HandleFieldValue(field, values, value);
        }

        return values;
    }

    internal static void HandleFieldValue(FormElementField field, Dictionary<string, object?> values, string? value)
    {
        try
        {
            var parsedValue = GetParsedValue(field, value);

            if (parsedValue is not null)
                values.Add(field.Name, parsedValue);
            else if (value?.Length == 0)
                values.Add(field.Name, null);
        }
        catch (Exception ex)
        {
            throw new FormValuesException(field, value, ex);
        }
    }

    private static object? GetParsedValue(FormElementField field, string? value)
    {
        object? parsedValue = null;
        switch (field.Component)
        {
            case FormComponent.Hour:
                if (string.IsNullOrWhiteSpace(value))
                    break;

                if (TimeSpan.TryParse(value, out var parsedTimeValue))
                    parsedValue = parsedTimeValue;
                else
                    parsedValue = value;
                break;
            case FormComponent.Date:
            case FormComponent.DateTime:
                if (string.IsNullOrWhiteSpace(value))
                    break;
                if (DateTime.TryParse(value, out var parsedDateTimeValue))
                    parsedValue = parsedDateTimeValue;
                else
                    parsedValue = value;
                break;
            case FormComponent.Currency:
                if (string.IsNullOrWhiteSpace(value))
                    break;

                parsedValue = HandleCurrencyComponent(field, value);

                break;
            case FormComponent.Slider:
            case FormComponent.Number:
                parsedValue = HandleNumericComponent(field.DataType, value);

                break;
            case FormComponent.CheckBox:
                if (string.IsNullOrWhiteSpace(value))
                    break;

                var boolValue = StringManager.ParseBool(value);

                if (field.DataType is FieldType.Bit)
                    parsedValue = boolValue;
                else //Legacy compatibility when FieldType.Bit didn't exist.
                    parsedValue = boolValue ? "1" : "0";
                break;
#if NET48
            //.NET Framework 4.8 don't handle well multiple inputs with the same name.
            case FormComponent.ComboBox when field.DataItem?.EnableMultiSelect is true:
                parsedValue = string.IsNullOrEmpty(value) ? null : value?.TrimEnd(',');
                break;
#endif
            default:
                parsedValue = string.IsNullOrEmpty(value) ? null : value;
                break;
        }

        return parsedValue;
    }

    internal static object? HandleCurrencyComponent(FormElementField field, string? value)
    {
        if (string.IsNullOrEmpty(value))
            return null;

        CultureInfo cultureInfo;
        if (field.Attributes.TryGetValue(FormElementField.CultureInfoAttribute, out var cultureInfoName) &&
            !string.IsNullOrEmpty(cultureInfoName?.ToString()))
            cultureInfo = CultureInfo.GetCultureInfo(cultureInfoName?.ToString()!);
        else
            cultureInfo = CultureInfo.CurrentUICulture;

        object? parsedValue = value;

        switch (field.DataType)
        {
            case FieldType.Float:
                if (double.TryParse(value, NumberStyles.Any,
                        cultureInfo, out var doubleValue))
                {
                    parsedValue = doubleValue;
                }

                break;
            case FieldType.Int:
                if (int.TryParse(value, NumberStyles.Currency,
                        cultureInfo, out var numericValue))
                {
                    parsedValue = numericValue;
                }

                break;
        }

        return parsedValue;
    }

    internal static object? HandleNumericComponent(FieldType dataType, string? value)
    {
        if (string.IsNullOrEmpty(value))
            return null;

        object? parsedValue = value;

        switch (dataType)
        {
            case FieldType.Float:
                if (double.TryParse(value, out var doubleValue))
                    parsedValue = doubleValue;
                break;
            case FieldType.Int:
                if (int.TryParse(value, out var numericValue))
                    parsedValue = numericValue;
                break;
        }

        return parsedValue;
    }

    public async ValueTask<Dictionary<string, object?>> GetFormValuesWithMergedValuesAsync(
        FormElement formElement,
        FormStateData formStateData,
        bool autoReloadFormFields,
        string? prefix = null)
    {
        if (formElement == null)
            throw new ArgumentNullException(nameof(formElement));
        
        if (!autoReloadFormFields || formStateData.Values.Count == 0 )
        {
            var dbValues = await GetDbValues(formElement);

            DataHelper.CopyIntoDictionary(formStateData.Values, dbValues);
        }

        if (autoReloadFormFields && httpRequest.Form.ContainsFormValues())
        {
            var formValues = GetFormValues(formElement, prefix);
            DataHelper.CopyIntoDictionary(formStateData.Values, formValues, true);
        }

        return await fieldValuesService.MergeWithExpressionValuesAsync(formElement, formStateData,
            !httpRequest.Form.ContainsFormValues());
    }
    
    private async Task<Dictionary<string, object?>> GetDbValues(Element element)
    {
        var encryptedPkValues = httpRequest.Form[
            $"data-panel-pk-values-{element.Name}"];

        if (string.IsNullOrEmpty(encryptedPkValues))
        {
            var encryptedFkValues = httpRequest.Form[
                $"form-view-relation-values-{element.Name}"];

            if (!string.IsNullOrEmpty(encryptedFkValues))
            {
                return encryptionService.DecryptDictionary(encryptedFkValues)!;
            }
        }

        if (encryptedPkValues is null)
            return new Dictionary<string, object?>();

        var pkValues = encryptionService.DecryptStringWithUrlUnescape(encryptedPkValues)!;
        var filters = DataHelper.GetPkValues(element, pkValues, '|');

        var result = await entityRepository.GetFieldsAsync(element, filters);

        if (result.Count == 0)
        {
            logger.LogWarning(
                "Nothing returned from the database. ElementName: {ElementName} Filters: {Filters}",
                element.Name, filters);
        }

        return result;
    }
}