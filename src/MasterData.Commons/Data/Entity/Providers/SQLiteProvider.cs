#nullable enable
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using JJMasterData.Commons.Configuration.Options;
using JJMasterData.Commons.Data.Entity.Models;
using JJMasterData.Commons.Data.Entity.Repository;
using JJMasterData.Commons.Data.Entity.Repository.Abstractions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace JJMasterData.Commons.Data.Entity.Providers;

public class SQLiteProvider : IEntityProvider
{
    private const char Tab = '\t';
    public string VariablePrefix => "@";

    public string GetCreateTableScript(Element element, List<RelationshipReference>? relationships = null)
    {
        if (element == null)
            throw new ArgumentNullException(nameof(element));

        if (element.Fields == null || element.Fields.Count == 0)
            throw new ArgumentNullException(nameof(element.Fields));

        StringBuilder sql = new StringBuilder();

        sql.AppendLine("-- TABLE");
        sql.Append("CREATE TABLE [");
        sql.Append(element.TableName);
        sql.AppendLine("] (");
        bool isFirst = true;
        var fields = element.Fields
            .ToList()
            .FindAll(x => x.DataBehavior == FieldBehavior.Real);

        foreach (var f in fields)
        {
            if (isFirst)
                isFirst = false;
            else
                sql.AppendLine(",");

            sql.Append(Tab);
            sql.Append('[');
            sql.Append(f.Name);
            sql.Append("] ");

            switch (f.DataType)
            {
                case FieldType.Int:
                    sql.Append("INTEGER");
                    break;
                case FieldType.Float:
                    sql.Append("Real");
                    break;
                case FieldType.NText:
                case FieldType.NVarchar:
                case FieldType.Varchar:
                    sql.Append("TEXT");
                    break;
                default:
                    sql.Append(f.DataType.ToString());
                    break;
            }

            if (f.IsRequired)
                sql.Append(" NOT NULL");

            if (f.AutoNum && f.IsPk)
                sql.Append(" PRIMARY KEY AUTOINCREMENT ");
        }

        isFirst = true;
        foreach (var f in fields.FindAll(x => x.IsPk && !x.AutoNum))
        {
            if (isFirst)
            {
                isFirst = false;
                sql.AppendLine(",");
                sql.Append(Tab);
                sql.Append("PRIMARY KEY (");
            }
            else
            {
                sql.Append(',');
            }

            sql.Append('[');
            sql.Append(f.Name);
            sql.Append("] ");
        }

        if (!isFirst)
            sql.Append(')');

        sql.AppendLine();
        sql.AppendLine(")");
        sql.AppendLine("GO");
        sql.AppendLine();

        //sql.AppendLine(DoSqlCreateRelation(element));
        sql.AppendLine();

        int nIndex = 1;
        if (element.Indexes.Count > 0)
        {
            foreach (var index in element.Indexes)
            {
                sql.Append("CREATE");
                sql.Append(index.IsUnique ? " UNIQUE" : "");
                sql.Append(index.IsClustered ? " CLUSTERED" : "");
                sql.Append(" INDEX [IX_");
                sql.Append(element.TableName);
                sql.Append('_');
                sql.Append(nIndex);
                sql.Append("] ON ");
                sql.AppendLine(element.TableName);

                sql.Append(Tab);
                sql.AppendLine("(");
                for (int i = 0; i < index.Columns.Count; i++)
                {
                    if (i > 0)
                        sql.AppendLine(", ");

                    sql.Append(Tab);
                    sql.Append(index.Columns[i]);
                }

                sql.AppendLine();
                sql.Append(Tab);
                sql.AppendLine(")");
                sql.AppendLine("GO");
                nIndex++;
            }
        }

        sql.AppendLine();
        return sql.ToString();
    }


