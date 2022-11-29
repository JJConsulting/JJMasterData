using JJMasterData.Commons.Options;
using JJMasterData.Web.Areas.MasterData.Models;

namespace JJMasterData.Web.Models;

public class JJConfigurationWrapper
{
    public JJMasterDataOptions JJMasterDataOptions { get; set; }
    public ConnectionStrings ConnectionStrings { get;  set;} 
    public ConnectionProviders ConnectionProviders { get; set; } 

    public JJConfigurationWrapper()
    {
        ConnectionStrings = new ConnectionStrings();
        JJMasterDataOptions = new JJMasterDataOptions();
        ConnectionProviders = new ConnectionProviders();
    }
}