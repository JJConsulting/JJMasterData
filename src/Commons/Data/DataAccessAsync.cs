#nullable enable

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using JJMasterData.Commons.Data.Extensions;
using JJMasterData.Commons.Exceptions;

namespace JJMasterData.Commons.Data;

public partial class DataAccess
{
    [MustDisposeResource]
    private async Task<DbConnection> CreateConnectionAsync(CancellationToken cancellationToken = default)
    {
        var connection = _dbProviderFactory.CreateConnection();

        connection!.ConnectionString = _connectionString;
        await connection.OpenAsync(cancellationToken);

        return connection;
    }

    ///<inheritdoc cref="GetDataTable(string)"/>
    public Task<DataTable> GetDataTableAsync(string sql, CancellationToken cancellationToken = default)
    {
        return GetDataTableAsync(new DataAccessCommand(sql), cancellationToken);
    }

    ///<inheritdoc cref="GetDataTable(DataAccessCommand)"/>
    public async Task<DataTable> GetDataTableAsync(DataAccessCommand command,
        CancellationToken cancellationToken = default)

    {
        var dataTable = new DataTable();
        await ExecuteDataCommandAsync(command,
            dataReader => dataReader.FillAsync(dataTable, cancellationToken: cancellationToken), cancellationToken);
        return dataTable;
    }

    ///<inheritdoc cref="GetDataSet(string)"/>
    public Task<DataSet> GetDataSetAsync(string sql, CancellationToken cancellationToken = default)
    {
        return GetDataSetAsync(new DataAccessCommand(sql), cancellationToken);
    }

    ///<inheritdoc cref="GetDataSet(DataAccessCommand)"/>
    public async Task<DataSet> GetDataSetAsync(DataAccessCommand command, CancellationToken cancellationToken = default)
    {
        var dataSet = new DataSet();
        await ExecuteDataCommandAsync(command,
            dataAdapter => dataAdapter.FillAsync(dataSet, cancellationToken: cancellationToken), cancellationToken);
        return dataSet;
    }

    public async Task ExecuteDataCommandAsync(DataAccessCommand command, Func<DbDataReader, Task> readerAction,
        CancellationToken cancellationToken = default)
    {
        try
        {
            using var dbCommand = CreateDbCommand(command);
            dbCommand.Connection = await CreateConnectionAsync(cancellationToken);

            using (dbCommand.Connection)
            {
                using (var reader = await dbCommand.ExecuteReaderAsync(cancellationToken))
                {
                    await readerAction(reader);

                    SetOutputParameters(command, dbCommand);
                }
            }
        }
        catch (Exception ex)
        {
            throw GetDataAccessException(ex, command);
        }
    }

    /// <inheritdoc cref="GetResult(string)"/>
    public Task<object?> GetResultAsync(string sql, CancellationToken cancellationToken = default)
    {
        return GetResultAsync(new DataAccessCommand(sql), cancellationToken);
    }

    /// <inheritdoc cref="GetResult(DataAccessCommand)"/>
    public async Task<object?> GetResultAsync(DataAccessCommand command, CancellationToken cancellationToken = default)
    {
        object? scalarResult;
        try
        {
            using var dbCommand = CreateDbCommand(command);
            dbCommand.Connection = await CreateConnectionAsync(cancellationToken);
            using (dbCommand.Connection)
            {
                scalarResult = await dbCommand.ExecuteScalarAsync(cancellationToken);

                SetOutputParameters(command, dbCommand);
            }
        }
        catch (Exception ex)
        {
            throw GetDataAccessException(ex, command);
        }

        return scalarResult;
    }

    /// <inheritdoc cref="SetCommand(DataAccessCommand)"/>
    public async Task<int> SetCommandAsync(DataAccessCommand command, CancellationToken cancellationToken = default)
    {
        int rowsAffected;
        try
        {
            using var dbCommand = CreateDbCommand(command);
            dbCommand.Connection = await CreateConnectionAsync(cancellationToken);
            using (dbCommand.Connection)
            {
                rowsAffected = await dbCommand.ExecuteNonQueryAsync(cancellationToken);

                SetOutputParameters(command, dbCommand);
            }
        }
        catch (Exception ex)
        {
            throw GetDataAccessException(ex, command);
        }

        return rowsAffected;
    }

    /// <inheritdoc cref="SetCommand(DataAccessCommand)"/>
    public async Task<int> SetCommandListAsync(IEnumerable<DataAccessCommand> commands,
        CancellationToken cancellationToken = default)
    {
        int numberOfRowsAffected = 0;
        DataAccessCommand? currentCommand = null;

        using var connection = await CreateConnectionAsync(cancellationToken);
        
#if NETSTANDARD
        using var transaction = connection.BeginTransaction();
#else
        using var transaction = await connection.BeginTransactionAsync(cancellationToken);
#endif
        try
        {
            foreach (var command in commands)
            {
                currentCommand = command;
                using var dbCommand = CreateDbCommand(command);
                dbCommand.Connection = connection;
                dbCommand.Transaction = transaction;

                numberOfRowsAffected += await dbCommand.ExecuteNonQueryAsync(cancellationToken);
            }
#if NETSTANDARD
            transaction.Commit();
#else
            await transaction.CommitAsync(cancellationToken);
#endif
        }
        catch (Exception ex)
        {
#if NETSTANDARD
            transaction.Rollback();
#else
            await transaction.RollbackAsync(cancellationToken);
#endif
            throw GetDataAccessException(ex, currentCommand);
        }


        return numberOfRowsAffected;
    }

