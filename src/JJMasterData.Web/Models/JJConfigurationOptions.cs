using JJMasterData.Commons.Options;
using JJMasterData.Web.Areas.MasterData.Models;

namespace JJMasterData.Web.Models;

public class JJConfigurationOptions
{
    public JJMasterDataCommonsOptions JJMasterDataCommons { get; set; }
    public ConnectionStrings ConnectionStrings { get;  set;} 
    public ConnectionProviders ConnectionProviders { get; set; } 

    public JJConfigurationOptions()
    {
        ConnectionStrings = new ConnectionStrings();
        JJMasterDataCommons = new JJMasterDataCommonsOptions();
        ConnectionProviders = new ConnectionProviders();
    }
}