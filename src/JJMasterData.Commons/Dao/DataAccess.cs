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
    public DataTable GetDataTable(string sql, List<DataAccessParameter> parameters = null)
    {
        return GetDataTable(new DataAccessCommand(sql, parameters ?? new List<DataAccessParameter>()));
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
    public async Task<DataTable> GetDataTableAsync(string sql, List<DataAccessParameter> parameters = null)
    {
        parameters ??= new List<DataAccessParameter>();
        return await GetDataTableAsync(new DataAccessCommand(sql, parameters));
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
    /// <returns></returns>
    public object GetResult(string sql, List<DataAccessParameter> parameters = null)
    {
        return GetResult(new DataAccessCommand(sql, parameters ?? new List<DataAccessParameter>()));
    }

    /// <inheritdoc cref="GetResult(string,System.Collections.Generic.List{JJMasterData.Commons.Dao.DataAccessParameter})"/>
    public object GetResult(DataAccessCommand cmd)
    {
        object scalarResult;
        DbCommand dbCommand = null;
        try
        {
            dbCommand = GetFactory().CreateCommand();
            foreach (var parameter in cmd.Parameters)
            {
                var dbParameter = GetFactory().CreateParameter();
                dbParameter!.DbType = parameter.Type;
                dbParameter.Value = parameter.Value ?? DBNull.Value;
                dbParameter.ParameterName = parameter.Name;
                dbParameter.Direction = parameter.Direction;
                dbCommand!.Parameters.Add(dbParameter);
            }

            dbCommand!.CommandType = cmd.CmdType;
            dbCommand.CommandText = cmd.Sql;
            dbCommand.Connection = GetConnection();
            dbCommand.CommandTimeout = TimeOut;
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


    /// <inheritdoc cref="GetResult(string,System.Collections.Generic.List{JJMasterData.Commons.Dao.DataAccessParameter})"/>
    public async Task<object> GetResultAsync(string sql, List<DataAccessParameter> parameters = null)
    {
        return await GetResultAsync(new DataAccessCommand(sql, parameters ?? new List<DataAccessParameter>()));
    }

    /// <inheritdoc cref="GetResult(string,System.Collections.Generic.List{JJMasterData.Commons.Dao.DataAccessParameter})"/>
    public async Task<object> GetResultAsync(DataAccessCommand cmd)
    {
        object scalarResult;
        DbCommand dbCommand = null;
        try
        {
            dbCommand = GetFactory().CreateCommand();
            foreach (var parameter in cmd.Parameters)
            {
                var dbParameter = GetFactory().CreateParameter();
                dbParameter!.DbType = parameter.Type;
                dbParameter.Value = parameter.Value ?? DBNull.Value;
                dbParameter.ParameterName = parameter.Name;
                dbParameter.Direction = parameter.Direction;
                dbCommand!.Parameters.Add(dbParameter);
            }

            dbCommand!.CommandType = cmd.CmdType;
            dbCommand.CommandText = cmd.Sql;
            dbCommand.Connection = await GetConnectionAsync();
            dbCommand.CommandTimeout = TimeOut;
            scalarResult = await dbCommand.ExecuteScalarAsync();

            foreach (DataAccessParameter p in cmd.Parameters)
            {
                if (p.Direction == ParameterDirection.Output || p.Direction == ParameterDirection.InputOutput)
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
        object oRet;
        DbCommand sqlCmd = null;
        try
        {
            sqlCmd = GetFactory().CreateCommand();
            foreach (var parameter in cmd.Parameters)
            {
                DbParameter dbParameter = GetFactory().CreateParameter();
                dbParameter!.DbType = parameter.Type;
                dbParameter.Value = parameter.Value ?? DBNull.Value;
                dbParameter.ParameterName = parameter.Name;

                dbParameter.Direction = parameter.Direction;
                if (parameter.Size > 0)
                    dbParameter.Size = parameter.Size;

                sqlCmd!.Parameters.Add(dbParameter);
            }

            sqlCmd!.CommandType = cmd.CmdType;
            sqlCmd.CommandText = cmd.Sql;
            sqlCmd.Connection = sqlConn;
            sqlCmd.CommandTimeout = TimeOut;
            sqlCmd.Transaction = trans;
            oRet = sqlCmd.ExecuteScalar();
        }
        catch (Exception ex)
        {
            BuildErrorLog(cmd.Sql, cmd.Parameters, ex);
            throw;
        }
        finally
        {
            sqlCmd?.Dispose();
        }

        return oRet;
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
            dbCommand = GetFactory().CreateCommand();
            dbCommand!.CommandType = cmd.CmdType;
            dbCommand.CommandText = cmd.Sql;
            dbCommand.Connection = GetConnection();
            dbCommand.CommandTimeout = TimeOut;

            foreach (var parm in cmd.Parameters)
            {
                var oPar = GetFactory().CreateParameter();
                oPar!.DbType = parm.Type;
                oPar.Value = parm.Value ?? DBNull.Value;
                oPar.ParameterName = parm.Name;
                oPar.Direction = parm.Direction;

                if (parm.Size > 0)
                    oPar.Size = parm.Size;

                oPar.IsNullable = true;
                dbCommand.Parameters.Add(oPar);
            }

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
        DbCommand sqlCmd = null;
        try
        {
            sqlCmd = GetFactory().CreateCommand();
            sqlCmd!.CommandType = cmd.CmdType;
            sqlCmd.CommandText = cmd.Sql;
            sqlCmd.Connection = await GetConnectionAsync();
            sqlCmd.CommandTimeout = TimeOut;

            foreach (DataAccessParameter parameter in cmd.Parameters)
            {
                DbParameter dbParameter = GetFactory().CreateParameter();
                dbParameter!.DbType = parameter.Type;
                dbParameter.Value = parameter.Value ?? DBNull.Value;
                dbParameter.ParameterName = parameter.Name;
                dbParameter.Direction = parameter.Direction;

                if (parameter.Size > 0)
                    dbParameter.Size = parameter.Size;

                dbParameter.IsNullable = true;
                sqlCmd.Parameters.Add(dbParameter);
            }

            rowsAffected = await sqlCmd.ExecuteNonQueryAsync();

            foreach (DataAccessParameter parameter in cmd.Parameters)
            {
                if (parameter.Direction is ParameterDirection.Output or ParameterDirection.InputOutput)
                    parameter.Value = sqlCmd.Parameters[parameter.Name].Value;
            }
        }
        catch (Exception ex)
        {
            BuildErrorLog(cmd.Sql, cmd.Parameters, ex);
            throw;
        }
        finally
        {
            sqlCmd?.Dispose();

            CloseConnection();
        }

        return rowsAffected;
    }

    /// <inheritdoc cref="SetCommand(JJMasterData.Commons.Dao.DataAccessCommand)"/>
    public int SetCommand(List<DataAccessCommand> commands)
    {
        int nRet = 0;
        int index = 0;
        DbCommand sqlCmd = null;
        DbTransaction sqlTras = GetConnection().BeginTransaction();
        try
        {
            foreach (DataAccessCommand cmd in commands)
            {
                sqlCmd = GetFactory().CreateCommand();
                sqlCmd!.CommandType = cmd.CmdType;
                sqlCmd.CommandText = cmd.Sql;
                sqlCmd.Connection = GetConnection();
                sqlCmd.CommandTimeout = TimeOut;
                sqlCmd.Transaction = sqlTras;

                foreach (DataAccessParameter parm in cmd.Parameters)
                {
                    var dbParameter = GetFactory().CreateParameter();
                    dbParameter!.DbType = parm.Type;
                    dbParameter.Value = parm.Value ?? DBNull.Value;
                    dbParameter.ParameterName = parm.Name;

                    dbParameter.Direction = parm.Direction;
                    if (parm.Size > 0)
                        dbParameter.Size = parm.Size;

                    dbParameter.IsNullable = true;
                    sqlCmd.Parameters.Add(dbParameter);
                }

                nRet += sqlCmd.ExecuteNonQuery();
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

            sqlCmd?.Dispose();

            CloseConnection();
        }

        return nRet;
    }


    /// <inheritdoc cref="SetCommand(JJMasterData.Commons.Dao.DataAccessCommand)"/>
    public async Task<int> SetCommandAsync(List<DataAccessCommand> commands)
    {
        int nRet = 0;
        int index = 0;
        var connection = await GetConnectionAsync();
        var dbTransaction = connection.BeginTransaction();
        DbCommand sqlCmd = null;
        try
        {
            foreach (var cmd in commands)
            {
                sqlCmd = GetFactory().CreateCommand();
                sqlCmd!.CommandType = cmd.CmdType;
                sqlCmd.CommandText = cmd.Sql;
                sqlCmd.Connection = await GetConnectionAsync();
                sqlCmd.CommandTimeout = TimeOut;
                sqlCmd.Transaction = dbTransaction;

                foreach (DataAccessParameter parameter in cmd.Parameters)
                {
                    DbParameter dbParameter = GetFactory().CreateParameter();
                    dbParameter!.DbType = parameter.Type;
                    dbParameter.Value = parameter.Value ?? DBNull.Value;
                    dbParameter.ParameterName = parameter.Name;

                    dbParameter.Direction = parameter.Direction;
                    if (parameter.Size > 0)
                        dbParameter.Size = parameter.Size;

                    dbParameter.IsNullable = true;
                    sqlCmd.Parameters.Add(dbParameter);
                }

                nRet += await sqlCmd.ExecuteNonQueryAsync();
                index++;
            }

            dbTransaction.Commit();
        }
        catch (Exception ex)
        {
            dbTransaction.Rollback();
            var cmd = commands[index];
            BuildErrorLog(cmd.Sql, cmd.Parameters, ex);
            throw;
        }
        finally
        {
            dbTransaction.Dispose();

            sqlCmd?.Dispose();

            CloseConnection();
        }

        return nRet;
    }

    /// <inheritdoc cref="SetCommand(JJMasterData.Commons.Dao.DataAccessCommand)"/>
    public int SetCommand(string sql, List<DataAccessParameter> parameters = null)
    {
        parameters ??= new List<DataAccessParameter>();
        int numberOfRows = 0;
        DbCommand sqlCmd = null;
        try
        {
            sqlCmd = GetFactory().CreateCommand();
            sqlCmd!.CommandType = CommandType.Text;
            sqlCmd.CommandText = sql.Replace("\n", string.Empty);
            sqlCmd.Connection = GetConnection();
            sqlCmd.CommandTimeout = TimeOut;

            foreach (DataAccessParameter parameter in parameters)
            {
                DbParameter dbParameter = GetFactory().CreateParameter();
                dbParameter!.DbType = parameter.Type;
                dbParameter.Value = parameter.Value ?? DBNull.Value;
                dbParameter.ParameterName = parameter.Name;

                dbParameter.Direction = parameter.Direction;
                if (parameter.Size > 0)
                    dbParameter.Size = parameter.Size;

                dbParameter.IsNullable = true;
                sqlCmd.Parameters.Add(dbParameter);
            }

            numberOfRows += sqlCmd.ExecuteNonQuery();
        }
        catch (Exception ex)
        {
            BuildErrorLog(sql, parameters, ex);
            throw;
        }
        finally
        {
            sqlCmd?.Dispose();

            CloseConnection();
        }

        return numberOfRows;
    }


    /// <inheritdoc cref="SetCommand(JJMasterData.Commons.Dao.DataAccessCommand)"/>
    public async Task<int> SetCommandAsync(string sql, List<DataAccessParameter> parameters = null)
    {
        parameters ??= new List<DataAccessParameter>();
        int nRet = 0;
        DbCommand sqlCmd = null;
        try
        {
            sqlCmd = GetFactory().CreateCommand();
            sqlCmd!.CommandType = CommandType.Text;
            sqlCmd.CommandText = sql.Replace("\n", string.Empty);
            sqlCmd.Connection = await GetConnectionAsync();
            sqlCmd.CommandTimeout = TimeOut;

            foreach (DataAccessParameter parm in parameters)
            {
                DbParameter oPar = GetFactory().CreateParameter();
                oPar!.DbType = parm.Type;
                oPar.Value = parm.Value ?? DBNull.Value;
                oPar.ParameterName = parm.Name;

                oPar.Direction = parm.Direction;
                if (parm.Size > 0)
                    oPar.Size = parm.Size;

                oPar.IsNullable = true;
                sqlCmd.Parameters.Add(oPar);
            }

            nRet += await sqlCmd.ExecuteNonQueryAsync();
        }
        catch (Exception ex)
        {
            BuildErrorLog(sql, parameters, ex);
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
        int nRet = await SetCommandAsync(commands);
        return nRet;
    }


    /// <inheritdoc cref="SetCommand(JJMasterData.Commons.Dao.DataAccessCommand)"/>
    public int SetCommand(DataAccessCommand cmd, ref DbConnection sqlConn, ref DbTransaction trans)
    {
        int nRet = 0;
        DbCommand sqlCmd = null;
        try
        {
            sqlCmd = GetFactory().CreateCommand();
            sqlCmd!.CommandText = cmd.Sql;
            sqlCmd.Connection = sqlConn;
            sqlCmd.CommandType = cmd.CmdType;
            sqlCmd.CommandTimeout = TimeOut;
            sqlCmd.Transaction = trans;

            foreach (DataAccessParameter parm in cmd.Parameters)
            {
                DbParameter dbParameter = GetFactory().CreateParameter();
                dbParameter!.DbType = parm.Type;
                dbParameter.Value = parm.Value ?? DBNull.Value;
                dbParameter.ParameterName = parm.Name;

                dbParameter.Direction = parm.Direction;
                if (parm.Size > 0)
                    dbParameter.Size = parm.Size;

                sqlCmd.Parameters.Add(dbParameter);
            }

            nRet += sqlCmd.ExecuteNonQuery();
        }
        catch (Exception ex)
        {
            BuildErrorLog(cmd.Sql, cmd.Parameters, ex);
            throw;
        }
        finally
        {
            sqlCmd?.Dispose();
        }

        return nRet;
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
        DbCommand dbCmd = null;
        try
        {
            dbCmd = GetFactory().CreateCommand();
            foreach (var param in cmd.Parameters)
            {
                var dbParameter = GetFactory().CreateParameter();
                dbParameter!.DbType = param.Type;
                dbParameter.Value = param.Value ?? DBNull.Value;
                dbParameter.ParameterName = param.Name;
                dbParameter.Direction = param.Direction;
                dbCmd!.Parameters.Add(dbParameter);
            }

            dbCmd!.CommandType = cmd.CmdType;
            dbCmd.CommandText = cmd.Sql;
            dbCmd.Connection = GetConnection();
            dbCmd.CommandTimeout = TimeOut;

            DbDataReader dr = dbCmd.ExecuteReader(CommandBehavior.SingleRow);

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
                    parameter.Value = dbCmd.Parameters[parameter.Name].Value;
            }
        }
        catch (Exception ex)
        {
            BuildErrorLog(cmd.Sql, cmd.Parameters, ex);
            throw;
        }
        finally
        {
            dbCmd?.Dispose();

            CloseConnection();
        }

        return retCollection;
    }

    /// <inheritdoc cref="GetFields(string)"/>
    public async Task<Hashtable> GetFieldsAsync(DataAccessCommand cmd)
    {
        Hashtable hashtable = null;
        DbCommand dbCmd = null;
        try
        {
            dbCmd = GetFactory().CreateCommand();
            foreach (var parameter in cmd.Parameters)
            {
                DbParameter dbParameter = GetFactory().CreateParameter();
                dbParameter!.DbType = parameter.Type;
                dbParameter.Value = parameter.Value ?? DBNull.Value;
                dbParameter.ParameterName = parameter.Name;
                dbParameter.Direction = parameter.Direction;
                dbCmd!.Parameters.Add(dbParameter);
            }

            dbCmd!.CommandType = cmd.CmdType;
            dbCmd.CommandText = cmd.Sql;
            dbCmd.Connection = await GetConnectionAsync();
            dbCmd.CommandTimeout = TimeOut;

            var dr = await dbCmd.ExecuteReaderAsync(CommandBehavior.SingleRow);

            while (await dr.ReadAsync())
            {
                hashtable = new Hashtable();
                int nQtd = 0;

                while (nQtd < dr.FieldCount)
                {
                    string fieldName = dr.GetName(nQtd);
                    if (hashtable.ContainsKey(fieldName))
                        throw new DataAccessException(
                            TranslateKey("[{0}] field duplicated in get procedure", fieldName));

                    hashtable.Add(fieldName, dr.GetValue(nQtd));
                    nQtd += 1;
                }
            }

            if (!dr.IsClosed)
                dr.Close();

            foreach (var param in cmd.Parameters.Where(p =>
                         p.Direction is ParameterDirection.Output or ParameterDirection.InputOutput))
            {
                param.Value = dbCmd.Parameters[param.Name].Value;
            }
        }
        catch (Exception ex)
        {
            BuildErrorLog(cmd.Sql, cmd.Parameters, ex);
            throw;
        }
        finally
        {
            dbCmd?.Dispose();

            CloseConnection();
        }

        return hashtable;
    }

    [Obsolete("Impossible to be in DataAccess, needs to be in Provider")]
    private DataAccessCommand GetTableExistsCommand(string table)
    {
        ///TODO: Migrate this to each provider.
        const string sql = @"SELECT 
            HAS_PERMS_BY_NAME
            (
                N'dbo.' + @Table, 
                N'OBJECT', 
                N'SELECT'
            )";

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

    [Obsolete("Impossible to be in DataAccess, needs to be in Provider")]
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

    [Obsolete("Impossible to be in DataAccess, needs to be in Provider")]
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

    private static DataAccessCommand GetValueExistsCommand(string tableName, string columnName, object value)
    {
        var command = new DataAccessCommand
        {
            Sql = $"SELECT COUNT(*) from {tableName} WHERE {columnName} = @Value",
            Parameters =
            {
                new DataAccessParameter
                {
                    Name = "@Value",
                    Value = value,
                    Type = value is string ? DbType.String : DbType.Int64
                }
            }
        };
        return command;
    }

    /// <summary>
    /// Verify if a value exists in the database.
    /// </summary>
    /// <returns>Returns true if the value exists.</returns>
    public bool ValueExists(string tableName, string columnName, string value)
    {
        var command = GetValueExistsCommand(tableName, columnName, value);
        return (int)GetResult(command) > 0;
    }

    /// <inheritdoc cref="ValueExists(string,string,string)"/>
    public bool ValueExists(string tableName, string columnName, int value)
    {
        var command = GetValueExistsCommand(tableName, columnName, value);
        return (int)GetResult(command) > 0;
    }

    /// <inheritdoc cref="ValueExists(string,string,string)"/>
    public bool ValueExists(string tableName, params DataAccessParameter[] filters)
    {
        var sql = new StringBuilder();
        sql.Append($"SELECT COUNT(*) from {tableName}");

        for (int i = 0; i < filters.Length; i++)
        {
            sql.Append(i == 0 ? " WHERE " : " AND ");

            sql.Append(filters[i].Name);

            switch (filters[i].Type)
            {
                case DbType.Int32:
                case DbType.Int16:
                case DbType.Int64:
                case DbType.Decimal:
                case DbType.Double:

                    sql.Append(" = ");
                    sql.Append(StringManager.ClearText(filters[i].Value.ToString()));
                    break;
                case DbType.DateTime:
                    sql.Append(" = '");
                    sql.Append(((DateTime)filters[i].Value).ToString("yyyyMMdd"));
                    sql.Append("'");
                    break;
                default:
                    sql.Append(" = ");
                    sql.Append(filters[i].Value);
                    sql.Append("'");
                    break;
            }
        }

        return (int)GetResult(sql.ToString()) > 0;
    }


    /// <inheritdoc cref="ValueExists(string,string,string)"/>
    public async Task<bool> ValueExistsAsync(string tableName, string columnName, string value)
    {
        var command = GetValueExistsCommand(tableName, columnName, value);
        return (int)await GetResultAsync(command) > 0;
    }

    /// <inheritdoc cref="ValueExists(string,string,string)"/>
    public async Task<bool> ValueExistsAsync(string tableName, string columnName, int value)
    {
        var command = GetValueExistsCommand(tableName, columnName, value);
        return (int)await GetResultAsync(command) > 0;
    }

    /// <inheritdoc cref="ValueExists(string,string,string)"/>
    public async Task<bool> ValueExistsAsync(string tableName, params DataAccessParameter[] filters)
    {
        var sql = new StringBuilder();
        sql.Append($"SELECT COUNT(*) from {tableName}");

        for (int i = 0; i < filters.Length; i++)
        {
            sql.Append(i == 0 ? " WHERE " : " AND ");

            sql.Append(filters[i].Name);

            switch (filters[i].Type)
            {
                case DbType.Int32:
                case DbType.Int16:
                case DbType.Int64:
                case DbType.Decimal:
                case DbType.Double:

                    sql.Append(" = ");
                    sql.Append(StringManager.ClearText(filters[i].Value.ToString()));
                    break;
                case DbType.DateTime:
                    sql.Append(" = '");
                    sql.Append(((DateTime)filters[i].Value).ToString("yyyyMMdd"));
                    sql.Append("'");
                    break;
                default:
                    sql.Append(" = ");
                    sql.Append(filters[i].Value);
                    sql.Append("'");
                    break;
            }
        }

        return (int) await GetResultAsync(sql.ToString()) > 0;
    }

    /// <summary>
    /// Recupera um determinado valor de um campo em uma tabela
    /// </summary>
    /// <param name="tableName">Nome da tabela</param>
    /// <param name="columnName">Nome da coluna</param>
    /// <param name="value">Filtro da Coluna</param>
    /// <returns></returns>
    [Obsolete("Will be removed at before Saturn merging at main. Pointless SQL.")]
    public object GetValue(string tableName, string columnName, string value)
    {
        string query = "select " + columnName + " from " + tableName + " where " + columnName + " = '" + value + "'";
        return GetResult(query);
    }

    /// <summary>
    /// Recupera um determinado valor de um campo em uma tabela
    /// </summary>
    /// <param name="tableName">Nome da tabela</param>
    /// <param name="columnName">Nome da coluna</param>
    /// <param name="value">Filtro da Coluna</param>
    /// <returns></returns>
    [Obsolete("Will be removed at before Saturn merging at main. Pointless SQL.")]
    public object GetValue(string tableName, string columnName, int value)
    {
        string query = "select " + columnName + " from " + tableName + " where " + columnName + " = " + value;
        return GetResult(query);
    }

    /// <summary>
    /// Recupera um determinado valor de um campo em uma tabela
    /// </summary>
    /// <param name="tableName">Nome da tabela</param>
    /// <param name="columnName">Nome da coluna</param>
    /// <param name="value">Filtro da Coluna</param>
    /// <returns></returns>
    [Obsolete("Will be removed at before Saturn merging at main. Pointless SQL.")]
    public async Task<object> GetValueAsync(string tableName, string columnName, string value)
    {
        string query = "select " + columnName + " from " + tableName + " where " + columnName + " = '" + value + "'";
        return await GetResultAsync(query);
    }

    /// <summary>
    /// Recupera um determinado valor de um campo em uma tabela
    /// </summary>
    /// <param name="tableName">Nome da tabela</param>
    /// <param name="columnName">Nome da coluna</param>
    /// <param name="value">Filtro da Coluna</param>
    /// <returns></returns>
    [Obsolete("Will be removed at before Saturn merging at main. Pointless SQL.")]
    public async Task<object> GetValueAsync(string tableName, string columnName, int value)
    {
        string query = "select " + columnName + " from " + tableName + " where " + columnName + " = " + value;
        return await GetResultAsync(query);
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