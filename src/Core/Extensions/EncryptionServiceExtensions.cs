using System;
using System.Collections;
using System.Collections.Generic;
using JJMasterData.Commons.Cryptography;
using JJMasterData.Core.DataManager;
using JJMasterData.Core.UI.Components;
using Newtonsoft.Json;

namespace JJMasterData.Core.Extensions;

public static class EncryptionServiceExtensions
{
    /// <summary>
    /// Encrypts the string with URL escape to prevent errors in parsing, algorithms like AES generate characters like '/'
    /// </summary>
    /// <param name="service"></param>
    /// <param name="plainText"></param>
    /// <returns></returns>
    public static string EncryptStringWithUrlEscape(this IEncryptionService service, string plainText)
    {
        return Uri.EscapeDataString(service.EncryptString(plainText));
    }
    
    /// <summary>
    /// Decrypts the string with URL unescape to prevent errors in parsing, algorithms like AES generate characters like '/'
    /// </summary>
    /// <param name="service"></param>
    /// <param name="cipherText"></param>
    /// <returns></returns>
    public static string DecryptStringWithUrlUnescape(this IEncryptionService service, string cipherText)
    {
        return service.DecryptString(Uri.UnescapeDataString(cipherText));
    }
    
    internal static string EncryptActionMap(this IEncryptionService service, ActionMap actionMap)
    {
        return service.EncryptStringWithUrlEscape(JsonConvert.SerializeObject(actionMap));
    }
    
    internal static string EncryptRoute(this IEncryptionService service, RouteContext routeContext)
    {
        return service.EncryptStringWithUrlEscape(JsonConvert.SerializeObject(routeContext));
    }
    
    internal static RouteContext DecryptRoute(this IEncryptionService service, string encryptedRouteContext)
    {
        return JsonConvert.DeserializeObject<RouteContext>(service.DecryptStringWithUrlUnescape(encryptedRouteContext));
    }
    
    internal static ActionMap DecryptActionMap(this IEncryptionService service, string encryptedActionMap)
    {
        return JsonConvert.DeserializeObject<ActionMap>(service.DecryptStringWithUrlUnescape(encryptedActionMap));
    }
    
    internal static string EncryptDictionary(this IEncryptionService service, IDictionary<string,object> dictionary)
    {
        return service.EncryptStringWithUrlEscape(JsonConvert.SerializeObject(dictionary));
    }
    
    internal static Dictionary<string,object> DecryptDictionary(this IEncryptionService service, string encryptedDictionary)
    {
        return JsonConvert.DeserializeObject<Dictionary<string,object>>(service.DecryptStringWithUrlUnescape(encryptedDictionary));
    }
}