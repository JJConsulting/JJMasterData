namespace JJMasterData.Core.DataManager.Exportation;

public sealed class ExportProgress(long processed, long? total, string message)
{
    public long Processed { get; } = processed;
    public long? Total { get; } = total;
    public string Message { get; } = message;
    public int Percentage => Total > 0 ? (int)System.Math.Min(100, Processed * 100 / Total.Value) : 0;
}
