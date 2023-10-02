using System;
using System.Runtime.Serialization;

namespace JJMasterData.Core.DataManager.Models;

/// <summary>
/// Record used to send data to the client.
/// </summary>
[Serializable]
[DataContract]
public record DataItemResult(string Id, string Name)
{
    [DataMember(Name = "id")]
    public string Id { get; set; } = Id;

    [DataMember(Name = "name")]
    public string Name { get; set; } = Name;
}