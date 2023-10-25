using JJMasterData.Commons.Configuration.Options;
using JJMasterData.Commons.Data;
using JJMasterData.Core.UI.Components;

namespace JJMasterData.Web.Areas.DataDictionary.Models.ViewModels;

public class OptionsViewModel
{
    public ConnectionString ConnectionString { get; set; }
    public MasterDataCommonsOptions? Options { get; set; } 
    public bool IsFullscreen { get; set; }
    public string? FilePath { get; set; }
    public DataAccessProvider ConnectionProvider { get; set; }
    public bool? IsConnectionSuccessful { get; set; }
    public bool PathExists => File.Exists(FilePath);
    public JJValidationSummary? ValidationSummary { get; set; }

    public OptionsViewModel()
    {
        ConnectionString = new ConnectionString();
    }
}