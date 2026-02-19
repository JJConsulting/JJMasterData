#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JJMasterData.Commons.Util;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.DataManager.Expressions;
using JJMasterData.Core.DataManager.Expressions.Providers;
using JJMasterData.Core.DataManager.Models;

namespace JJMasterData.Core.DataManager.Services;

public class FieldValuesService(ExpressionsService expressionsService)
{
    /// <summary>
    /// Apply default and triggers expression values
    /// </summary>
    /// <param name="formElement"></param>
    /// <param name="formStateData">Form values</param>
    /// <param name="replaceNullValues">Change the field's default value even if it is empty</param>
    /// <returns>
    /// Returns a new Dictionary with the updated values
    /// </returns>
    public async ValueTask<Dictionary<string, object?>> MergeWithExpressionValuesAsync(FormElement formElement, FormStateData formStateData, bool replaceNullValues = true)
    {
        var formValues = formStateData.Values;
        foreach (var field in formElement.Fields)
        {
            if (formValues.TryGetValue(field.Name, out var value))
            {
                formValues[field.Name] = ClearSpecialChars(field, value);
            }
        }

        await ApplyDefaultValues(formElement, formStateData, replaceNullValues);
        await ApplyTriggerValues(formElement, formStateData);

        return new Dictionary<string, object?>(formStateData.Values);
    }

    public async ValueTask<Dictionary<string, object?>> GetDefaultValuesAsync(
        FormElement formElement,
        FormStateData formStateData,
        bool allowSqlValues = true)
    {
        var defaultValues = new Dictionary<string, object?>(StringComparer.InvariantCultureIgnoreCase);
        var formStateDataCopy = formStateData.DeepCopy();
        var values = formStateData.Values;

        foreach (var field in formElement.Fields)
        {
            var hasExpression = !string.IsNullOrEmpty(field.DefaultValue);
            var valueDoesNotExist = !values.ContainsKey(field.Name) || string.IsNullOrEmpty(values[field.Name]?.ToString());
            if (hasExpression && valueDoesNotExist)
            {
                if (!allowSqlValues && field.DefaultValue!.StartsWith(SqlExpressionProvider.Prefix))
                    continue;
                
                var fieldSelector = new FormElementFieldSelector(formElement, field.Name);
                var defaultValue = await expressionsService.GetDefaultValueAsync(fieldSelector, formStateDataCopy);

                if (!string.IsNullOrEmpty(defaultValue?.ToString()))
                {
                    defaultValues.Add(field.Name, defaultValue);
                    formStateDataCopy.Values[field.Name] = defaultValue;
                }
            }
        }

        return defaultValues;
    }
    
    public async ValueTask<Dictionary<string, object?>> MergeWithDefaultValuesAsync(FormElement formElement,FormStateData formStateData)
    {
        await ApplyDefaultValues(formElement, formStateData, false);
        
        return formStateData.Values;
    }

    private async ValueTask ApplyDefaultValues(FormElement formElement, FormStateData formStateData, bool replaceNullValues)
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

    private async ValueTask ApplyTriggerValues(FormElement formElement, FormStateData formStateData)
    {
        foreach (var field in formElement.Fields)
        {
            if (string.IsNullOrEmpty(field.TriggerExpression)) 
                continue;
            
            var fieldSelector = new FormElementFieldSelector(formElement, field.Name);
            var value = await expressionsService.GetTriggerValueAsync(fieldSelector, formStateData);
            if (value != null)
                formStateData.Values[field.Name] = value;
        }
    }

    private static object? ClearSpecialChars(FormElementField field, object? value)
    {
        if (value is null)
            return value;

        switch (field.Component)
        {
            case FormComponent.Cnpj or FormComponent.CnpjCpf:
                return StringManager.ClearCpfCnpjChars(value.ToString()!);
            case FormComponent.Tel:
                return StringManager.ClearPhoneChars(value.ToString()!);
            case FormComponent.Cep:
                return value.ToString()!.Replace("-", "");
            default:
                return value;
        }
    }

}