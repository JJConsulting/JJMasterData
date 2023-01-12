using JJMasterData.Commons.Options;
using JJMasterData.Core.Options;
using JJMasterData.Web.Areas.MasterData.Models;
using JJMasterData.Web.Options;

namespace JJMasterData.Web.Models;

public class JJConfigurationOptions
{
    public JJMasterDataCommonsOptions JJMasterDataCommons { get; set; }
    public JJMasterDataCoreOptions JJMasterDataCore { get; set; }
    public JJMasterDataWebOptions JJMasterDataWeb{ get; set; }
    public ConnectionStrings ConnectionStrings { get;  set;} 
    public ConnectionProviders ConnectionProviders { get; set; } 

    public JJConfigurationOptions()
    {
        ConnectionStrings = new ConnectionStrings();
        JJMasterDataCommons = new JJMasterDataCommonsOptions();
        JJMasterDataCore = new JJMasterDataCoreOptions();
        JJMasterDataWeb = new JJMasterDataWebOptions();
        ConnectionProviders = new ConnectionProviders();
    }
}