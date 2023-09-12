#nullable enable

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using JJMasterData.Commons.Exceptions;

namespace JJMasterData.Commons.Data;

public partial class DataAccess
{
    public async Task<DbConnection> GetConnectionAsync(CancellationToken cancellationToken = default)
    {
        var connection = Factory.CreateConnection();

        try
        {
            connection!.ConnectionString = ConnectionString;
            await connection.OpenAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            throw new DataAccessException(ex);
        }

        return connection;
    }
    
    ///<inheritdoc cref="GetDataTable(string)"/>
    public async Task<DataTable> GetDataTableAsync(string sql, CancellationToken cancellationToken = default)
    {
        return await GetDataTableAsync(new DataAccessCommand(sql), cancellationToken);
    }

    ///<inheritdoc cref="GetDataTable(DataAccessCommand)"/>
    public async Task<DataTable> GetDataTableAsync(DataAccessCommand cmd, CancellationToken cancellationToken = default)
    {
        try
        {
#if NET
            await
#endif
            using var dbCommand = CreateDbCommand(cmd);
            dbCommand.Connection = await GetConnectionAsync(cancellationToken);

#if NET
            await
#endif
            using (dbCommand.Connection)
            {
#if NET
                await
#endif
                using (var reader = await dbCommand.ExecuteReaderAsync(cancellationToken))
                {
                    var dataTable = new DataTable();
                    dataTable.Load(reader);
 
                    foreach (var parameter in cmd.Parameters)
                    {
                        if (parameter.Direction is ParameterDirection.Output or ParameterDirection.InputOutput)
                            parameter.Value = dbCommand.Parameters[parameter.Name].Value;
                    }
               
                    return dataTable;
                }
            }
        }
        catch (Exception ex)
        {
            throw GetDataAccessException(ex, cmd);
        }
    }
    
    /// <inheritdoc cref="GetResult(string)"/>
    public async Task<object?> GetResultAsync(string sql, CancellationToken cancellationToken = default)
    {
        return await GetResultAsync(new DataAccessCommand(sql), cancellationToken);
    }

