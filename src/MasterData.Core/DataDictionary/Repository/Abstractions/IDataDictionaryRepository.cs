using System.Collections.Generic;
using System.Threading.Tasks;
using JJMasterData.Commons.Data.Entity.Repository;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.DataDictionary.Structure;

namespace JJMasterData.Core.DataDictionary.Repository.Abstractions;

/// <summary>
/// The repository of Data Dictionaries (metadata)
/// </summary>
public interface IDataDictionaryRepository
{
    Task CreateStructureIfNotExistsAsync();
    FormElement GetFormElement(string elementName);
    ValueTask<FormElement> GetFormElementAsync(string elementName);
    Task<List<FormElement>> GetFormElementListAsync(bool? apiSync = null);
    ValueTask<List<string>> GetElementNameListAsync();
    List<FormElement> GetFormElementList(bool? apiSync = null);

    Task<ListResult<FormElementInfo>> GetFormElementInfoListAsync(DataDictionaryFilter filters, OrderByData orderByData, int recordsPerPage, int currentPage);
    Task<bool> ExistsAsync(string elementName);
    Task InsertOrReplaceAsync(FormElement formElement);
    Task InsertOrReplaceAsync(IEnumerable<FormElement> formElements);
    void InsertOrReplace(FormElement formElement);
    Task DeleteAsync(string elementName);
}