using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using JJMasterData.Commons.Dao;
using JJMasterData.Commons.Dao.Entity;
using JJMasterData.Commons.DI;
using JJMasterData.Commons.Options;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;

namespace JJMasterData.Commons.Language;

public class JJMasterDataLocalizationProvider : ILocalizationProvider
{
    internal IStringLocalizer<JJMasterDataResources> StringLocalizer { get; }
    internal IEntityRepository EntityRepository { get; }
    
    internal string ResourcesTableName { get;}

    public JJMasterDataLocalizationProvider(
        IEntityRepository entityRepository,
        IOptions<JJMasterDataCommonsOptions> options,
        IStringLocalizer<JJMasterDataResources> stringLocalizer)
    {
        EntityRepository = entityRepository;
        ResourcesTableName = options.Value.ResourcesTableName; 
        StringLocalizer = stringLocalizer;
    }

    public IDictionary<string, string> GetLocalizedStrings(string culture)
    {
        if (string.IsNullOrEmpty(ResourcesTableName))
            return new Dictionary<string,string>();
        if (string.IsNullOrEmpty(JJMasterDataCommonsOptions.GetConnectionString()))
            return new Dictionary<string,string>();

        var element = GetElement(ResourcesTableName);
        if (!EntityRepository.TableExists(element.TableName))
            EntityRepository.CreateDataModel(element);
        
        var stringLocalizerValues = GetStringLocalizerValues();
        var databaseValues = GetDatabaseValues(element, culture);

        if (databaseValues.Count > 0)
        {
            foreach (var dbValue in databaseValues.ToList())
            {
                stringLocalizerValues[dbValue.Key] = dbValue.Value;
            }

            return stringLocalizerValues;
        }
        
        if (stringLocalizerValues?.Count > 0)
        {
            SetDatabaseValues(element, ConvertDictionaryToHashtableList(stringLocalizerValues,culture));
        }
        
        return stringLocalizerValues;
    }
    private void SetDatabaseValues(Element element, IEnumerable<Hashtable> values)
    {
        foreach (var value in values)
            EntityRepository.SetValues(element, value);
    }
    private static IEnumerable<Hashtable> ConvertDictionaryToHashtableList(IDictionary<string,string> dictionary, string culture)
    {
        return dictionary.Select(pair => new Hashtable {                
            { "cultureCode", culture },
            { "resourceKey", pair.Key },
            { "resourceValue",  pair.Value },
            { "resourceOrigin", "JJMasterData" } }).ToList();
    }

    private Dictionary<string, string> GetStringLocalizerValues()
    {
        try
        {
            var values = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
            var localizedStrings = StringLocalizer.GetAllStrings();

            foreach (var localizedString in localizedStrings)
            {
                values.Add(localizedString.Name, localizedString.Value);
            }

            return values;
        }
        catch
        {
            return new Dictionary<string, string>();
        }
    }
    private Dictionary<string, string> GetDatabaseValues(Element element, string culture)
    {
        var values = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
        var filter = new Hashtable { { "cultureCode", culture } };
        var dataTable = EntityRepository.GetDataTable(element, filter);
        foreach (DataRow row in dataTable.Rows)
        {
            values.Add(row["resourceKey"].ToString(), row["resourceValue"].ToString());
        }
        return values;
    }
    public static Element GetElement(string tablename)
    {
        var element = new Element
        {
            Name = tablename,
            TableName = tablename,
            CustomProcNameGet = JJMasterDataCommonsOptions.GetReadProcedureName(tablename),
            CustomProcNameSet = JJMasterDataCommonsOptions.GetWriteProcedureName(tablename),
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
}