#nullable disable warnings
using System.Collections.Generic;
using System.Text.Json;
using JJMasterData.Commons.Security;
using JJMasterData.Commons.Serialization;
using JJMasterData.Core.UI.Components;
using JJMasterData.Core.UI.Routing;

namespace JJMasterData.Core.Extensions;

public static class DataProtectionExtensions
{
    extension(DataProtectionService service)
    {
        public string ProtectObject<T>(T @object)
        {
            return service.Protect(JsonSerializer.Serialize(@object, MasterDataJsonSerializerOptions.Default));
        }

        public T UnprotectObject<T>(string encryptedObject)
        {
            return JsonSerializer.Deserialize<T>(service.Unprotect(encryptedObject), MasterDataJsonSerializerOptions.Default);
        }

        public Dictionary<string,object> ProtectDictionary(string encryptedDictionary)
        {
            return service.UnprotectObject<Dictionary<string,object>>(encryptedDictionary);
        }

        public RouteContext UnprotectRouteContext(string encryptedRouteContext)
        {
            return service.UnprotectObject<RouteContext>(encryptedRouteContext);
        }

        internal ActionMap UnprotectActionMap(string encryptedActionMap)
        {
            return service.UnprotectObject<ActionMap>(encryptedActionMap);
        }
    }
}
