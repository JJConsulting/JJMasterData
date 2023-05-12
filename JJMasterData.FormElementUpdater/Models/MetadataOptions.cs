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
    public GridToolBarActions ToolBarActions { get; set; }

    [DataMember(Name = "gridActions")]
    public GridActions GridActions { get; set; }

    public MetadataOptions()
    {
        Grid = new GridUI();
        Form = new FormUI();
        ToolBarActions = new GridToolBarActions();
        GridActions = new GridActions();
    }
}