#nullable enable

using System.Runtime.Serialization;

namespace JJMasterData.Core.DataDictionary;

public class FormElementOptions
{
    [DataMember(Name="gridOptions")]
    public GridUI Grid { get; set; }
    
    [DataMember(Name="formOptions")]
    public FormUI Form { get; set; }
    
    [DataMember(Name="gridToolbarActions")]
    public GridToolbarActions ToolbarActions { get; set; }

    [DataMember(Name = "formToolbarActions")]
    public FormToolbarActions FormToolbarActions { get; set; }

    [DataMember(Name="gridActions")]
    public GridActions GridActions { get; set; }

    public FormElementOptions()
    {
        Grid = new GridUI();
        Form = new FormUI();
        ToolbarActions = new GridToolbarActions();
        FormToolbarActions = new FormToolbarActions();
        GridActions = new GridActions();
    }
}