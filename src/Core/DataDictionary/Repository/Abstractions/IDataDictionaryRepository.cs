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
    Task<FormElement> GetFormElementAsync(string elementName);
    Task<IEnumerable<FormElement>> GetFormElementListAsync(bool? apiEnabled = null);
    IAsyncEnumerable<string> GetNameListAsync();
    Task<ListResult<FormElementInfo>> GetFormElementInfoListAsync(DataDictionaryFilter filters, OrderByData orderByData, int recordsPerPage, int currentPage);
    Task<bool> ExistsAsync(string elementName);
    Task InsertOrReplaceAsync(FormElement metadata);
    Task DeleteAsync(string elementName);
}