using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Resources;
using JJMasterData.Commons.Dao.Entity;
using JJMasterData.Commons.DI;
using JJMasterData.Commons.Settings;

namespace JJMasterData.Commons.Language;

public class DbTranslatorProvider : ITranslator
{
    public Dictionary<string, string> GetDictionaryStrings(string culture)
    {
        var dic = new Dictionary<string, string>();

        string tablename = JJService.Settings.ResourcesTableName;
        if (string.IsNullOrEmpty(tablename))
            return dic;

        var element = GetElement(tablename);
        
        var da = JJService.DataAccess;
        
        da.TranslateErrorMessage = false;
        da.GenerateLog = false;
        var factory = new Factory(da);
        if (!da.TableExists(element.TableName))
        {
            factory.CreateDataModel(element);
            var listValues = GetDefaultValues();
            foreach(Hashtable values in listValues)
            {
                factory.Insert(element, values);
            }
        }

        var filter = new Hashtable();
        filter.Add("cultureCode", culture);

        var dataTable = factory.GetDataTable(element, filter);
        foreach (DataRow row in dataTable.Rows)
        {
            dic.Add(row["resourceKey"].ToString(), row["resourceValue"].ToString());
        }

        return dic;
    }

    public Element GetElement()
    {
        return GetElement(JJService.Settings.ResourcesTableName);
    }

    private Element GetElement(string tablename)
    {
        var element = new Element
        {
            Name = tablename,
            TableName = tablename,
            CustomProcNameGet = JJMasterDataSettings.GetDefaultProcNameGet(tablename),
            CustomProcNameSet = JJMasterDataSettings.GetDefaultProcNameSet(tablename),
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


    private List<Hashtable> GetDefaultValues()
    {
        var values = new List<Hashtable>();
        string resourcePath = "JJMasterData.Commons.Resources.JJMasterDataResources.pt.resources";
        
        var fs =  Assembly.GetExecutingAssembly().GetManifestResourceStream(resourcePath);
        
        using var res = new ResourceReader(fs);
        
        var dict = res.GetEnumerator();
        while (dict.MoveNext())
        {
            values.Add(GetValue(dict.Key.ToString(), dict.Value.ToString()));
        }
        
        res.Close();

        return values;
    }

    private Hashtable GetValue(string resourceKey, string resourceValue)
    {
        var val = new Hashtable
        {
            { "cultureCode", "pt-BR" },
            { "resourceKey", resourceKey },
            { "resourceValue", resourceValue },
            { "resourceOrigin", "JJMasterData" }
        };

        return val;
    }

}