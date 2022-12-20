#nullable disable

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text;
using System.Threading.Tasks;
using JJMasterData.Commons.Exceptions;
using JJMasterData.Commons.Extensions;
using JJMasterData.Commons.Options;

namespace JJMasterData.Commons.Dao;

/// <summary>
/// Classes that expose data access services and implements ADO methods.<br />
/// Provides functionality to developers who write managed code similar to the functionality provided to native component object model (COM)
/// </summary>
/// <example>
/// [!include[Test](../../../doc/JJMasterData.Documentation/articles/usages/dataaccess.md)]
/// </example>
public class DataAccess
{
    private DbProviderFactory _factory;
    private DbConnection _connection;
    private bool _keepAlive;

    ///<summary>
    ///Database connection string; 
    ///Default value configured in app.config as "ConnectionString";
    ///</summary>
    ///<returns>Connection string</returns>
    ///<remarks>
    ///Author: Lucio Pelinson 14-04-2012
    ///</remarks>
    public string ConnectionString { get; set; }

    ///<summary>
    ///Database Connection Provider; 
    ///Default value configured in app.config as "ConnectionString";
    ///</summary>
    ///<returns>Provider Name</returns>
    ///<remarks>
    ///Author: Lucio Pelinson 14-04-2012
    ///</remarks>
    public string ConnectionProvider { get; set; }

    /// <summary>
    /// Waiting time to execute a command on the database (seconds - default 240s)
    /// </summary>
    public int TimeOut { get; set; } = 240;

    /// <summary>
    /// Keeps the database connection open, 
    /// Allowing to execute a sequence of commands; 
    /// </summary>
    /// <example>
    /// <a href="https://portal.jjconsulting.com.br/jjdoc/articles/miscellaneous/dataaccess.html">To read more click here</a>
    /// </example>
    /// <remarks>
    /// Default value is false;
    /// </remarks>
    public bool KeepConnAlive
    {
        get => _keepAlive;
        set
        {
            _keepAlive = value;
            if (_keepAlive == false)
                CloseConnection();
        }
    }

    /// <summary>
    /// By default DataAccess recover connection string from appsettings.json with name ConnectionString
    /// </summary>
    public DataAccess()
    {
        ConnectionString = JJMasterDataOptions.GetConnectionString();
        ConnectionProvider = JJMasterDataOptions.GetConnectionProvider();
    }

    /// <summary>
    /// New instance from a custom connection string name
    /// </summary>
    /// <param name="connectionStringName">Name of connection string in appsettings.json or webconfig.xml file</param>
    public DataAccess(string connectionStringName)
    {
        ConnectionString = JJMasterDataOptions.GetConnectionString(connectionStringName);
        ConnectionProvider = JJMasterDataOptions.GetConnectionProvider(connectionStringName);
    }

    /// <summary>
    /// Initialize a connectionString with a specific providerName.
    /// See also <see cref="DataAccessProvider"/>.
    /// </summary>
    /// <param name="connectionString">Conections string with data source, user etc...</param>
    /// <param name="connectionProviderName">Provider name. Avaliable provider see <see cref="DataAccessProviderType"/></param>
    public DataAccess(string connectionString, string connectionProviderName)
    {
        ConnectionString = connectionString;
        ConnectionProvider = connectionProviderName;
    }

    public DbProviderFactory GetFactory()
    {
        if (_factory != null) return _factory;

        if (ConnectionString == null)
        {
            var error = new StringBuilder();
            error.AppendLine("Connection string not found in configuration file.");
            error.AppendLine("Default connection name is [ConnectionString].");
            error.AppendLine("Please check JJ001 for more information.");
            error.Append("https://portal.jjconsulting.com.br/jjdoc/articles/errors/jj001.html");
            throw new DataAccessException(error.ToString());
        }

        if (ConnectionProvider == null)
        {
            var error = new StringBuilder();
            error.AppendLine("Connection provider not found in configuration file.");
            error.Append("Default connection name is [ConnectionString]");
            throw new DataAccessException(error.ToString());
        }

        try
        {
            _factory = DataAccessProvider.GetDbProviderFactory(ConnectionProvider);
        }
        catch (DataAccessProviderException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new DataAccessException(ex);
        }

        return _factory;
    }

