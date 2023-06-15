using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using JJMasterData.Commons.Data.Entity;
using JJMasterData.Commons.Data.Entity.Abstractions;
using JJMasterData.Commons.Options;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;

namespace JJMasterData.Commons.Localization;

public class JJMasterDataStringLocalizer : IStringLocalizer
{
    private ResourceManagerStringLocalizer ResourceManagerStringLocalizer { get; }
    private IEntityRepository EntityRepository { get; }
    private IMemoryCache Cache { get; }
    private JJMasterDataCommonsOptions Options { get; }

    private string ResourceName { get; }
    
    public JJMasterDataStringLocalizer(
        string resourceName,
        ResourceManagerStringLocalizer resourcesStringLocalizer,
        IEntityRepository entityRepository, 
        IMemoryCache cache,
        IOptions<JJMasterDataCommonsOptions> options)
    {
        ResourceName = resourceName;
        ResourceManagerStringLocalizer = resourcesStringLocalizer;
        EntityRepository = entityRepository;
        Cache = cache;
        Options = options.Value;
    }

    public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures)
    {
        return GetAllStringsAsDictionary().Select(e=>new LocalizedString(e.Key, e.Value));
    }

    public LocalizedString this[string name]
    {
        get
        {
            var value = GetString(name);
            return new LocalizedString(name, value ?? name, value == null);
        }
    }

    public LocalizedString this[string name, params object[] arguments] => new(name, string.Format(this[name],arguments));

    
    private string GetString(string key)
    {
        if (string.IsNullOrEmpty(key))
            return key;
        
        var culture = Thread.CurrentThread.CurrentCulture.Name;
        var cacheKey = $"{ResourceName}_localization_strings_{culture}";

        if (Cache.TryGetValue<Dictionary<string, string>>(cacheKey, out var cachedDictionary))
        {
            if (cachedDictionary.TryGetValue(key, out var cachedValue))
                return cachedValue;
            return key;
        }

        var localizedStrings = GetAllStringsAsDictionary();
        
        Cache.Set(cacheKey, localizedStrings);

        if (localizedStrings.TryGetValue(key, out var localizedValue))
            return localizedValue;
   
        return key;
    }
    
    public IDictionary<string, string> GetAllStringsAsDictionary()
    {
        string culture = Thread.CurrentThread.CurrentCulture.Name;
        var tableName = Options.ResourcesTableName;

        var element = JJMasterDataStringLocalizerElement.GetElement(Options);
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
        
        SetDatabaseValues(element, ConvertDictionaryToList(stringLocalizerValues, culture));

        return stringLocalizerValues;
    }

    private void SetDatabaseValues(Element element, IEnumerable<IDictionary> values)
    {
        foreach (var value in values)
            EntityRepository.SetValues(element, value);
    }

    private IEnumerable<IDictionary> ConvertDictionaryToList(IDictionary<string, string> dictionary,
        string culture)
    {
        return dictionary.Select(pair => new Dictionary<string,string>
        {
            { "cultureCode", culture },
            { "resourceKey", pair.Key },
            { "resourceValue", pair.Value },
            { "resourceOrigin", ResourceName}
        }).ToList();
    }

    private Dictionary<string, string> GetStringLocalizerValues()
    {
        try
        {
            var values = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
            var localizedStrings = ResourceManagerStringLocalizer.GetAllStrings();

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
        var filter = new Dictionary<string,string> { { "cultureCode", culture} };
        var dataTable = EntityRepository.GetDataTable(element, filter);
        foreach (DataRow row in dataTable.Rows)
        {
            values.Add(row["resourceKey"].ToString(), row["resourceValue"].ToString());
        }

        return values;
    }
}