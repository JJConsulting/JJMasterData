#nullable enable
using System;

namespace JJMasterData.Commons.Background;

public sealed class BackgroundJobProgress(int percentage, string message, object? details = null)
{
    public int Percentage { get; } = Math.Clamp(percentage, 0, 100);
    public string Message { get; } = message;
    public object? Details { get; } = details;
}
