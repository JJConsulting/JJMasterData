using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JJMasterData.Commons.Data.Entity;
using JJMasterData.Commons.Options;
using Microsoft.Extensions.Logging;

namespace JJMasterData.Commons.Data.Providers;

public class ProviderSQLite : BaseProvider
{
    private const string Tab = "\t";
    public override string VariablePrefix => "@";
    public override DataAccessProvider DataAccessProvider => DataAccessProvider.SqLite;

    public ProviderSQLite(DataAccess dataAccess, JJMasterDataCommonsOptions options) : base(dataAccess, options)
    {
    }

    public override string GetCreateTableScript(Element element)
    {
        if (element == null)
            throw new ArgumentNullException(nameof(element));

        if (element.Fields == null || element.Fields.Count == 0)
            throw new ArgumentNullException(nameof(element.Fields));

        StringBuilder sSql = new StringBuilder();

        sSql.AppendLine("-- TABLE");
        sSql.Append("CREATE TABLE [");
        sSql.Append(element.TableName);
        sSql.AppendLine("] (");
        bool isFirst = true;
        var fields = element.Fields
            .ToList()
            .FindAll(x => x.DataBehavior == FieldBehavior.Real);

        foreach (var f in fields)
        {
            if (isFirst)
                isFirst = false;
            else
                sSql.AppendLine(",");

            sSql.Append(Tab);
            sSql.Append("[");
            sSql.Append(f.Name);
            sSql.Append("] ");

            switch (f.DataType)
            {
                case FieldType.Int:
                    sSql.Append("INTEGER");
                    break;
                case FieldType.Float:
                    sSql.Append("Real");
                    break;
                case FieldType.NText:
                case FieldType.NVarchar:
                case FieldType.Varchar:
                    sSql.Append("TEXT");
                    break;
                default:
                    sSql.Append(f.DataType.ToString());
                    break;
            }

            if (f.IsRequired)
                sSql.Append(" NOT NULL");

            if (f.AutoNum && f.IsPk)
                sSql.Append(" PRIMARY KEY AUTOINCREMENT ");
        }

        isFirst = true;
        foreach (var f in fields.ToList().FindAll(x => x.IsPk && !x.AutoNum))
        {
            if (isFirst)
            {
                isFirst = false;
                sSql.AppendLine(",");
                sSql.Append(Tab);
                sSql.Append("PRIMARY KEY (");
            }
            else
            {
                sSql.Append(",");
            }

            sSql.Append("[");
            sSql.Append(f.Name);
            sSql.Append("] ");
        }

        if (!isFirst)
            sSql.Append(")");


        sSql.AppendLine("");
        sSql.AppendLine(")");
        sSql.AppendLine("GO");
        sSql.AppendLine("");

        //sSql.AppendLine(DoSqlCreateRelation(element));
        sSql.AppendLine("");

        int nIndex = 1;
        if (element.Indexes.Count > 0)
        {
            foreach (var index in element.Indexes)
            {
                sSql.Append("CREATE");
                sSql.Append(index.IsUnique ? " UNIQUE" : "");
                sSql.Append(index.IsClustered ? " CLUSTERED" : "");
                sSql.Append(" INDEX [IX_");
                sSql.Append(element.TableName);
                sSql.Append("_");
                sSql.Append(nIndex);
                sSql.Append("] ON ");
                sSql.AppendLine(element.TableName);

                sSql.Append(Tab);
                sSql.AppendLine("(");
                for (int i = 0; i < index.Columns.Count; i++)
                {
                    if (i > 0)
                        sSql.AppendLine(", ");

                    sSql.Append(Tab);
                    sSql.Append(index.Columns[i]);
                }
                sSql.AppendLine("");
                sSql.Append(Tab);
                sSql.AppendLine(")");
                sSql.AppendLine("GO");
                nIndex++;
            }
        }

        sSql.AppendLine("");
        return sSql.ToString();
    }

