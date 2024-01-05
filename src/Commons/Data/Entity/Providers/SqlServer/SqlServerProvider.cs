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
    DataAccess dataAccess,
    SqlServerScripts sqlServerScripts,
    IMemoryCache memoryCache,
    IOptions<MasterDataCommonsOptions> options,
    ILoggerFactory loggerFactory)
    : EntityProviderBase(dataAccess, options, loggerFactory)
{
    private readonly TimeSpan _cacheExpiration = new (4, 0, 0);
    private SqlServerScripts SqlServerScripts { get; } = sqlServerScripts;
    private IMemoryCache MemoryCache { get; } = memoryCache;
    private const string InsertInitial = "I";
    private const string UpdateInitial = "A";
    private const string DeleteInitial = "E";
    public override string VariablePrefix => "@";

    public override string GetCreateTableScript(Element element)
    {
        return SqlServerScripts.GetCreateTableScript(element);
    }
    
    public override string GetWriteProcedureScript(Element element)
    {
        return SqlServerScripts.GetWriteProcedureScript(element);
    }

    public override string GetReadProcedureScript(Element element)
    {
        return SqlServerScripts.GetReadProcedureScript(element);
    }
    
    public override DataAccessCommand GetInsertCommand(Element element, IDictionary<string,object?> values)
    {
        return GetWriteCommand(InsertInitial, element, values);
    }

    public override DataAccessCommand GetUpdateCommand(Element element, IDictionary<string,object?> values)
    {
        return GetWriteCommand(UpdateInitial, element, values);
    }

    public override DataAccessCommand GetDeleteCommand(Element element, IDictionary<string,object> filters)
    {
        return GetWriteCommand(DeleteInitial, element, filters!);
    }

    protected override DataAccessCommand GetInsertOrReplaceCommand(Element element, IDictionary<string,object?> values)
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
            if (MemoryCache.TryGetValue(cacheKey, out string readScript))
            {
                sql = readScript;
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


    private DataAccessCommand GetWriteCommand(string action, Element element, IDictionary<string,object?> values)
    {
        string sql;

        if (element.UseWriteProcedure)
        {
            sql = Options.GetWriteProcedureName(element);
        }
        else
        {
            var cacheKey = $"{element.Name}_WriteScript";
            if (MemoryCache.TryGetValue(cacheKey, out string writeScript))
            {
                sql = writeScript;
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
        writeCommand.Parameters.Add(new DataAccessParameter("@action", action, DbType.String, 1));

        var fields = element.Fields
            .ToList()
            .FindAll(x => x.DataBehavior == FieldBehavior.Real);

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
    
    private static object GetElementValue(ElementField field, IDictionary<string,object?> values)
    {
        if (!values.ContainsKey(field.Name)) 
            return DBNull.Value;
        
        var value = values[field.Name];
        
        if (value == null)
            return DBNull.Value;

        return field.DataType switch
        {
            FieldType.Date or FieldType.DateTime or FieldType.Float or FieldType.Int or FieldType.Time when
                string.IsNullOrEmpty(value.ToString()) => DBNull.Value,
            FieldType.UniqueIdentifier => Guid.Parse(value.ToString()!),
            FieldType.Bit => StringManager.ParseBool(values[field.Name]),
            _ => value
        };
    }

    private static DbType GetDbType(FieldType dataType)
    {
        return dataType switch
        {
            FieldType.Time => DbType.Time,
            FieldType.Date => DbType.Date,
            FieldType.DateTime => DbType.DateTime,
            FieldType.DateTime2 => DbType.DateTime,
            FieldType.Float => DbType.Double,
            FieldType.Int => DbType.Int32,
            FieldType.Bit => DbType.Boolean,
            FieldType.UniqueIdentifier => DbType.Guid,
            _ => DbType.String
        };
    }

    private static FieldType GetDataType(string databaseType)
    {
        if (string.IsNullOrEmpty(databaseType))
            return FieldType.NVarchar;

        databaseType = databaseType.ToLower().Trim();

        if (databaseType.Equals("varchar") ||
            databaseType.Equals("char"))
            return FieldType.Varchar;

        if (databaseType.Equals("bit"))
            return FieldType.Bit;
        
        if (databaseType.Equals("nvarchar") ||
            databaseType.Equals("nchar"))
            return FieldType.NVarchar;

        if (databaseType.Equals("int") ||
            databaseType.Equals("bigint") ||
            databaseType.Equals("tinyint"))
            return FieldType.Int;

        if (databaseType.Equals("float") ||
            databaseType.Equals("decimal") ||
            databaseType.Equals("numeric") ||
            databaseType.Equals("money") ||
            databaseType.Equals("smallmoney") ||
            databaseType.Equals("real"))
            return FieldType.Float;

        if (databaseType.Equals("time"))
            return FieldType.Time;
        
        if (databaseType.Equals("date"))
            return FieldType.Date;

        if (databaseType.Equals("datetime"))
            return FieldType.DateTime;
        
        if (databaseType.Equals("datetime2"))
            return FieldType.DateTime2;

        if (databaseType.Equals("text"))
            return FieldType.Text;

        return databaseType.Equals("ntext") ? FieldType.NText : FieldType.NVarchar;
    }
    
    public override async Task<Element> GetElementFromTableAsync(string tableName)
    {
        if (string.IsNullOrEmpty(tableName))
            throw new ArgumentNullException(nameof(tableName));

        if (!await DataAccess.TableExistsAsync(tableName))
            throw new JJMasterDataException($"Table {tableName} not found");

        var element = new Element
        {
            Name = tableName,
            TableName = tableName
        };

        var cmdFields = new DataAccessCommand
        {
            Type = CommandType.StoredProcedure,
            Sql = "sp_columns"
        };
        cmdFields.Parameters.Add(new DataAccessParameter("@table_name", tableName));

        var dtFields = await DataAccess.GetDataTableAsync(cmdFields);
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
        var primaryKeys = await DataAccess.GetDictionaryListAsync(cmdPks);
        foreach (var row in primaryKeys)
        {
            element.Fields[row["COLUMN_NAME"]?.ToString()].IsPk = true;
        }

        return element;
    }
    
}