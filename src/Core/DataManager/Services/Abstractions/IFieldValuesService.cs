using System.Collections.Generic;
using System.Threading.Tasks;
using JJMasterData.Core.DataDictionary;

namespace JJMasterData.Core.DataManager;

public interface IFieldValuesService
{
    
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
    Task<IDictionary<string,dynamic>> MergeWithExpressionValuesAsync(FormElement formElement, IDictionary<string,dynamic> formValues, PageState pageState, bool replaceNullValues);

    Task<IDictionary<string,dynamic>> GetDefaultValuesAsync(FormElement formElement, IDictionary<string,dynamic> formValues, PageState state);
    Task<IDictionary<string,dynamic>> MergeWithDefaultValuesAsync(FormElement formElement, IDictionary<string,dynamic> formValues, PageState pageState);
}