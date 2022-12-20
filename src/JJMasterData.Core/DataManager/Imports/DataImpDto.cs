using System.Runtime.Serialization;

namespace JJMasterData.Core.DataManager.Imports;

internal record DataImpDto
{
    [DataMember(Name = "StartDate")]
    public string StartDate { get; set; }

    [DataMember(Name = "Insert")]
    public int Insert { get; set; }

    [DataMember(Name = "Update")]
    public int Update { get; set; }

    [DataMember(Name = "Delete")]
    public int Delete { get; set; }

    [DataMember(Name = "Error")]
    public int Error { get; set; }

    [DataMember(Name = "Ignore")]
    public int Ignore { get; set; }

    [DataMember(Name = "IsProcessing")]
    public bool IsProcessing { get; set; }

    [DataMember(Name = "PercentProcess")]
    public int PercentProcess { get; set; }

    [DataMember(Name = "Message")]
    public string Message { get; set; }

   
}