    public DbConnection GetConnection()
    {
        _connection ??= GetFactory().CreateConnection();

        if (_connection?.State is ConnectionState.Open or ConnectionState.Connecting)
            return _connection;

        try
        {
            _connection!.ConnectionString = ConnectionString;
            _connection.Open();
        }
        catch (Exception ex)
        {
            throw new DataAccessException(ex);
        }

        return _connection;
    }

    public async Task<DbConnection> GetConnectionAsync()
    {
        _connection ??= GetFactory().CreateConnection();

        if (_connection?.State is ConnectionState.Open or ConnectionState.Connecting)
            return _connection;

        try
        {
            _connection!.ConnectionString = ConnectionString;
            await _connection.OpenAsync();
        }
        catch (Exception ex)
        {
            throw new DataAccessException(ex);
        }

        return _connection;
    }

    public void CloseConnection()
    {
        if (KeepConnAlive)
            return;

        if (_connection == null)
            return;

        if (_connection.State == ConnectionState.Open)
            _connection.Close();

        _connection?.Dispose();
        _connection = null;
    }

    /// <summary>
    /// Returns a DataTable object populated from a sql.
    /// </summary>
    /// <returns>
    /// Returns a DataTable object populated by a query with parameters
    /// </returns>
    public DataTable GetDataTable(string sql)
    {
        return GetDataTable(new DataAccessCommand(sql));
    }

    /// <summary>
    /// Returns a DataTable object populated from a sql command.
    /// </summary>
    /// <returns>
    /// Returns a DataTable object populated by a <see cref="DataAccessCommand"/> with parameters
    /// </returns>
    public DataTable GetDataTable(DataAccessCommand cmd)
    {
        DataTable dt = new DataTable();
        try
        {
            using var dbCommand = CreateDbCommand(cmd);
            dbCommand.Connection = GetConnection();

            using var dataAdapter = GetFactory().CreateDataAdapter();
            dataAdapter!.SelectCommand = dbCommand;
            dataAdapter.Fill(dt);

            if (cmd.Parameters != null)
            {
                foreach (var parameter in cmd.Parameters)
                {
                    if (parameter.Direction is ParameterDirection.Output or ParameterDirection.InputOutput)
                        parameter.Value = dbCommand.Parameters[parameter.Name].Value;
                }
            }
        }
        catch (Exception ex)
        {
            throw GetDataAccessException(ex, cmd);
        }
        finally
        {
            CloseConnection();
        }

        return dt;
    }

    ///<inheritdoc cref="GetDataTable(string)"/>
    public async Task<DataTable> GetDataTableAsync(string sql)
    {
        return await GetDataTableAsync(new DataAccessCommand(sql));
    }

    ///<inheritdoc cref="GetDataTable(DataAccessCommand)"/>
    public async Task<DataTable> GetDataTableAsync(DataAccessCommand cmd)
    {
        var dt = new DataTable();
        try
        {
            using var dbCommand = CreateDbCommand(cmd);
            dbCommand.Connection = await GetConnectionAsync();

            using var dataAdapter = GetFactory().CreateDataAdapter();
            dataAdapter!.SelectCommand = dbCommand;
            dataAdapter.Fill(dt);

            if (cmd.Parameters != null)
            {
                foreach (var param in cmd.Parameters)
                {
                    if (param.Direction is ParameterDirection.Output or ParameterDirection.InputOutput)
                        param.Value = dbCommand.Parameters[param.Name].Value;
                }
            }
        }
        catch (Exception ex)
        {
            throw GetDataAccessException(ex, cmd);
        }
        finally
        {
            CloseConnection();
        }

        return dt;
    }

