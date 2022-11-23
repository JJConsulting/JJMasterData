

using JJMasterData.Commons.Dao;
using JJMasterData.Commons.Options;

namespace JJMasterData.Web.Areas.MasterData.Models;

public class OptionsViewModel
{
    public  ConnectionString ConnectionString { get; set; }
    public  JJMasterDataOptions Options { get; set; } 
    public bool IsFullscreen { get; set; }
    public  string FilePath { get; set; }
    public DataAccessProviderType ConnectionProvider { get; set; }
}