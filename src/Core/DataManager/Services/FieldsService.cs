using System.Collections.Generic;
using System.Threading.Tasks;
using JJMasterData.Core.DataDictionary.Models;


namespace JJMasterData.Core.DataManager.Services;

public class FieldsService(FieldFormattingService fieldFormattingService,
    FieldValuesService fieldValuesService,
    FieldValidationService fieldValidationService)
{
    private FieldFormattingService FieldFormattingService { get; } = fieldFormattingService;
    private FieldValidationService FieldValidationService { get; } = fieldValidationService;
    private FieldValuesService FieldValuesService { get; } = fieldValuesService;

    public IDictionary<string, string> ValidateFields(FormElement formElement, IDictionary<string, object> formValues, PageState pageState, bool enableErrorLink)
    {
       return FieldValidationService.ValidateFields(formElement, formValues, pageState, enableErrorLink);
    }

    public string ValidateField(FormElementField field, string fieldId, string value, bool enableErrorLink = true)
    {
        return FieldValidationService.ValidateField(field, fieldId, value, enableErrorLink);
    }

    public Task<string> FormatGridValueAsync(FormElement formElement,FormElementField field, IDictionary<string, object> values, IDictionary<string, object> userValues)
    {
        return FieldFormattingService.FormatGridValueAsync(field, values, userValues);
    }

    public static string FormatValue(FormElementField field, object value)
    {
        return FieldFormattingService.FormatValue(field, value);
    }

    public Task<Dictionary<string, object>> MergeWithExpressionValuesAsync(FormElement formElement, IDictionary<string, object> formValues, PageState pageState,
        bool replaceNullValues)
    {
        return FieldValuesService.MergeWithExpressionValuesAsync(formElement, formValues, pageState, replaceNullValues);
    }

    public Task<Dictionary<string, object>> GetDefaultValuesAsync(FormElement formElement, IDictionary<string, object> formValues, PageState pageState)
    {
        return FieldValuesService.GetDefaultValuesAsync(formElement, formValues, pageState);
    }

    public Task<Dictionary<string, object>>  MergeWithDefaultValuesAsync(FormElement formElement, IDictionary<string, object> formValues, PageState pageState)
    {
        return FieldValuesService.MergeWithDefaultValuesAsync(formElement, formValues, pageState);
    }
}