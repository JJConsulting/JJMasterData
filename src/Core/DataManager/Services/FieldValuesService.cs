#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JJMasterData.Commons.Data.Entity.Repository.Abstractions;
using JJMasterData.Commons.Util;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.DataManager.Expressions;
using JJMasterData.Core.DataManager.Models;

namespace JJMasterData.Core.DataManager.Services;

public class FieldValuesService
{
    private IEntityRepository EntityRepository { get; }
    private FieldValidationService FieldValidationService { get; }

    private ExpressionsService ExpressionsService { get; }


    public FieldValuesService(ExpressionsService expressionsService, IEntityRepository entityRepository, FieldValidationService fieldValidationService)
    {
        ExpressionsService = expressionsService;
        EntityRepository = entityRepository;
        FieldValidationService = fieldValidationService;
    }

    /// <summary>
    /// Apply default and triggers expression values
    /// </summary>
    /// <param name="formElement"></param>
    /// <param name="formValues">Form values</param>
    /// <param name="pageState">FormStateData</param>
    /// <param name="replaceNullValues">Change the field's default value even if it is empty</param>
    /// <returns>
    /// Returns a new Dictionary with the updated values
    /// </returns>
    public async Task<IDictionary<string, object?>> MergeWithExpressionValuesAsync(FormElement formElement, IDictionary<string, object?> formValues, PageState pageState, bool replaceNullValues)
    {
        if (formValues == null)
            throw new ArgumentNullException(nameof(formValues));

        IDictionary<string, object?> newValues = new Dictionary<string, object?>(StringComparer.InvariantCultureIgnoreCase);
        foreach (var f in formElement.Fields)
        {
            if (formValues.TryGetValue(f.Name, out var value) && value != null)
            {
                object valueWithoutSpecialCharacters = ClearSpecialChars(f, value);
                newValues.Add(f.Name, valueWithoutSpecialCharacters);
            }
        }

        await ApplyDefaultValues(formElement, newValues, pageState, replaceNullValues);
        await ApplyTriggerValues(formElement, newValues, pageState);

        return newValues;
    }

    public async Task<IDictionary<string, object?>> GetDefaultValuesAsync(FormElement formElement, IDictionary<string, object?> formValues, PageState state)
    {
        var filters = new Dictionary<string, object?>(StringComparer.InvariantCultureIgnoreCase);
        var fieldsWithDefaultValue = formElement.Fields
            .ToList()
            .FindAll(x => !string.IsNullOrEmpty(x.DefaultValue));

        var formState = new FormStateData(formValues, state);
        foreach (var field in fieldsWithDefaultValue)
        {
            var defaultValue = await ExpressionsService.GetDefaultValueAsync(field, formState);
            if (defaultValue is not null && !string.IsNullOrEmpty(defaultValue.ToString()))
            {
                filters.Add(field.Name, defaultValue);
            }
        }

        return filters;
    }

    public async Task<IDictionary<string, object?>> MergeWithDefaultValuesAsync(FormElement formElement, IDictionary<string, object?> formValues, PageState pageState)
    {
        IDictionary<string, object?> values = new Dictionary<string, object?>(StringComparer.InvariantCultureIgnoreCase);
  
        foreach (var v in formValues)
            values.Add(v.Key, v.Value);
        

        await ApplyDefaultValues(formElement,values, pageState, false);
        return values;
    }

    private async Task ApplyDefaultValues(FormElement formElement, IDictionary<string, object?> formValues, PageState pageState, bool replaceNullValues)
    {
        var defaultValues = await GetDefaultValuesAsync(formElement,formValues, pageState);

        foreach (var d in defaultValues)
        {
            if (!formValues.ContainsKey(d.Key))
            {
                formValues.Add(d.Key, d.Value);
            }
            else
            {
                if ((formValues[d.Key] == null || string.IsNullOrEmpty(formValues[d.Key]?.ToString())) && replaceNullValues)
                {
                    formValues[d.Key] = d.Value;
                }
            }
        }
    }

    private async Task ApplyTriggerValues(FormElement formElement, IDictionary<string, object?> formValues, PageState pageState)
    {
        var fieldList = formElement.Fields
            .ToList()
            .FindAll(x => !string.IsNullOrEmpty(x.TriggerExpression));

        var formState = new FormStateData(formValues, pageState);
        foreach (var field in fieldList)
        {
            object? value = await ExpressionsService.GetTriggerValueAsync(field, formState);
            if (value != null)
            {
                formValues[field.Name] = value;
            }
        }
    }

    private static object ClearSpecialChars(FormElementField f, object value)
    {
        value = f.Component switch
        {
            FormComponent.Cnpj or FormComponent.Cnpj or FormComponent.CnpjCpf => StringManager.ClearCpfCnpjChars(
                value.ToString()!),
            FormComponent.Tel => StringManager.ClearTelChars(value.ToString()!),
            FormComponent.Cep => value.ToString()!.Replace("-", ""),
            _ => value
        };

        return value;
    }

}