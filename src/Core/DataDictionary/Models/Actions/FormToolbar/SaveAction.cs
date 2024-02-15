#nullable enable

using System.ComponentModel.DataAnnotations;

namespace JJMasterData.Core.DataDictionary.Models.Actions;

/// <summary>
/// Action to save a DataPanel at a FormView.
/// </summary>
public class SaveAction : FormToolbarAction
{
    public const string ActionName = "save";

    public FormEnterKey EnterKeyBehavior { get; set; }
    
    [Display(Name = "Submit On Save")]
    public bool SubmitOnSave { get; set; }
    
    public SaveAction()
    {
        Order = 1;
        Name = ActionName;
        Icon = IconType.Check;
        Text = "Save";
        Location = FormToolbarActionLocation.Panel;
        Color = PanelColor.Primary;
        ShowAsButton = true;
        VisibleExpression = "exp: '{PageState}' <> 'View'";
    }
}