    // ReSharper disable once UnusedMember.Local
    private static string GetRelationshipsScript(Element element)
    {
        StringBuilder sql = new StringBuilder();

        if (element.Relationships.Count > 0)
        {
            sql.AppendLine("-- RELATIONSHIPS");
            var listConstraint = new List<string>();
            foreach (var r in element.Relationships)
            {
                string constraintName = $"FK_{r.ChildElement}_{element.TableName}";
                if (!listConstraint.Contains(constraintName))
                {
                    listConstraint.Add(constraintName);
                }
                else
                {
                    bool hasContraint = true;
                    for (int nCount = 1; hasContraint; nCount++)
                    {
                        if (!listConstraint.Contains(constraintName + nCount))
                        {
                            constraintName += nCount;
                            listConstraint.Add(constraintName);
                            hasContraint = false;
                        }
                    }
                }

                sql.Append("ALTER TABLE ");
                sql.AppendLine(r.ChildElement);
                sql.Append("ADD CONSTRAINT [");
                sql.Append(constraintName);
                sql.AppendLine("] ");
                sql.Append(Tab);
                sql.Append("FOREIGN KEY (");

                for (int rc = 0; rc < r.Columns.Count; rc++)
                {
                    if (rc > 0)
                        sql.Append(", ");

                    sql.Append('[');
                    sql.Append(r.Columns[rc].FkColumn);
                    sql.Append(']');
                }

                sql.AppendLine(")");
                sql.Append(Tab);
                sql.Append("REFERENCES ");
                sql.Append(element.TableName);
                sql.Append(" (");
                for (int rc = 0; rc < r.Columns.Count; rc++)
                {
                    if (rc > 0)
                        sql.Append(", ");

                    sql.Append('[');
                    sql.Append(r.Columns[rc].PkColumn);
                    sql.Append(']');
                }

                sql.Append(')');

                if (r.UpdateOnCascade)
                {
                    sql.AppendLine();
                    sql.Append(Tab).Append(Tab);
                    sql.Append("ON UPDATE CASCADE ");
                }

                if (r.DeleteOnCascade)
                {
                    sql.AppendLine();
                    sql.Append(Tab).Append(Tab);
                    sql.Append("ON DELETE CASCADE ");
                }

                sql.AppendLine();
                sql.AppendLine("GO");
            }
        }

        return sql.ToString();
    }

    public string GetWriteProcedureScript(Element element)
    {
        return string.Empty;
    }

    public string GetReadProcedureScript(Element element)
    {
        return string.Empty;
    }

    public Task<Element> GetElementFromTableAsync(string tableName, Guid? connectionId = null)
    {
        throw new NotImplementedException();
    }

    public Task<Element> GetElementFromTableAsync(string? schemaName, string connectionId, Guid? guid)
    {
        throw new NotImplementedException();
    }

    public DataAccessCommand GetInsertCommand(Element element, Dictionary<string, object?> values)
    {
        return GetScriptInsert(element, values, false);
    }

    public DataAccessCommand GetUpdateCommand(Element element, Dictionary<string, object?> values)
    {
        return GetScriptUpdate(element, values);
    }

    public DataAccessCommand GetDeleteCommand(Element element, Dictionary<string, object> filters)
    {
        return GetScriptDelete(element, filters);
    }

    public DataAccessCommand GetInsertOrReplaceCommand(Element element, Dictionary<string, object?> values)
    {
        return GetScriptInsert(element, values, true);
    }

    public bool TableExists(string tableName, Guid? connectionId = null)
    {
        throw new NotImplementedException();
    }

