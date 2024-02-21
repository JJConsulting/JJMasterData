using JJMasterData.Commons.Configuration.Options;
using JJMasterData.Core.Configuration.Options;

namespace JJMasterData.Web.Configuration.Options;

public class MasterDataWebOptionsConfiguration
{
    public Action<MasterDataWebOptions>? ConfigureWeb { get; init; }
    public Action<MasterDataCoreOptions>? ConfigureCore { get; init; } 
    public Action<MasterDataCommonsOptions>? ConfigureCommons { get; init; }
}