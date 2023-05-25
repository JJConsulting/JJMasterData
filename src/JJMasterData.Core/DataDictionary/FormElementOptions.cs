#nullable enable

using System.Runtime.Serialization;
using JJMasterData.Core.Web.Components;
using Newtonsoft.Json;

namespace JJMasterData.Core.DataDictionary;

public class FormElementOptions
{
    [DataMember(Name="gridOptions")]
    public GridUI Grid { get; set; }
    
    [DataMember(Name="formOptions")]
    public FormUI Form { get; set; }
    
    [DataMember(Name="gridToolbarActions")]
    public GridToolbarActionList ToolbarActions { get;}

    [DataMember(Name = "formToolbarActions")]
    public FormToolbarActionList FormToolbarActions { get; }

    [DataMember(Name="gridActions")]
    public GridTableActionList GridActions { get; }

    public FormElementOptions()
    {
        Grid = new GridUI();
        Form = new FormUI();
        ToolbarActions = new GridToolbarActionList();
        FormToolbarActions = new FormToolbarActionList();
        GridActions = new GridTableActionList();
    }
    
    [JsonConstructor]
    private FormElementOptions(
        GridUI gridUI,
        FormUI formUI,
        GridToolbarActionList toolbarActions,
        GridTableActionList gridActions,
        FormToolbarActionList formToolbarActions)
    {
        Grid = gridUI;
        Form = formUI;
        ToolbarActions = toolbarActions;
        GridActions = gridActions;
        FormToolbarActions = formToolbarActions;
    }
}