using JJMasterData.Core.DataDictionary.Actions.Abstractions;
using JJMasterData.Core.Web;
using Newtonsoft.Json;

namespace JJMasterData.Core.DataDictionary.Actions.GridToolbar;

/// <summary>
/// Represents the dictionary export button.
/// </summary>
public class ExportAction : BasicAction
{
    /// <summary>
    /// Default action name
    /// </summary>
    public const string ActionName = "export";

    public override bool IsUserCreated => false;

    [JsonProperty("processOptions")] 
    public ProcessOptions ProcessOptions { get; set; }


    public ExportAction()
    {
        Name = ActionName;
        ToolTip = "Export";
        Icon = IconType.Download;
        ShowAsButton = true;
        CssClass = BootstrapHelper.PullRight;
        Order = 3;
        ProcessOptions = new ProcessOptions();
    }
}