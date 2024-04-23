#nullable enable

using System;
using System.Diagnostics.CodeAnalysis;

namespace JJMasterData.Core.DataManager.Models;

public class DataQuery
{
    public required Guid? ConnectionId { get; set; }
    public required FormStateData FormStateData { get; set; }
    public string? SearchText { get; set; }
    public string? SearchId { get; set; }

    [SetsRequiredMembers]
    public DataQuery(FormStateData formStateData, Guid? connectionId)
    {
        FormStateData = formStateData;
        ConnectionId = connectionId;
    }
}