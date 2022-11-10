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

    DataTable GetDataTable(string sql);

    DataTable GetDataTable(DataAccessCommand cmd);

    Task<DataTable> GetDataTableAsync(string sql);

    Task<DataTable> GetDataTableAsync(DataAccessCommand cmd);

    object GetResult(string sql);

    object GetResult(DataAccessCommand cmd);

    Task<object> GetResultAsync(string sql);

    Task<object> GetResultAsync(DataAccessCommand cmd);

    int SetCommand(DataAccessCommand cmd);

    int SetCommand(List<DataAccessCommand> commands);

    int SetCommand(string sql);

    int SetCommand(ArrayList sqlList);

    Task<int> SetCommandAsync(DataAccessCommand cmd);

    Task<int> SetCommandAsync(List<DataAccessCommand> commands);

    Task<int> SetCommandAsync(string sql);

    Task<int> SetCommandAsync(ArrayList sqlList);

    Hashtable GetFields(string sql);

    Hashtable GetFields(DataAccessCommand cmd);

    Task<Hashtable> GetFieldsAsync(string sql);

    Task<Hashtable> GetFieldsAsync(DataAccessCommand cmd);

    bool TableExists(string table);

    Task<bool> TableExistsAsync(string table);

    bool ExecuteBatch(string script);

    Task<bool> ExecuteBatchAsync(string script);
}