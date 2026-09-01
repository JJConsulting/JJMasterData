#nullable enable
namespace JJMasterData.Commons.Background;

public sealed class BackgroundJobProgress(int percentage, string message, object? details = null)
{
    public int Percentage { get; } = percentage;
    public string Message { get; } = message;
    public object? Details { get; } = details;
}