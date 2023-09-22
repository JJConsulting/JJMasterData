using System;
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
    
    public static string EncryptObject<T>(this IEncryptionService service, T @object)
    {
        return service.EncryptStringWithUrlEscape(JsonConvert.SerializeObject(@object));
    }
    
    public static T DecryptObject<T>(this IEncryptionService service, string encryptedObject)
    {
        return JsonConvert.DeserializeObject<T>(service.DecryptStringWithUrlUnescape(encryptedObject)!);
    }
    
    
    public static string EncryptDictionary(this IEncryptionService service, IDictionary<string,object> dictionary)
    {
        return service.EncryptObject(dictionary);
    }
    
    public static IDictionary<string,object> DecryptDictionary(this IEncryptionService service, string encryptedDictionary)
    {
        return service.DecryptObject<IDictionary<string,object>>(encryptedDictionary);
    }
    
    internal static string EncryptActionMap(this IEncryptionService service, ActionMap actionMap)
    {
        return service.EncryptStringWithUrlEscape(JsonConvert.SerializeObject(actionMap));
    }
    
    internal static string EncryptRouteContext(this IEncryptionService service, RouteContext routeContext)
    {
        return service.EncryptObject(routeContext);
    }
    
    internal static RouteContext DecryptRouteContext(this IEncryptionService service, string encryptedRouteContext)
    {
        return service.DecryptObject<RouteContext>(encryptedRouteContext);
    }
    
    internal static ActionMap DecryptActionMap(this IEncryptionService service, string encryptedActionMap)
    {
        return service.DecryptObject<ActionMap>(encryptedActionMap);
    }
}