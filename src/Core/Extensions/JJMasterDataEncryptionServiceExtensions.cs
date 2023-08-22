using System;
using JJMasterData.Commons.Cryptography;
using JJMasterData.Core.DataManager;
using Newtonsoft.Json;

namespace JJMasterData.Core.Extensions;

public static class JJMasterDataEncryptionServiceExtensions
{
    /// <summary>
    /// Encrypts the string with URL escape to prevent errors in parsing, algorithms like AES generate characters like '/'
    /// </summary>
    /// <param name="service"></param>
    /// <param name="plainText"></param>
    /// <returns></returns>
    public static string EncryptStringWithUrlEscape(this JJMasterDataEncryptionService service, string plainText)
    {
        return Uri.EscapeDataString(service.EncryptString(plainText));
    }
    
    /// <summary>
    /// Decrypts the string with URL unescape to prevent errors in parsing, algorithms like AES generate characters like '/'
    /// </summary>
    /// <param name="service"></param>
    /// <param name="cipherText"></param>
    /// <returns></returns>
    public static string DecryptStringWithUrlUnescape(this JJMasterDataEncryptionService service, string cipherText)
    {
        return service.DecryptString(Uri.UnescapeDataString(cipherText));
    }
    
    public static string EncryptActionMap(this JJMasterDataEncryptionService service, ActionMap actionMap)
    {
        return service.EncryptStringWithUrlEscape(JsonConvert.SerializeObject(actionMap));
    }
    
    public static ActionMap DecryptActionMap(this JJMasterDataEncryptionService service, string encryptedActionMap)
    {
        return JsonConvert.DeserializeObject<ActionMap>(service.DecryptStringWithUrlUnescape(encryptedActionMap))!;
    }
}