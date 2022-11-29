using System;
using System.Collections;
using System.Collections.Generic;
using JJMasterData.Commons.Dao.Entity;
using JJMasterData.Commons.DI;
using Newtonsoft.Json;

namespace JJMasterData.Core.DataDictionary;

public class MetadataStructure
{
    public static Element GetElement()
    {
        var element = new Element(JJService.Options.TableName, "Data Dictionaries");
        element.Fields.AddPK("type", "Type", FieldType.Varchar, 1, false, FilterMode.Equal);
        element.Fields["type"].EnableOnDelete = false;
        element.Fields.AddPK("name", "Dictionary Name", FieldType.NVarchar, 64, false, FilterMode.Equal);
        element.Fields.Add("namefilter", "Dictionary Name", FieldType.NVarchar, 30, false, FilterMode.Contain, FieldBehavior.ViewOnly);
        element.Fields.Add("tablename", "Table Name", FieldType.NVarchar, 64, false, FilterMode.MultValuesContain);
        element.Fields.Add("info", "Info", FieldType.NVarchar, 150, false, FilterMode.None);
        element.Fields.Add("owner", "Owner", FieldType.NVarchar, 64, false, FilterMode.None);
        element.Fields.Add("sync", "Sync", FieldType.Varchar, 1, false, FilterMode.Equal);
        element.Fields.Add("modified", "Last Modified", FieldType.DateTime, 15, true, FilterMode.Range);
        element.Fields.Add("json", "Object", FieldType.Text, 0, false, FilterMode.None);
        return element;
    }

    public static IList<IDictionary> GetStructure(Metadata metadata)
    {
        string name = metadata.Table.Name;
        string jsonTable = JsonConvert.SerializeObject(metadata.Table);

        var structure = new List<IDictionary>();
        
        var dNow = DateTime.Now;

        if (metadata.Table != null)
        {
            var values = new Hashtable
            {
                { "name", name },
                { "tablename", metadata.Table.TableName },
                { "info", metadata.Table.Info },
                { "type", "T" },
                { "owner", null },
                { "json", jsonTable },
                { "sync", metadata.Table.Sync ? "1" : "0" },
                { "modified", dNow }
            };
            
            structure.Add(values);
        }
        
        if (metadata.Form != null)
        {
            string jsonForm = JsonConvert.SerializeObject(metadata.Form);

            var values = new Hashtable
            {
                { "name", name },
                { "tablename", metadata.Table.TableName },
                { "info", "" },
                { "type", "F" },
                { "owner", name },
                { "json", jsonForm },
                { "sync", metadata.Table.Sync ? "1" : "0" },
                { "modified", dNow }
            };

            structure.Add(values);
        }

        if (metadata.UIOptions != null)
        {
            string json = JsonConvert.SerializeObject(metadata.UIOptions);
            var values = new Hashtable
            {
                { "name", name },
                { "tablename", metadata.Table.TableName },
                { "info", "" },
                { "type", "L" },
                { "owner", name },
                { "json", json },
                { "sync", metadata.Table.Sync ? "1" : "0" },
                { "modified", dNow }
            };
            structure.Add(values);
        }

        if (metadata.Api != null)
        {
            string jsonForm = JsonConvert.SerializeObject(metadata.Api);
            var values = new Hashtable
            {
                { "name", name },
                { "info", "" },
                { "type", "A" },
                { "owner", name },
                { "json", jsonForm },
                { "sync", metadata.Table.Sync ? "1" : "0" },
                { "modified", dNow }
            };
            structure.Add(values);
        }

        return structure;
    }
}