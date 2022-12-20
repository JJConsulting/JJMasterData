using JJMasterData.Commons.Options;
using JJMasterData.Web.Areas.MasterData.Models;

namespace JJMasterData.Web.Models;

public class JJConfigurationOptions
{
    public JJMasterDataOptions JJMasterData { get; set; }
    public ConnectionStrings ConnectionStrings { get;  set;} 
    public ConnectionProviders ConnectionProviders { get; set; } 

    public JJConfigurationOptions()
    {
        ConnectionStrings = new ConnectionStrings();
        JJMasterData = new JJMasterDataOptions();
        ConnectionProviders = new ConnectionProviders();
    }
}