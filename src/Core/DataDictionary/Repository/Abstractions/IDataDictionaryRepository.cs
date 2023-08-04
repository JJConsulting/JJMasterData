using System.Collections.Generic;
using System.Threading.Tasks;
using JJMasterData.Commons.Data.Entity;

namespace JJMasterData.Core.DataDictionary.Repository.Abstractions;

/// <summary>
/// The repository of Data Dictionaries (metadata)
/// </summary>
/// BREAKING_CHANGE: Rename this to IFormElementRepository
public interface IDataDictionaryRepository
{
    /// <summary>
    /// Create Data Dictionary Structure
    /// </summary>
    void CreateStructureIfNotExists();
    
    /// <inheritdoc cref="CreateStructureIfNotExists"/>
    Task CreateStructureIfNotExistsAsync();

    /// <summary>
    /// Returns metadata stored in the database
    /// </summary>
    /// <returns>
    /// Returns Object stored in the database.
    /// Responsible for assembling the Element, FormElement and other layout settings
    /// </returns>
    FormElement GetMetadata(string dictionaryName);
    
    /// <inheritdoc cref="GetMetadata"/>
    Task<FormElement> GetMetadataAsync(string dictionaryName);
    
    /// <summary>
    /// Retrieves a list of metadata stored in the database
    /// </summary>
    /// <param name="sync">
    /// true=Only items that will be sync. 
    /// false=Only not sync items
    /// null=All
    /// </param>
    /// <remarks>
    /// Method normally used for synchronizing dictionaries between systems.
    /// Allowing to rebuild the original inheritance in the legacy system.
    /// </remarks>
    IEnumerable<FormElement> GetMetadataList(bool? sync = null);
    
    /// <inheritdoc cref="GetMetadataList"/>
    Task<IEnumerable<FormElement>> GetMetadataListAsync(bool? sync = null);
    
    /// <summary>
    /// Retrieve the list of names from the dictionary
    /// </summary>
    IEnumerable<string> GetNameList();
    
    /// <inheritdoc cref="GetNameList"/>
    IAsyncEnumerable<string> GetNameListAsync();
    
    /// <summary>
    /// Returns records from the database based on the filter.
    /// </summary>
    /// <param name="filters">Available filters</param>
    /// <param name="orderBy">Record Order, field followed by ASC or DESC</param>
    /// <param name="recordsPerPage">Number of records to be displayed per page</param>
    /// <param name="currentPage">Current page (start with 1)</param>
    /// <param name="totalRecords">If the value is zero, it returns as a reference the number of records based on the filter.</param>
    IEnumerable<FormElementInfo> GetMetadataInfoList(DataDictionaryFilter filters, string orderBy, int recordsPerPage, int currentPage, ref int totalRecords);

    /// <inheritdoc cref="GetMetadataInfoList"/>
    Task<EntityResult<IEnumerable<FormElementInfo>>> GetFormElementInfoListAsync(DataDictionaryFilter filters, string orderBy, int recordsPerPage, int currentPage);
    
    /// <summary>
    /// Checks if the dictionary exists
    /// </summary>
    bool Exists(string dictionaryName);

    /// <inheritdoc cref="Exists"/>
    Task<bool> ExistsAsync(string dictionaryName);
    
    /// <summary>
    /// Persist the dictionary
    /// </summary>
    void InsertOrReplace(FormElement formElement);

    /// <inheritdoc cref="InsertOrReplace"/>
    Task InsertOrReplaceAsync(FormElement metadata);
    
    /// <summary>
    /// Delete the dictionary
    /// </summary>
    void Delete(string dictionaryName);
    
    /// <inheritdoc cref="Delete"/>
    Task DeleteAsync(string dictionaryName);
}