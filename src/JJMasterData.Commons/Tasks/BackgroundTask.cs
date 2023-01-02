using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using JJMasterData.Commons.Exceptions;
using JJMasterData.Commons.Language;
using JJMasterData.Commons.Tasks.Progress;

namespace JJMasterData.Commons.Tasks;

internal sealed class BackgroundTask : IBackgroundTask
{
    private Lazy<List<TaskWrapper>> _taskList;
    internal List<TaskWrapper> TaskList
    {
        get
        {
            _taskList ??= new Lazy<List<TaskWrapper>>();

            return _taskList.Value;
        }
    }

    private TaskWrapper GetTask(string key)
    {
        return TaskList.Find(x => x.Key.Equals(key));
    }

    public void Run(string key, IBackgroundTaskWorker worker)
    {
        if (IsRunning(key))
            throw new JJMasterDataException(Translate.Key("Background task is already running."));

        var cancellationSource = new CancellationTokenSource();

        var taskWrapper = new TaskWrapper
        {
            Key = key,
            CancellationSource = cancellationSource
        };

        taskWrapper.Task = new Task(() => {
            worker.OnProgressChanged += (_, e) => { taskWrapper.ProgressResult = e; };
            worker.RunWorkerAsync(cancellationSource.Token).Wait();
        });

        var olderPipeline = GetTask(key);
        if (olderPipeline != null)
            TaskList.Remove(olderPipeline);

        TaskList.Add(taskWrapper);

        taskWrapper.Task.Start();
    }

    public bool IsRunning(string key)
    {
        var taskWrapper = GetTask(key);

        return taskWrapper?.Task.Status is TaskStatus.Running or TaskStatus.WaitingForActivation;
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
