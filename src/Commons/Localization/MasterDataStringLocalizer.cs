#nullable enable
using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using JJMasterData.Commons.Configuration.Options;
using JJMasterData.Commons.Data.Entity.Models;
using JJMasterData.Commons.Data.Entity.Repository;
using JJMasterData.Commons.Data.Entity.Repository.Abstractions;
using JJMasterData.Commons.Extensions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;


namespace JJMasterData.Commons.Localization;

public sealed class MasterDataStringLocalizer<TResourceSource> : IStringLocalizer<TResourceSource>
{
    private readonly IStringLocalizer _localizer;
    
    public MasterDataStringLocalizer(IStringLocalizerFactory factory)
    {
        if (factory == null)
        {
            throw new ArgumentNullException(nameof(factory));
        }

        _localizer = factory.Create(typeof(TResourceSource));
    }

    /// <inheritdoc />
    public LocalizedString this[string? name] => _localizer[name!];

    /// <inheritdoc />
    public LocalizedString this[string? name, params object[] arguments] => _localizer[name!, arguments];

    /// <inheritdoc />
    public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures) =>
        _localizer.GetAllStrings(includeParentCultures);
}


public class MasterDataStringLocalizer(
    string resourceName,
    ResourceManagerStringLocalizer resourcesStringLocalizer,
    IEntityRepository entityRepository,
    IMemoryCache cache,
    IOptions<MasterDataCommonsOptions> options)
    : IStringLocalizer
{
    private ResourceManagerStringLocalizer ResourceManagerStringLocalizer { get; } = resourcesStringLocalizer;
    private IEntityRepository EntityRepository { get; } = entityRepository;
    private IMemoryCache Cache { get; } = cache;
    private MasterDataCommonsOptions Options { get; } = options.Value;

    private string ResourceName { get; } = resourceName;

    public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures)
    {
        return GetAllStringsAsDictionary().Select(e=>new LocalizedString(e.Key, e.Value));
    }

    public LocalizedString this[string? name]
    {
        get
        {
            if (name == null)
                return new LocalizedString(string.Empty,string.Empty, true);
            
            var value = GetString(name);
            return new LocalizedString(name, value, false);
        }
    }

    public LocalizedString this[string? name, params object[] arguments]
    {
        get
        {
            if (name == null)
                return new LocalizedString(string.Empty,string.Empty);
            
            return new LocalizedString(name, string.Format(this[name], arguments));
        }
    }


    private string GetString(string key)
    {
        if (string.IsNullOrEmpty(key))
            return key;
        
        var culture = Thread.CurrentThread.CurrentCulture.Name;
        var cacheKey = $"{ResourceName}_localization_strings_{culture}";

        if (Cache.TryGetValue<FrozenDictionary<string, string>>(cacheKey, out var cachedDictionary))
        {
            return cachedDictionary.GetValueOrDefault(key, key);
        }

        var localizedStrings = GetAllStringsAsDictionary();
        
        Cache.Set(cacheKey, localizedStrings);

        return localizedStrings.GetValueOrDefault(key, key);
    }

    private FrozenDictionary<string, string> GetAllStringsAsDictionary()
    {
        string culture = Thread.CurrentThread.CurrentCulture.Name;

        var element = MasterDataStringLocalizerElement.GetElement(Options);

        var tableExists = EntityRepository.TableExists(element.TableName);
        
        if (!tableExists)
             EntityRepository.CreateDataModel(element,[]);

        var stringLocalizerValues = GetStringLocalizerValues();
        var databaseValues = GetDatabaseValues(element, culture);

        if (databaseValues.Count > 0)
        {
            foreach (var dbValue in databaseValues.ToList())
            {
                stringLocalizerValues[dbValue.Key] = dbValue.Value?.ToString() ?? string.Empty;
            }
        }
        

        return stringLocalizerValues.ToFrozenDictionary();
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

    private Dictionary<string, object?> GetDatabaseValues(Element element, string culture)
    {
        var values = new Dictionary<string, object?>(StringComparer.InvariantCultureIgnoreCase);
        var filter = new Dictionary<string,object?> { { "cultureCode", culture} };
        var result =EntityRepository.GetDictionaryListResult(element, new EntityParameters {Filters = filter},false);
        foreach (var row in result.Data)
        {
            values.Add(row["resourceKey"]!.ToString()!, row["resourceValue"]?.ToString());
        }

        return values;
    }
}