    /// <inheritdoc cref="GetResult(DataAccessCommand)"/>
    public async Task<object?> GetResultAsync(DataAccessCommand cmd, CancellationToken cancellationToken = default)
    {
        object? scalarResult;
        try
        {
#if NET
            await
#endif
            using var dbCommand = CreateDbCommand(cmd);
            dbCommand.Connection = await GetConnectionAsync(cancellationToken);

#if NET
            await
#endif
            using (dbCommand.Connection)
            {
                scalarResult = await dbCommand.ExecuteScalarAsync(cancellationToken);

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

        return scalarResult;
    }
    
    /// <inheritdoc cref="SetCommand(DataAccessCommand)"/>
    public async Task<int> SetCommandAsync(DataAccessCommand cmd, CancellationToken cancellationToken = default)
    {
        int rowsAffected;
        try
        {
#if NET
            await
#endif
            using var dbCommand = CreateDbCommand(cmd);
            dbCommand.Connection = await GetConnectionAsync(cancellationToken);
#if NET
            await
#endif
            using (dbCommand.Connection)
            {
                rowsAffected = await dbCommand.ExecuteNonQueryAsync(cancellationToken);

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
    
    /// <inheritdoc cref="SetCommand(DataAccessCommand)"/>
    public async Task<int> SetCommandListAsync(IEnumerable<DataAccessCommand> commands, CancellationToken cancellationToken = default)
    {
        int numberOfRowsAffected = 0;
        DataAccessCommand? currentCommand = null;

        var connection = await GetConnectionAsync(cancellationToken);
#if NET
        await
#endif
        using (connection)
        {
#if NET
            await using var transaction = await connection.BeginTransactionAsync(cancellationToken);
#else
            using var transaction = connection.BeginTransaction();
#endif

            try
            {
                foreach (var command in commands)
                {
                    currentCommand = command;
#if NET
                    await
#endif
                    using var dbCommand = CreateDbCommand(command);
                    dbCommand.Connection = connection;
                    dbCommand.Transaction = transaction;

                    numberOfRowsAffected += await dbCommand.ExecuteNonQueryAsync(cancellationToken);
                }
#if NET
                await transaction.CommitAsync(cancellationToken);
                
#else
                transaction.Commit();
#endif
            }
            catch (Exception ex)
            {
#if NET
                await transaction.RollbackAsync(cancellationToken);
#else
                transaction.Rollback();
#endif
                throw GetDataAccessException(ex, currentCommand);
            }
        }

        return numberOfRowsAffected;
    }
    
    /// <inheritdoc cref="SetCommand(string)"/>
    public async Task<int> SetCommandAsync(string sql, CancellationToken cancellationToken = default)
    {
        return await SetCommandAsync(new DataAccessCommand(sql), cancellationToken);
    }
    
    /// <inheritdoc cref="SetCommand(IEnumerable&lt;string&gt;)"/>
    public async Task<int> SetCommandAsync(IEnumerable<string> sqlList, CancellationToken cancellationToken = default)
    {
        var commandList = sqlList.Select(sql => new DataAccessCommand(sql));

        int numberOfRowsAffected = await SetCommandListAsync(commandList, cancellationToken);
        return numberOfRowsAffected;
    }
    
    /// <inheritdoc cref="GetFields(string)"/>
    public Task<Dictionary<string,object?>> GetDictionaryAsync(string sql, CancellationToken cancellationToken = default) =>
        GetDictionaryAsync(new DataAccessCommand(sql), cancellationToken);
    
    
    /// <inheritdoc cref="GetFields(DataAccessCommand)"/>
    public async Task<Dictionary<string,object?>> GetDictionaryAsync(DataAccessCommand command, CancellationToken cancellationToken = default)
    {
        var result = new Dictionary<string, object?>();
        try
        {
#if NET
            await
#endif
            using var dbCommand = CreateDbCommand(command);
            dbCommand.Connection = await GetConnectionAsync(cancellationToken);
#if NET
            await
#endif
            using (dbCommand.Connection)
            {
#if NET
                await
#endif
                using (var dataReader =
                       await dbCommand.ExecuteReaderAsync(CommandBehavior.SingleRow, cancellationToken))
                {
                    while (await dataReader.ReadAsync(cancellationToken))
                    {
                        int count = 0;

                        while (count < dataReader.FieldCount)
                        {
                            string fieldName = dataReader.GetName(count);
                            if (result.ContainsKey(fieldName))
                                throw new DataAccessException($"[{fieldName}] field duplicated in get procedure");

                            result.Add(fieldName, dataReader.GetValue(count));
                            count += 1;
                        }
                    }
                }

                foreach (var parameter in command.Parameters)
                {
                    if (parameter.Direction is ParameterDirection.Output or ParameterDirection.InputOutput)
                        parameter.Value = dbCommand.Parameters[parameter.Name].Value;
                }
            }
        }
        catch (Exception ex)
        {
            throw GetDataAccessException(ex, command);
        }

        return result;
    }

    public async Task<List<Dictionary<string, object?>>> GetDictionaryListAsync(DataAccessCommand cmd, CancellationToken cancellationToken = default)
    {
        var dictionaryList = new List<Dictionary<string, object?>>();

        try
        {
#if NET
            await
#endif
            using var dbCommand = CreateDbCommand(cmd);
            dbCommand.Connection = await GetConnectionAsync(cancellationToken);

#if NET
            await
#endif
            using (dbCommand.Connection)
            {
#if NET
                await
#endif
                using (var dataReader = await dbCommand.ExecuteReaderAsync(cancellationToken))
                {
                    var columnNames = Enumerable.Range(0, dataReader.FieldCount)
                        .Select(i => dataReader.GetName(i))
                        .ToList();

                    while (await dataReader.ReadAsync(cancellationToken))
                    {
                        var dictionary = new Dictionary<string, object?>(StringComparer.InvariantCultureIgnoreCase);
                        foreach (var columnName in columnNames)
                        {
                            var value = dataReader.IsDBNull(dataReader.GetOrdinal(columnName))
                                ? null
                                : dataReader.GetValue(dataReader.GetOrdinal(columnName));
                            dictionary[columnName] = value;
                        }

                        dictionaryList.Add(dictionary);
                    }
                }

                foreach (var param in cmd.Parameters.Where(param =>
                             param.Direction is ParameterDirection.Output or ParameterDirection.InputOutput))
                {
                    param.Value = dbCommand.Parameters[param.Name].Value;
                }
                
            }
        }
        catch (Exception ex)
        {
            throw GetDataAccessException(ex, cmd);
        }

        return dictionaryList;
    }
    
    /// <inheritdoc cref="TableExists"/>
    public async Task<bool> TableExistsAsync(string tableName, CancellationToken cancellationToken = default)
    {
        var command = GetTableExistsCommand(tableName);
        var result = await GetResultAsync(command, cancellationToken);
        return (int)result! == 1;
    }
    
    /// <inheritdoc cref="TryConnection"/>
    public async Task<(bool, string?)> TryConnectionAsync(CancellationToken cancellationToken = default)
    {
        bool result;
        DbConnection? connection = null;
        string? errorMessage = null;
        try
        {
            connection = Factory.CreateConnection();
            connection!.ConnectionString = ConnectionString;
            await connection.OpenAsync(cancellationToken);
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
#if NET
                       await connection.CloseAsync();
#else
                    connection.Close();
#endif
                }

                connection.Dispose();
            }
        }

        return (result, errorMessage);
    }
    
    
    /// <inheritdoc cref="ExecuteBatch(string)"/>
    public async Task<bool> ExecuteBatchAsync(string script, CancellationToken cancellationToken = default)
    {
        string markpar = "GO";
        if (ConnectionProvider is DataAccessProvider.Oracle or DataAccessProvider.OracleNetCore)
        {
            markpar = "/";
        }

        if (script.Trim().Length <= 0) 
            return true;

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

        await SetCommandAsync(sqlList, cancellationToken);
        return true;
    }
    

    public async Task<bool> ColumnExistsAsync(string tableName, string columnName, CancellationToken cancellationToken = default)
    {
        var command = GetColumnExistsCommand(tableName, columnName);
        try
        {
#if NET
            await
#endif
            using var dbCommand = CreateDbCommand(command);
            dbCommand.Connection = await GetConnectionAsync(cancellationToken);
#if NET
            await
#endif
            using (dbCommand.Connection)
            {
#if NET
                await
#endif
                using (var reader = await dbCommand.ExecuteReaderAsync(CommandBehavior.SingleRow, cancellationToken))
                {
                    return await reader.ReadAsync(cancellationToken);
                }
            }
        }
        catch (Exception ex)
        {
            throw GetDataAccessException(ex, command);
        }
    }
    
}