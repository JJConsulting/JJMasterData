using System.Collections;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Globalization;
using JJMasterData.BlazorClient.Services.Abstractions;
using JJMasterData.Commons.Data.Entity;
using JJMasterData.Commons.Data.Entity.Abstractions;
using JJMasterData.Commons.Localization;
using JJMasterData.Commons.Util;
using JJMasterData.Core.DataDictionary;

namespace JJMasterData.BlazorClient.Services;

public class FieldsService : IFieldsService
{
    private IEntityRepository EntityRepository { get; }
    private IFieldValidationService FieldValidationService { get; }

    private IExpressionsService ExpressionsService { get;  }

    public FieldsService(
        IExpressionsService expressionsService,
        IEntityRepository entityRepository, 
        IFieldValidationService fieldValidationService)
    {
        ExpressionsService = expressionsService;
        EntityRepository = entityRepository;
        FieldValidationService = fieldValidationService;
    }

    /// <summary>
    /// Validates form fields and returns a list of errors found
    /// </summary>
    /// <param name="formElement"></param>
    /// <param name="formValues">Form values</param>
    /// <param name="pageState">Context</param>
    /// <param name="enableErrorLink">Add html link in error fields</param>
    /// <returns>
    /// Key = Field name
    /// Value = Error message
    /// </returns>
    public IEnumerable<ValidationResult> ValidateFields(FormElement formElement, IDictionary<string,dynamic?> formValues, PageState pageState, bool enableErrorLink)
    {
        if (formValues == null)
            throw new ArgumentNullException(nameof(formValues));

        foreach (var field in formElement.Fields)
        {
            bool isVisible = ExpressionsService.GetBoolValue(field.VisibleExpression, field.Name, pageState, formValues);
            if (!isVisible)
                continue;

            bool isEnable = ExpressionsService.GetBoolValue(field.EnableExpression, field.Name, pageState, formValues);
            if (!isEnable)
                continue;

            string value;
            if (formValues.ContainsKey(field.Name) && formValues[field.Name] != null)
                value = formValues[field.Name].ToString();
            else
                value = "";

            var error = FieldValidationService.ValidateField(field, value, enableErrorLink);
            if (!string.IsNullOrEmpty(error))
                yield return new ValidationResult(error, new[] { field.Name });
        }
    }

    /// <summary>
    /// Apply default and triggers expression values
    /// </summary> 
    /// <param name="formValues">Form values</param>
    /// <param name="pageState">Context</param>
    /// <param name="replaceNullValues">Change the field's default value even if it is empty</param>
    /// <returns>
    /// Returns a new hashtable with the updated values
    /// </returns>
    public IDictionary<string,dynamic?>  MergeWithExpressionValues(FormElement formElement ,IDictionary<string,dynamic?> formValues, PageState pageState, bool replaceNullValues)
    {
        if (formValues == null)
            throw new ArgumentNullException(Translate.Key("Invalid parameter or not found"), nameof(formValues));

        IDictionary<string,dynamic?> newValues = new Dictionary<string,dynamic?>(StringComparer.InvariantCultureIgnoreCase);
        foreach (var f in formElement.Fields)
        {
            if (formValues.TryGetValue(f.Name, out var value))
            {
                object? val = ClearSpecialChars(f, value);
                newValues.Add(f.Name, val);
            }
        }

        ApplyDefaultValues(formElement, ref newValues, pageState, replaceNullValues);
        ApplyTriggerValues(formElement, ref newValues, pageState);

        return newValues;
    }

    public IDictionary<string,dynamic?> GetDefaultValues(FormElement formElement, IDictionary<string,dynamic?> formValues, PageState state)
    {
        IDictionary<string,dynamic?> filters = new Dictionary<string,dynamic?>(StringComparer.InvariantCultureIgnoreCase);
        var list = formElement.Fields
            .ToList()
            .FindAll(x => !string.IsNullOrEmpty(x.DefaultValue));

        foreach (var e in list)
        {
            var val = ExpressionsService.GetDefaultValue(e, state, formValues);
            if (!string.IsNullOrEmpty(val))
            {
                filters.Add(e.Name, val);
            }
        }

        return filters;
    }

