using JJMasterData.Commons.Data;
using JJMasterData.Commons.Options;
using JJMasterData.Core.Web.Components;

namespace JJMasterData.Web.Areas.MasterData.Models.ViewModels;

public class OptionsViewModel
{
    public ConnectionString ConnectionString { get; set; }
    public JJMasterDataCommonsOptions? Options { get; set; } 
    public bool IsFullscreen { get; set; }
    public string? FilePath { get; set; }
    public DataAccessProviderType ConnectionProvider { get; set; }
    public bool? IsConnectionSuccessful { get; set; }
    public bool PathExists => File.Exists(FilePath);
    public JJValidationSummary? ValidationSummary { get; set; }

    public OptionsViewModel()
    {
        ConnectionString = new ConnectionString();
    }
}