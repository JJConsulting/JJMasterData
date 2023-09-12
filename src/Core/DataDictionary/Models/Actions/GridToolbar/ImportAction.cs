using Newtonsoft.Json;

namespace JJMasterData.Core.DataDictionary.Actions.GridToolbar;

public class ImportAction : GridToolbarAction
{
    public const string ActionName = "import";
    
    [JsonProperty("processOptions")]
    public ProcessOptions ProcessOptions { get; set; }
    public ImportAction()
    {
        Name = ActionName;
        Tooltip = "Upload";
        Icon = IconType.Upload;
        ShowAsButton = true;
        CssClass = "float-end";
        Order = 4;
        ProcessOptions = new ProcessOptions();
        SetVisible(false);
    }
}