    /// <summary>
    /// Returns a DataTable object populated from a sql.
    /// </summary>
    /// <param name="sqlConn">Open Connection</param>
    /// <param name="sql">Script sql, never use with concat parameters</param>
    public DataTable GetDataTable(ref DbConnection sqlConn, string sql)
    {
        var dt = new DataTable();
        try
        {
            using var dbCommand = GetFactory().CreateCommand();
            dbCommand!.CommandType = CommandType.Text;
            dbCommand.Connection = sqlConn;
            dbCommand.CommandText = sql;
            dbCommand.CommandTimeout = TimeOut;

            using var dataAdapter = GetFactory().CreateDataAdapter();
            dataAdapter!.SelectCommand = dbCommand;
            dataAdapter.Fill(dt);
        }
        catch (Exception ex)
        {
            throw GetDataAccessException(ex, sql);
        }

        return dt;
    }

    /// <summary>
    /// ExecuteScalar command and returns the first column of the first row in the result set returned by the query.
    /// All other columns and rows are ignored.
    /// </summary>
    /// <remarks>
    /// To execute command with parameters and prevent SQL injection, please use the DataAccessCommand overload.
    /// </remarks>
    public object GetResult(string sql)
    {
        return GetResult(new DataAccessCommand(sql));
    }

    /// <summary>
    /// ExecuteScalar command and returns the first column of the first row in the result set returned by the query.
    /// All other columns and rows are ignored.
    /// </summary>
    public object GetResult(DataAccessCommand cmd)
    {
        object scalarResult;
        try
        {
            using var dbCommand = CreateDbCommand(cmd);
            dbCommand.Connection = GetConnection();
            scalarResult = dbCommand.ExecuteScalar();

            foreach (var param in cmd.Parameters)
            {
                if (param.Direction is ParameterDirection.Output or ParameterDirection.InputOutput)
                    param.Value = dbCommand.Parameters[param.Name].Value;
            }
        }
        catch (Exception ex)
        {
            throw GetDataAccessException(ex, cmd);
        }
        finally
        {
            CloseConnection();
        }

        return scalarResult;
    }

    /// <inheritdoc cref="GetResult(string)"/>
    public async Task<object> GetResultAsync(string sql)
    {
        return await GetResultAsync(new DataAccessCommand(sql));
    }

    /// <inheritdoc cref="GetResult(DataAccessCommand)"/>
    public async Task<object> GetResultAsync(DataAccessCommand cmd)
    {
        object scalarResult;
        try
        {
            using var dbCommand = CreateDbCommand(cmd);
            dbCommand.Connection = await GetConnectionAsync();
            scalarResult = await dbCommand.ExecuteScalarAsync();

            foreach (var parameter in cmd.Parameters)
            {
                if (parameter.Direction is ParameterDirection.Output or ParameterDirection.InputOutput)
                    parameter.Value = dbCommand.Parameters[parameter.Name].Value;
            }
        }
        catch (Exception ex)
        {
            throw GetDataAccessException(ex, cmd);
        }
        finally
        {
            CloseConnection();
        }

        return scalarResult;
    }

    /// <summary>
    /// ExecuteScalar command and returns the first column of the first row in the result set returned by the query.
    /// All other columns and rows are ignored.
    /// </summary>
    /// <param name="cmd">Command</param>
    /// <param name="sqlConn">Open Connection</param>
    /// <param name="trans">Transactions with Connection</param>
    /// <returns>Returns a DataTable object populated by a <see cref="DataAccessCommand"/>.
    /// This method uses a <see cref="DbConnection"/> by ref.
    /// </returns>
    public object GetResult(DataAccessCommand cmd, ref DbConnection sqlConn, ref DbTransaction trans)
    {
        object scalarResult;
        try
        {
            using var dbCommand = CreateDbCommand(cmd);
            dbCommand.Connection = sqlConn;
            dbCommand.Transaction = trans;
            scalarResult = dbCommand.ExecuteScalar();
        }
        catch (Exception ex)
        {
            throw GetDataAccessException(ex, cmd);
        }

        return scalarResult;
    }

