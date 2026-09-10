namespace JJMasterData.Commons.Background;

public enum BackgroundJobState
{
    Queued,
    Running,
    Succeeded,
    Failed,
    Cancelled
}