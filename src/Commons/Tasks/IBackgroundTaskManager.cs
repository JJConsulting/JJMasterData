using JJMasterData.Commons.Tasks.Progress;

namespace JJMasterData.Commons.Tasks;

public interface IBackgroundTaskManager
{
    void Abort(string key);
    T GetProgress<T>(string key) where T : IProgressReporter;
    bool IsRunning(string key);
    void Run(string key, IBackgroundTaskWorker worker);
}