using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace JJMasterData.Commons.Data.Entity.Abstractions;

public interface IEntityRepository
{
    /// <summary>
    /// Returns database records based on filter.  
    /// </summary>
    /// <param name="element">Estruture basic from a table</param>
    /// <param name="filters">List of filters to be used. [key(database field), valor(value stored in database)]</param>
    /// <param name="orderBy">Record Order, field followed by ASC or DESC</param>
    /// <param name="recordsPerPage">Number of records to be displayed per page</param>
    /// <param name="currentPage">Current page</param>
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
    public string GetListFieldsAsText(Element element, IDictionary filters, string orderBy, int recordsPerPage, int currentPage, bool showLogInfo, string delimiter = "|");
    
    /// <inheritdoc cref="GetListFieldsAsText"/>
    // public Task<string> GetListFieldsAsTextAsync(Element element, IDictionary filters, string orderBy, int recordsPerPage, int currentPage, bool showLogInfo, string delimiter = "|");

    /// <summary>
    /// Returns records from the database based on the filter.    
    /// </summary>
    /// <param name="element">Base element with the basic structure of the table.</param>
    /// <param name="filters">List of filters to be used. [key(database field), value(value stored in database)]</param>
    /// <param name="orderBy">Record Order, field followed by ASC or DESC</param>
    /// <param name="recordsPerPage">Number of records to be displayed per page</param>
    /// <param name="currentPage">Current page</param>
    /// <param name="totalRecords">If the value is zero, it returns as a reference the number of records based on the filter.</param>
    /// <returns>
    /// Returns a DataTable with the records found.
    /// If no record is found it returns null.
    /// </returns>
    public DataTable GetDataTable(Element element, IDictionary filters, string orderBy, int recordsPerPage, int currentPage, ref int totalRecords);

    /// <inheritdoc cref="GetDataTable(Element, IDictionary, string , int ,int , ref int)"/>
    public Task<(DataTable, int)> GetDataTableAsync(Element element, IDictionary filters, string orderBy, int recordsPerPage, int currentPage, int totalRecords);
    
    /// <summary>
    /// Returns records from the database based on the filter.  
    /// </summary>
    /// <param name="element">Base element with the basic structure of the table.</param>
    /// <param name="filters">List of filters to be used. [key(database field), value(value stored in database)]</param>
    /// <returns>
    /// Returns a DataTable with the records found. 
    /// If no record is found it returns null.
    /// </returns>
    public DataTable GetDataTable(Element element, IDictionary filters);

    
    /// <inheritdoc cref=" GetDataTable(Element, IDictionary)"/>
    public Task<DataTable> GetDataTableAsync(Element element, IDictionary filters);

    
    /// <summary>
    /// Returns first record based on filter.  
    /// </summary>
    /// <param name="element">Base element with the basic structure of the table.</param>
    /// <param name="filters">List of filters to be used. [key(database field), valor(value stored in database)]</param>
    /// <returns>
    /// Return a Hashtable Object. 
    /// If no record is found then returns null.
    /// </returns>
    public Hashtable GetFields(Element element, IDictionary filters);

    /// <inheritdoc cref=" GetFieldsAsync(Element, IDictionary)"/>
    public Task<Hashtable> GetFieldsAsync(Element element, IDictionary filters);
    
    /// <summary>
    /// Returns the number of records in the database
    /// </summary>
    /// <param name="element">Base element with the basic structure of the table.</param>
    /// <param name="filters">List of filters to be used. [key(database field), valor(value stored in database)]</param>
    /// <returns>
    /// Returns an integer.
    /// </returns>
    public int GetCount(Element element, IDictionary filters);

    /// <inheritdoc cref=" GetCount(Element, IDictionary)"/>
    public Task<int> GetCountAsync(Element element, IDictionary filters);
    
    /// <summary>
    /// Update a record in the database 
    /// [key(database field name), value(value to be stored in the database)].
    /// </summary>
    /// <param name="element">Base element with a basic table structure</param>
    /// <param name="values">List of values ​​to be stored in the database</param>
    /// <returns>Return the number of the rows affected</returns>
    public int Update(Element element, IDictionary values);

    /// <inheritdoc cref="Update(Element, IDictionary)"/>
    public Task<int> UpdateAsync(Element element, IDictionary values);
    
    /// <summary>
    /// Delete records based on filter.  
    /// [key(database field), valor(value stored in database)].
    /// </summary>
    /// <param name="element">Base element with the basic structure of the table</param>
    /// <param name="filters">List of filters to be used</param>
    /// <returns>Return the number of the rows affected</returns>
    public int Delete(Element element, IDictionary filters);

    /// <inheritdoc cref="Delete(Element, IDictionary)"/>
    public Task<int> DeleteAsync(Element element, IDictionary filters);
    
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
    public void Insert(Element element, IDictionary values);
    