    public IDictionary<string,dynamic?>  MergeWithDefaultValues(FormElement formElement, IDictionary<string,dynamic?>? formValues, PageState pageState)
    {
        IDictionary<string,dynamic?> values = new Dictionary<string,dynamic?>(StringComparer.InvariantCultureIgnoreCase);
        if (formValues != null)
        {
            foreach (var v in formValues)
                values.Add(v.Key, v.Value);
        }

        ApplyDefaultValues(formElement,ref values, pageState, false);
        return values;
    }

    private void ApplyDefaultValues(FormElement formElement, ref IDictionary<string,dynamic?>? formValues, PageState pageState, bool replaceNullValues)
    {
        var defaultValues = GetDefaultValues(formElement, formValues, pageState);
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

    private void ApplyTriggerValues(FormElement formElement,ref IDictionary<string,dynamic?> formValues, PageState pageState)
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

    private object? ClearSpecialChars(FormElementField f, object? value)
    {
        if (value != null)
        {
            if (f.Component is FormComponent.Cnpj or FormComponent.Cnpj or FormComponent.CnpjCpf)
            {
                value = StringManager.ClearCpfCnpjChars(value.ToString());
            }
            else if (f.Component == FormComponent.Tel)
            {
                value = StringManager.ClearTelChars(value.ToString());
            }
            else if (f.Component == FormComponent.Cep)
            {
                value = value?.ToString()?.Replace("-", "");
            }
        }

        return value;
    }

    public IList<DataItemValue> GetDataItemValues(FormElementDataItem dataItem, IDictionary<string,dynamic?> formValues, PageState pageState)
    {
        if (dataItem == null)
            return null;

        IList<DataItemValue> values = new List<DataItemValue>();
        if (dataItem.Command != null && !string.IsNullOrEmpty(dataItem.Command.Sql))
        {

            string sql = dataItem.Command.Sql;
            if (sql.Contains("{"))
            {
                sql = ExpressionsService.ParseExpression(sql, pageState, false, formValues);
            }

            DataTable dt = EntityRepository.GetDataTable(sql);
            foreach (DataRow row in dt.Rows)
            {
                var item = new DataItemValue
                {
                    Id = row[0].ToString(),
                    Description = row[1].ToString()?.Trim()
                };
                if (dataItem.ShowImageLegend)
                {
                    item.Icon = (IconType)int.Parse(row[2].ToString() ?? string.Empty);
                    item.ImageColor = row[3].ToString();
                }
                values.Add(item);
            }
        }
        else
        {
            values = dataItem.Items;
        }

        return values;
    }
    
    public async Task<bool> IsVisibleAsync(FormElementField field, PageState state, IDictionary<string,dynamic?>? formValues = null)
    {
        if (field == null)
            throw new ArgumentNullException(nameof(field), "FormElementField can not be null");

        return await ExpressionsService.GetBoolValueAsync(field.VisibleExpression, field.Name, state, formValues);
    }

    public bool IsEnabled(FormElementField field, PageState state, IDictionary<string,dynamic?> formValues, IDictionary<string,dynamic?> userValues)
    {
        if (state == PageState.View)
            return false;

        if (field == null)
            throw new ArgumentNullException(nameof(field), "FormElementField can not be null");

        return ExpressionsService.GetBoolValue(field.EnableExpression, field.Name, state, formValues, userValues);
    }

    public async IAsyncEnumerable<FormElementField> GetVisibleFields(FormElementList fields, PageState pageState, IDictionary<string,dynamic?>? formValues = null)
    {
        foreach (var field in fields)
        {
            var isVisible = await IsVisibleAsync(field, pageState,formValues);
            if (isVisible)
            {
                yield return field;
            }
        }
    }
}