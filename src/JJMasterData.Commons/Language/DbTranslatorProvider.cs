using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Resources;
using JJMasterData.Commons.Dao;
using JJMasterData.Commons.Dao.Entity;
using JJMasterData.Commons.DI;
using JJMasterData.Commons.Logging;
using JJMasterData.Commons.Options;

namespace JJMasterData.Commons.Language;

public class DbTranslatorProvider : ITranslator
{
    internal IEntityRepository EntityRepository { get; }

    public DbTranslatorProvider(IEntityRepository entityRepository)
    {
        EntityRepository = entityRepository;
    }

    public Dictionary<string, string> GetDictionaryStrings(string culture)
    {
        var dic = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
        try
        {
            string tablename = JJService.Options.ResourcesTableName;
            if (string.IsNullOrEmpty(tablename))
                return dic;

            if (string.IsNullOrEmpty(JJMasterDataOptions.GetConnectionString()))
                return dic;

            var element = GetElement(tablename);
            if (!EntityRepository.TableExists(element.TableName))
                EntityRepository.CreateDataModel(element);

            dic = GetDatabaseValues(element, culture);
            if (dic.Count > 0)
                return dic;

            var values = GetDefaultValues(culture);
            if (values?.Count > 0)
            {
                AddDefaultValues(element, values);
                foreach (Hashtable row in values)
                    dic.Add(row["resourceKey"].ToString(), row["resourceValue"].ToString());
            }
        }
        catch 
        {
            // ignore
        }
        
        return dic;
    }

    public static Element GetElement()
    {
        return GetElement(JJService.Options.ResourcesTableName);
    }

    private static Element GetElement(string tablename)
    {
        var element = new Element
        {
            Name = tablename,
            TableName = tablename,
            CustomProcNameGet = JJMasterDataOptions.GetReadProcedureName(tablename),
            CustomProcNameSet = JJMasterDataOptions.GetWriteProcedureName(tablename),
            Info = "Resources"
        };

        var culture = new ElementField
        {
            Name = "cultureCode",
            Label = "Culture Code",
            DataType = FieldType.Varchar,
            Filter =
            {
                Type = FilterMode.Equal
            },
            Size = 10,
            IsPk = true
        };
        element.Fields.Add(culture);

        var key = new ElementField
        {
            Name = "resourceKey",
            Label = "Key",
            DataType = FieldType.Varchar,
            Size = 255,
            Filter =
            {
                Type = FilterMode.Contain
            },
            IsPk = true
        };
        element.Fields.Add(key);

        var value = new ElementField
        {
            Name = "resourceValue",
            Label = "Value",
            DataType = FieldType.Varchar,
            Size = 500,
            Filter =
            {
                Type = FilterMode.Contain
            },
            IsRequired = true
        };
        element.Fields.Add(value);

        var origin = new ElementField
        {
            Name = "resourceOrigin",
            Label = "Origin",
            DataType = FieldType.Varchar,
            Size = 50,
            Filter =
            {
                Type = FilterMode.Equal
            }
        };
        element.Fields.Add(origin);

        return element;
    }

    private void AddDefaultValues(Element element, List<Hashtable> listValues)
    {
        foreach (Hashtable values in listValues)
            EntityRepository.Insert(element, values);
    }


    private List<Hashtable> GetDefaultValues(string culture)
    {
        var values = new List<Hashtable>();
        string resourcePath = $"JJMasterData.Commons.Language.ResourceStrings_{culture.ToLower()}.resources";
        
        var fs = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourcePath);
        if (fs == null)
            return values;

        using var res = new ResourceReader(fs);
        var dict = res.GetEnumerator();
        while (dict.MoveNext())
        {
            var val = new Hashtable
            {
                { "cultureCode", culture },
                { "resourceKey", dict.Key.ToString() },
                { "resourceValue", dict.Value.ToString() },
                { "resourceOrigin", "JJMasterData" }
            };

            values.Add(val);
        }
        res.Close();

        return values;
    }

    private Dictionary<string, string> GetDatabaseValues(Element element, string culture)
    {
        var dic = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
        var filter = new Hashtable();
        filter.Add("cultureCode", culture);
        var dt = EntityRepository.GetDataTable(element, filter);
        foreach (DataRow row in dt.Rows)
        {
            dic.Add(row["resourceKey"].ToString(), row["resourceValue"].ToString());
        }
        return dic;
    }

}