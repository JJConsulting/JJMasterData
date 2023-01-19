using JJMasterData.Commons.Options;
using JJMasterData.Core.Options;
using JJMasterData.Web.Areas.MasterData.Models;
using JJMasterData.Web.Options;

namespace JJMasterData.Web.Models;

public record JJConfigurationOptions
{
    public JJMasterDataCommonsOptions JJMasterDataCommons { get;  } = new();
    public JJMasterDataCoreOptions JJMasterDataCore { get; } = new();
    public JJMasterDataWebOptions JJMasterDataWeb{ get; } = new();
    public ConnectionStrings ConnectionStrings { get;  } = new();
    public ConnectionProviders ConnectionProviders { get;  } = new();
}