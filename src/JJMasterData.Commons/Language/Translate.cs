using System.Collections.Generic;
using System.Data;
using System.Globalization;
using JJMasterData.Commons.DI;

namespace JJMasterData.Commons.Language;

/// <summary>
/// Class to i18n 
/// </summary>
public static class Translate
{
    private static Dictionary<string, Dictionary<string, string>> _resourcesDictionary;

    private static ITranslator Translator => JJService.Translator;

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

        _resourcesDictionary ??= new Dictionary<string, Dictionary<string, string>>();

        lock (_resourcesDictionary)
        {
            if (!_resourcesDictionary.ContainsKey(culture))
                _resourcesDictionary.Add(culture, Translator.GetDictionaryStrings(culture));
            
            if (_resourcesDictionary[culture].ContainsKey(resourceKey))
                return _resourcesDictionary[culture][resourceKey];
        }
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