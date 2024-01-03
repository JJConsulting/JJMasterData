using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using JJMasterData.Commons.Exceptions;
using JJMasterData.Commons.Tasks.Progress;

namespace JJMasterData.Commons.Tasks;

internal sealed class BackgroundTaskManager : IBackgroundTaskManager
{
    private readonly List<TaskWrapper> _tasks = [];

    private TaskWrapper GetTask(string key)
    {
        return _tasks.Find(x => x.Key.Equals(key));
    }

    public void Run(string key, IBackgroundTaskWorker worker)
    {
        if (IsRunning(key))
            throw new JJMasterDataException("Background task is already running.");

        var cancellationSource = new CancellationTokenSource();

        var taskWrapper = new TaskWrapper
        {
            Key = key,
            CancellationSource = cancellationSource
        };
        
        worker.OnProgressChanged += (_, e) => taskWrapper.ProgressResult = e;

        taskWrapper.Task = new Task(() => worker.RunWorkerAsync(cancellationSource.Token));

        var olderTask = GetTask(key);
        if (olderTask != null)
            _tasks.Remove(olderTask);

        _tasks.Add(taskWrapper);

        taskWrapper.Task.Start();
    }

    public bool IsRunning(string key)
    {
        var taskWrapper = GetTask(key);
        if (taskWrapper == null)
            return false;

        return taskWrapper.Task.Status == TaskStatus.Running |
            taskWrapper.Task.Status == TaskStatus.WaitingForActivation;
    }

    public void Abort(string key)
    {
        var taskWrapper = GetTask(key);
        
        taskWrapper?.CancellationSource.Cancel();
    }

    public T GetProgress<T>(string key) where T : IProgressReporter
    {
        var taskWrapper = GetTask(key);
        if (taskWrapper != null)
            return (T)taskWrapper.ProgressResult;

        return default;
    }
}
