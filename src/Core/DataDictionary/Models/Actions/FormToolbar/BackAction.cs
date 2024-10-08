#nullable enable


namespace JJMasterData.Core.DataDictionary.Models.Actions;

/// <summary>
/// Action to return to the Grid at PageState.View or close the edit mode at a relationship.
/// </summary>
public sealed class BackAction : FormToolbarAction
{
    public const string ActionName = "back";

    public BackAction()
    {
        Name = ActionName;
        VisibleExpression = "val:{IsView}";
        Icon = IconType.ArrowLeft;
        ShowAsButton = true;
        Location = FormToolbarActionLocation.Bottom;
        Order = 0;
        Text = "Back";
    }
    public override BasicAction DeepCopy() => (BasicAction)MemberwiseClone();
}