    /// <inheritdoc cref="Insert(Element, IDictionary)"/>
    public Task InsertAsync(Element element, IDictionary values);
    
    /// <summary>
    /// Insert or Update a record in the database.
    /// If it exists then update it, otherwise add.
    /// Include PK in Hashtable in case of indentity
    /// </summary>
    /// <returns>NONE=-1, INSERT=0, UPDATE=1, DELETE=2</returns>
    /// <param name="element">Base element with the basic structure of the table</param>
    /// <param name="values">List of values ​​to be stored in the database</param>
    /// <remarks>
    /// How to do:
    /// [key(database field name), value(value to be stored in the database)].
    /// </remarks>
    public CommandOperation SetValues(Element element, IDictionary values);
    
    /// <inheritdoc cref="SetValues(Element, IDictionary)"/>
    public Task<CommandOperation> SetValuesAsync(Element element, IDictionary values);

    /// <summary>
    /// Set a record in the database.
    /// If it exists then update it, otherwise add.
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
    public CommandOperation SetValues(Element element, IDictionary values, bool ignoreResults);

    /// <inheritdoc cref="SetValues(Element, IDictionary, bool)"/>
    public Task<CommandOperation> SetValuesAsync(Element element, IDictionary values, bool ignoreResults);
    
    /// <summary>
    /// Create an element's tables and procedures
    /// </summary>
    /// <param name="element">Element with table data</param>
    public void CreateDataModel(Element element);
    
    /// <inheritdoc cref="CreateDataModel"/>
    public Task CreateDataModelAsync(Element element);
    
    /// <summary>
    /// Build a struture script to create table
    /// </summary>
    public string GetScriptCreateTable(Element element);

    /// <summary>
    /// Build a struture script to procedure of get
    /// </summary>
    public string GetScriptReadProcedure(Element element);

    /// <summary>
    /// Build a struture script to procedure of set
    /// </summary>
    public string GetScriptWriteProcedure(Element element);

    /// <summary>
    /// Build a element from a existing table
    /// </summary>
    public Element GetElementFromTable(string tableName);

    /// <inheritdoc cref="GetElementFromTable(string)"/>
    public Task<Element> GetElementFromTableAsync(string tableName);
    
    ///<summary>
    ///Returns DataTable object populated by a query with parameters
    ///</summary>
    ///<returns>Returns DataTable object populated by a query with parameters</returns>
    ///<remarks>
    ///It's used to return a query from data dictionary to populate components how JJComboBox, JJSearchBox etc..
    ///</remarks>
    public DataTable GetDataTable(string sql);

    /// <inheritdoc cref="GetDataTable(string)"/>
    public Task<DataTable> GetDataTableAsync(string sql);
    
    /// <summary>
    /// Returns a single sql command value with parameters
    /// </summary>
    /// <remarks>
    /// It's used to return sql expressions commands
    /// </remarks>
    public object GetResult(string sql);
    
        
    /// <inheritdoc cref="GetResult"/>
    public Task<object> GetResultAsync(string sql);

    /// <summary>
    /// Check if table exists in the database
    /// </summary>
    public bool TableExists(string tableName);
    
    /// <inheritdoc cref="TableExists(string)"/>
    public Task<bool> TableExistsAsync(string tableName);

    /// <summary>
    /// Execute the command in the database.
    /// </summary>
    /// <remarks>
    /// It's used to run scripts from data dictionary at importation files
    /// </remarks>
    public void SetCommand(string sql);

    /// <inheritdoc cref="SetCommand(string)"/>
    public Task SetCommandAsync(string sql);
    
    /// <summary>
    /// Runs one or more commands on the database with transactions.
    /// </summary>>
    /// <remarks>
    /// It's used to run delete scripts
    /// </remarks>
    public int SetCommand(IEnumerable<string> sqlList);
    
        
    /// <inheritdoc cref="SetCommand(IEnumerable)"/>
    public Task<int> SetCommandAsync(IEnumerable<string> sqlList);

    /// <summary>
    /// Executes a database script.
    /// </summary>
    /// <returns>Retorns true if the execution is successful.</returns>
    /// <remarks>It's used to exec struture scripts</remarks> 
    public bool ExecuteBatch(string script);
    
    /// <inheritdoc cref="ExecuteBatch"/>
    public Task<bool> ExecuteBatchAsync(string script);

    Task<IDictionary<string, dynamic>> GetDictionaryAsync(Element metadata, IDictionary<string,dynamic> filters);
    Task<(List<Dictionary<string, dynamic>>, int)>  GetDictionaryListAsync(Element metadata, IDictionary parametersParameters, string toString, int paginationDataRecordsPerPage, int paginationDataPage, int i);
}