    /// <summary>
    /// ExecuteNonQuery command in the database and return the number of affected records.
    /// </summary>
    public int SetCommand(DataAccessCommand cmd)
    {
        int rowsAffected = 0;
        try
        {
            using var dbCommand = CreateDbCommand(cmd);
            dbCommand.Connection = GetConnection();

            rowsAffected += dbCommand.ExecuteNonQuery();

            foreach (DataAccessParameter parameter in cmd.Parameters)
            {
                if (parameter.Direction == ParameterDirection.Output ||
                    parameter.Direction == ParameterDirection.InputOutput)
                    parameter.Value = dbCommand.Parameters[parameter.Name].Value;
            }
        }
        catch (Exception ex)
        {
            throw GetDataAccessException(ex, cmd);
        }
        finally
        {
            CloseConnection();
        }

        return rowsAffected;
    }

    /// <inheritdoc cref="SetCommand(DataAccessCommand)"/>
    public async Task<int> SetCommandAsync(DataAccessCommand cmd)
    {
        int rowsAffected;
        try
        {
            using var dbCommand = CreateDbCommand(cmd);
            dbCommand.Connection = await GetConnectionAsync();

            rowsAffected = await dbCommand.ExecuteNonQueryAsync();

            foreach (DataAccessParameter parameter in cmd.Parameters)
            {
                if (parameter.Direction is ParameterDirection.Output or ParameterDirection.InputOutput)
                    parameter.Value = dbCommand.Parameters[parameter.Name].Value;
            }
        }
        catch (Exception ex)
        {
            throw GetDataAccessException(ex, cmd);
        }
        finally
        {
            CloseConnection();
        }

        return rowsAffected;
    }

    /// <summary>Runs one or more commands on the database with transactions.</summary>
    /// <returns>Returns the number of affected records.</returns>
    /// <remarks>Author: Lucio Pelinson 14-04-2012</remarks>
    public int SetCommand(List<DataAccessCommand> commands)
    {
        int numberOfRowsAffected = 0;
        int index = 0;

        DbConnection connection = GetConnection();
        using var sqlTras = connection.BeginTransaction();
        try
        {
            foreach (var cmd in commands)
            {
                using var dbCommand = CreateDbCommand(cmd);
                dbCommand.Connection = connection;
                dbCommand.Transaction = sqlTras;

                numberOfRowsAffected += dbCommand.ExecuteNonQuery();
                index++;
            }

            sqlTras.Commit();
        }
        catch (Exception ex)
        {
            sqlTras.Rollback();
            var cmd = commands[index];
            throw GetDataAccessException(ex, cmd);
        }
        finally
        {
            CloseConnection();
        }

        return numberOfRowsAffected;
    }

    public async Task<int> SetCommandAsync(List<DataAccessCommand> commands)
    {
        int numberOfRowsAffected = 0;
        int index = 0;

        var connection = await GetConnectionAsync();
        using var transaction = connection.BeginTransaction();
        try
        {
            foreach (DataAccessCommand cmd in commands)
            {
                using var dbCommand = CreateDbCommand(cmd);
                dbCommand.Connection = connection;
                dbCommand.Transaction = transaction;

                numberOfRowsAffected += await dbCommand.ExecuteNonQueryAsync();
                index++;
            }

            transaction.Commit();
        }
        catch (Exception ex)
        {
            transaction.Rollback();
            var cmd = commands[index];
            throw GetDataAccessException(ex, cmd);
        }
        finally
        {
            CloseConnection();
        }

        return numberOfRowsAffected;
    }

    /// <summary>
    /// Execute the command in the database and return the number of affected records.
    /// </summary>
    public int SetCommand(string sql)
    {
        return SetCommand(new DataAccessCommand(sql));
    }

    /// <inheritdoc cref="SetCommand(string)"/>
    public async Task<int> SetCommandAsync(string sql)
    {
        return await SetCommandAsync(new DataAccessCommand(sql));
    }

    /// <summary>Runs one or more commands on the database with transactions.</summary>
    /// <returns>Returns the number of affected records.</returns>
    /// <remarks>Author: Lucio Pelinson 14-04-2012</remarks>
    public int SetCommand(ArrayList sqlList)
    {
        var cmdList = new List<DataAccessCommand>();
        foreach (string sql in sqlList)
        {
            cmdList.Add(new DataAccessCommand(sql));
        }

        int numberOfRowsAffected = SetCommand(cmdList);
        return numberOfRowsAffected;
    }