    /// <inheritdoc cref="SetCommand(string)"/>
    public Task<int> SetCommandAsync(string sql, CancellationToken cancellationToken = default)
    {
        return SetCommandAsync(new DataAccessCommand(sql), cancellationToken);
    }

    /// <inheritdoc cref="SetCommand(IEnumerable&lt;string&gt;)"/>
    public async Task<int> SetCommandAsync(IEnumerable<string> sqlList, CancellationToken cancellationToken = default)
    {
        var commandList = sqlList.Select(sql => new DataAccessCommand(sql));

        int numberOfRowsAffected = await SetCommandListAsync(commandList, cancellationToken);
        return numberOfRowsAffected;
    }

    /// <inheritdoc cref="GetHashtable(string)"/>
    public Task<Dictionary<string, object?>> GetDictionaryAsync(string sql,
        CancellationToken cancellationToken = default)
    {
        return GetDictionaryAsync(new DataAccessCommand(sql), cancellationToken);
    }

    public Task<Dictionary<string, object?>> GetDictionaryAsync(DataAccessCommand command,
        CancellationToken cancellationToken = default)
    {
        var result = new Dictionary<string,object?>(StringComparer.InvariantCultureIgnoreCase);
        return GetDataAsync(result, command, cancellationToken);
    }

    public Task<Hashtable> GetHashtableAsync(DataAccessCommand command,
        CancellationToken cancellationToken = default)
    {
        var result = new Hashtable(StringComparer.InvariantCultureIgnoreCase);
        return GetDataAsync(result, command, cancellationToken);
    }

    private async Task<T> GetDataAsync<T>(
        T result,
        DataAccessCommand command,
        CancellationToken cancellationToken) where T : IDictionary, new()
    {
        try
        {
            using var dbCommand = CreateDbCommand(command);
            dbCommand.Connection = await CreateConnectionAsync(cancellationToken);

            using (dbCommand.Connection)
            {
                using (var dataReader =
                       await dbCommand.ExecuteReaderAsync(CommandBehavior.SingleRow, cancellationToken))
                {
                    while (await dataReader.ReadAsync(cancellationToken))
                    {
                        for (var count = 0; count < dataReader.FieldCount; count++)
                        {
                            var fieldName = dataReader.GetName(count);
                            if (result.Contains(fieldName))
                                throw new DataAccessException($"[{fieldName}] field duplicated at SQL query.");

                            result.Add(fieldName, dataReader.GetValue(count));
                        }
                    }
                }

                SetOutputParameters(command, dbCommand);
            }
        }
        catch (Exception ex)
        {
            throw GetDataAccessException(ex, command);
        }

        return result;
    }
    
    public async Task<List<Dictionary<string, object?>>> GetDictionaryListAsync(DataAccessCommand command,
        CancellationToken cancellationToken = default)
    {
        var dictionaryList = new List<Dictionary<string, object?>>();

        try
        {
            using var dbCommand = CreateDbCommand(command);
            dbCommand.Connection = await CreateConnectionAsync(cancellationToken);

            using var connection = dbCommand.Connection;
            using (var dataReader = await dbCommand.ExecuteReaderAsync(cancellationToken))
            {
                List<string> columnNames = [];

                for (var i = 0; i < dataReader.FieldCount; i++)
                {
                    columnNames.Add(dataReader.GetName(i));
                }

                while (await dataReader.ReadAsync(cancellationToken))
                {
                    var dictionary = new Dictionary<string, object?>(StringComparer.InvariantCultureIgnoreCase);
                    foreach (var columnName in columnNames)
                    {
                        var ordinal = dataReader.GetOrdinal(columnName);
                        var value = dataReader.IsDBNull(ordinal)
                            ? null
                            : dataReader.GetValue(ordinal);
                        dictionary[columnName] = value;
                    }

                    dictionaryList.Add(dictionary);
                }
            }

            SetOutputParameters(command, dbCommand);
        }
        catch (Exception ex)
        {
            throw GetDataAccessException(ex, command);
        }

        return dictionaryList;
    }

    /// <inheritdoc cref="TryConnection"/>
    public async Task<ConnectionResult> TryConnectionAsync(CancellationToken cancellationToken = default)
    {
        bool result;
        DbConnection? connection = null;
        string? errorMessage = null;
        try
        {
            connection = _dbProviderFactory.CreateConnection();
            connection!.ConnectionString = _connectionString;
            await connection.OpenAsync(cancellationToken);
            result = true;
        }
        catch (Exception ex)
        {
            result = false;
            var error = new StringBuilder();
            error.AppendLine(ex.Message);
            if (ex.InnerException is { Message: not null })
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

        return new(result, errorMessage);
    }

    /// <inheritdoc cref="ExecuteBatch(string)"/>
    public async Task<bool> ExecuteBatchAsync(string script, CancellationToken cancellationToken = default)
    {
        string markpar = "GO";
        if (_connectionProvider is DataAccessProvider.Oracle or DataAccessProvider.OracleNetCore)
        {
            markpar = "/";
        }

        if (script.Trim().Length == 0)
            return true;

        var sqlList = new List<string>();
        string sqlBatch = string.Empty;
        script += $"\n{markpar}"; // make sure last batch is executed. 

        foreach (string line in script.Split(["\n", "\r"], StringSplitOptions.RemoveEmptyEntries))
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
}