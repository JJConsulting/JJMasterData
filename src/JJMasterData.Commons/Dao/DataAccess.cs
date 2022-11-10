#nullable disable

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JJMasterData.Commons.DI;
using JJMasterData.Commons.Exceptions;
using JJMasterData.Commons.Language;
using JJMasterData.Commons.Logging;
using JJMasterData.Commons.Util;

namespace JJMasterData.Commons.Dao;

public class DataAccess : IDataAccess
{
    private DbProviderFactory _factory;
    private DbConnection _connection;
    private bool _keepAlive;
    
    public bool TranslateErrorMessage { get; set; } = true;
    
    public bool GenerateLog { get; set; } = true;

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
    /// This example shows how the KeepConnAlive method should be used
    /// <code>
    /// class TestClass  
    /// { 
    ///     private void Test()
    ///     {
    ///         var dao = new DataAccess())
    ///         try
    ///         {
    ///             dao.KeepConnAlive = true;
    ///             dao.SetCommand("update table1 set ...");
    ///             dao.SetCommand("update table2 set ...");
    ///         }
    ///         catch (Exception ex)
    ///         {
    ///             //Do Log
    ///         }
    ///         finally
    ///         {
    ///            dao.KeepConnAlive = false;
    ///         }
    ///     }
    /// }
    /// </code> 
    /// </example>
    /// <remarks>
    /// Default value is false;
    /// Always run the Dispose() method;
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

    public DataAccess()
    {
        ConnectionString = JJService.Settings.ConnectionString;
        ConnectionProvider = JJService.Settings.ConnectionProvider;
    }

    public DataAccess(string connectionStringName)
    {
        ConnectionString = JJService.Settings.GetConnectionString(connectionStringName);
        ConnectionProvider = JJService.Settings.GetConnectionProvider(connectionStringName);
    }

    /// <summary>
    /// Initialize a connectionString with a specific providerName.
    /// See also <see cref="DataAccessProvider"/>.
    /// </summary>
    public DataAccess(string connectionString, string connectionProviderName)
    {
        ConnectionString = connectionString;
        ConnectionProvider = connectionProviderName;
    }

    public IDataAccess WithParameters(string connectionStringName)
    {
        return new DataAccess(connectionStringName);
    }

    public IDataAccess WithParameters(string connectionString, string connectionProvider)
    {
        return new DataAccess(connectionString, connectionProvider);
    }

    public DbProviderFactory GetFactory()
    {
        if (_factory != null) return _factory;

        if (ConnectionString == null)
        {
            var error = new StringBuilder();
            error.AppendLine(TranslateKey("Connection string not found in configuration file."));
            error.AppendLine(TranslateKey("Default connection name is [ConnectionString]."));
            error.AppendLine(TranslateKey("Please check JJ001 for more information."));
            error.Append("https://portal.jjconsulting.com.br/jjdoc/articles/errors/jj001.html");
            AddLog(error.ToString());
            throw new DataAccessException(error.ToString());
        }

        if (ConnectionProvider == null)
        {
            var error = new StringBuilder();
            error.AppendLine(TranslateKey("Connection provider not found in configuration file."));
            error.Append(TranslateKey("Default connection name is [ConnectionString]"));
            AddLog(error.ToString());
            throw new DataAccessException(error.ToString());
        }

        try
        {
            _factory = DataAccessProvider.GetDbProviderFactory(ConnectionProvider);
        }
        catch (Exception ex)
        {
            string sErr = TranslateKey("Error starting connection provider {0}. Error message: {1}", ConnectionProvider,
                ex.Message);
            AddLog(sErr);
            throw new DataAccessException(sErr);
        }

        return _factory;
    }

    public DbConnection GetConnection()
    {
        _connection ??= GetFactory().CreateConnection();

        if (_connection?.State == ConnectionState.Open) return _connection;

        try
        {
            _connection!.ConnectionString = ConnectionString;
            _connection.Open();
        }
        catch (Exception ex)
        {
            AddLog(ex.Message);
            throw;
        }

        return _connection;
    }