    public Task<bool> TableExistsAsync(string schema, string tableName, Guid? connectionId = null,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<bool> TableExistsAsync(string tableName, Guid? connectionId = null, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<bool> ColumnExistsAsync(string tableName, string columnName, Guid? connectionId = null,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<string?> GetStoredProcedureDefinitionAsync(string procedureName, Guid? connectionId = null)
    {
        throw new NotImplementedException();
    }

    public Task DropStoredProcedureAsync(string procedureName, Guid? connectionId = null)
    {
        throw new NotImplementedException();
    }

    public Task<List<string>> GetStoredProcedureListAsync(Guid? connectionId = null)
    {
        throw new NotImplementedException();
    }

    public DataAccessCommand GetReadCommand(Element element, EntityParameters entityParameters,
        DataAccessParameter totalOfRecordsParameter)
    {
        var (filters, orderBy, currentPage, recordsPerPage) = entityParameters;
        var isFirst = true;
        var sqlScript = new StringBuilder();

        sqlScript.Append("SELECT * FROM ");
        sqlScript.Append(element.TableName);

        foreach (var filter in filters)
        {
            sqlScript.Append(Tab).Append(Tab);
            if (isFirst)
            {
                sqlScript.Append(" WHERE ");
                isFirst = false;
            }
            else
            {
                sqlScript.Append("AND ");
            }

            sqlScript.Append(filter.Key);
            sqlScript.Append(" = ");
            sqlScript.AppendLine("?");
        }

        if (!string.IsNullOrEmpty(orderBy.ToQueryParameter()))
        {
            sqlScript.Append(" ORDER BY ");
            sqlScript.Append(orderBy);
        }

        if ((int)totalOfRecordsParameter.Value == 0 && recordsPerPage > 0)
        {
            var offset = (currentPage - 1) * recordsPerPage;
            sqlScript.Append("LIMIT ");
            sqlScript.Append(recordsPerPage);
            sqlScript.Append(" OFFSET");
            sqlScript.Append(offset);
        }

        DataAccessCommand command = new DataAccessCommand
        {
            Type = CommandType.Text,
            Sql = sqlScript.ToString(),
        };

        foreach (var filter in filters)
        {
            ElementField f = element.Fields[filter.Key];
            var param = new DataAccessParameter
            {
                Direction = ParameterDirection.Input,
                Value = filter.Value,
                Type = GetDbType(f.DataType)
            };
            command.Parameters.Add(param);
        }

        return command;
    }

    private static DataAccessCommand GetScriptInsert(Element element, Dictionary<string, object?> values, bool isReplace)
    {
        var fields = element.Fields
            .ToList()
            .FindAll(x => x.DataBehavior == FieldBehavior.Real
                          && !x.AutoNum);

        var sql = new StringBuilder();
        if (isReplace)
            sql.Append("REPLACE INTO ");
        else
            sql.Append("INSERT INTO ");

        sql.Append(element.TableName);
        sql.Append(" (");

        bool isFirst = true;
        foreach (var c in fields)
        {
            if (isFirst)
                isFirst = false;
            else
                sql.AppendLine(",");

            sql.Append(c.Name);
        }

        sql.Append(')');
        sql.Append(" VALUES (");
        isFirst = true;
        foreach (var unused in fields)
        {
            if (isFirst)
                isFirst = false;
            else
                sql.AppendLine(",");

            sql.Append('?');
        }

        sql.Append(')');

        var cmd = new DataAccessCommand
        {
            Type = CommandType.Text,
            Sql = sql.ToString()
        };

        foreach (var f in fields)
        {
            object value = GetElementValue(f, values);
            var param = new DataAccessParameter
            {
                Direction = ParameterDirection.Input,
                Value = value,
                Type = GetDbType(f.DataType)
            };
            cmd.Parameters.Add(param);
        }

        return cmd;
    }

    private DataAccessCommand GetScriptUpdate(Element element, Dictionary<string, object?> values)
    {
        var fields = element.Fields
            .ToList()
            .FindAll(x => x.DataBehavior == FieldBehavior.Real);

        var sql = new StringBuilder();
        sql.Append("UPDATE ");
        sql.Append(element.TableName);
        sql.Append(" SET ");

        bool isFirst = true;
        foreach (var c in fields)
        {
            if (!c.IsPk)
            {
                if (isFirst)
                    isFirst = false;
                else
                    sql.AppendLine(",");

                sql.Append(c.Name);
                sql.Append(" = ");
                sql.Append(VariablePrefix + c.Name);
            }
        }

        isFirst = true;
        foreach (var f in fields)
        {
            if (f.IsPk)
            {
                sql.Append(Tab).Append(Tab);
                if (isFirst)
                {
                    sql.Append(" WHERE ");
                    isFirst = false;
                }
                else
                {
                    sql.Append("AND ");
                }

                sql.Append(f.Name);
                sql.Append(" = ");
                sql.AppendLine(VariablePrefix + f.Name);
            }
        }


        var cmd = new DataAccessCommand
        {
            Type = CommandType.Text,
            Sql = sql.ToString()
        };

        foreach (var f in fields)
        {
            object value = GetElementValue(f, values);
            var param = new DataAccessParameter
            {
                Name = string.Format(VariablePrefix + f.Name),
                //param.Size = f.Size;
                Direction = ParameterDirection.Input,
                Value = value,
                Type = GetDbType(f.DataType)
            };
            cmd.Parameters.Add(param);
        }

        return cmd;
    }

    private DataAccessCommand GetScriptDelete(Element element, Dictionary<string, object> values)
    {
        var fields = element.Fields
            .ToList()
            .FindAll(x => x.DataBehavior == FieldBehavior.Real);

        bool isFirst = true;
        var sqlScript = new StringBuilder();

        sqlScript.Append("DELETE FROM ");
        sqlScript.Append(element.TableName);
        foreach (var f in fields)
        {
            if (f.IsPk)
            {
                sqlScript.Append(Tab).Append(Tab);
                if (isFirst)
                {
                    sqlScript.Append(" WHERE ");
                    isFirst = false;
                }
                else
                {
                    sqlScript.Append("AND ");
                }

                sqlScript.Append(f.Name);
                sqlScript.Append(" = ");
                sqlScript.AppendLine(VariablePrefix + f.Name);
            }
        }

        var cmd = new DataAccessCommand
        {
            Type = CommandType.Text,
            Sql = sqlScript.ToString()
        };

        foreach (var f in fields)
        {
            object value = GetElementValue(f, values!);
            var param = new DataAccessParameter
            {
                Name = string.Format(VariablePrefix + f.Name),
                Direction = ParameterDirection.Input,
                Value = value,
                Type = GetDbType(f.DataType)
            };
            cmd.Parameters.Add(param);
        }

        return cmd;
    }

    private static object GetElementValue(ElementField f, Dictionary<string, object?> values)
    {
        if (!values.TryGetValue(f.Name, out object? value))
            return DBNull.Value;

        if (value == null)
        {
            return DBNull.Value;
        }

        if (f.DataType is FieldType.Date or FieldType.DateTime or FieldType.Float or FieldType.Int &&
            string.IsNullOrEmpty(value.ToString()))
        {
            return DBNull.Value;
        }

        return value;
    }

    private static DbType GetDbType(FieldType dataType)
    {
        DbType t = DbType.AnsiString;
        switch (dataType)
        {
            case FieldType.Date:
                t = DbType.Date;
                break;
            case FieldType.DateTime:
                t = DbType.DateTime;
                break;
            case FieldType.DateTime2:
                t = DbType.DateTime2;
                break;
            case FieldType.Float:
                t = DbType.Single;
                break;
            case FieldType.Int:
                t = DbType.Int32;
                break;
        }

        return t;
    }

    // ReSharper disable once UnusedMember.Local
    private static DataAccessCommand GetScriptCount(Element element, Dictionary<string, object?> filters)
    {
        var fields = element.Fields
            .ToList()
            .FindAll(x => x.DataBehavior == FieldBehavior.Real);

        var isFirst = true;
        var sqlScript = new StringBuilder();

        sqlScript.Append("SELECT Count(*) FROM ");
        sqlScript.Append(element.TableName);

        foreach (var f in fields)
        {
            if (f.IsPk)
            {
                sqlScript.Append(Tab).Append(Tab);
                if (isFirst)
                {
                    sqlScript.Append(" WHERE ");
                    isFirst = false;
                }
                else
                {
                    sqlScript.Append("AND ");
                }

                sqlScript.Append(f.Name);
                sqlScript.Append(" = ");
                sqlScript.AppendLine("?");
            }
        }

        var cmd = new DataAccessCommand();
        cmd.Sql = sqlScript.ToString();

        foreach (var f in fields)
        {
            object value = GetElementValue(f, filters);
            var param = new DataAccessParameter
            {
                //param.Name = string.Format(f.Name);
                //param.Size = f.Size;
                Direction = ParameterDirection.Input,
                Value = value,
                Type = GetDbType(f.DataType)
            };
            cmd.Parameters.Add(param);
        }

        return cmd;
    }

    public string GetAlterTableScript(Element element, IEnumerable<ElementField> fields)
    {
        throw new NotImplementedException();
    }
}