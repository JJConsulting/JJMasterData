using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using JJMasterData.Core.Configuration.Options;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace JJMasterData.Web.Configuration.Options;

public sealed class MasterDataWebOptions
{
    /// <summary>
    /// Default value: _MasterDataLayout <br></br>
    /// </summary>
    [Display(Name = "Razor Layout Path")]
    public string LayoutPath { get; set; } = "_MasterDataLayout";

    /// <summary>
    /// Default value:_MasterDataLayout.Popup <br></br>
    /// </summary>
    [Display(Name = "Razor Modal Layout Path")]
    public string ModalLayoutPath { get; set; } = "_MasterDataLayout.Modal";

    /// <summary>
    /// Default value: false (Generate a link to the default bootstrap layout)
    /// </summary>
    /// <remarks>
    /// False = A default bootstrap.min.css will be added in stylesheets
    /// True = Bootstrap will not be added by default, and a custom bootstrap.css must be included
    /// </remarks>
    [JsonIgnore]
    internal bool UseCustomBootstrap => CustomBootstrapPath is not null;
    
    [Display(Name = "Custom Bootstrap Path")]
    public string? CustomBootstrapPath { get; set; }
    
    public bool SupportNetFramework { get; set; }
    
    [Display(Name = "Enable Bundle and Minification")]
    public bool EnableBundleAndMinification { get; set; }
    
    [Display(Name = "Use Advanced Mode At Expressions")]
    public bool UseAdvancedModeAtExpressions { get; set; }
    
    /// <summary>
    /// Custom scripts to be added at _MasterDataScripts. The paths are relative.
    /// </summary>
    [BindNever]
    public List<string> CustomScriptsPaths { get; } = [];

        
    /// <summary>
    /// Custom scripts to be added at _MasterDataStylesheets. The paths are relative.
    /// </summary>
    [BindNever]
    public List<string> CustomStylesheetsPaths { get; } = [];
    
    [BindNever]
    public Action<MasterDataCoreOptions>? ConfigureCoreOptions { get; set; }
}