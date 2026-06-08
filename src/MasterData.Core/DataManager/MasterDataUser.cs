using JJMasterData.Core.Configuration.Options;
using JJMasterData.Core.Extensions;
using Microsoft.AspNetCore.Http;
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
    public string Id { get; set; } = httpContext.HttpContext!.User?.GetUserId(coreOptions.Value.UserIdClaimType);
}