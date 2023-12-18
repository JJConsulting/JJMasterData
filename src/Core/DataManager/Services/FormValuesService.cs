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

    private IDictionary<string, object?> GetFormValues(FormElement formElement,
        string? fieldPrefix = null)
    {
        if (formElement == null)
            throw new ArgumentException(nameof(FormElement));

        var values = new Dictionary<string, object?>(StringComparer.InvariantCultureIgnoreCase);
        foreach (var field in formElement.Fields)
        {
            var fieldName = (fieldPrefix ?? string.Empty) + field.Name;
            
#if NET48
            object? value = field.ValidateRequest
                ? FormValues[fieldName]
                : FormValues.GetUnvalidated(fieldName);
#else
            object? value = FormValues[fieldName];
#endif
            switch (field.Component)
            {
                case FormComponent.Date when field.DataType is FieldType.DateTime or FieldType.DateTime2:
                case FormComponent.DateTime when field.DataType is FieldType.DateTime or FieldType.DateTime2:
                    if (value is not null && !string.IsNullOrEmpty(value.ToString()))
                        value = DateTime.Parse(value.ToString()!);
                    else
                        value = null;
                    break;
                case FormComponent.Currency:
                case FormComponent.Slider: 
                case FormComponent.Number:
                    if (value is null)
                        break;

                    value = HandleNumericComponent(value, field.DataType);

                    break;
                case FormComponent.CheckBox:
                    value = StringManager.ParseBool(value);
                    break;
            }

            if(value is not null)
                values.Add(field.Name, value);
        }

        return values;
    }

    private static object HandleNumericComponent(object value, FieldType dataType)
    {
        var culture =  CultureInfo.CurrentCulture;
        object parsedValue = 0;
        
        switch (dataType)
        {
            case FieldType.Float:
                if (float.TryParse(value.ToString(), NumberStyles.Any, culture, out var floatValue))
                    parsedValue = floatValue;
                break;
            case FieldType.Int:
                if (int.TryParse(value.ToString(), NumberStyles.Any, culture, out var numericValue))
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
        
        return await FieldValuesService.MergeWithExpressionValuesAsync(formElement, formStateData, !FormValues.ContainsFormValues());
    }



    private async Task<IDictionary<string, object?>> GetDbValues(Element element)
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