using JJMasterData.Core.Web;
using Newtonsoft.Json;

namespace JJMasterData.Core.DataDictionary.Actions.GridToolbar;

public class ImportAction : GridToolbarAction
{
    /// <summary>
    /// Nome padrão da ação
    /// </summary>
    public const string ActionName = "import";
    
    [JsonProperty("processOptions")]
    public ProcessOptions ProcessOptions { get; set; }
    public ImportAction()
    {
        Name = ActionName;
        ToolTip = "Upload";
        Icon = IconType.Upload;
        ShowAsButton = true;
        CssClass = BootstrapHelper.PullRight;
        Order = 4;
        ProcessOptions = new ProcessOptions();
        SetVisible(false);
    }
}