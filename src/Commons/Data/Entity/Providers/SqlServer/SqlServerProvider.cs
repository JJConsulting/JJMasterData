#nullable enable

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using JJMasterData.Commons.Configuration.Options;
using JJMasterData.Commons.Data.Entity.Models;
using JJMasterData.Commons.Data.Entity.Repository;
using JJMasterData.Commons.Exceptions;
using JJMasterData.Commons.Util;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace JJMasterData.Commons.Data.Entity.Providers;

public class SqlServerProvider(
    SqlServerScripts sqlServerScripts,
    IMemoryCache memoryCache,
    IOptionsSnapshot<MasterDataCommonsOptions> options,
    ILoggerFactory loggerFactory)
    : EntityProviderBase(options, loggerFactory)
{
    private readonly TimeSpan _cacheExpiration = new (4, 0, 0);
    private SqlServerScripts SqlServerScripts { get; } = sqlServerScripts;
    private IMemoryCache MemoryCache { get; } = memoryCache;
    private const string InsertInitial = "I";
    private const string UpdateInitial = "A";
    private const string DeleteInitial = "E";
    public override string VariablePrefix => "@";

    public override string GetCreateTableScript(Element element, List<RelationshipReference>? relationships = null)
    {
        return SqlServerScripts.GetCreateTableScript(element,relationships ?? []);
    }
    
    public override string GetWriteProcedureScript(Element element)
    {
        return SqlServerScripts.GetWriteProcedureScript(element);
    }

    public override string GetReadProcedureScript(Element element)
    {
        return SqlServerScripts.GetReadProcedureScript(element);
    }
    
    public override DataAccessCommand GetInsertCommand(Element element, Dictionary<string,object?> values)
    {
        return GetWriteCommand(InsertInitial, element, values);
    }

    public override DataAccessCommand GetUpdateCommand(Element element, Dictionary<string,object?> values)
    {
        return GetWriteCommand(UpdateInitial, element, values);
    }

    public override DataAccessCommand GetDeleteCommand(Element element, Dictionary<string,object> filters)
    {
        return GetWriteCommand(DeleteInitial, element, filters!);
    }

    protected override DataAccessCommand GetInsertOrReplaceCommand(Element element, Dictionary<string,object?> values)
    {
        return GetWriteCommand(string.Empty, element, values);
    }

    public override string GetAlterTableScript(Element element, IEnumerable<ElementField> fields)
    {
        return SqlServerScripts.GetAlterTableScript(element, fields);
    }
    
    public override DataAccessCommand GetReadCommand(Element element, EntityParameters parameters, DataAccessParameter totalOfRecordsParameter)
    {
        string sql;

        if (element.UseReadProcedure)
        {
            sql = Options.GetReadProcedureName(element);
        }
        else
        {
            var cacheKey = $"{element.Name}_ReadScript";
            if (MemoryCache.TryGetValue(cacheKey, out string? readScript))
            {
                sql = readScript!;
            }
            else
            {
                sql = SqlServerScripts.GetReadScript(element);
                MemoryCache.Set(cacheKey, sql, _cacheExpiration);
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


    private DataAccessCommand GetWriteCommand(string action, Element element, Dictionary<string,object?> values)
    {
        string sql;

        if (element.UseWriteProcedure)
        {
            sql = Options.GetWriteProcedureName(element);
        }
        else
        {
            var cacheKey = $"{element.Name}_WriteScript";
            if (MemoryCache.TryGetValue(cacheKey, out string? writeScript))
            {
                sql = writeScript!;
            }
            else
            {
                sql = SqlServerScripts.GetWriteScript(element);
                MemoryCache.Set(cacheKey, sql, _cacheExpiration);
            }
        }
        var writeCommand = new DataAccessCommand
        {
            Type = element.UseWriteProcedure ? CommandType.StoredProcedure : CommandType.Text,
            Sql = sql
        };
        writeCommand.Parameters.Add(new DataAccessParameter("@action", action, DbType.AnsiString, 1));

        var fields = element.Fields
            .ToList()
            .FindAll(x => x.DataBehavior is FieldBehavior.Real or FieldBehavior.WriteOnly);

        foreach (var field in fields)
        {
            var value = GetElementValue(field, values);
            var parameter = new DataAccessParameter
            {
                Name = $"@{field.Name}",
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
    
    private static object GetElementValue(ElementField field, Dictionary<string,object?> values)
    {
        if (!values.TryGetValue(field.Name, out var value)) 
            return DBNull.Value;

        if (value is null)
            return DBNull.Value;

        return field.DataType switch
        {
            FieldType.Date or FieldType.DateTime or FieldType.Float or FieldType.Int or FieldType.Time when
                string.IsNullOrEmpty(value.ToString()) => DBNull.Value,
            FieldType.UniqueIdentifier => TryGetGuid(value),
            FieldType.Bit => StringManager.ParseBool(values[field.Name]),
            FieldType.Varchar or FieldType.NVarchar => value.ToString(),
            _ => value
        };
    }

    private static object TryGetGuid(object? value)
    {
        if (value is Guid guid)
            return guid;
        
        if(Guid.TryParse(value?.ToString(), out guid))
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
            "float" or "decimal" or "numeric" or "money" or "smallmoney" or "real" => FieldType.Float,
            "time" => FieldType.Time,
            "date" => FieldType.Date,
            "datetime" => FieldType.DateTime,
            "datetime2" => FieldType.DateTime2,
            "text" => FieldType.Text,
            "ntext" => FieldType.NText,
            "uniqueidentifier" => FieldType.UniqueIdentifier,
            _ => FieldType.Varchar
        };
    }
    
    public override async Task<Element> GetElementFromTableAsync(string tableName, Guid? connectionId = null)
    {
        var dataAccess = GetDataAccess(connectionId);
        
        if (string.IsNullOrEmpty(tableName))
            throw new ArgumentNullException(nameof(tableName));

        if (!await dataAccess.TableExistsAsync(tableName))
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
        
        var dtFields = await dataAccess.GetDataTableAsync(cmdFields);
        if (dtFields.Rows.Count == 0)
            throw new JJMasterDataException($"Table {tableName} has invalid structure");

        foreach (DataRow row in dtFields.Rows)
        {
            var field = new ElementField
            {
                Name = row["COLUMN_NAME"].ToString()!.Replace(" ","_"),
                Label = (string)row["COLUMN_NAME"],
                Size = (int)row["LENGTH"],
                AutoNum = ((string)row["TYPE_NAME"]).ToUpper().Contains("IDENTITY"),
                IsRequired = row["NULLABLE"].ToString()?.Equals("0") ?? false,
                DataType = GetDataType((string)row["TYPE_NAME"])
            };

            element.Fields.Add(field);
        }

        //Primary Keys
        var cmdPks = new DataAccessCommand
        {
            Type = CommandType.StoredProcedure,
            Sql = "sp_pkeys"
        };

        cmdPks.Parameters.Add(new DataAccessParameter("@table_name", tableName));
        var primaryKeys = await dataAccess.GetDictionaryListAsync(cmdPks);
        foreach (var row in primaryKeys)
        {
            element.Fields[row["COLUMN_NAME"]?.ToString()].IsPk = true;
        }

        return element;
    }
    
}