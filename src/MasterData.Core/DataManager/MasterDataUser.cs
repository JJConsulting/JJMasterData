using JJMasterData.Core.Configuration.Options;
using JJMasterData.Core.Extensions;
using JJMasterData.Core.Http.Abstractions;
using Microsoft.Extensions.Options;

namespace JJMasterData.Core.DataManager;

public interface IMasterDataUser
{
    public string Id { get; set; }
}

internal sealed class MasterDataUser(
    IHttpContext httpContext,
    IOptionsSnapshot<MasterDataCoreOptions> coreOptions)
    : IMasterDataUser
{
    public string Id { get; set; } = httpContext.User.GetUserId(coreOptions.Value.UserIdClaimType);
} 