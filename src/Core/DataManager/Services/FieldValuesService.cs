#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
    /// <param name="formStateData">Form values</param>
    /// <param name="replaceNullValues">Change the field's default value even if it is empty</param>
    /// <returns>
    /// Returns a new Dictionary with the updated values
    /// </returns>
    public async ValueTask<Dictionary<string, object?>> MergeWithExpressionValuesAsync(FormElement formElement, FormStateData formStateData, bool replaceNullValues = true)
    {
        var formValues = formStateData.Values;
        foreach (var f in formElement.Fields)
        {
            if (formValues.TryGetValue(f.Name, out var value))
            {
                formValues[f.Name] = ClearSpecialChars(f, value);
            }
        }

        await ApplyDefaultValues(formElement, formStateData, replaceNullValues);
        await ApplyTriggerValues(formElement, formStateData);

        return new Dictionary<string, object?>(formStateData.Values);
    }

    public async ValueTask<Dictionary<string, object?>> GetDefaultValuesAsync(FormElement formElement,FormStateData formStateData)
    {
        var defaultValues = new Dictionary<string, object?>(StringComparer.InvariantCultureIgnoreCase);
        var formStateDataCopy = formStateData.DeepCopy();
        var fieldsWithDefaultValue = formElement.Fields
            .Where(FieldNeedsDefaultValue(formStateData));
        
        foreach (var field in fieldsWithDefaultValue)
        {
            var fieldSelector = new FormElementFieldSelector(formElement, field.Name);
            var defaultValue = await ExpressionsService.GetDefaultValueAsync(fieldSelector, formStateDataCopy);
            if (!string.IsNullOrEmpty(defaultValue?.ToString()))
            {
                defaultValues.Add(field.Name, defaultValue);
                formStateDataCopy.Values[field.Name] = defaultValue;
            }
        }

        return defaultValues;
    }

    private static Func<FormElementField, bool> FieldNeedsDefaultValue(FormStateData formStateData)
    {
        return f => !string.IsNullOrEmpty(f.DefaultValue) && 
                    (!formStateData.Values.ContainsKey(f.Name) || string.IsNullOrEmpty(formStateData.Values[f.Name]?.ToString()));
    }
    
    public async ValueTask<Dictionary<string, object?>> MergeWithDefaultValuesAsync(FormElement formElement,FormStateData formStateData)
    {
        var values = new Dictionary<string, object?>(StringComparer.InvariantCultureIgnoreCase);
  
        foreach (var v in formStateData.Values)
            values.Add(v.Key, v.Value);
        

        await ApplyDefaultValues(formElement, formStateData, false);
        return values;
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
        var fieldsWithTrigger = formElement.Fields
            .Where(x => !string.IsNullOrEmpty(x.TriggerExpression));
        
        foreach (var field in fieldsWithTrigger)
        {
            var fieldSelector = new FormElementFieldSelector(formElement, field.Name);
            var value = await ExpressionsService.GetTriggerValueAsync(fieldSelector, formStateData);
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