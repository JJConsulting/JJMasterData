#nullable enable


using System.Text.Json.Serialization;
using JJConsulting.FontAwesome;

namespace JJMasterData.Core.DataDictionary.Models.Actions;

/// <summary>
/// Action to return to the Grid at PageState.View or close the edit mode at a relationship.
/// </summary>
public sealed class BackAction : FormToolbarAction, ISubmittableAction
{
    public const string ActionName = "back";

    [JsonPropertyName("isSubmit")]
    public bool IsSubmit { get; set; }

    public override bool IsSystemDefined => true;

    public BackAction()
    {
        Name = ActionName;
        VisibleExpression = "val:{IsView}";
        Icon = FontAwesomeIcon.ArrowLeft;
        ShowAsButton = true;
        Location = FormToolbarActionLocation.Bottom;
        Order = 0;
        Text = "Back";
    }
    public override BasicAction DeepCopy() => (BasicAction)MemberwiseClone();
}