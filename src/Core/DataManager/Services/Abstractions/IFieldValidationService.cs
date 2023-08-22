using System.Collections.Generic;
using System.Threading.Tasks;
using JJMasterData.Core.DataDictionary;

namespace JJMasterData.Core.DataManager;

public interface IFieldValidationService
{
    /// <summary>
    /// Validates form fields and returns a list of errors found
    /// </summary>
    /// <param name="formElement">FormElement</param>
    /// <param name="formValues">Form values</param>
    /// <param name="pageState">FormStateData</param>
    /// <param name="enableErrorLink">Add html link in error fields</param>
    /// <returns>
    /// Key = Field name
    /// Value = Error message
    /// </returns>
    Task<IDictionary<string, string>> ValidateFieldsAsync(FormElement formElement, IDictionary<string, object?> formValues, PageState pageState, bool enableErrorLink);
    string ValidateField(FormElementField field, string fieldId, string? value, bool enableErrorLink = true);
}