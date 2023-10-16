using JJMasterData.Core.Configuration.Options;

namespace JJMasterData.Web.Configuration.Options;

public class JJMasterDataWebOptions : JJMasterDataCoreOptions
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
    public bool UseCustomBootstrap { get; set; }
    
    public string? CustomBootstrapPath { get; set; }
}