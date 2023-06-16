using System.ComponentModel.DataAnnotations;
using JJMasterData.Core.DataDictionary;

namespace JJMasterData.BlazorClient.Services.Abstractions;

public interface IFieldsService
{
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
    IEnumerable<ValidationResult> ValidateFields(FormElement formElement, IDictionary<string, dynamic?> formValues,
        PageState pageState, bool enableErrorLink);

    /// <summary>
    /// Apply default and triggers expression values
    /// </summary> 
    /// <param name="formValues">Form values</param>
    /// <param name="pageState">Context</param>
    /// <param name="replaceNullValues">Change the field's default value even if it is empty</param>
    /// <returns>
    /// Returns a new hashtable with the updated values
    /// </returns>
    IDictionary<string, dynamic?> MergeWithExpressionValues(FormElement formElement,
        IDictionary<string, dynamic?> formValues, PageState pageState, bool replaceNullValues);

    IDictionary<string, dynamic?> GetDefaultValues(FormElement formElement, IDictionary<string, dynamic?> formValues,
        PageState state);

    IDictionary<string, dynamic?> MergeWithDefaultValues(FormElement formElement,
        IDictionary<string, dynamic?> formValues, PageState pageState);

    IList<DataItemValue> GetDataItemValues(FormElementDataItem dataItem, IDictionary<string, dynamic?> formValues,
        PageState pageState);

    Task<bool> IsVisibleAsync(FormElementField field, PageState state,
        IDictionary<string, dynamic?>? formValues = null);

    bool IsEnabled(FormElementField field, PageState state, IDictionary<string, dynamic?> formValues,
        IDictionary<string, dynamic?> userValues);

    IAsyncEnumerable<FormElementField> GetVisibleFields(FormElementList fields, PageState pageState,
        IDictionary<string, dynamic?>? formValues = null);
}