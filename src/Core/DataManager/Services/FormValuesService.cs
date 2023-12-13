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
using JJMasterData.Core.DataManager.Models;
using JJMasterData.Core.Extensions;
using JJMasterData.Core.Http.Abstractions;

namespace JJMasterData.Core.DataManager.Services;

public class FormValuesService(
    IEntityRepository entityRepository,
    FieldValuesService fieldValuesService,
    DataItemService dataItemService,
    LookupService lookupService,
    IEncryptionService encryptionService,
    IQueryString queryString,
    IFormValues httpFormValues)
{
    private IEntityRepository EntityRepository { get; } = entityRepository;
    private FieldValuesService FieldValuesService { get; } = fieldValuesService;
    private DataItemService DataItemService { get; } = dataItemService;
    private LookupService LookupService { get; } = lookupService;
    private IEncryptionService EncryptionService { get; } = encryptionService;
    private IQueryString QueryString { get; } = queryString;
    private IFormValues FormValues { get; } = httpFormValues;

    public async Task<IDictionary<string, object?>> GetFormValuesAsync(FormElement formElement, PageState pageState,
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
                case FormComponent.Search:
                    {
                        var formData = new FormStateData(values, pageState);
                        value = await DataItemService.GetSelectedValueAsync(field, formData);
                        break;
                    }
                case FormComponent.Lookup:
                    {
                        value = LookupService.GetSelectedValue(fieldName);
                        break;
                    }
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
        value = dataType switch
        {
            FieldType.Float when double.TryParse(value.ToString(), NumberStyles.Any,
                CultureInfo.InvariantCulture, out var numericValue) => numericValue,
            FieldType.Float => 0,
            FieldType.Int when int.TryParse(value.ToString(), NumberStyles.Any,
                CultureInfo.InvariantCulture, out var numericValue) => numericValue,
            FieldType.Int => 0,
            _ => value
        };
        return value;
    }

    public async Task<Dictionary<string, object?>> GetFormValuesWithMergedValuesAsync(
        FormElement formElement,
        PageState pageState,
        IDictionary<string,object?> userValues,
        bool autoReloadFormFields,
        string? fieldPrefix = null)
    {
        var dbValues = await GetDbValues(formElement);
        
        return await GetFormValuesWithMergedValuesAsync(formElement, new FormStateData(dbValues, userValues, pageState), autoReloadFormFields, fieldPrefix);
    }

    public async Task<Dictionary<string, object?>> GetFormValuesWithMergedValuesAsync(
        FormElement formElement,
        FormStateData formStateData,
        bool autoReloadFormFields,
        string? prefix = null)
    {
        if (formElement == null)
            throw new ArgumentNullException(nameof(formElement));
        
        if (FormValues.ContainsFormValues() && autoReloadFormFields)
        {
            var formValues = await GetFormValuesAsync(formElement, formStateData.PageState, prefix);
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