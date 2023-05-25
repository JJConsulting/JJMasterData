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
    [JsonProperty(ObjectCreationHandling = ObjectCreationHandling.Replace)]
    public GridToolbarActionList ToolbarActions { get;}

    [DataMember(Name = "formToolbarActions")]
    [JsonProperty(ObjectCreationHandling = ObjectCreationHandling.Replace)]
    public FormToolbarActionList FormToolbarActions { get; }

    [DataMember(Name="gridActions")]
    [JsonProperty(ObjectCreationHandling = ObjectCreationHandling.Replace)]
    public GridTableActionList GridActions { get; }

    public FormElementOptions()
    {
        Grid = new GridUI();
        Form = new FormUI();
        ToolbarActions = new GridToolbarActionList();
        FormToolbarActions = new FormToolbarActionList();
        GridActions = new GridTableActionList();
    }
}