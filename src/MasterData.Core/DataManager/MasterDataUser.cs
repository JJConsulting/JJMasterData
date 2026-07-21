#nullable disable warnings
using JJMasterData.Core.Abstractions;
using JJMasterData.Core.Configuration.Options;
using Microsoft.Extensions.Options;

namespace JJMasterData.Core.DataManager;

public interface IMasterDataUser
{
    public string Id { get; set; }
}

internal sealed class MasterDataUser(
    IMasterDataRequestContext requestContext,
    IOptionsSnapshot<MasterDataCoreOptions> coreOptions)
    : IMasterDataUser
{
    public string Id { get; set; } = requestContext.GetClaimValue(coreOptions.Value.UserIdClaimType);
}
