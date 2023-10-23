#nullable enable

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using JJMasterData.Commons.Exceptions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace JJMasterData.Commons.Data;

/// <summary>
/// Classes that expose data access services and implements ADO methods.<br />
/// Provides functionality to developers who write managed code similar to the functionality provided to native component object model (COM)
/// </summary>
/// <example>
/// [!include[Test](../../../doc/JJMasterData.Documentation/articles/usages/dataaccess.md)]
/// </example>
public partial class DataAccess
{
    private DbProviderFactory? _factory;

    public DbProviderFactory Factory
    {
        get
        {
            if (_factory != null)
                return _factory;

            if (ConnectionString == null)
            {
                var error = new StringBuilder();
                error.AppendLine("Connection string not found in configuration file.");
                error.AppendLine("Default connection name is [ConnectionString].");
                error.AppendLine("Please check the docs for more information.");
                error.Append("https://portal.jjconsulting.com.br/jjdoc/articles/errors/connection_string.html");
                throw new DataAccessException(error.ToString());
            }

            try
            {
                _factory = DataAccessProviderFactory.GetDbProviderFactory(ConnectionProvider);
            }
            catch (Exception ex)
            {
                throw new DataAccessException(ex);
            }

            return _factory;
        }
    }

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
    public DataAccessProvider ConnectionProvider { get; set; }

    /// <summary>
    /// Waiting time to execute a command on the database (seconds - default 240s)
    /// </summary>
    public int TimeOut { get; set; } = 240;


#if NET48
    /// <summary>
    /// By default DataAccess recover connection string from appsettings.json with name ConnectionString
    /// </summary>
    public DataAccess()
    {
        ConnectionString =
            System.Configuration.ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
        if (Enum.TryParse<DataAccessProvider>(
                System.Configuration.ConfigurationManager.ConnectionStrings["ConnectionString"].ProviderName,
                out var provider))
        {
            ConnectionProvider = provider;
        }
        else
        {
            throw new DataAccessProviderException("Invalid DataAccess provider.");
        }
    }


    /// <summary>
    /// New instance from a custom connection string name
    /// </summary>
    /// <param name="connectionStringName">Name of connection string in appsettings.json or webconfig.xml file</param>
    public DataAccess(string connectionStringName)
    {
        ConnectionString =
            System.Configuration.ConfigurationManager.ConnectionStrings[connectionStringName].ConnectionString;
        if (Enum.TryParse<DataAccessProvider>(
                System.Configuration.ConfigurationManager.ConnectionStrings[connectionStringName].ProviderName,
                out var provider))
        {
            ConnectionProvider = provider;
        }
        else
        {
            throw new DataAccessProviderException("Invalid DataAccess provider.");
        }
    }
#endif
    
    /// <summary>
    /// Initialize a with a connectionString and a specific providerName.
    /// See also <see cref="DataAccessProvider"/>.
    /// </summary>
    /// <param name="connectionString">Conections string with data source, user etc...</param>
    /// <param name="connectionProviderName">Provider name. Avaliable provider see <see cref="DataAccessProvider"/></param>
    public DataAccess(string connectionString, string connectionProviderName)
    {
        ConnectionString = connectionString;

        if (Enum.TryParse<DataAccessProvider>(connectionProviderName, out var provider))
        {
            ConnectionProvider = provider;
        }
        else
        {
            throw new DataAccessProviderException("Invalid DataAccess provider.");
        }
    }

    public DataAccess(string connectionString, DataAccessProvider dataAccessProvider)
    {
        ConnectionString = connectionString;
        ConnectionProvider = dataAccessProvider;
    }

    /// <summary>
    /// Initialize with a IConfiguration instance.
    /// </summary>
    [ActivatorUtilitiesConstructor]
    public DataAccess(IConfiguration configuration)
    {
        ConnectionString = configuration.GetConnectionString("ConnectionString");
        ConnectionProvider = configuration.GetSection("ConnectionProviders")
                                 .GetValue<DataAccessProvider?>("ConnectionString") ??
                             DataAccessProvider.SqlServer;
    }

