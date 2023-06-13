
using Newtonsoft.Json;

namespace JJMasterData.Core.DataDictionary.Actions.GridToolbar;

/// <summary>
/// Represents the dictionary export button.
/// </summary>
public class ExportAction : GridToolbarAction
{
    /// <summary>
    /// Default action name
    /// </summary>
    public const string ActionName = "export";

    [JsonProperty("processOptions")] 
    public ProcessOptions ProcessOptions { get; set; }


    public ExportAction()
    {
        Name = ActionName;
        ToolTip = "Export";
        Icon = IconType.Download;
        ShowAsButton = true;
        CssClass = "float-end";
        Order = 3;
        ProcessOptions = new ProcessOptions();
    }
}