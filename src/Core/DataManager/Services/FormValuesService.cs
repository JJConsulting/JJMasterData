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

public class FormValuesService 
{
    private IEntityRepository EntityRepository { get; }
    private FieldValuesService FieldValuesService { get; }
    private DataItemService DataItemService { get; }
    private LookupService LookupService { get; }
    private IEncryptionService EncryptionService { get; }
    private IFormValues FormValues { get; }
    public FormValuesService(
        IEntityRepository entityRepository,
        FieldValuesService fieldValuesService,
        DataItemService dataItemService,
        LookupService lookupService,
        IEncryptionService encryptionService,
        IFormValues formValues)
    {
        EntityRepository = entityRepository;
        FieldValuesService = fieldValuesService;
        DataItemService = dataItemService;
        LookupService = lookupService;
        EncryptionService = encryptionService;
        FormValues = formValues;
    }

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
                case FormComponent.Slider:
                case FormComponent.Currency:
                case FormComponent.Number:
                    if (double.TryParse(value?.ToString(), NumberStyles.Number, CultureInfo.CurrentCulture,
                            out var numericValue))
                        value = numericValue;
                    else
                        value = null;
                    break;
                case FormComponent.CheckBox:
                    value = StringManager.ParseBool(value);
                    break;
            }

            if (value != null)
            {
                values.Add(field.Name, value);
            }
        }

        return values;
    }

    public async Task<IDictionary<string, object?>> GetFormValuesWithMergedValuesAsync(
        FormElement formElement,
        PageState pageState,
        bool autoReloadFormFields,
        string? fieldPrefix = null)
    {
        var dbValues = await GetDbValues(formElement);
        
        return await GetFormValuesWithMergedValuesAsync(formElement, pageState, dbValues, autoReloadFormFields, fieldPrefix);
    }

    public async Task<IDictionary<string, object?>> GetFormValuesWithMergedValuesAsync(
        FormElement formElement,
        PageState pageState,
        IDictionary<string, object?>? values,
        bool autoReloadFormFields,
        string? prefix = null)
    {
        if (formElement == null)
            throw new ArgumentNullException(nameof(formElement));

        var valuesToBeReceived = new Dictionary<string, object?>();
        DataHelper.CopyIntoDictionary(valuesToBeReceived, values, true);

        if (FormValues.ContainsFormValues() && autoReloadFormFields)
        {
            var requestedValues = await GetFormValuesAsync(formElement, pageState, prefix);
            DataHelper.CopyIntoDictionary(valuesToBeReceived, requestedValues, true);
        }

        return await FieldValuesService.MergeWithExpressionValuesAsync(formElement, valuesToBeReceived, pageState, !FormValues.ContainsFormValues());
    }



    private async Task<IDictionary<string, object?>?> GetDbValues(Element element)
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

        string pkValues = EncryptionService.DecryptStringWithUrlUnescape(encryptedPkValues)!;
        var filters = DataHelper.GetPkValues(element, pkValues, '|');

        return await EntityRepository.GetFieldsAsync(element, filters);
    }
}