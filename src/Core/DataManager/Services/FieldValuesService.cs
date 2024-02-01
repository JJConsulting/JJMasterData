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

public class FieldValuesService(ExpressionsService expressionsService)
{
    private ExpressionsService ExpressionsService { get; } = expressionsService;


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
    public async Task<Dictionary<string, object?>> MergeWithExpressionValuesAsync(FormElement formElement, FormStateData formStateData, bool replaceNullValues = true)
    {
        var formValues = formStateData.Values;
        foreach (var f in formElement.Fields)
        {
            if (formValues.TryGetValue(f.Name, out var value))
            {
                var valueWithoutSpecialCharacters = ClearSpecialChars(f, value);
                formValues[f.Name] = valueWithoutSpecialCharacters;
            }
        }

        await ApplyDefaultValues(formElement, formStateData, replaceNullValues);
        await ApplyTriggerValues(formElement, formStateData);

        return new Dictionary<string, object?>(formStateData.Values);
    }

    public async Task<Dictionary<string, object?>> GetDefaultValuesAsync(FormElement formElement,FormStateData formStateData)
    {
        var defaultValues = new Dictionary<string, object?>(StringComparer.InvariantCultureIgnoreCase);
        var fieldsWithDefaultValue = formElement.Fields
            .ToList()
            .FindAll(x => !string.IsNullOrEmpty(x.DefaultValue));
        
        foreach (var field in fieldsWithDefaultValue)
        {
            var defaultValue = await ExpressionsService.GetDefaultValueAsync(field, formStateData);
            if (defaultValue is not null && !string.IsNullOrEmpty(defaultValue.ToString()))
            {
                defaultValues.Add(field.Name, defaultValue);
                DataHelper.CopyIntoDictionary(formStateData.Values, defaultValues);
            }
        }

        return defaultValues;
    }

    public async Task<Dictionary<string, object?>> MergeWithDefaultValuesAsync(FormElement formElement,FormStateData formStateData)
    {
        var values = new Dictionary<string, object?>(StringComparer.InvariantCultureIgnoreCase);
  
        foreach (var v in formStateData.Values)
            values.Add(v.Key, v.Value);
        

        await ApplyDefaultValues(formElement, formStateData, false);
        return values;
    }

    private async Task ApplyDefaultValues(FormElement formElement, FormStateData formStateData, bool replaceNullValues)
    {
        var defaultValues = await GetDefaultValuesAsync(formElement,formStateData);

        var formValues = formStateData.Values;
        
        foreach (var d in defaultValues)
        {
            if (!formValues.TryGetValue(d.Key, out var value))
            {
                formValues.Add(d.Key, d.Value);
            }
            else
            {
                if ((value == null || string.IsNullOrEmpty(value.ToString())) && replaceNullValues)
                {
                    formValues[d.Key] = d.Value;
                }
            }
        }
    }

    private async Task ApplyTriggerValues(FormElement formElement, FormStateData formStateData)
    {
        var fieldList = formElement.Fields
            .ToList()
            .FindAll(x => !string.IsNullOrEmpty(x.TriggerExpression));
        
        foreach (var field in fieldList)
        {
            object? value = await ExpressionsService.GetTriggerValueAsync(field, formStateData);
            if (value != null)
            {
                formStateData.Values[field.Name] = value;
            }
        }
    }

    private static object? ClearSpecialChars(FormElementField f, object? value)
    {
        if (value is null)
            return value;
        
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