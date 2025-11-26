
#nullable enable
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using JJConsulting.FontAwesome;

namespace JJMasterData.Core.DataDictionary.Models.Actions;

/// <summary>
/// Represents the dictionary export button.
/// </summary>
public sealed class ExportAction : GridToolbarAction
{
    public const string ActionName = "export";

    [JsonPropertyName("processOptions")] 
    public ProcessOptions ProcessOptions { get; set; }

    [JsonPropertyName("fileName")]
    [Display(Name="Download File Name")]
    public string? FileName { get; set; }
    
    public ExportAction()
    {
        Name = ActionName;
        Tooltip = "Export";
        Icon = FontAwesomeIcon.Download;
        ShowAsButton = true;
        CssClass = "float-end";
        Order = 3;
        ProcessOptions = new ProcessOptions();
    }

    public override BasicAction DeepCopy()
    {
        var newAction = (ExportAction)MemberwiseClone();
        newAction.FileName = FileName;
        newAction.ProcessOptions = ProcessOptions.DeepCopy();
        return newAction;
    }
}