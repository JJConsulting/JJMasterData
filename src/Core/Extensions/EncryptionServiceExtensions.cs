using System;
using System.Collections.Generic;
using System.Text.Json;
using JJMasterData.Commons.Security.Cryptography.Abstractions;
using JJMasterData.Commons.Serialization;
using JJMasterData.Core.UI.Components;
using JJMasterData.Core.UI.Routing;

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
        return service.EncryptStringWithUrlEscape(JsonSerializer.Serialize(@object, MasterDataJsonSerializerOptions.Default));
    }
    
    public static T DecryptObject<T>(this IEncryptionService service, string encryptedObject)
    {
        return JsonSerializer.Deserialize<T>(service.DecryptStringWithUrlUnescape(encryptedObject)!, MasterDataJsonSerializerOptions.Default);
    }
    
    public static Dictionary<string,object> DecryptDictionary(this IEncryptionService service, string encryptedDictionary)
    {
        return service.DecryptObject<Dictionary<string,object>>(encryptedDictionary);
    }
    
    public static RouteContext DecryptRouteContext(this IEncryptionService service, string encryptedRouteContext)
    {
        return service.DecryptObject<RouteContext>(encryptedRouteContext);
    }
    
    internal static ActionMap DecryptActionMap(this IEncryptionService service, string encryptedActionMap)
    {
        return service.DecryptObject<ActionMap>(encryptedActionMap);
    }
}