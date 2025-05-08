#nullable enable

using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using JJMasterData.Commons.Configuration.Options;
using JJMasterData.Commons.Data.Entity.Models;
using JJMasterData.Commons.Data.Entity.Repository;
using JJMasterData.Commons.Data.Entity.Repository.Abstractions;
using JJMasterData.Commons.Exceptions;
using JJMasterData.Commons.Util;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace JJMasterData.Commons.Data.Entity.Providers;

public class SqlServerProvider(
    IConnectionRepository connectionRepository,
    SqlServerScripts sqlServerScripts,
    IMemoryCache memoryCache,
    IOptionsSnapshot<MasterDataCommonsOptions> options,
    ILoggerFactory loggerFactory)
    : EntityProviderBase(connectionRepository, options, loggerFactory)
{
    private static readonly TimeSpan CacheExpiration = new(4, 0, 0);

    private const string InsertInitial = "I";
    private const string UpdateInitial = "A";
    private const string DeleteInitial = "E";
    public override string VariablePrefix => "@";

    public override string GetCreateTableScript(Element element, List<RelationshipReference>? relationships = null)
    {
        return SqlServerScripts.GetCreateTableScript(element, relationships ?? []);
    }

    public override string GetWriteProcedureScript(Element element)
    {
        return sqlServerScripts.GetWriteProcedureScript(element);
    }

    public override string GetReadProcedureScript(Element element)
    {
        return sqlServerScripts.GetReadProcedureScript(element);
    }

    public override DataAccessCommand GetInsertCommand(Element element, Dictionary<string, object?> values)
    {
        return GetWriteCommand(InsertInitial, element, values);
    }

    public override DataAccessCommand GetUpdateCommand(Element element, Dictionary<string, object?> values)
    {
        return GetWriteCommand(UpdateInitial, element, values);
    }

    public override DataAccessCommand GetDeleteCommand(Element element, Dictionary<string, object> filters)
    {
        return GetWriteCommand(DeleteInitial, element, filters!);
    }

    protected internal override DataAccessCommand GetInsertOrReplaceCommand(Element element, Dictionary<string, object?> values)
    {
        return GetWriteCommand(string.Empty, element, values);
    }

    public override string? GetAlterTableScript(Element element, IEnumerable<ElementField> fields)
    {
        return SqlServerScripts.GetAlterTableScript(element, fields);
    }

    public override DataAccessCommand GetReadCommand(Element element, EntityParameters parameters,
        DataAccessParameter totalOfRecordsParameter)
    {
        string sql;

        if (element.UseReadProcedure)
        {
            sql = Options.GetReadProcedureName(element);
        }
        else
        {
            var cacheKey = $"{element.Name}_ReadScript";
            if (memoryCache.TryGetValue(cacheKey, out string? readScript))
            {
                sql = readScript!;
            }
            else
            {
                sql = sqlServerScripts.GetReadScript(element);
                memoryCache.Set(cacheKey, sql, CacheExpiration);
            }
        }

        var readCommand = new DataAccessCommand
        {
            Type = element.UseReadProcedure ? CommandType.StoredProcedure : CommandType.Text,
            Sql = sql,
            Parameters =
            {
                new("@orderby", parameters.OrderBy.ToQueryParameter()),
                new("@regporpag", parameters.RecordsPerPage),
                new("@pag", parameters.CurrentPage)
            }
        };

        foreach (var field in element.Fields)
        {
            if (field.Filter.Type == FilterMode.Range)
            {
                object? valueFrom = DBNull.Value;
                if (parameters.Filters.ContainsKey($"{field.Name}_from") &&
                    parameters.Filters[$"{field.Name}_from"] != null)
                {
                    valueFrom = parameters.Filters[$"{field.Name}_from"];
                    valueFrom = valueFrom?.ToString();
                }

                var fromParameter = new DataAccessParameter
                {
                    Direction = ParameterDirection.Input,
                    Type = GetDbType(field.DataType),
                    Size = field.Size,
                    Name = $"{field.Name}_from",
                    Value = valueFrom
                };
                readCommand.Parameters.Add(fromParameter);

                object? valueTo = DBNull.Value;
                if (parameters.Filters.ContainsKey($"{field.Name}_to") &&
                    parameters.Filters[$"{field.Name}_to"] != null)
                {
                    valueTo = parameters.Filters[$"{field.Name}_to"];
                    valueTo = valueTo?.ToString();
                }

                var toParameter = new DataAccessParameter
                {
                    Direction = ParameterDirection.Input,
                    Type = GetDbType(field.DataType),
                    Size = field.Size,
                    Name = $"{field.Name}_to",
                    Value = valueTo
                };
                readCommand.Parameters.Add(toParameter);
            }
            else if (field.Filter.Type != FilterMode.None || field.IsPk)
            {
                var value = GetElementValue(field, parameters.Filters);

                var dbType = GetDbType(field.DataType);
                var parameter = new DataAccessParameter
                {
                    Direction = ParameterDirection.Input,
                    Type = dbType,
                    Size = field.Size,
                    Name = field.Name,
                    Value = value
                };
                readCommand.Parameters.Add(parameter);
            }
        }

        readCommand.Parameters.Add(totalOfRecordsParameter);

        return readCommand;
    }


    private DataAccessCommand GetWriteCommand(string action, Element element, Dictionary<string, object?> values)
    {
        string sql;

        if (element.UseWriteProcedure)
        {
            sql = Options.GetWriteProcedureName(element);
        }
        else
        {
            var cacheKey = $"{element.Name}_WriteScript";
            if (memoryCache.TryGetValue(cacheKey, out string? writeScript))
            {
                sql = writeScript!;
            }
            else
            {
                sql = SqlServerScripts.GetWriteScript(element);
                memoryCache.Set(cacheKey, sql, CacheExpiration);
            }
        }

        var writeCommand = new DataAccessCommand
        {
            Type = element.UseWriteProcedure ? CommandType.StoredProcedure : CommandType.Text,
            Sql = sql
        };

        writeCommand.AddParameter("@action", action, DbType.AnsiString, 1);

        foreach (var field in element.Fields)
        {
            if (field.DataBehavior is not (FieldBehavior.Real or FieldBehavior.WriteOnly))
                continue;

            var value = GetElementValue(field, values);
            var parameter = new DataAccessParameter
            {
                Name = field.Name,
                Size = field.Size,
                Value = value,
                Type = GetDbType(field.DataType)
            };
            writeCommand.Parameters.Add(parameter);
        }

        var resultParameter = new DataAccessParameter
        {
            Direction = ParameterDirection.Output,
            Name = "@RET",
            Value = 0,
            Type = DbType.Int32
        };
        writeCommand.Parameters.Add(resultParameter);

        return writeCommand;
    }

    private static object GetElementValue(ElementField field, Dictionary<string, object?> values)
    {
        if (!values.TryGetValue(field.Name, out var value))
            return DBNull.Value;

        if (value is null)
            return DBNull.Value;

        return field.DataType switch
        {
            FieldType.Date or FieldType.DateTime or FieldType.Float or FieldType.Decimal or FieldType.Int
                or FieldType.Time when
                string.IsNullOrEmpty(value.ToString()) => DBNull.Value,
            FieldType.UniqueIdentifier => TryGetGuid(value),
            FieldType.Bit => StringManager.ParseBool(values[field.Name]),
            FieldType.Varchar or FieldType.NVarchar => value.ToString()!,
            _ => value!
        };
    }

    private static object TryGetGuid(object? value)
    {
        if (value is Guid guid)
            return guid;

        if (Guid.TryParse(value?.ToString(), out guid))
            return guid;

        return DBNull.Value;
    }

    private static DbType GetDbType(FieldType dataType)
    {
        return dataType switch
        {
            FieldType.Time => DbType.Time,
            FieldType.Date => DbType.Date,
            FieldType.DateTime => DbType.DateTime,
            FieldType.DateTime2 => DbType.DateTime,
            FieldType.Float => DbType.Double, //SQL Server Float is equivalent of the C# double.
            FieldType.Decimal => DbType.Decimal,
            FieldType.Int => DbType.Int32,
            FieldType.Bit => DbType.Boolean,
            FieldType.UniqueIdentifier => DbType.Guid,
            FieldType.NVarchar => DbType.String,
            FieldType.NText => DbType.String,
            FieldType.Varchar => DbType.AnsiString,
            _ => DbType.AnsiString
        };
    }

    private static FieldType GetDataType(string databaseType)
    {
        if (string.IsNullOrEmpty(databaseType))
            return FieldType.NVarchar;

        databaseType = databaseType.ToLower().Trim();

        return databaseType switch
        {
            "varchar" or "char" => FieldType.Varchar,
            "bit" => FieldType.Bit,
            "nvarchar" or "nchar" => FieldType.NVarchar,
            "int" or "int identity" or "bigint" or "tinyint" => FieldType.Int,
            "float" or "numeric" or "money" or "smallmoney" or "real" => FieldType.Float,
            "time" => FieldType.Time,
            "decimal" => FieldType.Decimal,
            "date" => FieldType.Date,
            "datetime" => FieldType.DateTime,
            "datetime2" => FieldType.DateTime2,
            "text" => FieldType.Text,
            "ntext" => FieldType.NText,
            "uniqueidentifier" => FieldType.UniqueIdentifier,
            _ => FieldType.Varchar
        };
    }

    public override Task<Element> GetElementFromTableAsync(string schemaName, string connectionId, Guid? guid)
    {
        return GetElementFromTableAsyncCore(schemaName, connectionId, guid);
    }

    public override Task<Element> GetElementFromTableAsync(string tableName, Guid? connectionId = null)
    {
        return GetElementFromTableAsyncCore("dbo", tableName, connectionId);
    }

    private async Task<Element> GetElementFromTableAsyncCore(string schemaName, string tableName,
        Guid? connectionId)
    {
        var dataAccess = GetDataAccess(connectionId);

        if (string.IsNullOrEmpty(tableName))
            throw new ArgumentNullException(nameof(tableName));

        if (!await TableExistsAsync(schemaName, tableName, connectionId))
            throw new JJMasterDataException($"Table {tableName} not found");

        var element = new Element
        {
            Name = tableName,
            TableName = tableName,
            ConnectionId = connectionId
        };

        var cmdFields = new DataAccessCommand
        {
            Type = CommandType.StoredProcedure,
            Sql = "sp_columns"
        };
        cmdFields.Parameters.Add(new DataAccessParameter("@table_name", tableName));
        cmdFields.Parameters.Add(new DataAccessParameter("@table_owner", schemaName));

        var dtFields = await dataAccess.GetDataTableAsync(cmdFields);
        if (dtFields.Rows.Count == 0)
            throw new JJMasterDataException($"Table {tableName} has invalid structure");

        foreach (DataRow row in dtFields.Rows)
        {
            var field = new ElementField
            {
                Name = row["COLUMN_NAME"].ToString()!.Replace(" ", "_"),
                Label = (string)row["COLUMN_NAME"],
                Size = (int)row["LENGTH"],
                AutoNum = ((string)row["TYPE_NAME"]).IndexOf("IDENTITY", StringComparison.OrdinalIgnoreCase) >= 0,
                IsRequired = row["NULLABLE"].ToString()?.Equals("0") ?? false,
                DataType = GetDataType((string)row["TYPE_NAME"])
            };

            element.Fields.Add(field);
        }

        var cmdPks = new DataAccessCommand
        {
            Type = CommandType.StoredProcedure,
            Sql = "sp_pkeys"
        };

        cmdPks.Parameters.Add(new DataAccessParameter("@table_name", tableName));
        cmdPks.Parameters.Add(new DataAccessParameter("@table_owner", schemaName));

        var primaryKeys = await dataAccess.GetDictionaryListAsync(cmdPks);
        foreach (var row in primaryKeys)
        {
            element.Fields[row["COLUMN_NAME"]?.ToString()].IsPk = true;
        }

        return element;
    }

    /// <summary>
    /// Check if table exists in the database
    /// </summary>
    public override bool TableExists(string tableName, Guid? connectionId = null)
    {
        var dataAccess = GetDataAccess(connectionId);
        var result = dataAccess.GetResult(GetTableExistsCommand("dbo", tableName));
        return result as int? == 1;
    }

    public override async Task<bool> TableExistsAsync(
        string schema,
        string tableName,
        Guid? connectionId = null,
        CancellationToken cancellationToken = default)
    {
        var dataAccess = GetDataAccess(connectionId);
        var command = GetTableExistsCommand(schema, tableName);
        var result = await dataAccess.GetResultAsync(command, cancellationToken);
        return result as int? == 1;
    }

    /// <inheritdoc cref="TableExists"/>
    public override async Task<bool> TableExistsAsync(string tableName, Guid? connectionId = null,
        CancellationToken cancellationToken = default)
    {
        var dataAccess = GetDataAccess(connectionId);
        var command = GetTableExistsCommand("dbo", tableName);
        var result = await dataAccess.GetResultAsync(command, cancellationToken);
        return result as int? == 1;
    }

    public override async Task<bool> ColumnExistsAsync(string tableName, string columnName, Guid? connectionId = null,
        CancellationToken cancellationToken = default)
    {
        var dataAccess = GetDataAccess(connectionId);
        var command = GetColumnExistsCommand(tableName, columnName);
        var result = await dataAccess.GetResultAsync(command, cancellationToken);
        return result as int? == 1;
    }

    private static DataAccessCommand GetTableExistsCommand(string schema, string table)
    {
        const string sql =
            "SELECT COUNT(1) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = @Table AND TABLE_SCHEMA = @Schema";
        var command = new DataAccessCommand
        {
            Sql = sql,
            Parameters =
            {
                new DataAccessParameter
                {
                    Name = "@Table",
                    Value = table
                },
                new DataAccessParameter
                {
                    Name = "@Schema",
                    Value = schema
                }
            }
        };

        return command;
    }

    public override async Task DropStoredProcedureAsync(string procedureName, Guid? connectionId = null)
    {
        if (string.IsNullOrEmpty(procedureName))
            throw new ArgumentNullException(nameof(procedureName));

        var dataAccess = GetDataAccess(connectionId);

        var command = new DataAccessCommand
        {
            Sql = $"IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = '{procedureName}') " +
                  $"BEGIN DROP PROCEDURE [dbo].[{procedureName}] END",
            Type = CommandType.Text
        };


        await dataAccess.SetCommandAsync(command);
    }

    public override async Task<List<string>> GetStoredProcedureListAsync(Guid? connectionId = null)
    {
        var dataAccess = GetDataAccess(connectionId);
        var command = new DataAccessCommand
        {
            Sql =
                "SELECT ROUTINE_NAME FROM INFORMATION_SCHEMA.ROUTINES WHERE ROUTINE_TYPE = 'PROCEDURE' order by ROUTINE_NAME",
            Type = CommandType.Text
        };

        var procedures = await dataAccess.GetDictionaryListAsync(command);
        var procedureList = new List<string>();
        foreach (var procedure in procedures)
        {
            if (procedure.TryGetValue("ROUTINE_NAME", out var procedureName) && procedureName is not null)
            {
                procedureList.Add(procedureName.ToString()!);
            }
        }

        return procedureList;
    }

    public override async Task<string?> GetStoredProcedureDefinitionAsync(string procedureName,
        Guid? connectionId = null)
    {
        if (string.IsNullOrEmpty(procedureName))
            throw new ArgumentNullException(nameof(procedureName));

        var dataAccess = GetDataAccess(connectionId);

        var command = new DataAccessCommand
        {
            Sql = @"SELECT OBJECT_DEFINITION(OBJECT_ID(@ProcedureName)) AS Definition",
            Type = CommandType.Text
        };

        command.Parameters.Add(new DataAccessParameter("@ProcedureName", procedureName, DbType.String));

        var result = await dataAccess.GetResultAsync(command);
        return result?.ToString();
    }


    private static DataAccessCommand GetColumnExistsCommand(string tableName, string columnName)
    {
        var command = new DataAccessCommand
        {
            Sql =
                "SELECT COUNT(1) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = @TableName AND COLUMN_NAME = @ColumnName"
        };

        command.AddParameter("@TableName", tableName, DbType.AnsiString);
        command.AddParameter("@ColumnName", columnName, DbType.AnsiString);

        return command;
    }
}