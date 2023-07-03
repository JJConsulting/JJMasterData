using System.Collections.Generic;
using JJMasterData.Core.DataDictionary;

namespace JJMasterData.Core.DataManager;

public interface IFormFieldsService
{
    /// <summary>
    /// Validates form fields and returns a list of errors found
    /// </summary>
    /// <param name="formElement">FormElement</param>
    /// <param name="formValues">Form values</param>
    /// <param name="pageState">Context</param>
    /// <param name="enableErrorLink">Add html link in error fields</param>
    /// <returns>
    /// Key = Field name
    /// Value = Error message
    /// </returns>
    IDictionary<string,dynamic>ValidateFields(FormElement formElement, IDictionary<string,dynamic> formValues, PageState pageState, bool enableErrorLink);

    /// <summary>
    /// Apply default and triggers expression values
    /// </summary>
    /// <param name="formElement"></param>
    /// <param name="formValues">Form values</param>
    /// <param name="pageState">Context</param>
    /// <param name="replaceNullValues">Change the field's default value even if it is empty</param>
    /// <returns>
    /// Returns a new hashtable with the updated values
    /// </returns>
    IDictionary<string,dynamic> MergeWithExpressionValues(FormElement formElement, IDictionary<string,dynamic> formValues, PageState pageState, bool replaceNullValues);

    IDictionary<string,dynamic> GetDefaultValues(FormElement formElement, IDictionary<string,dynamic> formValues, PageState state);
    IDictionary<string,dynamic> MergeWithDefaultValues(FormElement formElement, IDictionary<string,dynamic> formValues, PageState pageState);
}