    /// <inheritdoc cref="SetCommand(ArrayList)"/>
    public async Task<int> SetCommandAsync(ArrayList sqlList)
    {
        var cmdList = new List<DataAccessCommand>();
        foreach (string sql in sqlList)
        {
            cmdList.Add(new DataAccessCommand(sql));
        }

        int numberOfRowsAffected = await SetCommandAsync(cmdList);
        return numberOfRowsAffected;
    }

    /// <summary>
    /// Execute the command in the database and return the number of affected records.
    /// </summary>
    /// <param name="cmd">Command</param>
    /// <param name="sqlConn">Open Connection</param>
    /// <param name="trans">Transactions with Connection</param>
    /// <returns>
    /// Returns a DataTable object populated by a <see cref="DataAccessCommand"/>.
    /// This method uses a <see cref="DbConnection"/> and a <see cref="DbTransaction"/> by ref.
    /// </returns>
    public int SetCommand(DataAccessCommand cmd, ref DbConnection sqlConn, ref DbTransaction trans)
    {
        int numberOfRowsAffected = 0;
        try
        {
            using var dbCommand = CreateDbCommand(cmd);
            dbCommand.Connection = GetConnection();
            dbCommand.Transaction = trans;

            numberOfRowsAffected += dbCommand.ExecuteNonQuery();
        }
        catch (Exception ex)
        {
            throw GetDataAccessException(ex, cmd);
        }

        return numberOfRowsAffected;
    }

    /// <summary>
    /// Retrieves the first record of the sql statement in a Hashtable object.
    /// [key(database field), value(value stored in database)] 
    /// </summary>
    /// <returns>
    /// Return a Hashtable Object. 
    /// If no record is found it returns null.
    /// </returns>
    public Hashtable GetFields(string sql) => GetFields(new DataAccessCommand(sql));

    /// <inheritdoc cref="GetFields(string)"/>
    public Task<Hashtable> GetFieldsAsync(string sql) => GetFieldsAsync(new DataAccessCommand(sql));

    /// <summary>
    /// Retrieves the first record of the sql statement in a Hashtable object.
    /// [key(database field), value(value stored in database)]
    /// </summary>
    /// <param name="cmd">Command</param>
    /// <returns>
    /// Return a Hashtable Object. 
    /// If no record is found it returns null.
    /// </returns>
    public Hashtable GetFields(DataAccessCommand cmd)
    {
        Hashtable retCollection = null;
        try
        {
            using var dbCommand = CreateDbCommand(cmd);
            dbCommand.Connection = GetConnection();

            using var dr = dbCommand.ExecuteReader(CommandBehavior.SingleRow);

            while (dr.Read())
            {
                retCollection = new Hashtable();
                int nQtd = 0;

                while (nQtd < dr.FieldCount)
                {
                    string fieldName = dr.GetName(nQtd);
                    if (retCollection.ContainsKey(fieldName))
                        throw new DataAccessException($"[{fieldName}] field duplicated in get procedure");

                    retCollection.Add(fieldName, dr.GetValue(nQtd));
                    nQtd += 1;
                }
            }

            if (!dr.IsClosed)
                dr.Close();

            foreach (var parameter in cmd.Parameters)
            {
                if (parameter.Direction is ParameterDirection.Output or ParameterDirection.InputOutput)
                    parameter.Value = dbCommand.Parameters[parameter.Name].Value;
            }
        }
        catch (Exception ex)
        {
            throw GetDataAccessException(ex, cmd);
        }
        finally
        {
            CloseConnection();
        }

        return retCollection;
    }

