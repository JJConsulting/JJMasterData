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
    Task<FormElement> GetFormElementAsync(string elementName);
    Task<List<FormElement>> GetFormElementListAsync(bool? apiSync = null);
    Task<List<string>> GetNameListAsync();
    List<FormElement> GetFormElementList(bool? apiSync = null);

    Task<ListResult<FormElementInfo>> GetFormElementInfoListAsync(DataDictionaryFilter filters, OrderByData orderByData, int recordsPerPage, int currentPage);
    Task<bool> ExistsAsync(string elementName);
    Task InsertOrReplaceAsync(FormElement metadata);
    void InsertOrReplace(FormElement formElement);
    Task DeleteAsync(string elementName);
}