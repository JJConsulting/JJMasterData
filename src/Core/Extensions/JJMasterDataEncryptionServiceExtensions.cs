using System.Web;
using JJMasterData.Commons.Cryptography;
using JJMasterData.Core.DataManager;
using Newtonsoft.Json;

namespace JJMasterData.Core.Extensions;

public static class JJMasterDataEncryptionServiceExtensions
{
    public static string EncryptStringWithUrlEncode(this JJMasterDataEncryptionService service, string plainText)
    {
        return HttpUtility.UrlEncode(service.EncryptString(plainText));
    }
    
    public static string DecryptStringWithUrlDecode(this JJMasterDataEncryptionService service, string cipherText)
    {
        return service.DecryptString(HttpUtility.UrlDecode(cipherText));
    }
    
    public static string EncryptActionMap(this JJMasterDataEncryptionService service, ActionMap actionMap)
    {
        return service.EncryptStringWithUrlEncode(JsonConvert.SerializeObject(actionMap));
    }
    
    public static ActionMap DecryptActionMap(this JJMasterDataEncryptionService service, string encryptedActionMap)
    {
        return JsonConvert.DeserializeObject<ActionMap>(service.DecryptStringWithUrlDecode(encryptedActionMap));
    }
}