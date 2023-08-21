#nullable enable
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JJMasterData.Commons.Configuration.Options;
using JJMasterData.Commons.Data.Entity;
using JJMasterData.Commons.Data.Entity.Abstractions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;


namespace JJMasterData.Commons.Localization;

public sealed class JJMasterDataStringLocalizer<TResourceSource> : IStringLocalizer<TResourceSource>
{
    private readonly IStringLocalizer _localizer;
    
    public JJMasterDataStringLocalizer(IStringLocalizerFactory factory)
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
        return GetAllStringsAsDictionary().GetAwaiter().GetResult().Select(e=>new LocalizedString(e.Key, e.Value));
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

        if (Cache.TryGetValue<Dictionary<string, string>>(cacheKey, out var cachedDictionary))
        {
            if (cachedDictionary.TryGetValue(key, out var cachedValue))
                return cachedValue;
            return key;
        }

        var localizedStrings = GetAllStringsAsDictionary().GetAwaiter().GetResult();
        
        Cache.Set(cacheKey, localizedStrings);

        if (localizedStrings.TryGetValue(key, out var localizedValue))
            return localizedValue;
   
        return key;
    }

    private async Task<IDictionary<string, string>> GetAllStringsAsDictionary()
    {
        string culture = Thread.CurrentThread.CurrentCulture.Name;

        var element = JJMasterDataStringLocalizerElement.GetElement(Options);

        var tableExists = await EntityRepository.TableExistsAsync(element.TableName);
        
        if (!tableExists)
            await EntityRepository.CreateDataModelAsync(element);

        var stringLocalizerValues = GetStringLocalizerValues();
        var databaseValues = GetDatabaseValues(element, culture);

        if (databaseValues.Count > 0)
        {
            foreach (var dbValue in databaseValues.ToList())
            {
                stringLocalizerValues[dbValue.Key] = dbValue.Value?.ToString() ?? string.Empty;
            }

            return stringLocalizerValues;
        }
        
        await SetDatabaseValues(element, ConvertDictionaryToList(stringLocalizerValues, culture));

        return stringLocalizerValues;
    }

    private async Task SetDatabaseValues(Element element, IEnumerable<IDictionary<string,object?>> values)
    {
        foreach (var value in values)
            await EntityRepository.SetValuesAsync(element, value);
    }

    private IEnumerable<IDictionary<string,object?>> ConvertDictionaryToList(IDictionary<string, string> dictionary,
        string culture)
    {
        return dictionary.Select(pair => new Dictionary<string,object?>
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

    private Dictionary<string, object?> GetDatabaseValues(Element element, string culture)
    {
        var values = new Dictionary<string, object?>(StringComparer.InvariantCultureIgnoreCase);
        var filter = new Dictionary<string,object?> { { "cultureCode", culture} };
        var result = EntityRepository.GetDictionaryListAsync(element, new EntityParameters(){Parameters = filter},false).GetAwaiter().GetResult();
        foreach (var row in result.Data)
        {
            values.Add(row["resourceKey"]!.ToString()!, row["resourceValue"]?.ToString());
        }

        return values;
    }
}