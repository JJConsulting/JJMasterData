using System.ComponentModel.DataAnnotations;
using JJMasterData.Commons.Configuration.Options;
using JJMasterData.Commons.Data;
using JJMasterData.Core.Configuration.Options;
using JJMasterData.Core.UI.Components;
using JJMasterData.Web.Configuration.Options;

namespace JJMasterData.Web.Areas.DataDictionary.Models.ViewModels;

public class SettingsViewModel
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