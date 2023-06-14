using System.Threading;
using JJMasterData.Commons.DI;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;

namespace JJMasterData.Commons.Localization;

/// <summary>
/// Translates a key into a localized value.
/// </summary>
public static class Translate
{
    private static IStringLocalizer<JJMasterDataResources> Localizer { get; }
    static Translate()
    {
        Localizer = JJService.Provider.GetRequiredService<IStringLocalizer<JJMasterDataResources>>();
    }
    
    /// <summary>
    /// Translates the requested key.
    /// </summary>
    /// <param name="resourceKey"></param>
    /// <returns>The value equivalent to that key.</returns>
    public static string Key(string resourceKey)
    {
        if (string.IsNullOrEmpty(resourceKey))
            return resourceKey;
        return Localizer[resourceKey];
    }


    /// <summary>
    /// Translates the value 
    /// </summary>
    /// <param name="resourceKey"></param>
    /// <param name="args"></param>
    /// <returns>The value equivalent to that key.</returns>
    public static string Key(string resourceKey, params object[] args)
    {
        if (string.IsNullOrEmpty(resourceKey))
            return resourceKey;
        return Localizer[resourceKey, args]; 
    }
}