#nullable enable
using JJMasterData.Commons.Data.Entity.Abstractions;
using JJMasterData.Commons.Util;
using JJMasterData.Core.DataDictionary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JJMasterData.Core.DataManager.Services;

namespace JJMasterData.Core.DataManager;

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
                object val = ClearSpecialChars(f, value);
                newValues.Add(f.Name, val);
            }
        }

        await ApplyDefaultValues(formElement, newValues, pageState, replaceNullValues);
        await ApplyTriggerValues(formElement, newValues, pageState);

        return newValues;
    }

    public async Task<IDictionary<string, object?>> GetDefaultValuesAsync(FormElement formElement, IDictionary<string, object?> formValues, PageState state)
    {
        var filters = new Dictionary<string, object?>(StringComparer.InvariantCultureIgnoreCase);
        var list = formElement.Fields
            .ToList()
            .FindAll(x => !string.IsNullOrEmpty(x.DefaultValue));

        var formState = new FormStateData(formValues, state);
        foreach (var e in list)
        {
            string? val = await ExpressionsService.GetDefaultValueAsync(e, formState);
            if (val != null && !string.IsNullOrEmpty(val))
            {
                filters.Add(e.Name, val);
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
                if ((formValues[d.Key] == null || string.IsNullOrEmpty(formValues[d.Key]?.ToString()))
                    && replaceNullValues)
                {
                    formValues[d.Key] = d.Value;
                }
            }
        }
    }

    private async Task ApplyTriggerValues(FormElement formElement, IDictionary<string, object?> formValues, PageState pageState)
    {
        var listFields = formElement.Fields
            .ToList()
            .FindAll(x => !string.IsNullOrEmpty(x.TriggerExpression));

        var formState = new FormStateData(formValues, pageState);
        foreach (var e in listFields)
        {
            string? val = await ExpressionsService.GetTriggerValueAsync(e, formState);
            if (val != null)
            {
                formValues[e.Name] = val;
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