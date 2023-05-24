#nullable disable
using System.Runtime.Serialization;
using JJMasterData.Core.DataDictionary;

namespace JJMasterData.FormElementUpdater.Models;

[Serializable]
[DataContract]
public class MetadataOptions
{
    [DataMember(Name = "grid")]
    public GridUI Grid { get; set; }

    [DataMember(Name = "form")]
    public FormUI Form { get; set; }

    [DataMember(Name = "toolBarActions")]
    public GridToolbarActions ToolbarActions { get; set; }

    [DataMember(Name = "gridActions")]
    public GridActions GridActions { get; set; }

    public MetadataOptions()
    {
        Grid = new GridUI();
        Form = new FormUI();
        ToolbarActions = new GridToolbarActions();
        GridActions = new GridActions();
    }
}