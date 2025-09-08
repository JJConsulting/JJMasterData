#nullable enable

using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using JJMasterData.Commons.Data.Entity.Models;

namespace JJMasterData.Commons.Data.Entity.Repository.Abstractions;

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
    /// Update a record in the database
    /// </summary>
    public Task<int> UpdateAsync(Element element, Dictionary<string,object?> values);
    int Update(Element element, Dictionary<string, object?> values);


    /// <summary>
    /// Delete records based on filter.
    /// </summary>
    public Task<int> DeleteAsync(Element element, Dictionary<string,object> primaryKeys);
    int Delete(Element element, Dictionary<string, object> primaryKeys);
    
    void Insert(Element element, Dictionary<string, object?> values);
    public int Insert(Element element, IEnumerable<Dictionary<string, object?>> values);

    
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
    public Task InsertAsync(Element element, Dictionary<string,object?> values);
    public Task<int> InsertAsync(Element element, IEnumerable<Dictionary<string, object?>> values);
    
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
    public Task<CommandOperation> SetValuesAsync(Element element, Dictionary<string,object?> values, bool ignoreResults = false);
    CommandOperation SetValues(Element element, Dictionary<string, object?> values, bool ignoreResults = false);

    public Task SetValuesAsync(Element element, IEnumerable<Dictionary<string,object?>> values);

    
    void CreateDataModel(Element element,List<RelationshipReference>? relationships = null);
    
    /// <summary>
    /// Create an element's tables and procedures
    /// </summary>
    public Task CreateDataModelAsync(Element element,List<RelationshipReference>? relationships = null);
    
    /// <summary>
    /// Build a structure script to create table
    /// </summary>
    public string GetCreateTableScript(Element element,List<RelationshipReference>? relationships = null);

    /// <summary>
    /// Build a structure script to procedure of get
    /// </summary>
    public string? GetReadProcedureScript(Element element);

    /// <summary>
    /// Build a structure script to procedure of set
    /// </summary>
    public string? GetWriteProcedureScript(Element element);
    
    public Task<string?> GetAlterTableScriptAsync(Element element);

    Dictionary<string, object?> GetFields(Element element, Dictionary<string, object> primaryKeys);

    
    Task<Dictionary<string, object?>> GetFieldsAsync(Element element, Dictionary<string, object> primaryKeys);
    
    /// <summary>
    /// Returns records from the database based on the filter.  
    /// </summary>
    /// <returns>
    /// Returns a DictionaryListResult with the records found and the count of records at your data source.
    /// </returns>
    Task<DictionaryListResult> GetDictionaryListResultAsync(
        Element element,
        EntityParameters? parameters = null,
        bool recoverTotalOfRecords = true);
    
    DictionaryListResult GetDictionaryListResult(
        Element element,
        EntityParameters? parameters = null,
        bool recoverTotalOfRecords = true
        );
    
    List<Dictionary<string,object?>> GetDictionaryList(
        Element element,
        EntityParameters? parameters = null
    );
    
    Task<List<Dictionary<string,object?>>> GetDictionaryListAsync(
        Element element,
        EntityParameters? parameters = null
    );
    
    Task<DataTable> GetDataTableAsync(Element element, EntityParameters? entityParameters = null);
    int GetCount(Element element, Dictionary<string, object?> filters);
    Task<int> GetCountAsync(Element element, Dictionary<string, object?> filters);
    
    
    /// <summary>
    /// Build an element from an existing table.
    /// </summary>
    Task<Element> GetElementFromTableAsync(string? schemaName, string tableName, Guid? connectionId = null);

        
    /// <summary>
    /// <inheritdoc cref="GetElementFromTableAsync(string,string,System.Guid?)"/>
    /// </summary>
    public Task<Element> GetElementFromTableAsync(string tableName, Guid? connectionId = null);

    
    /// <summary>
    /// Check if table exists in the database
    /// </summary>
    public Task<bool> TableExistsAsync(string? schema, string tableName, Guid? connectionId = null);
    
    /// <summary>
    /// Check if table exists in the database
    /// </summary>
    public Task<bool> TableExistsAsync(string tableName, Guid? connectionId = null);
    
    /// <summary>
    /// Returns a single sql command value with parameters
    /// </summary>
    /// <remarks>
    /// It's used to return sql expressions commands
    /// </remarks>
    public Task<object?> GetResultAsync(DataAccessCommand command, Guid? connectionId = null);
    
    /// <summary>
    /// Execute the command in the database.
    /// </summary>
    /// <remarks>
    /// It's used to run scripts from data dictionary at importation files
    /// </remarks>
    public Task SetCommandAsync(DataAccessCommand command, Guid? connectionId = null);
    
    /// <inheritdoc>
    ///     <cref>SetCommand(IEnumerable)</cref>
    /// </inheritdoc>
    public Task<int> SetCommandListAsync(IEnumerable<DataAccessCommand> commandList, Guid? connectionId = null);
    
    public Task<bool> ExecuteBatchAsync(string script, Guid? connectionId = null);

    Dictionary<string, object?> GetFields(DataAccessCommand command, Guid? connectionId = null);
    Task<Dictionary<string, object?>> GetFieldsAsync(DataAccessCommand command, Guid? connectionId = null);
    Task<List<Dictionary<string, object?>>> GetDictionaryListAsync(DataAccessCommand command, Guid? connectionId = null);
    DataTable GetDataTable(DataAccessCommand dataAccessCommand, Guid? connectionId = null);
    Task<DataTable> GetDataTableAsync(DataAccessCommand dataAccessCommand, Guid? connectionId = null);
    bool TableExists(string tableName, Guid? connectionId = null);

    DataSet GetDataSet(DataAccessCommand command, Guid? connectionId = null);
    
    Task<DataSet> GetDataSetAsync(DataAccessCommand command, Guid? connectionId = null, CancellationToken cancellationToken = default);
    
    public Task<List<string>> GetStoredProcedureListAsync(Guid? connectionId = null);

    public Task DropStoredProcedureAsync(string procedureName, Guid? connectionId = null);
    public Task<string?> GetStoredProcedureDefinitionAsync(string procedureName, Guid? connectionId = null);
}