#nullable enable
using System.Collections.Generic;
using System.Threading.Tasks;
using JJMasterData.Core.DataDictionary;

namespace JJMasterData.Core.DataManager;

public interface IFormValuesService
{
    IDictionary<string,dynamic> GetFormValues(
        FormElement formElement,
        PageState pageState, 
        string? fieldPrefix = null);
    
    /// <summary>
    /// Recover form values with database and expression values.
    /// </summary>
    /// <param name="formElement"></param>
    /// <param name="pageState"></param>
    /// <param name="autoReloadFormFields"></param>
    /// <param name="fieldPrefix"></param>
    /// <returns></returns>
    public Task<IDictionary<string, dynamic>> GetFormValuesWithMergedValues(
        FormElement formElement,
        PageState pageState,
        bool autoReloadFormFields,
        string? fieldPrefix = null);

    /// <summary>
    /// Recover form values with database values.
    /// </summary>
    /// <param name="formElement"></param>
    /// <param name="pageState"></param>
    /// <param name="autoReloadFormFields"></param>
    /// <param name="fieldPrefix"></param>
    /// <returns></returns>
    public Task<IDictionary<string,dynamic>> GetFormValuesWithMergedValues(
        FormElement formElement, 
        PageState pageState, 
        IDictionary<string,dynamic>? values,
        bool autoReloadFormFields,
        string? prefix = null);
}