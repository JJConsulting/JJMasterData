using System.Collections.Generic;
using System.Threading.Tasks;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataManager.Services.Abstractions;

namespace JJMasterData.Core.DataManager.Services;

public class FieldsService : IFieldsService
{
    private IFieldVisibilityService FieldVisibilityService { get; }
    private IFieldFormattingService FieldFormattingService { get; }
    private IFieldValidationService FieldValidationService { get; }
    private IFieldValuesService  FieldValuesService { get; }
    
    public FieldsService(
        IFieldVisibilityService fieldVisibilityService,
        IFieldFormattingService fieldFormattingService,
        IFieldValuesService fieldValuesService,
        IFieldValidationService fieldValidationService)
    {
        FieldVisibilityService = fieldVisibilityService;
        FieldFormattingService = fieldFormattingService;
        FieldValidationService = fieldValidationService;
        FieldValuesService = fieldValuesService;
    }
    
    public bool IsVisible(FormElementField field, PageState state, IDictionary<string, dynamic> formValues)
    {
        return FieldVisibilityService.IsVisible(field, state, formValues);
    }

    public bool IsEnabled(FormElementField field, PageState state, IDictionary<string, dynamic> formValues)
    {
        return FieldVisibilityService.IsEnabled(field, state, formValues);
    }

    public IDictionary<string, dynamic> ValidateFields(FormElement formElement, IDictionary<string, dynamic> formValues, PageState pageState, bool enableErrorLink)
    {
       return FieldValidationService.ValidateFields(formElement, formValues, pageState, enableErrorLink);
    }

    public string ValidateField(FormElementField field, string fieldId, string value, bool enableErrorLink = true)
    {
        return FieldValidationService.ValidateField(field, fieldId, value, enableErrorLink);
    }

    public async Task<string> FormatGridValueAsync(FormElementField field, IDictionary<string, dynamic> values, IDictionary<string, dynamic> userValues)
    {
        return await FieldFormattingService.FormatGridValueAsync(field, values, userValues);
    }

    public string FormatValue(FormElementField field, object value)
    {
        return FieldFormattingService.FormatValue(field, value);
    }

    public IDictionary<string, dynamic> MergeWithExpressionValues(FormElement formElement, IDictionary<string, dynamic> formValues, PageState pageState,
        bool replaceNullValues)
    {
        return FieldValuesService.MergeWithExpressionValues(formElement, formValues, pageState, replaceNullValues);
    }

    public IDictionary<string, dynamic> GetDefaultValues(FormElement formElement, IDictionary<string, dynamic> formValues, PageState pageState)
    {
        return FieldValuesService.GetDefaultValues(formElement, formValues, pageState);
    }

    public IDictionary<string, dynamic> MergeWithDefaultValues(FormElement formElement, IDictionary<string, dynamic> formValues, PageState pageState)
    {
        return FieldValuesService.MergeWithDefaultValues(formElement, formValues, pageState);
    }
}