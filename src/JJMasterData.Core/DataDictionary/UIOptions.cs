using System;
using System.Runtime.Serialization;

namespace JJMasterData.Core.DataDictionary;

[Serializable]
[DataContract]
public class UIOptions
{
    [DataMember(Name = "grid")]
    public UIGrid Grid { get; set; }

    [DataMember(Name = "form")]
    public UIForm Form { get; set; }

    [DataMember(Name = "toolBarActions")]
    public GridToolBarActions ToolBarActions { get; set; }

    [DataMember(Name = "gridActions")]
    public GridActions GridActions { get; set; }

    public UIOptions()
    {
        Grid = new UIGrid();
        Form = new UIForm();
        ToolBarActions = new GridToolBarActions();
        GridActions = new GridActions();
    }
}