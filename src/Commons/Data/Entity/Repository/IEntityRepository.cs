#nullable enable

using System.Collections.Generic;
using System.Threading.Tasks;
using JJMasterData.Commons.Data.Entity.Repository;

namespace JJMasterData.Commons.Data.Entity.Abstractions;

public interface IEntityRepository
{
    /// <summary>
    /// Returns database records based on filter.  
    /// </summary>
    /// <param name="element">Structure basic from a table</param>
    /// <param name="parameters"></param>
    /// <param name="showLogInfo">Records detailed log of each operation</param>
    /// <param name="delimiter">Field delimiter in text file (default is pipe)</param>
    /// <returns>
    /// Returns a string with one record per line separated by the delimiter.<para/>
    /// If no record is found it returns null. <para/>
    /// *Warning: <para/>
    /// - Some special characters will be replaced:<para/>
    ///   enter =  #182;<para/>
    ///   {delimiter} = #124;<para/>
    /// - Submitted formats:<para/>
    ///   Numbers = en-US<para/>
    ///   Date = yyyy-MM-dd HH:mm:ss
    /// </returns>
    public Task<string> GetListFieldsAsTextAsync(
        Element element, 
        EntityParameters? parameters = null,
        bool showLogInfo = false, 
        string delimiter = "|");
    

    /// <summary>
    /// Returns the number of records in the database
    /// </summary>
    /// </returns>
    public Task<int> GetCountAsync(Element element, IDictionary<string,object?> filters);

    /// <summary>
    /// Update a record in the database
    /// </summary>
    public Task<int> UpdateAsync(Element element, IDictionary<string,object?> values);
    

    /// <summary>
    /// Delete records based on filter.
    /// </summary>
    public Task<int> DeleteAsync(Element element, IDictionary<string,object> filters);
    
    /// <summary>
    /// Add a record to the database.
    /// Return the id in the values ​​field as a reference
    /// </summary>
    /// <param name="element">Base element with the basic structure of the table</param>
    /// <param name="values">List of values ​​to be stored in the database</param>
    /// <remarks>
    /// How to do:
    /// [key(database field name), value(value to be stored in the database)].
    /// </remarks>
    public Task InsertAsync(Element element, IDictionary<string,object?> values);
    
    /// <summary>
    /// Set a record in the database.
    /// If it exists then update it, otherwise add.
    /// Include PK in Hashtable in case of indentity
    /// </summary>
    /// <returns>NONE=-1, INSERT=0, UPDATE=1, DELETE=2</returns>
    /// <param name="element">Base element with the basic structure of the table</param>
    /// <param name="values">List of values to be stored in the database</param>
    /// <param name="ignoreResults">By default the values returned in the set procedures are returned by reference in the hashtable values object, 
    /// if this ignoreResults parameter is true this action is ignored, improving performance</param>
    /// <remarks>
    /// How to do:
    /// [key(database field name), value(value to be stored in the database)].
    /// </remarks>
    public Task<CommandOperation> SetValuesAsync(Element element, IDictionary<string,object?> values, bool ignoreResults = false);
    
    
    /// <summary>
    /// Create an element's tables and procedures
    /// </summary>
    public Task CreateDataModelAsync(Element element);
    
    /// <summary>
    /// Build a structure script to create table
    /// </summary>
    public string GetScriptCreateTable(Element element);

    /// <summary>
    /// Build a structure script to procedure of get
    /// </summary>
    public string? GetScriptReadProcedure(Element element);

    /// <summary>
    /// Build a structure script to procedure of set
    /// </summary>
    public string? GetScriptWriteProcedure(Element element);
    
    public Task<string> GetAlterTableScriptAsync(Element element);

    
    /// <summary>
    /// Build a element from a existing table
    /// </summary>
    public Task<Element> GetElementFromTableAsync(string tableName);
        
    /// <summary>
    /// Returns a single sql command value with parameters
    /// </summary>
    /// <remarks>
    /// It's used to return sql expressions commands
    /// </remarks>
    public Task<object?> GetResultAsync(DataAccessCommand command);
    
    /// <summary>
    /// Check if table exists in the database
    /// </summary>
    public Task<bool> TableExistsAsync(string tableName);
    

    /// <summary>
    /// Execute the command in the database.
    /// </summary>
    /// <remarks>
    /// It's used to run scripts from data dictionary at importation files
    /// </remarks>
    public Task SetCommandAsync(DataAccessCommand command);
    
    /// <inheritdoc>
    ///     <cref>SetCommand(IEnumerable)</cref>
    /// </inheritdoc>
    public Task<int> SetCommandListAsync(IEnumerable<DataAccessCommand> commandList);
    
    /// <inheritdoc cref="ExecuteBatch"/>
    public Task<bool> ExecuteBatchAsync(string script);
    
    Task<IDictionary<string, object?>> GetDictionaryAsync(DataAccessCommand command);
    
    Task<IDictionary<string, object?>> GetDictionaryAsync(Element metadata, IDictionary<string, object?> filters);
    
    Task<List<Dictionary<string, object?>>> GetDictionaryListAsync(DataAccessCommand command);
    
    /// <summary>
    /// Returns records from the database based on the filter.  
    /// </summary>
    /// <returns>
    /// Returns a DictionaryListResult with the records found and the count of records at your data source.
    /// </returns>
    Task<DictionaryListResult> GetDictionaryListAsync(
        Element element,
        EntityParameters? parameters = null,
        bool recoverTotalOfRecords = true);
}