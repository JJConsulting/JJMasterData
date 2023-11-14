﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JJMasterData.Commons.Data.Entity.Models;

namespace JJMasterData.Commons.Data.Entity.Providers;

public class SqlServerCreateTableScripts : SqlServerScriptsBase
{
    public static string GetCreateTableScript(Element element)
    {
        if (element == null)
            throw new ArgumentNullException(nameof(element));

        if (element.Fields == null || element.Fields.Count == 0)
            throw new ArgumentNullException(nameof(Element.Fields));

        var sql = new StringBuilder();
        var keys = new StringBuilder();

        sql.Append("CREATE TABLE [");
        sql.Append(element.TableName);
        sql.AppendLine("] (");

        var fields = element.Fields
            .ToList()
            .FindAll(x => x.DataBehavior == FieldBehavior.Real);

        bool isFirst = true;
        foreach (var f in fields)
        {
            if (isFirst)
                isFirst = false;
            else
                sql.AppendLine(",");

            sql.Append(Tab);
            sql.Append(GetFieldDefinition(f));

            if (!f.IsPk) continue;
            if (keys.Length > 0)
                keys.Append(",");

            keys.Append("[");
            keys.Append(f.Name);
            keys.Append("] ASC ");
        }

        if (keys.Length > 0)
        {
            sql.AppendLine(", ");
            sql.Append(Tab);
            sql.Append("CONSTRAINT [PK_");
            sql.Append(element.TableName);
            sql.Append("] PRIMARY KEY NONCLUSTERED (");
            sql.Append(keys);
            sql.Append(")");
        }

        sql.AppendLine("");
        sql.AppendLine(")");

        sql.AppendLine("");

        sql.AppendLine(GetRelationshipsScript(element));
        sql.AppendLine("");

        int counter = 1;
        if (element.Indexes.Count > 0)
        {
            foreach (var index in element.Indexes)
            {
                sql.Append("CREATE");
                sql.Append(index.IsUnique ? " UNIQUE" : "");
                sql.Append(index.IsClustered ? " CLUSTERED" : " NONCLUSTERED");
                sql.Append(" INDEX [IX_");
                sql.Append(element.TableName);
                sql.Append("_");
                sql.Append(counter);
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

                sql.AppendLine("");
                sql.Append(Tab);
                sql.AppendLine(")");
                sql.AppendLine("GO");
                counter++;
            }
        }

        return sql.ToString();
    }

    private static string GetRelationshipsScript(Element element)
    {
        var sql = new StringBuilder();

        if (element.Relationships.Count > 0)
        {
            sql.AppendLine("-- RELATIONSHIPS");
            var listContraint = new List<string>();
            foreach (var r in element.Relationships)
            {
                string contraintName = $"FK_{r.ChildElement}_{element.TableName}";

                //Prevents repeated name.
                if (!listContraint.Contains(contraintName))
                {
                    listContraint.Add(contraintName);
                }
                else
                {
                    bool hasContraint = true;

                    int counter = 1;
                    while (hasContraint)
                    {
                        if (!listContraint.Contains(contraintName + counter))
                        {
                            contraintName += counter;
                            listContraint.Add(contraintName);
                            hasContraint = false;
                        }

                        counter++;
                    }
                }

                sql.Append("ALTER TABLE ");
                sql.AppendLine(r.ChildElement);
                sql.Append("ADD CONSTRAINT [");
                sql.Append(contraintName);
                sql.AppendLine("] ");
                sql.Append(Tab);
                sql.Append("FOREIGN KEY (");

                for (int rc = 0; rc < r.Columns.Count; rc++)
                {
                    if (rc > 0)
                        sql.Append(", ");

                    sql.Append("[");
                    sql.Append(r.Columns[rc].FkColumn);
                    sql.Append("]");
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

                    sql.Append("[");
                    sql.Append(r.Columns[rc].PkColumn);
                    sql.Append("]");
                }

                sql.Append(")");

                if (r.UpdateOnCascade)
                {
                    sql.AppendLine("");
                    sql.Append(Tab).Append(Tab);
                    sql.Append("ON UPDATE CASCADE ");
                }

                if (r.DeleteOnCascade)
                {
                    sql.AppendLine("");
                    sql.Append(Tab).Append(Tab);
                    sql.Append("ON DELETE CASCADE ");
                }

                sql.AppendLine("");
                sql.AppendLine("GO");
            }
        }

        return sql.ToString();
    }

}