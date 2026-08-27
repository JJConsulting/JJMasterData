#nullable disable warnings
using System;
using System.Collections.Generic;
using System.Text.Json;
using JJMasterData.Commons.Security;
using JJMasterData.Commons.Serialization;
using JJMasterData.Core.UI.Components;
using JJMasterData.Core.UI.Routing;

namespace JJMasterData.Core.Extensions;

public static class EncryptionServiceExtensions
{
    extension(DataProtectionService service)
    {
        public string EncryptObject<T>(T @object)
        {
            return service.Protect(JsonSerializer.Serialize(@object, MasterDataJsonSerializerOptions.Default));
        }

        public T DecryptObject<T>(string encryptedObject)
        {
            return JsonSerializer.Deserialize<T>(service.Unprotect(encryptedObject), MasterDataJsonSerializerOptions.Default);
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
