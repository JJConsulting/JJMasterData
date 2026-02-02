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
    extension(IEncryptionService service)
    {
        /// <summary>
        /// Encrypts the string with URL escape to prevent errors in parsing, algorithms like AES generate characters like '/'
        /// </summary>
        /// <param name="plainText"></param>
        /// <returns></returns>
        public string EncryptStringWithUrlEscape(string plainText)
        {
            return Uri.EscapeDataString(service.EncryptString(plainText));
        }

        /// <summary>
        /// Decrypts the string with URL unescape to prevent errors in parsing, algorithms like AES generate characters like '/'
        /// </summary>
        /// <param name="cipherText"></param>
        /// <returns></returns>
        public string DecryptStringWithUrlUnescape(string cipherText)
        {
            return service.DecryptString(Uri.UnescapeDataString(cipherText));
        }

        public string EncryptObject<T>(T @object)
        {
            return service.EncryptStringWithUrlEscape(JsonSerializer.Serialize(@object, MasterDataJsonSerializerOptions.Default));
        }

        public T DecryptObject<T>(string encryptedObject)
        {
            return JsonSerializer.Deserialize<T>(service.DecryptStringWithUrlUnescape(encryptedObject)!, MasterDataJsonSerializerOptions.Default);
        }

        public Dictionary<string,object> DecryptDictionary(string encryptedDictionary)
        {
            return service.DecryptObject<Dictionary<string,object>>(encryptedDictionary);
        }

        public RouteContext DecryptRouteContext(string encryptedRouteContext)
        {
            return service.DecryptObject<RouteContext>(encryptedRouteContext);
        }

        internal ActionMap DecryptActionMap(string encryptedActionMap)
        {
            return service.DecryptObject<ActionMap>(encryptedActionMap);
        }
    }
}