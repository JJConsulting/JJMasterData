using JJMasterData.Commons.Util;
using JJMasterData.Core.DataDictionary;
using System;
using System.Collections.Generic;
using System.Linq;
using JJMasterData.Commons.Data.Entity.Abstractions;
using JJMasterData.Commons.Localization;
using JJMasterData.Core.DataManager.Services.Abstractions;

namespace JJMasterData.Core.DataManager;

public class FieldValuesService : IFieldValuesService
{
    private IEntityRepository EntityRepository { get; }
    private IFieldValidationService FieldValidationService { get; }

    private IExpressionsService ExpressionsService { get; }


    public FieldValuesService(IExpressionsService expressionsService, IEntityRepository entityRepository, IFieldValidationService fieldValidationService)
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
    /// <param name="pageState">Context</param>
    /// <param name="replaceNullValues">Change the field's default value even if it is empty</param>
    /// <returns>
    /// Returns a new Dictionary with the updated values
    /// </returns>
    public IDictionary<string,dynamic> MergeWithExpressionValues(FormElement formElement, IDictionary<string,dynamic> formValues, PageState pageState, bool replaceNullValues)
    {
        if (formValues == null)
            throw new ArgumentNullException(nameof(formValues));

        IDictionary<string,dynamic> newValues = new Dictionary<string,dynamic>(StringComparer.InvariantCultureIgnoreCase);
        foreach (var f in formElement.Fields)
        {
            if (formValues.TryGetValue(f.Name, out var value))
            {
                object val = ClearSpecialChars(f, value);
                newValues.Add(f.Name, val);
            }
        }

        ApplyDefaultValues(formElement, newValues, pageState, replaceNullValues);
        ApplyTriggerValues(formElement, newValues, pageState);

        return newValues;
    }

    public IDictionary<string,dynamic> GetDefaultValues(FormElement formElement, IDictionary<string,dynamic> formValues, PageState state)
    {
        var filters = new Dictionary<string,dynamic>(StringComparer.InvariantCultureIgnoreCase);
        var list = formElement.Fields
            .ToList()
            .FindAll(x => !string.IsNullOrEmpty(x.DefaultValue));

        foreach (var e in list)
        {
            string val = ExpressionsService.GetDefaultValue(e, state, formValues);
            if (!string.IsNullOrEmpty(val))
            {
                filters.Add(e.Name, val);
            }
        }

        return filters;
    }

    public IDictionary<string,dynamic> MergeWithDefaultValues(FormElement formElement, IDictionary<string,dynamic> formValues, PageState pageState)
    {
        IDictionary<string,dynamic> values = new Dictionary<string,dynamic>(StringComparer.InvariantCultureIgnoreCase);
        if (formValues != null)
        {
            foreach (var v in formValues)
                values.Add(v.Key, v.Value);
        }

        ApplyDefaultValues(formElement,values, pageState, false);
        return values;
    }

    private void ApplyDefaultValues(FormElement formElement, IDictionary<string,dynamic> formValues, PageState pageState, bool replaceNullValues)
    {
        var defaultValues = GetDefaultValues(formElement,formValues, pageState);
        if (defaultValues == null)
            return;

        foreach (var d in defaultValues)
        {
            if (!formValues.ContainsKey(d.Key))
            {
                formValues.Add(d.Key, d.Value);
            }
            else
            {
                if ((formValues[d.Key] == null || string.IsNullOrEmpty(formValues[d.Key].ToString()))
                    && replaceNullValues)
                {
                    formValues[d.Key] = d.Value;
                }
            }
        }
    }

    private void ApplyTriggerValues(FormElement formElement, IDictionary<string,dynamic> formValues, PageState pageState)
    {
        var listFields = formElement.Fields
            .ToList()
            .FindAll(x => !string.IsNullOrEmpty(x.TriggerExpression));
        foreach (var e in listFields)
        {
            string val = ExpressionsService.GetTriggerValue(e, pageState, formValues);
            if (val != null)
            {
                formValues[e.Name] = val;
            }
        }
    }

    private static object ClearSpecialChars(FormElementField f, object val)
    {
        if (val != null)
        {
            val = f.Component switch
            {
                FormComponent.Cnpj or FormComponent.Cnpj or FormComponent.CnpjCpf => StringManager.ClearCpfCnpjChars(
                    val.ToString()),
                FormComponent.Tel => StringManager.ClearTelChars(val.ToString()),
                FormComponent.Cep => val.ToString().Replace("-", ""),
                _ => val
            };
        }

        return val;
    }

}