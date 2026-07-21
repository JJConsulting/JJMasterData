using JJMasterData.Commons.Security.Cryptography.Abstractions;
using JJMasterData.Core.Extensions;
using JJMasterData.Web.Components;
using JJMasterData.Web.Routing;

namespace JJMasterData.Web.Extensions;

public static class EncryptionServiceExtensions
{
    extension(IEncryptionService service)
    {
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
