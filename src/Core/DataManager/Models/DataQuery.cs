#nullable enable

using System;
using System.Diagnostics.CodeAnalysis;

namespace JJMasterData.Core.DataManager.Models;

public class DataQuery(FormStateData formStateData, Guid? connectionId)
{
    public Guid? ConnectionId { get; } = connectionId;
    public FormStateData FormStateData { get; } = formStateData;
    public string? SearchText { get; init; }
    public string? SearchId { get; init; }
}