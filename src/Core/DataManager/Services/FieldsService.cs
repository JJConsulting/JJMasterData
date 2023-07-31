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
    
    public async Task<bool> IsVisibleAsync(FormElementField field, PageState state, IDictionary<string, dynamic> formValues)
    {
        return await FieldVisibilityService.IsVisibleAsync(field, state, formValues);
    }

    public async Task<bool> IsEnabledAsync(FormElementField field, PageState state, IDictionary<string, dynamic> formValues)
    {
        return await FieldVisibilityService.IsEnabledAsync(field, state, formValues);
    }

    public async Task<IDictionary<string, dynamic>> ValidateFieldsAsync(FormElement formElement, IDictionary<string, dynamic> formValues, PageState pageState, bool enableErrorLink)
    {
       return await FieldValidationService.ValidateFieldsAsync(formElement, formValues, pageState, enableErrorLink);
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

    public async Task<IDictionary<string,dynamic>> MergeWithExpressionValuesAsync(FormElement formElement, IDictionary<string, dynamic> formValues, PageState pageState,
        bool replaceNullValues)
    {
        return await FieldValuesService.MergeWithExpressionValuesAsync(formElement, formValues, pageState, replaceNullValues);
    }

    public async Task<IDictionary<string,dynamic>> GetDefaultValuesAsync(FormElement formElement, IDictionary<string, dynamic> formValues, PageState pageState)
    {
        return await FieldValuesService.GetDefaultValuesAsync(formElement, formValues, pageState);
    }

    public async Task<IDictionary<string,dynamic>>  MergeWithDefaultValuesAsync(FormElement formElement, IDictionary<string, dynamic> formValues, PageState pageState)
    {
        return await FieldValuesService.MergeWithDefaultValuesAsync(formElement, formValues, pageState);
    }
}