    /// <inheritdoc cref="GetFields(DataAccessCommand)"/>
    public async Task<Hashtable> GetFieldsAsync(DataAccessCommand cmd)
    {
        Hashtable retCollection = null;
        try
        {
            using var dbCommand = CreateDbCommand(cmd);
            dbCommand.Connection = await GetConnectionAsync();
            DbDataReader dr = await dbCommand.ExecuteReaderAsync(CommandBehavior.SingleRow);
            while (await dr.ReadAsync())
            {
                retCollection = new Hashtable();
                int nQtd = 0;

                while (nQtd < dr.FieldCount)
                {
                    string fieldName = dr.GetName(nQtd);
                    if (retCollection.ContainsKey(fieldName))
                        throw new DataAccessException($"[{fieldName}] field duplicated in get procedure");

                    retCollection.Add(fieldName, dr.GetValue(nQtd));
                    nQtd += 1;
                }
            }

            if (!dr.IsClosed)
                dr.Close();

            foreach (DataAccessParameter parameter in cmd.Parameters)
            {
                if (parameter.Direction is ParameterDirection.Output or ParameterDirection.InputOutput)
                    parameter.Value = dbCommand.Parameters[parameter.Name].Value;
            }
        }
        catch (Exception ex)
        {
            throw GetDataAccessException(ex, cmd);
        }
        finally
        {
            CloseConnection();
        }

        return retCollection;
    }

    private static DataAccessCommand GetTableExistsCommand(string table)
    {
        const string sql = "SELECT COUNT(1) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = @Table";
        var command = new DataAccessCommand
        {
            Sql = sql,
            Parameters =
            {
                new DataAccessParameter
                {
                    Name = "@Table",
                    Value = table
                }
            }
        };

        return command;
    }

    /// <summary>
    /// Check if table exists in the database
    /// </summary>
    public bool TableExists(string tableName)
    {
        bool result;
        try
        {
            var ret = GetResult(GetTableExistsCommand(tableName));
            result = ret as int? == 1;
        }
        finally
        {
            CloseConnection();
        }

        return result;
    }

    /// <inheritdoc cref="TableExists"/>
    public async Task<bool> TableExistsAsync(string tableName)
    {
        bool result;
        try
        {
            result = (int)await GetResultAsync(GetTableExistsCommand(tableName)) == 1;
        }
        finally
        {
            CloseConnection();
        }

        return result;
    }

    /// <summary>Verify the database connection</summary>
    /// <returns>True if the connection is successful.</returns>
    /// <remarks>Author: Lucio Pelinson 28-04-2014</remarks>
    public bool TryConnection(out string errorMessage)
    {
        bool result;
        DbConnection connection = null;
        errorMessage = null;
        try
        {
            connection = GetFactory().CreateConnection();
            connection!.ConnectionString = ConnectionString;
            connection.Open();
            result = true;
        }
        catch (Exception ex)
        {
            var error = new StringBuilder();
            error.AppendLine(ex.Message);
            if (ex.InnerException is { Message: { } })
                error.Append(ex.InnerException.Message);

            errorMessage = error.ToString();
            result = false;
        }
        finally
        {
            if (connection != null)
            {
                if (connection.State == ConnectionState.Open)
                {
                    connection.Close();
                }

                connection.Dispose();
            }
        }

        return result;
    }

    /// <inheritdoc cref="TryConnection"/>
    public async Task<(bool, string)> TryConnectionAsync()
    {
        bool result;
        DbConnection connection = null;
        string errorMessage = null;
        try
        {
            connection = GetFactory().CreateConnection();
            connection!.ConnectionString = ConnectionString;
            await connection.OpenAsync();
            result = true;
        }
        catch (Exception ex)
        {
            result = false;
            var error = new StringBuilder();
            error.AppendLine(ex.Message);
            if (ex.InnerException is { Message: { } })
                error.Append(ex.InnerException.Message);

            errorMessage = error.ToString();
        }
        finally
        {
            if (connection != null)
            {
                if (connection.State == ConnectionState.Open)
                {
                    connection.Close();
                }

                connection.Dispose();
            }
        }

        return (result, errorMessage);
    }