    private string GetRelationshipsScript(Element element)
    {
        StringBuilder sSql = new StringBuilder();

        if (element.Relationships.Count > 0)
        {
            sSql.AppendLine("-- RELATIONSHIPS");
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
                    int nCount = 1;
                    while (hasContraint)
                    {
                        if (!listConstraint.Contains(constraintName + nCount))
                        {
                            constraintName += nCount;
                            listConstraint.Add(constraintName);
                            hasContraint = false;
                        }
                        nCount++;
                    }
                }

                sSql.Append("ALTER TABLE ");
                sSql.AppendLine(r.ChildElement);
                sSql.Append("ADD CONSTRAINT [");
                sSql.Append(constraintName);
                sSql.AppendLine("] ");
                sSql.Append(Tab);
                sSql.Append("FOREIGN KEY (");

                for (int rc = 0; rc < r.Columns.Count; rc++)
                {
                    if (rc > 0)
                        sSql.Append(", ");

                    sSql.Append("[");
                    sSql.Append(r.Columns[rc].FkColumn);
                    sSql.Append("]");
                }
                sSql.AppendLine(")");
                sSql.Append(Tab);
                sSql.Append("REFERENCES ");
                sSql.Append(element.TableName);
                sSql.Append(" (");
                for (int rc = 0; rc < r.Columns.Count; rc++)
                {
                    if (rc > 0)
                        sSql.Append(", ");

                    sSql.Append("[");
                    sSql.Append(r.Columns[rc].PkColumn);
                    sSql.Append("]");
                }
                sSql.Append(")");

                if (r.UpdateOnCascade)
                {
                    sSql.AppendLine("");
                    sSql.Append(Tab).Append(Tab);
                    sSql.Append("ON UPDATE CASCADE ");
                }

                if (r.DeleteOnCascade)
                {
                    sSql.AppendLine("");
                    sSql.Append(Tab).Append(Tab);
                    sSql.Append("ON DELETE CASCADE ");
                }

                sSql.AppendLine("");
                sSql.AppendLine("GO");
            }
        }

        return sSql.ToString();
    }

    public override string GetWriteProcedureScript(Element element)
    {
        return null;
    }

    public override string GetReadProcedureScript(Element element)
    {
        return null;
    }

    public override Task<Element> GetElementFromTableAsync(string tableName)
    {
        throw new NotImplementedException();
    }

    public override DataAccessCommand GetInsertCommand(Element element, IDictionary values)
    {
        return GetScriptInsert(element, values, false);
    }

    public override DataAccessCommand GetUpdateCommand(Element element, IDictionary values)
    {
        return GetScriptUpdate(element, values);
    }

    public override DataAccessCommand GetDeleteCommand(Element element, IDictionary filters)
    {
        return GetScriptDelete(element, filters);
    }

    public override DataAccessCommand GetInsertOrReplaceCommand(Element element, IDictionary values)
    {
        return GetScriptInsert(element, values, true);
    }

    public override DataAccessCommand GetReadCommand(Element element, IDictionary filters, string orderBy, int recordsPerPage, int currentPage, ref DataAccessParameter pTot)
    {
        var isFirst = true;
        var sSql = new StringBuilder();

        sSql.Append("SELECT * FROM ");
        sSql.Append(element.TableName);

        foreach (DictionaryEntry filter in filters)
        {
            if (isFirst)
            {
                sSql.Append(Tab).Append(Tab);
                sSql.Append(" WHERE ");
                isFirst = false;
            }
            else
            {
                sSql.Append(Tab).Append(Tab);
                sSql.Append("AND ");
            }

            sSql.Append(filter.Key);
            sSql.Append(" = ");
            sSql.AppendLine("?");

        }

        if (!string.IsNullOrEmpty(orderBy))
        {
            sSql.Append(" ORDER BY ");
            sSql.Append(orderBy);
        }

        if (pTot != null && (int)pTot.Value == 0 && recordsPerPage > 0)
        {
            var offset = (currentPage - 1) * recordsPerPage;
            sSql.Append("LIMIT ");
            sSql.Append(recordsPerPage);
            sSql.Append(" OFFSET");
            sSql.Append(offset);
        }

        DataAccessCommand cmd = new DataAccessCommand();
        cmd.CmdType = CommandType.Text;
        cmd.Sql = sSql.ToString();
        cmd.Parameters = new List<DataAccessParameter>();

        foreach (DictionaryEntry filter in filters)
        {
            ElementField f = element.Fields[filter.Key.ToString()];
            var param = new DataAccessParameter();
            //param.Name = string.Format(f.Name);
            //param.Size = f.Size;
            param.Direction = ParameterDirection.Input;
            param.Value = filter.Value;
            param.Type = GetDbType(f.DataType);
            cmd.Parameters.Add(param);
        }

        return cmd;
    }

    public new DataTable GetDataTable(Element element, IDictionary filters, string orderby, int regporpag, int pag, ref int tot)
    {
        DataAccessParameter pTot = null;
        var cmd = GetReadCommand(element, filters, orderby, regporpag, pag, ref pTot);
        DataTable dt = DataAccess.GetDataTable(cmd);
        tot = 0;

        if (regporpag > 0 && tot == 0)
        {
            var obj = DataAccess.GetResult(GetScriptCount(element, filters));
            if (obj != null)
                tot = int.Parse(obj.ToString());
        }

        return dt;
    }

    private DataAccessCommand GetScriptInsert(Element element, IDictionary values, bool isReplace)
    {
        var fields = element.Fields
            .ToList()
            .FindAll(x => x.DataBehavior == FieldBehavior.Real
                          && !x.AutoNum);

        var sSql = new StringBuilder();
        if (isReplace)
            sSql.Append("REPLACE INTO ");
        else
            sSql.Append("INSERT INTO ");

        sSql.Append(element.TableName);
        sSql.Append(" (");

        bool isFirst = true;
        foreach (var c in fields)
        {
            if (isFirst)
                isFirst = false;
            else
                sSql.AppendLine(",");

            sSql.Append(c.Name);
        }
        sSql.Append(")");
        sSql.Append(" VALUES (");
        isFirst = true;
        foreach (var unused in fields)
        {
            if (isFirst)
                isFirst = false;
            else
                sSql.AppendLine(",");

            sSql.Append("?");

        }
        sSql.Append(")");

        DataAccessCommand cmd = new DataAccessCommand();
        cmd.CmdType = CommandType.Text;
        cmd.Sql = sSql.ToString();
        cmd.Parameters = new List<DataAccessParameter>();

        foreach (var f in fields)
        {
            object value = GetElementValue(f, values);
            var param = new DataAccessParameter();
            //param.Name = string.Format(f.Name);
            //param.Size = f.Size;
            param.Direction = ParameterDirection.Input;
            param.Value = value;
            param.Type = GetDbType(f.DataType);
            cmd.Parameters.Add(param);
        }

        return cmd;
    }

    private DataAccessCommand GetScriptUpdate(Element element, IDictionary values)
    {
        var fields = element.Fields
            .ToList()
            .FindAll(x => x.DataBehavior == FieldBehavior.Real);

        var sSql = new StringBuilder();
        sSql.Append("UPDATE ");
        sSql.Append(element.TableName);
        sSql.Append(" SET ");

        bool isFirst = true;
        foreach (var c in fields)
        {
            if (!c.IsPk)
            {
                if (isFirst)
                    isFirst = false;
                else
                    sSql.AppendLine(",");

                sSql.Append(c.Name);
                sSql.Append(" = ");
                sSql.Append(VariablePrefix + c.Name);
            }
        }

        isFirst = true;
        foreach (var f in fields)
        {
            if (f.IsPk)
            {
                if (isFirst)
                {
                    sSql.Append(Tab).Append(Tab);
                    sSql.Append(" WHERE ");
                    isFirst = false;
                }
                else
                {
                    sSql.Append(Tab).Append(Tab);
                    sSql.Append("AND ");
                }

                sSql.Append(f.Name);
                sSql.Append(" = ");
                sSql.AppendLine(VariablePrefix + f.Name);
            }
        }


        DataAccessCommand cmd = new DataAccessCommand();
        cmd.CmdType = CommandType.Text;
        cmd.Sql = sSql.ToString();
        cmd.Parameters = new List<DataAccessParameter>();

        foreach (var f in fields)
        {
            object value = GetElementValue(f, values);
            var param = new DataAccessParameter();
            param.Name = string.Format(VariablePrefix + f.Name);
            //param.Size = f.Size;
            param.Direction = ParameterDirection.Input;
            param.Value = value;
            param.Type = GetDbType(f.DataType);
            cmd.Parameters.Add(param);
        }

        return cmd;

    }

    private DataAccessCommand GetScriptDelete(Element element, IDictionary values)
    {
        var fields = element.Fields
            .ToList()
            .FindAll(x => x.DataBehavior == FieldBehavior.Real);

        bool isFirst = true;
        var sSql = new StringBuilder();

        sSql.Append("DELETE FROM ");
        sSql.Append(element.TableName);
        foreach (var f in fields)
        {
            if (f.IsPk)
            {
                if (isFirst)
                {
                    sSql.Append(Tab).Append(Tab);
                    sSql.Append(" WHERE ");
                    isFirst = false;
                }
                else
                {
                    sSql.Append(Tab).Append(Tab);
                    sSql.Append("AND ");
                }

                sSql.Append(f.Name);
                sSql.Append(" = ");
                sSql.AppendLine(VariablePrefix + f.Name);
            }
        }

        DataAccessCommand cmd = new DataAccessCommand();
        cmd.CmdType = CommandType.Text;
        cmd.Sql = sSql.ToString();
        cmd.Parameters = new List<DataAccessParameter>();

        foreach (var f in fields)
        {
            object value = GetElementValue(f, values);
            var param = new DataAccessParameter();
            param.Name = string.Format(VariablePrefix + f.Name);
            //param.Size = f.Size;
            param.Direction = ParameterDirection.Input;
            param.Value = value;
            param.Type = GetDbType(f.DataType);
            cmd.Parameters.Add(param);
        }

        return cmd;
    }

    private object GetElementValue(ElementField f, IDictionary values)
    {
        object value = DBNull.Value;
        if (values != null &&
            values.Contains(f.Name) &&
            values[f.Name] != null)
        {
            if (f.DataType is FieldType.Date or FieldType.DateTime or FieldType.Float or FieldType.Int &&
                values[f.Name].ToString().Trim().Length == 0)
            {

                value = DBNull.Value;
            }
            else
            {
                value = values[f.Name];
            }
        }

        return value;
    }

    private DbType GetDbType(FieldType dataType)
    {
        DbType t = DbType.String;
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

    private DataAccessCommand GetScriptCount(Element element, IDictionary filters)
    {
        var fields = element.Fields
            .ToList()
            .FindAll(x => x.DataBehavior == FieldBehavior.Real);

        var isFirst = true;
        var sSql = new StringBuilder();

        sSql.Append("SELECT Count(*) FROM ");
        sSql.Append(element.TableName);

        foreach (var f in fields)
        {
            if (f.IsPk)
            {
                if (isFirst)
                {
                    sSql.Append(Tab).Append(Tab);
                    sSql.Append(" WHERE ");
                    isFirst = false;
                }
                else
                {
                    sSql.Append(Tab).Append(Tab);
                    sSql.Append("AND ");
                }

                sSql.Append(f.Name);
                sSql.Append(" = ");
                sSql.AppendLine("?");
            }
        }

        DataAccessCommand cmd = new DataAccessCommand();
        cmd.CmdType = CommandType.Text;
        cmd.Sql = sSql.ToString();
        cmd.Parameters = new List<DataAccessParameter>();

        foreach (var f in fields)
        {
            object value = GetElementValue(f, filters);
            var param = new DataAccessParameter();
            //param.Name = string.Format(f.Name);
            //param.Size = f.Size;
            param.Direction = ParameterDirection.Input;
            param.Value = value;
            param.Type = GetDbType(f.DataType);
            cmd.Parameters.Add(param);
        }

        return cmd;
    }

    public override Element GetElementFromTable(string tableName)
    {
        return null;
    }
}