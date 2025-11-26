#nullable enable

using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using JJConsulting.FontAwesome;

namespace JJMasterData.Core.DataDictionary.Models.Actions;

/// <summary>
/// Action to save a DataPanel at a FormView.
/// </summary>
public sealed class SaveAction : FormToolbarAction, ISubmittableAction
{
    public const string ActionName = "save";

    [JsonPropertyName("enterKeyBehavior")]
    public FormEnterKey EnterKeyBehavior { get; set; }
    
    [JsonPropertyName("isSubmit")]
    [Display(Name = "Is Submit")]
    public bool IsSubmit { get; set; }
    
    public SaveAction()
    {
        Order = 1;
        Name = ActionName;
        Icon = FontAwesomeIcon.Check;
        Text = "Save";
        Location = FormToolbarActionLocation.Panel;
        Color = BootstrapColor.Primary;
        ShowAsButton = true;
        VisibleExpression = "exp: '{PageState}' <> 'View'";
    }
    public override BasicAction DeepCopy() => (BasicAction)MemberwiseClone();
}