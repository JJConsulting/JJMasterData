using JJMasterData.Commons.Configuration.Options;
using JJMasterData.Core.Configuration.Options;
using JJMasterData.Core.UI.Components;
using JJMasterData.Web.Configuration.Options;

namespace JJMasterData.Web.Areas.DataDictionary.Models;

public sealed class SettingsViewModel
{
    public required ConnectionString ConnectionString { get; init; } = new();
    public MasterDataCommonsOptions? Options { get; init; } 
    public string? FilePath { get; init; }
    
    public required MasterDataCommonsOptions CommonsOptions { get; init; }
    public required MasterDataCoreOptions CoreOptions { get; init; }
    public required MasterDataWebOptions WebOptions { get; init; }

    public bool PathExists => File.Exists(FilePath);
    public JJValidationSummary? ValidationSummary { get; set; }
}