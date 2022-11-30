using System.Runtime.Serialization;

namespace JJMasterData.Api.Models;

[Serializable]
[DataContract]
public class DicSyncInfoElement
{
    /// <summary>
    /// Dicionary Name
    /// </summary>
    [DataMember(Name = "name")]
    public string? Name { get; set; }

    /// <summary>
    /// Count command return
    /// </summary>
    [DataMember(Name = "recordSize")]
    public int RecordSize { get; set; }

    /// <summary>
    /// Executing time in milliseconds
    /// </summary>
    [DataMember(Name = "processMilliseconds")]
    public double ProcessMilliseconds { get; set; }
}