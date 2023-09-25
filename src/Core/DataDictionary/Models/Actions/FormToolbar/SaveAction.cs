#nullable enable

namespace JJMasterData.Core.DataDictionary.Models.Actions;

/// <summary>
/// Action to save a DataPanel at a FormView.
/// </summary>
public class SaveAction : FormToolbarAction
{
    public const string ActionName = "save";

    public SaveAction()
    {
        Order = 1;
        Name = ActionName;
        Icon = IconType.Check;
        Text = "Save";
        Location = FormToolbarActionLocation.Panel;
        ShowAsButton = true;
        VisibleExpression = "val:1";
    }

    public FormEnterKey EnterKeyBehavior { get; set; }
    
    public bool SubmitOnSave { get; set; }
}