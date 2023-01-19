using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using JJMasterData.Commons.DI;

namespace JJMasterData.Commons.Localization;

/// <summary>
/// Translates a key into a localized value.
/// </summary>
public static class Translate
{
    private static ConcurrentDictionary<string, IDictionary<string, string>> _resourcesDictionary;
    private static ILocalizationProvider Provider { get; }
    static Translate()
    {
        Provider = JJService.LocalizationProvider;
    }
    
    /// <summary>
    /// Translates the requested key.
    /// </summary>
    /// <param name="resourceKey"></param>
    /// <returns>The value equivalent to that key.</returns>
    public static string Key(string resourceKey)
    {
        string culture = CultureInfo.CurrentCulture.Name;

        if (string.IsNullOrEmpty(resourceKey))
            return resourceKey;

        _resourcesDictionary ??= new ConcurrentDictionary<string, IDictionary<string, string>>();
        
        if (!_resourcesDictionary.ContainsKey(culture))
            _resourcesDictionary.TryAdd(culture, Provider.GetLocalizedStrings(culture));
        
        if (_resourcesDictionary[culture].ContainsKey(resourceKey))
            return _resourcesDictionary[culture][resourceKey];
        
        return resourceKey;
    }


    /// <summary>
    /// Translates the value 
    /// </summary>
    /// <param name="formatKey"></param>
    /// <param name="args"></param>
    /// <returns>The value equivalent to that key.</returns>
    public static string Key(string formatKey, params object[] args)
    {
        return string.Format(Key(formatKey), args);
    }

    public static void ClearCache()
    {
        _resourcesDictionary.Clear();
        _resourcesDictionary = null;
    }
}