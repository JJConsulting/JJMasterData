using JJMasterData.Core.Configuration.Options;

namespace JJMasterData.Web.Configuration.Options;

public class MasterDataWebOptions
{
    /// <summary>
    /// Default value: _MasterDataLayout <br></br>
    /// </summary>
    public string LayoutPath { get; set; } = "_MasterDataLayout";

    /// <summary>
    /// Default value:_MasterDataLayout.Popup <br></br>
    /// </summary>
    public string ModalLayoutPath { get; set; } = "_MasterDataLayout.Modal";

    /// <summary>
    /// Default value: false (Generate a link to the default bootstrap layout)
    /// </summary>
    /// <remarks>
    /// False = A default bootstrap.min.css will be added in stylesheets
    /// True = Bootstrap will not be added by default, and a custom bootstrap.css must be included
    /// </remarks>
    internal bool UseCustomBootstrap => CustomBootstrapPath is not null;
    
    public string? CustomBootstrapPath { get; set; }
    
    public bool SupportNetFramework { get; set; }
    public bool EnableBundleAndMinification { get; set; }

    /// <summary>
    /// Custom scripts to be added at _MasterDataScripts. The paths are relative.
    /// </summary>
    public List<string> CustomScriptsPaths { get; } = [];

    public Action<MasterDataCoreOptions>? ConfigureCoreOptions { get; set; }
}