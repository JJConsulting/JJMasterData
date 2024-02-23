using System.ComponentModel.DataAnnotations;
using JJMasterData.Commons.Configuration.Options;
using JJMasterData.Commons.Data;
using JJMasterData.Core.UI.Components;

namespace JJMasterData.Web.Areas.DataDictionary.Models.ViewModels;

public class SettingsViewModel
{
    public required ConnectionString ConnectionString { get; set; } = new();
    public MasterDataCommonsOptions? Options { get; set; } 
    public string? FilePath { get; set; }
    public DataAccessProvider ConnectionProvider { get; set; }
    [Display(Name = "Custom Bootstrap Path")]
    public required string? CustomBootstrapPath { get; set; }
    [Display(Name = "Data Dictionary Table Name")]
    public required string? DataDictionaryTableName { get; set; }
    [Display(Name = "Use Advanced Mode at Expressions")]
    public required bool UseAdvancedModeAtExpressions { get; set; }
    public bool PathExists => File.Exists(FilePath);
    public JJValidationSummary? ValidationSummary { get; set; }
}