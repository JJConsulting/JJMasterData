#nullable enable

using Newtonsoft.Json;

namespace JJMasterData.Core.DataDictionary.Models.Actions;

/// <summary>
/// Action to save a DataPanel at a FormView.
/// </summary>
public class SaveAction : FormToolbarAction, ISubmittableAction
{
    public const string ActionName = "save";

    public FormEnterKey EnterKeyBehavior { get; set; }
    
    [JsonProperty("isSubmit")]
    public bool IsSubmit { get; set; }
    
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