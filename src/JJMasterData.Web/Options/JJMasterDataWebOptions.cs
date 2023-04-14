namespace JJMasterData.Web.Options;

public class JJMasterDataWebOptions
{
    /// <summary>
    /// Default value: _MasterDataLayout <br></br>
    /// </summary>
    public string? LayoutPath { get; set; } = "_MasterDataLayout";

    /// <summary>
    /// Default value:_MasterDataLayout.Popup <br></br>
    /// </summary>
    public string? PopUpLayoutPath { get; set; } = "_MasterDataLayout.PopUp";

    /// <summary>
    /// Default value: false (Generate a link to the default bootstrap layout)
    /// </summary>
    /// <remarks>
    /// False = A default bootstrap.min.css will be added in stylesheets
    /// True = Bootstrap will not be added by default, and a custom bootstrap.css must be included
    /// </remarks>
    public bool UseCustomBootstrap { get; set; }
}