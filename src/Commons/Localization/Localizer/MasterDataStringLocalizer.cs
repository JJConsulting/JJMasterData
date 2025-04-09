#nullable enable

using System;
using System.Collections.Concurrent;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using JJMasterData.Commons.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;

namespace JJMasterData.Commons.Localization;

public sealed class MasterDataStringLocalizer(
    string resourceName,
    IStringLocalizer resourcesStringLocalizer,
    IServiceScopeFactory serviceScopeFactory
    ) : IStringLocalizer
{
    private static readonly LocalizedString Empty = new(string.Empty, string.Empty, resourceNotFound: true);
    private static readonly ConcurrentDictionary<string, FrozenDictionary<string, string>> Cache = new();

    public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures)
    {
        return GetAllStringsAsDictionary().Select(e => new LocalizedString(e.Key, e.Value));
    }

    public LocalizedString this[string? name]
    {
        get
        {
            if (string.IsNullOrEmpty(name))
                return Empty;

            var value = GetString(name);

            if (string.IsNullOrEmpty(value))
                return new LocalizedString(name!, name!, resourceNotFound:true);
            
            return new LocalizedString(name!, value, resourceNotFound:false);
        }
    }

    public LocalizedString this[string? name, params object[] arguments]
    {
        get
        {
            if (string.IsNullOrEmpty(name))
                return Empty;

            var value = GetString(name);
            
            if (string.IsNullOrEmpty(value))
                return new LocalizedString(name!, string.Format(name, arguments), resourceNotFound:true);
            
            return new LocalizedString(name!, string.Format(value, arguments), resourceNotFound:false);
        }
    }

    private string GetString(string? key)
    {
        var culture = Thread.CurrentThread.CurrentCulture.Name;
        var cacheKey = $"{resourceName}_localization_strings_{culture}";

        if (Cache.TryGetValue(cacheKey, out var cachedDictionary))
            return cachedDictionary!.GetValueOrDefault(key, key)!;

        var localizedStrings = GetAllStringsAsDictionary();

        Cache[cacheKey] = localizedStrings;

        return localizedStrings!.GetValueOrDefault(key, key)!;
    }

    private FrozenDictionary<string, string> GetAllStringsAsDictionary()
    {
        var culture = Thread.CurrentThread.CurrentCulture.Name;

        var stringLocalizerValues = GetStringLocalizerValues();
        var customValues = GetCustomValues(culture);

        if (customValues.Count > 0)
        {
            foreach (var dbValue in customValues)
            {
                if (!string.IsNullOrEmpty(dbValue.Value))
                    stringLocalizerValues[dbValue.Key] = dbValue.Value;
            }
        }

        return stringLocalizerValues.ToFrozenDictionary();
    }

    private Dictionary<string, string> GetStringLocalizerValues()
    {
        try
        {
            var values = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
            var localizedStrings = resourcesStringLocalizer.GetAllStrings();

            foreach (var localizedString in localizedStrings)
            {
                if (!values.ContainsKey(localizedString.Name))
                    values.Add(localizedString.Name, localizedString.Value);
            }

            return values;
        }
        catch
        {
            return new Dictionary<string, string>();
        }
    }

    private Dictionary<string,string> GetCustomValues(string culture)
    {
        using var scope = serviceScopeFactory.CreateScope();
        var repository = scope.ServiceProvider.GetService<ILocalizationRepository>();

        return repository?.GetAllStrings(culture) ?? new();
    }
    public static void ClearCache()
    {
        Cache.Clear();
    }
}