    public async Task<DbConnection> GetConnectionAsync()
    {
        _connection ??= GetFactory().CreateConnection();

        if (_connection?.State == ConnectionState.Open) return _connection;

        try
        {
            _connection!.ConnectionString = ConnectionString;
            await _connection.OpenAsync();
        }
        catch (Exception ex)
        {
            AddLog(ex.Message);
            throw;
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

    ///<summary>
    ///Returns DataTable object populated by a query with parameters
    ///</summary>
    ///<returns>Returns DataTable object populated by a query with parameters</returns>
    ///<remarks>Lucio Pelinson 14-04-2012</remarks>
    public DataTable GetDataTable(string sql)
    {
        return GetDataTable(new DataAccessCommand(sql));
    }


    ///<inheritdoc cref="GetDataTable(string,System.Collections.Generic.List{JJMasterData.Commons.Dao.DataAccessParameter})"/>
    public DataTable GetDataTable(DataAccessCommand dataAccessCommand)
    {
        DbCommand cmd = GetFactory().CreateCommand();
        DbDataAdapter da = null;
        DataTable dt = new DataTable();
        try
        {
            if (dataAccessCommand.Parameters != null)
            {
                foreach (var parm in dataAccessCommand.Parameters)
                {
                    var dbParameter = GetFactory().CreateParameter();
                    dbParameter!.DbType = parm.Type;
                    dbParameter.Value = parm.Value ?? DBNull.Value;
                    dbParameter.ParameterName = parm.Name;
                    dbParameter.Direction = parm.Direction;
                    cmd!.Parameters.Add(dbParameter);
                }
            }

            cmd!.CommandType = dataAccessCommand.CmdType;
            cmd.CommandText = dataAccessCommand.Sql;
            cmd.Connection = GetConnection();
            cmd.CommandTimeout = TimeOut;

            da = GetFactory().CreateDataAdapter();
            da!.SelectCommand = cmd;

            da.Fill(dt);

            if (dataAccessCommand.Parameters != null)
            {
                foreach (var parameter in dataAccessCommand.Parameters)
                {
                    if (parameter.Direction is ParameterDirection.Output or ParameterDirection.InputOutput)
                        parameter.Value = cmd.Parameters[parameter.Name].Value;
                }
            }
        }
        catch (Exception ex)
        {
            BuildErrorLog(dataAccessCommand.Sql, dataAccessCommand.Parameters, ex);
            throw;
        }
        finally
        {
            da?.Dispose();

            cmd?.Dispose();

            CloseConnection();
        }

        return dt;
    }


    ///<inheritdoc cref="GetDataTable(string,System.Collections.Generic.List{JJMasterData.Commons.Dao.DataAccessParameter})"/>
    public async Task<DataTable> GetDataTableAsync(string sql)
    {
        return await GetDataTableAsync(new DataAccessCommand(sql));
    }

    ///<inheritdoc cref="GetDataTable(string,System.Collections.Generic.List{JJMasterData.Commons.Dao.DataAccessParameter})"/>
    public async Task<DataTable> GetDataTableAsync(DataAccessCommand dataAccessCommand)
    {
        var cmd = GetFactory().CreateCommand();

        DbDataAdapter da = null;
        var dt = new DataTable();
        try
        {
            if (dataAccessCommand.Parameters != null)
            {
                foreach (var parm in dataAccessCommand.Parameters)
                {
                    var dbParameter = GetFactory().CreateParameter();
                    dbParameter!.DbType = parm.Type;
                    dbParameter.Value = parm.Value ?? DBNull.Value;
                    dbParameter.ParameterName = parm.Name;
                    dbParameter.Direction = parm.Direction;
                    cmd?.Parameters.Add(dbParameter);
                }
            }

            cmd!.CommandType = dataAccessCommand.CmdType;
            cmd.CommandText = dataAccessCommand.Sql;
            cmd.Connection = await GetConnectionAsync();
            cmd.CommandTimeout = TimeOut;

            da = GetFactory().CreateDataAdapter();
            da!.SelectCommand = cmd;
            da.Fill(dt);

            if (dataAccessCommand.Parameters != null)
            {
                foreach (var param in dataAccessCommand.Parameters)
                {
                    if (param.Direction is ParameterDirection.Output or ParameterDirection.InputOutput)
                        param.Value = cmd.Parameters[param.Name].Value;
                }
            }
        }
        catch (Exception ex)
        {
            BuildErrorLog(dataAccessCommand.Sql, dataAccessCommand.Parameters, ex);
            throw;
        }
        finally
        {
            da?.Dispose();

            cmd?.Dispose();

            CloseConnection();
        }

        return dt;
    }

    ///<inheritdoc cref="GetDataTable(string,System.Collections.Generic.List{JJMasterData.Commons.Dao.DataAccessParameter})"/>
    public DataTable GetDataTable(ref DbConnection sqlConn, string sql)
    {
        DataTable dt = new DataTable();
        DbCommand sqlCmd = null;
        DbDataAdapter da = null;
        try
        {
            sqlCmd = GetFactory().CreateCommand();
            sqlCmd!.CommandType = CommandType.Text;
            sqlCmd.Connection = sqlConn;
            sqlCmd.CommandText = sql;
            sqlCmd.CommandTimeout = TimeOut;

            da = GetFactory().CreateDataAdapter();
            da!.SelectCommand = sqlCmd;
            da.Fill(dt);
        }
        catch (Exception ex)
        {
            BuildErrorLog(sql, new List<DataAccessParameter>(), ex);
            throw;
        }
        finally
        {
            da?.Dispose();

            sqlCmd?.Dispose();
        }

        return dt;
    }


    /// <summary>
    /// Returns a single sql command value with parameters
    /// </summary>
    /// <remarks>
    /// To prevent SQL injection, please use the DataAccessCommand overload.
    /// </remarks>
    public object GetResult(string sql)
    {
        return GetResult(new DataAccessCommand(sql));
    }

    /// <inheritdoc cref="GetResult(string,System.Collections.Generic.List{JJMasterData.Commons.Dao.DataAccessParameter})"/>
    public object GetResult(DataAccessCommand cmd)
    {
        object scalarResult;
        DbCommand dbCommand = null;
        try
        {
            dbCommand = CreateDbCommand(cmd);
            dbCommand.Connection = GetConnection();
            scalarResult = dbCommand.ExecuteScalar();

            foreach (var param in cmd.Parameters)
            {
                if(param.Direction is ParameterDirection.Output or ParameterDirection.InputOutput)
                    param.Value = dbCommand.Parameters[param.Name].Value;
            }
        }
        catch (Exception ex)
        {
            BuildErrorLog(cmd.Sql, cmd.Parameters, ex);
            throw;
        }
        finally
        {
            dbCommand?.Dispose();

            CloseConnection();
        }

        return scalarResult;
    }
    
    public async Task<object> GetResultAsync(string sql)
    {
        return await GetResultAsync(new DataAccessCommand(sql));
    }

    /// <inheritdoc cref="GetResult(string,System.Collections.Generic.List{JJMasterData.Commons.Dao.DataAccessParameter})"/>
    public async Task<object> GetResultAsync(DataAccessCommand cmd)
    {
        object scalarResult;
        DbCommand dbCommand = null;
        try
        {
            dbCommand =  CreateDbCommand(cmd);
            dbCommand.Connection = await GetConnectionAsync();
            scalarResult = await dbCommand.ExecuteScalarAsync();

            foreach (DataAccessParameter p in cmd.Parameters)
            {
                if (p.Direction is ParameterDirection.Output or ParameterDirection.InputOutput)
                    p.Value = dbCommand.Parameters[p.Name].Value;
            }
        }
        catch (Exception ex)
        {
            BuildErrorLog(cmd.Sql, cmd.Parameters, ex);
            throw;
        }
        finally
        {
            dbCommand?.Dispose();

            CloseConnection();
        }

        return scalarResult;
    }
    
    /// <inheritdoc cref="GetResult(string,System.Collections.Generic.List{JJMasterData.Commons.Dao.DataAccessParameter})"/>
    public object GetResult(DataAccessCommand cmd, ref DbConnection sqlConn, ref DbTransaction trans)
    {
        object scalarResult;
        DbCommand dbCommand = null;
        try
        {
            dbCommand = CreateDbCommand(cmd);
            dbCommand.Connection = sqlConn;
            dbCommand.Transaction = trans;
            scalarResult = dbCommand.ExecuteScalar();
        }
        catch (Exception ex)
        {
            BuildErrorLog(cmd.Sql, cmd.Parameters, ex);
            throw;
        }
        finally
        {
            dbCommand?.Dispose();
        }

        return scalarResult;
    }

    /// <summary>
    /// Runs one or more commands on the database with transactions.
    /// </summary>
    /// <returns>Returns the number of affected records.</returns>
    /// <remarks>
    /// Author: Lucio Pelinson 14-04-2012
    /// </remarks>
    public int SetCommand(DataAccessCommand cmd)
    {
        int rowsAffected = 0;
        DbCommand dbCommand = null;
        try
        {
            dbCommand = CreateDbCommand(cmd);
            dbCommand.Connection = GetConnection();
            
            rowsAffected += dbCommand.ExecuteNonQuery();

            foreach (var parameter in cmd.Parameters.Where(parameter =>
                         parameter.Direction is ParameterDirection.Output or ParameterDirection.InputOutput))
            {
                parameter.Value = dbCommand.Parameters[parameter.Name].Value;
            }
        }
        catch (Exception ex)
        {
            BuildErrorLog(cmd.Sql, cmd.Parameters, ex);
            throw;
        }
        finally
        {
            dbCommand?.Dispose();

            CloseConnection();
        }

        return rowsAffected;
    }

    /// <inheritdoc cref="SetCommand(JJMasterData.Commons.Dao.DataAccessCommand)"/>
    public async Task<int> SetCommandAsync(DataAccessCommand cmd)
    {
        int rowsAffected;
        DbCommand dbCommand = null;
        try
        {
            dbCommand = CreateDbCommand(cmd);
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
            BuildErrorLog(cmd.Sql, cmd.Parameters, ex);
            throw;
        }
        finally
        {
            dbCommand?.Dispose();

            CloseConnection();
        }

        return rowsAffected;
    }

    /// <inheritdoc cref="SetCommand(JJMasterData.Commons.Dao.DataAccessCommand)"/>
    public int SetCommand(List<DataAccessCommand> commands)
    {
        int numberOfRowsAffected = 0;
        int index = 0;
        
        DbCommand dbCommand = null;
        DbConnection connection = GetConnection();
        DbTransaction sqlTras = connection.BeginTransaction();
        try
        {
            foreach (DataAccessCommand cmd in commands)
            {
                dbCommand = CreateDbCommand(cmd);
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
            BuildErrorLog(cmd.Sql, cmd.Parameters, ex);
            throw;
        }
        finally
        {
            sqlTras.Dispose();

            dbCommand?.Dispose();

            CloseConnection();
        }

        return numberOfRowsAffected;
    }


    /// <inheritdoc cref="SetCommand(JJMasterData.Commons.Dao.DataAccessCommand)"/>
    public async Task<int> SetCommandAsync(List<DataAccessCommand> commands)
    {
        int numberOfRowsAffected = 0;
        int index = 0;
        
        DbCommand dbCommand = null;
        DbConnection connection = await GetConnectionAsync();
        DbTransaction sqlTras = connection.BeginTransaction();
        try
        {
            foreach (DataAccessCommand cmd in commands)
            {
                dbCommand = CreateDbCommand(cmd);
                dbCommand.Connection = connection;
                dbCommand.Transaction = sqlTras;

                numberOfRowsAffected += await dbCommand.ExecuteNonQueryAsync();
                index++;
            }

            sqlTras.Commit();
        }
        catch (Exception ex)
        {
            sqlTras.Rollback();
            var cmd = commands[index];
            BuildErrorLog(cmd.Sql, cmd.Parameters, ex);
            throw;
        }
        finally
        {
            sqlTras.Dispose();

            dbCommand?.Dispose();

            CloseConnection();
        }

        return numberOfRowsAffected;
    }

    /// <inheritdoc cref="SetCommand(JJMasterData.Commons.Dao.DataAccessCommand)"/>
    public int SetCommand(string sql)
    {
        int numberOfRowsAffected = 0;
        DbCommand sqlCmd = null;
        try
        {
            sqlCmd = GetFactory().CreateCommand();
            sqlCmd!.CommandType = CommandType.Text;
            sqlCmd.CommandText = sql.Replace("\n", string.Empty);
            sqlCmd.Connection = GetConnection();
            sqlCmd.CommandTimeout = TimeOut;

            numberOfRowsAffected += sqlCmd.ExecuteNonQuery();
        }
        catch (Exception ex)
        {
            BuildErrorLog(sql, null, ex);
            throw;
        }
        finally
        {
            sqlCmd?.Dispose();

            CloseConnection();
        }

        return numberOfRowsAffected;
    }

    /// <inheritdoc cref="SetCommand(JJMasterData.Commons.Dao.DataAccessCommand)"/>
    public async Task<int> SetCommandAsync(string sql)
    {
        int nRet = 0;
        DbCommand sqlCmd = null;
        try
        {
            sqlCmd = GetFactory().CreateCommand();
            sqlCmd!.CommandType = CommandType.Text;
            sqlCmd.CommandText = sql.Replace("\n", string.Empty);
            sqlCmd.Connection = await GetConnectionAsync();
            sqlCmd.CommandTimeout = TimeOut;

            nRet += await sqlCmd.ExecuteNonQueryAsync();
        }
        catch (Exception ex)
        {
            BuildErrorLog(sql, null, ex);
            throw;
        }
        finally
        {
            sqlCmd?.Dispose();

            CloseConnection();
        }

        return nRet;
    }

    /// <inheritdoc cref="SetCommand(JJMasterData.Commons.Dao.DataAccessCommand)"/>
    public int SetCommand(ArrayList sqlList)
    {
        List<DataAccessCommand> aCmd = new List<DataAccessCommand>();
        foreach (string sql in sqlList)
        {
            aCmd.Add(new DataAccessCommand(sql));
        }

        int nRet = SetCommand(aCmd);
        return nRet;
    }

    /// <inheritdoc cref="SetCommand(JJMasterData.Commons.Dao.DataAccessCommand)"/>
    public async Task<int> SetCommandAsync(ArrayList sqlList)
    {
        var commands = (from string sql in sqlList select new DataAccessCommand(sql)).ToList();
        int numberOfRowsAffected = await SetCommandAsync(commands);
        return numberOfRowsAffected;
    }
    
    /// <inheritdoc cref="SetCommand(JJMasterData.Commons.Dao.DataAccessCommand)"/>
    public int SetCommand(DataAccessCommand cmd, ref DbConnection sqlConn, ref DbTransaction trans)
    {
        int numberOfRowsAffected = 0;
        DbCommand dbCommand = null;
        try
        {
            dbCommand = CreateDbCommand(cmd);
            dbCommand.Connection = GetConnection();
            dbCommand.Transaction = trans;

            numberOfRowsAffected += dbCommand.ExecuteNonQuery();
        }
        catch (Exception ex)
        {
            BuildErrorLog(cmd.Sql, cmd.Parameters, ex);
            throw;
        }
        finally
        {
            dbCommand?.Dispose();
        }

        return numberOfRowsAffected;
    }

    /// <summary>
    /// Retrieves the first record of the sql statement in a Hashtable object.  [key(database field), value(value stored in database)] 
    /// </summary>
    /// <returns>
    /// Return a Hashtable Object. 
    /// If no record is found it returns null.
    /// </returns>
    /// <remarks>
    /// Author: Lucio Pelinson 17-04-2012
    /// </remarks>
    public Hashtable GetFields(string sql) => GetFields(new DataAccessCommand(sql));

    /// <inheritdoc cref="GetFields(string)"/>
    public Task<Hashtable> GetFieldsAsync(string sql) => GetFieldsAsync(new DataAccessCommand(sql));

    /// <inheritdoc cref="GetFields(string)"/>
    public Hashtable GetFields(DataAccessCommand cmd)
    {
        Hashtable retCollection = null;
        DbCommand dbCommand = null;
        try
        {
            dbCommand = CreateDbCommand(cmd);
            dbCommand.Connection = GetConnection();
            
            DbDataReader dr = dbCommand.ExecuteReader(CommandBehavior.SingleRow);

            while (dr.Read())
            {
                retCollection = new Hashtable();
                int nQtd = 0;

                while (nQtd < dr.FieldCount)
                {
                    string fieldName = dr.GetName(nQtd);
                    if (retCollection.ContainsKey(fieldName))
                        throw new DataAccessException(
                            TranslateKey("[{0}] field duplicated in get procedure", fieldName));

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
            BuildErrorLog(cmd.Sql, cmd.Parameters, ex);
            throw;
        }
        finally
        {
            dbCommand?.Dispose();

            CloseConnection();
        }

        return retCollection;
    }

    /// <inheritdoc cref="GetFields(string)"/>
    public async Task<Hashtable> GetFieldsAsync(DataAccessCommand cmd)
    {
        Hashtable retCollection = null;
        DbCommand dbCommand = null;
        try
        {
            dbCommand = CreateDbCommand(cmd);
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
                        throw new DataAccessException(
                            TranslateKey("[{0}] field duplicated in get procedure", fieldName));

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
            BuildErrorLog(cmd.Sql, cmd.Parameters, ex);
            throw;
        }
        finally
        {
            dbCommand?.Dispose();

            CloseConnection();
        }

        return retCollection;
    }
    

    private DataAccessCommand GetTableExistsCommand(string table)
    {
        const string sql = @"SELECT COUNT(1) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = @Table";

        var command = new DataAccessCommand
        {
            Sql = sql,
            Parameters =
            {
                new DataAccessParameter()
                {
                    Name = "@Table",
                    Value = table
                }
            }
        };
        
        return command;
    }
    
    public bool TableExists(string table)
    {
        bool result;
        try
        {
            var ret = GetResult(GetTableExistsCommand(table));
            result = (int)ret == 1;
        }
        finally
        {
            CloseConnection();
        }

        return result;
    }


    public async Task<bool> TableExistsAsync(string table)
    {
        bool result;
        try
        {
            result = (int)await GetResultAsync(GetTableExistsCommand(table)) == 1;
        }
        finally
        {
            CloseConnection();
        }

        return result;
    }


    /// <summary>
    /// Verifica se a conexão com o banco esta ok
    /// </summary>
    /// <returns>True = Conexão ok </returns>
    /// ///<remarks>
    ///Author: Lucio Pelinson 28-04-2014
    ///</remarks>
    public bool TryConnection(out string errorResult)
    {
        bool result;
        DbConnection sqlConn = null;
        errorResult = null;
        try
        {
            sqlConn = GetFactory().CreateConnection();
            sqlConn!.ConnectionString = ConnectionString;
            sqlConn.Open();
            result = true;
        }
        catch (Exception ex)
        {
            var error = new StringBuilder();
            error.AppendLine(ex.Message);
            if (ex.InnerException is { Message: { } })
                error.Append(ex.InnerException.Message);

            errorResult = error.ToString();
            result = false;
        }
        finally
        {
            if (sqlConn != null)
            {
                if (sqlConn.State == ConnectionState.Open)
                {
                    sqlConn.Close();
                }

                sqlConn.Dispose();
            }
        }

        return result;
    }

    /// <summary>
    /// Executes a database script.
    /// </summary>
    /// <returns>Retorns true if the execution is successful.</returns>
    /// <remarks>Lucio Pelinson 18-02-2013</remarks> 
    public bool ExecuteBatch(string script)
    {
        string markpar = "GO";
        if (ConnectionProvider.Equals(DataAccessProvider.Oracle))
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

    /// <inheritdoc cref="ExecuteBatch"/>
    public async Task<bool> ExecuteBatchAsync(string script)
    {
        string markpar = "GO";
        if (ConnectionProvider.Equals(DataAccessProvider.Oracle))
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
    
    
    private void BuildErrorLog(string sql, List<DataAccessParameter> parms, Exception ex)
    {
        if (ex is SqlException { Number: >= 50000 }) return;

        var error = new StringBuilder();
        try
        {
            error.AppendLine(TranslateKey("Error raised in DataAccess"));
            error.Append(TranslateKey("Error Message"));
            error.Append(": ");
            error.AppendLine(ex.Message);
            if (ex.InnerException is { Message: { } })
            {
                error.Append(TranslateKey("Detail Message"));
                error.Append(": ");
                error.AppendLine(ex.InnerException.Message);
            }

            error.Append(TranslateKey("Executed Query"));
            error.AppendLine(": ");
            error.AppendLine(sql);
            if (parms is { Count: > 0 })
            {
                error.Append(TranslateKey("Parameters"));
                error.AppendLine(": ");
                foreach (var parm in parms)
                {
                    error.Append(parm.Name);
                    error.Append(" = ");
                    error.Append(parm.Value);
                    error.Append(" [");
                    error.Append(parm.Type.ToString());
                    error.AppendLine("]");
                }
            }
        }
        catch
        {
            error.Append(ex);
        }

        AddLog(error.ToString());
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

    private void AddLog(string value)
    {
        if (GenerateLog)
            Log.AddError(value, "JJMasterData.Commons");
    }

    private string TranslateKey(string key)
    {
        return TranslateErrorMessage ? Translate.Key(key) : key;
    }

    private string TranslateKey(string formatKey, params object[] args)
    {
        return TranslateErrorMessage ? Translate.Key(formatKey, args) : string.Format(formatKey, args);
    }
}