using System.Runtime.Serialization;

namespace JJMasterData.Core.DataManager.Exports;

internal record DataExpDto
{
    [DataMember(Name = "StartDate")]
    public string StartDate { get; set; }

    [DataMember(Name = "IsProcessing")]
    public bool IsProcessing { get; set; }

    [DataMember(Name = "Message")]
    public string Message { get; set; }

    [DataMember(Name = "FinishedMessage")]
    public string FinishedMessage { get; set; }

    [DataMember(Name = "HasError")]
    public bool HasError { get; set; }

    [DataMember(Name = "PercentProcess")]
    public int PercentProcess { get; set; }
}
