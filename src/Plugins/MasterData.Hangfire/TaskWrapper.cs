using JJMasterData.Commons.Tasks;
using JJMasterData.Commons.Tasks.Progress;

namespace JJMasterData.Hangfire;

internal sealed class TaskWrapper
{
    public string Key { get; set; }

    public string JobId { get; set; }

    public IBackgroundTaskWorker TaskWorker { get; set; }

    public IProgressReporter ProgressResult { get; set; }

}
