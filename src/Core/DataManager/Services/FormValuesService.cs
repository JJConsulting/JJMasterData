#nullable enable

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using JJMasterData.Commons.Data.Entity.Models;
using JJMasterData.Commons.Data.Entity.Repository.Abstractions;
using JJMasterData.Commons.Security.Cryptography.Abstractions;
using JJMasterData.Commons.Util;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.DataManager.Models;
using JJMasterData.Core.Extensions;
using JJMasterData.Core.Http.Abstractions;

namespace JJMasterData.Core.DataManager.Services;

public class FormValuesService(
    IEntityRepository entityRepository,
    FieldValuesService fieldValuesService,
    IEncryptionService encryptionService,
    IHttpRequest httpRequest)
{
    private IEntityRepository EntityRepository { get; } = entityRepository;
    private FieldValuesService FieldValuesService { get; } = fieldValuesService;
    private IEncryptionService EncryptionService { get; } = encryptionService;
    private IFormValues FormValues { get; } = httpRequest.Form;

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
                ? FormValues[fieldName]
                : FormValues.GetUnvalidated(fieldName);
#else
            var value = FormValues[fieldName];
#endif
            HandleFieldValue(field, values, value);
        }

        return values;
    }

    internal static void HandleFieldValue(FormElementField field, Dictionary<string, object?> values, string? value)
    {
        object? parsedValue = null;
        switch (field.Component)
        {
            case FormComponent.Hour:
                if (string.IsNullOrWhiteSpace(value))
                    break;
                
                parsedValue = TimeSpan.Parse(value);
                break;
            case FormComponent.Date:
            case FormComponent.DateTime:
                if (string.IsNullOrWhiteSpace(value))
                    break;
                
                parsedValue = DateTime.Parse(value);
                
                break;
            case FormComponent.Currency:
                if (string.IsNullOrWhiteSpace(value))
                    break;

                parsedValue = HandleCurrencyComponent(field, value);

                break;
            case FormComponent.Slider:
            case FormComponent.Number:
                if (string.IsNullOrWhiteSpace(value))
                    break;

                parsedValue = HandleNumericComponent(field.DataType, value);

                break;
            case FormComponent.CheckBox:
                if (string.IsNullOrWhiteSpace(value))
                    break;
            
                var boolValue = StringManager.ParseBool(value);

                if (field.DataType is FieldType.Bit)
                    parsedValue = boolValue;
                else //Legacy compatibility when FieldType.Bit didn't exists.
                    parsedValue = boolValue ? "1" : "0";
                break;
            default:
                parsedValue = value;
                break;
        }

        if (parsedValue is not null)
            values.Add(field.Name, parsedValue);
    }

    internal static object? HandleCurrencyComponent(FormElementField field, string? value)
    {
        if (value is null)
            return value;

        CultureInfo cultureInfo;
        if (field.Attributes.TryGetValue(FormElementField.CultureInfoAttribute, out var cultureInfoName) &&
            !string.IsNullOrEmpty(cultureInfoName?.ToString()))
            cultureInfo = CultureInfo.GetCultureInfo(cultureInfoName?.ToString()!);
        else
            cultureInfo = CultureInfo.CurrentUICulture;

        object parsedValue = value;

        switch (field.DataType)
        {
            case FieldType.Float:
                if (float.TryParse(value, NumberStyles.Currency | NumberStyles.AllowCurrencySymbol,
                        cultureInfo, out var floatValue))
                    parsedValue = floatValue;
                break;
            case FieldType.Int:
                if (int.TryParse(value, NumberStyles.Currency | NumberStyles.AllowCurrencySymbol,
                        cultureInfo, out var numericValue))
                    parsedValue = numericValue;
                break;
        }

        return parsedValue;
    }

    internal static object? HandleNumericComponent(FieldType dataType, string? value)
    {
        if (value is null)
            return value;
        
        object? parsedValue = value;

        switch (dataType)
        {
            case FieldType.Float:
                if (float.TryParse(value, out var floatValue))
                    parsedValue = floatValue;
                break;
            case FieldType.Int:
                if (int.TryParse(value, out var numericValue))
                    parsedValue = numericValue;
                break;
        }

        return parsedValue;
    }

    public async Task<Dictionary<string, object?>> GetFormValuesWithMergedValuesAsync(
        FormElement formElement,
        FormStateData formStateData,
        bool autoReloadFormFields,
        string? prefix = null)
    {
        if (formElement == null)
            throw new ArgumentNullException(nameof(formElement));

        if (!formStateData.Values.Any())
        {
            var dbValues = await GetDbValues(formElement);
            DataHelper.CopyIntoDictionary(formStateData.Values, dbValues);
        }

        if (FormValues.ContainsFormValues() && autoReloadFormFields)
        {
            var formValues = GetFormValues(formElement, prefix);
            DataHelper.CopyIntoDictionary(formStateData.Values, formValues, true);
        }

        return await FieldValuesService.MergeWithExpressionValuesAsync(formElement, formStateData,
            !FormValues.ContainsFormValues());
    }


    private async Task<Dictionary<string, object?>> GetDbValues(Element element)
    {
        string encryptedPkValues = FormValues[
            $"data-panel-pk-values-{element.Name}"];

        if (string.IsNullOrEmpty(encryptedPkValues))
        {
            var encryptedFkValues = FormValues[
                $"form-view-relation-values-{element.Name}"];

            if (!string.IsNullOrEmpty(encryptedFkValues))
            {
                return EncryptionService.DecryptDictionary(encryptedFkValues)!;
            }
        }

        if (encryptedPkValues is null)
            return new Dictionary<string, object?>();

        string pkValues = EncryptionService.DecryptStringWithUrlUnescape(encryptedPkValues)!;
        var filters = DataHelper.GetPkValues(element, pkValues, '|');

        return await EntityRepository.GetFieldsAsync(element, filters);
    }
}