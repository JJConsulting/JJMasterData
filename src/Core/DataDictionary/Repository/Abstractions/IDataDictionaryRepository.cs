using System.Collections.Generic;
using System.Threading.Tasks;
using JJMasterData.Commons.Data.Entity;
using JJMasterData.Commons.Data.Entity.Repository;

namespace JJMasterData.Core.DataDictionary.Repository.Abstractions;

/// <summary>
/// The repository of Data Dictionaries (metadata)
/// </summary>
/// BREAKING_CHANGE: Rename this to IFormElementRepository
public interface IDataDictionaryRepository
{
    Task CreateStructureIfNotExistsAsync();
    Task<FormElement> GetMetadataAsync(string elementName);
    Task<IEnumerable<FormElement>> GetMetadataListAsync(bool? apiEnabled = null);
    IAsyncEnumerable<string> GetNameListAsync();
    Task<ListResult<FormElementInfo>> GetFormElementInfoListAsync(DataDictionaryFilter filters, OrderByData orderByData, int recordsPerPage, int currentPage);
    Task<bool> ExistsAsync(string elementName);
    Task InsertOrReplaceAsync(FormElement metadata);
    Task DeleteAsync(string elementName);
}