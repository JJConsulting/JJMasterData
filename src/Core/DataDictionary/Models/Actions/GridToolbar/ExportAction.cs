
#nullable enable
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace JJMasterData.Core.DataDictionary.Models.Actions;

/// <summary>
/// Represents the dictionary export button.
/// </summary>
public class ExportAction : GridToolbarAction
{
    public const string ActionName = "export";

    [JsonProperty("processOptions")] 
    public ProcessOptions ProcessOptions { get; set; }

    [JsonProperty("fileName")]
    [Display(Name="Download File Name")]
    public string? FileName { get; set; }
    
    public ExportAction()
    {
        Name = ActionName;
        Tooltip = "Export";
        Icon = IconType.Download;
        ShowAsButton = true;
        CssClass = "float-end";
        Order = 3;
        ProcessOptions = new ProcessOptions();
    }

    public override BasicAction DeepCopy()
    {
        var newAction = (ExportAction)CopyAction();
        newAction.FileName = FileName;
        newAction.ProcessOptions = ProcessOptions.DeepCopy();
        return newAction;
    }
}