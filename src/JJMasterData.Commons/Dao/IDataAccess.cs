using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;

namespace JJMasterData.Commons.Dao;

public interface IDataAccess
{
    string ConnectionString { get; set; }

    string ConnectionProvider { get; set; }

    int TimeOut { get; set; }

    public bool TranslateErrorMessage { get; set; }

    public bool GenerateLog { get; set; }
    IDataAccess WithParameters(string settingsConnectionStringName);
    IDataAccess WithParameters(string settingsConnectionString, string settingsConnectionProvider);
    DbProviderFactory GetFactory();
    DbConnection GetConnection();
    Task<DbConnection> GetConnectionAsync();
    void CloseConnection();

    DataTable GetDataTable(string sql, List<DataAccessParameter> parameters = null);

    DataTable GetDataTable(DataAccessCommand dataAccessCommand);

    Task<DataTable> GetDataTableAsync(string sql, List<DataAccessParameter> parameters = null);

    Task<DataTable> GetDataTableAsync(DataAccessCommand dataAccessCommand);

    object GetResult(string sql, List<DataAccessParameter> parameters = null);

    object GetResult(DataAccessCommand cmd);

    Task<object> GetResultAsync(string sql, List<DataAccessParameter> parameters = null);

    Task<object> GetResultAsync(DataAccessCommand cmd);

    int SetCommand(DataAccessCommand cmd);

    int SetCommand(List<DataAccessCommand> commands);

    int SetCommand(string sql, List<DataAccessParameter> parameters = null);

    int SetCommand(ArrayList sqlList);

    Task<int> SetCommandAsync(DataAccessCommand cmd);

    Task<int> SetCommandAsync(List<DataAccessCommand> commands);

    Task<int> SetCommandAsync(string sql, List<DataAccessParameter> parameters = null);

    Task<int> SetCommandAsync(ArrayList sqlList);

    Hashtable GetFields(string sql);

    Hashtable GetFields(DataAccessCommand cmd);

    Task<Hashtable> GetFieldsAsync(string sql);

    Task<Hashtable> GetFieldsAsync(DataAccessCommand cmd);

    bool TableExists(string table);

    Task<bool> TableExistsAsync(string table);

    bool ExecuteBatch(string script);

    Task<bool> ExecuteBatchAsync(string script);

    bool ValueExists(string tableName, string columnName, string value);

    bool ValueExists(string tableName, string columnName, int value);

    bool ValueExists(string tableName, params DataAccessParameter[] filters);

    Task<bool> ValueExistsAsync(string tableName, string columnName, string value);

    Task<bool> ValueExistsAsync(string tableName, string columnName, int value);

    Task<bool> ValueExistsAsync(string tableName, params DataAccessParameter[] filters);

    object GetValue(string tableName, string columnName, string value);

    object GetValue(string tableName, string columnName, int value);

    Task<object> GetValueAsync(string tableName, string columnName, string value);

    Task<object> GetValueAsync(string tableName, string columnName, int value);
}