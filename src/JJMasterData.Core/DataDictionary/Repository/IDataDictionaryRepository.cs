using System.Collections.Generic;

namespace JJMasterData.Core.DataDictionary.Repository;

/// <summary>
/// The repository of Data Dictionaries (metadata)
/// </summary>
public interface IDataDictionaryRepository
{
    /// <summary>
    /// Create Data Dictionary Structure
    /// </summary>
    void CreateStructureIfNotExists();

    /// <summary>
    /// Returns metadata stored in the database
    /// </summary>
    /// <returns>
    /// Returns Object stored in the database.
    /// Responsible for assembling the Element, FormElement and other layout settings
    /// </returns>
    Metadata GetMetadata(string dictionaryName);

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
    IEnumerable<Metadata> GetMetadataList(bool? sync = null);

    /// <summary>
    /// Retrieve the list of names from the dictionary
    /// </summary>
    IEnumerable<string> GetNameList();
    
    /// <summary>
    /// Returns records from the database based on the filter.    
    /// </summary>
    /// <param name="filters">Available filters</param>
    /// <param name="orderBy">Record Order, field followed by ASC or DESC</param>
    /// <param name="recordsPerPage">Number of records to be displayed per page</param>
    /// <param name="currentPage">Current page (start with 1)</param>
    /// <param name="totalRecords">If the value is zero, it returns as a reference the number of records based on the filter.</param>
    /// <returns>
    /// Returns a DataTable with the records found.
    /// If no record is found it returns null.
    /// </returns>
    IEnumerable<MetadataInfo> GetDataTable(DataDictionaryFilter filters, string orderBy, int recordsPerPage, int currentPage, ref int totalRecords);

    /// <summary>
    /// Checks if the dictionary exists
    /// </summary>
    bool Exists(string dictionaryName);

    /// <summary>
    /// Persist the dictionary
    /// </summary>
    void InsertOrReplace(Metadata metadata);

    /// <summary>
    /// Delete the dictionary
    /// </summary>
    void Delete(string dictionaryName);
}