    /// <summary>Executes a database script.</summary>
    /// <returns>Returns true if the execution is successful.</returns>
    /// <remarks>Lucio Pelinson 18-02-2013</remarks> 
    public bool ExecuteBatch(string script)
    {
        string markpar = "GO";
        if (ConnectionProvider == DataAccessProviderType.Oracle.GetDescription() ||
            ConnectionProvider == DataAccessProviderType.OracleNetCore.GetDescription())
        {
            markpar = "/";
        }

        if (script.Trim().Length > 0)
        {
            var aSql = new ArrayList();
            string sqlBatch = string.Empty;
            script += "\n" + markpar; // make sure last batch is executed. 

            foreach (string line in script.Split(new[] { "\n", "\r" }, StringSplitOptions.RemoveEmptyEntries))
            {
                if (line.ToUpperInvariant().Trim() == markpar)
                {
                    if (sqlBatch.Trim().Length > 0)
                    {
                        aSql.Add(sqlBatch);
                    }

                    sqlBatch = string.Empty;
                }
                else
                {
                    sqlBatch += line + "\n";
                }
            }

            SetCommand(aSql);
        }

        return true;
    }

    /// <inheritdoc cref="ExecuteBatch(string)"/>
    public async Task<bool> ExecuteBatchAsync(string script)
    {
        string markpar = "GO";
        if (ConnectionProvider == DataAccessProviderType.Oracle.GetDescription() ||
            ConnectionProvider == DataAccessProviderType.OracleNetCore.GetDescription())
        {
            markpar = "/";
        }

        if (script.Trim().Length <= 0) return await Task.FromResult(true);

        var arrayList = new ArrayList();
        string sqlBatch = string.Empty;
        script += "\n" + markpar; // make sure last batch is executed. 

        foreach (string line in script.Split(new[] { "\n", "\r" }, StringSplitOptions.RemoveEmptyEntries))
        {
            if (line.ToUpperInvariant().Trim() == markpar)
            {
                if (sqlBatch.Trim().Length > 0)
                {
                    arrayList.Add(sqlBatch);
                }

                sqlBatch = string.Empty;
            }
            else
            {
                sqlBatch += line + "\n";
            }
        }

        await SetCommandAsync(arrayList);
        return await Task.FromResult(true);
    }

    private static Exception GetDataAccessException(Exception ex, DataAccessCommand cmd)
    {
        return GetDataAccessException(ex, cmd.Sql, cmd.Parameters);
    }

    private static Exception GetDataAccessException(Exception ex, string sql,
        List<DataAccessParameter> parameters = null)
    {
        ex.Data.Add("DataAccess Query", sql);

        if (parameters?.Count > 0)
        {
            var error = new StringBuilder();
            foreach (var param in parameters)
            {
                error.Append(param.Name);
                error.Append(" = ");
                error.Append(param.Value);
                error.Append(" [");
                error.Append(param.Type.ToString());
                error.AppendLine("]");
            }

            ex.Data.Add("DataAccess Parameters", error.ToString());

            return ex;
        }

        return ex;
    }

    private DbCommand CreateDbCommand(DataAccessCommand command)
    {
        var dbCommand = GetFactory().CreateCommand();
        if (dbCommand == null)
            throw new ArgumentNullException(nameof(dbCommand));

        dbCommand.CommandType = command.CmdType;
        dbCommand.CommandText = command.Sql;
        dbCommand.CommandTimeout = TimeOut;
        foreach (var parameter in command.Parameters)
        {
            var dbParameter = CreateDbParameter(parameter);
            dbCommand.Parameters.Add(dbParameter);
        }

        return dbCommand;
    }

    private DbParameter CreateDbParameter(DataAccessParameter parameter)
    {
        var dbParameter = GetFactory().CreateParameter();
        dbParameter!.DbType = parameter.Type;
        dbParameter.Value = parameter.Value ?? DBNull.Value;
        dbParameter.ParameterName = parameter.Name;
        dbParameter.Direction = parameter.Direction;
        dbParameter.IsNullable = true;

        if (parameter.Size > 0)
            dbParameter.Size = parameter.Size;

        return dbParameter;
    }
}