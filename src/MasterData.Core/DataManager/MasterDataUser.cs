#nullable disable warnings
using JJMasterData.Core.Configuration.Options;
using Microsoft.Extensions.Options;

namespace JJMasterData.Core.DataManager;

public interface IMasterDataUser
{
    public string Id { get; set; }
}

internal sealed class MasterDataUser(
    IHttpContextAccessor httpContext,
    IOptionsSnapshot<MasterDataCoreOptions> coreOptions)
    : IMasterDataUser
{
    public string Id { get; set; } = httpContext.HttpContext?.User.GetUserId(coreOptions.Value.UserIdClaimType);
}