    public DbConnection GetConnection()
    {
        var connection = Factory.CreateConnection();

        try
        {
            connection!.ConnectionString = ConnectionString;
            connection.Open();
        }
        catch (Exception ex)
        {
            throw new DataAccessException(ex);
        }

        return connection;
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
        var dt = new DataTable();
        try
        {
            using var dbCommand = CreateDbCommand(cmd);
            dbCommand.Connection = GetConnection();

            using (dbCommand.Connection)
            {
                using var dataAdapter = Factory.CreateDataAdapter();
                dataAdapter!.SelectCommand = dbCommand;
                dataAdapter.Fill(dt);

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
            using var dbCommand = Factory.CreateCommand();
            dbCommand!.CommandType = CommandType.Text;
            dbCommand.Connection = sqlConn;
            dbCommand.CommandText = sql;
            dbCommand.CommandTimeout = TimeOut;

            using var dataAdapter = Factory.CreateDataAdapter();
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
    public object? GetResult(string sql)
    {
        return GetResult(new DataAccessCommand(sql));
    }

    /// <summary>
    /// ExecuteScalar command and returns the first column of the first row in the result set returned by the query.
    /// All other columns and rows are ignored.
    /// </summary>
    public object? GetResult(DataAccessCommand cmd)
    {
        object? scalarResult;
        try
        {
            using var dbCommand = CreateDbCommand(cmd);
            dbCommand.Connection = GetConnection();

            using (dbCommand.Connection)
            {
                scalarResult = dbCommand.ExecuteScalar();

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
    public object? GetResult(DataAccessCommand cmd, ref DbConnection sqlConn, ref DbTransaction trans)
    {
        object? scalarResult;
        try
        {
            using var dbCommand = CreateDbCommand(cmd);
            dbCommand.Connection = sqlConn;
            dbCommand.Transaction = trans;

            using (dbCommand.Connection)
            {
                scalarResult = dbCommand.ExecuteScalar();
            }
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

            using (dbCommand.Connection)
            {
                rowsAffected += dbCommand.ExecuteNonQuery();

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

        return rowsAffected;
    }

    /// <summary>Runs one or more commands on the database with transactions.</summary>
    /// <returns>Returns the number of affected records.</returns>
    /// <remarks>Author: Lucio Pelinson 14-04-2012</remarks>
    public int SetCommand(IEnumerable<DataAccessCommand> commands)
    {
        int numberOfRowsAffected = 0;
        DataAccessCommand? currentCommand = null;

        var connection = GetConnection();

        using (connection)
        {
            using var sqlTransaction = connection.BeginTransaction();
            try
            {
                foreach (var command in commands)
                {
                    currentCommand = command;

                    using var dbCommand = CreateDbCommand(command);
                    dbCommand.Connection = connection;
                    dbCommand.Transaction = sqlTransaction;

                    numberOfRowsAffected += dbCommand.ExecuteNonQuery();
                }

                sqlTransaction.Commit();
            }
            catch (Exception ex)
            {
                sqlTransaction.Rollback();
                throw GetDataAccessException(ex, currentCommand);
            }
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

    /// <summary>Runs one or more commands on the database with transactions.</summary>
    /// <returns>Returns the number of affected records.</returns>
    /// <remarks>Author: Lucio Pelinson 14-04-2012</remarks>
    public int SetCommand(IEnumerable<string> sqlList)
    {
        var commandList = from string sql in sqlList select new DataAccessCommand(sql);

        int numberOfRowsAffected = SetCommand(commandList);
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
            dbCommand.Connection = sqlConn;
            dbCommand.Transaction = trans;


            using (dbCommand.Connection)
            {
                numberOfRowsAffected += dbCommand.ExecuteNonQuery();
            }
        }
        catch (Exception ex)
        {
            throw GetDataAccessException(ex, cmd);
        }

        return numberOfRowsAffected;
    }

    /// <summary>
    /// Retrieves the first record of the sql statement in a Hashtable object.
    /// [key(database field), value(value stored in database)]<br/>
    /// Never concat string to SQL, please see DataAccessCommand<br/>
    /// </summary>
    /// <returns>
    /// Return a Hashtable Object. 
    /// If no record is found it returns null.
    /// </returns>
    public Hashtable? GetFields(string sql) => GetFields(new DataAccessCommand(sql));
    
    /// <summary>
    /// Retrieves the first record of the sql statement in a Hashtable object.
    /// [key(database field), value(value stored in database)]
    /// </summary>
    /// <param name="cmd">Command</param>
    /// <returns>
    /// Return a Hashtable Object. 
    /// If no record is found it returns null.
    /// </returns>
    public Hashtable? GetFields(DataAccessCommand cmd)
    {
        Hashtable? retCollection = null;
        try
        {
            using var dbCommand = CreateDbCommand(cmd);
            dbCommand.Connection = GetConnection();

            using (dbCommand.Connection)
            {
                using (var dr = dbCommand.ExecuteReader(CommandBehavior.SingleRow))
                {
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
                }

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

        return retCollection;
    }

    /// <summary>
    /// Check if table exists in the database
    /// </summary>
    public bool TableExists(string tableName)
    {
        var ret = GetResult(GetTableExistsCommand(tableName));
        var result = ret as int? == 1;

        return result;
    }

    /// <summary>Verify the database connection</summary>
    /// <returns>True if the connection is successful.</returns>
    /// <remarks>Author: Lucio Pelinson 28-04-2014</remarks>
    public bool TryConnection(out string? errorMessage)
    {
        bool result;
        DbConnection? connection = null;
        errorMessage = null;
        try
        {
            connection = Factory.CreateConnection();
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

    /// <summary>Executes a database script.</summary>
    /// <returns>Returns true if the execution is successful.</returns>
    /// <remarks>Lucio Pelinson 18-02-2013</remarks> 
    public bool ExecuteBatch(string script)
    {
        string markpar = "GO";
        if (ConnectionProvider is DataAccessProvider.Oracle or DataAccessProvider.OracleNetCore)
        {
            markpar = "/";
        }

        if (script.Trim().Length > 0)
        {
            var sqlList = new List<string>();
            string sqlBatch = string.Empty;
            script += $"\n{markpar}"; // make sure last batch is executed. 

            foreach (string line in script.Split(new[] { "\n", "\r" }, StringSplitOptions.RemoveEmptyEntries))
            {
                if (line.ToUpperInvariant().Trim() == markpar)
                {
                    if (sqlBatch.Trim().Length > 0)
                    {
                        sqlList.Add(sqlBatch);
                    }

                    sqlBatch = string.Empty;
                }
                else
                {
                    sqlBatch += $"{line}\n";
                }
            }

            SetCommand(sqlList);
        }

        return true;
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
    
    private static Exception GetDataAccessException(Exception ex, DataAccessCommand? cmd)
    {
        string sql = string.Empty;
        ICollection<DataAccessParameter>? parameters = null;
        if (cmd != null)
        {
            sql = cmd.Sql;
            parameters = cmd.Parameters;
        }
        return GetDataAccessException(ex, sql, parameters);
    }

    private static Exception GetDataAccessException(Exception ex, 
        string sql,
        ICollection<DataAccessParameter>? parameters = null)
    {
        ex.Data.Add("DataAccess Query", sql);

        if (!(parameters?.Count > 0)) 
            return ex;
        
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

    private DbCommand CreateDbCommand(DataAccessCommand command)
    {
        var dbCommand = Factory.CreateCommand();
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
        var dbParameter = Factory.CreateParameter();
        dbParameter!.DbType = parameter.Type;
        dbParameter.Value = parameter.Value ?? DBNull.Value;
        dbParameter.ParameterName = parameter.Name;
        dbParameter.Direction = parameter.Direction;
        dbParameter.IsNullable = parameter.IsNullable;

        if (parameter.Size is not null)
            dbParameter.Size = parameter.Size.Value;

        return dbParameter;
    }

    private static DataAccessCommand GetColumnExistsCommand(string tableName, string columnName)
    {
        var command = new DataAccessCommand
        {
            Sql =
                @"SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = @TableName AND COLUMN_NAME = @ColumnName"
        };

        command.AddParameter("@TableName", tableName, DbType.String);
        command.AddParameter("@ColumnName", columnName, DbType.String);

        return command;
    }

}