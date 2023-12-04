using System.Collections.Generic;
using Hangfire;
using Hangfire.States;
using JJMasterData.Commons.Exceptions;
using JJMasterData.Commons.Localization;
using JJMasterData.Commons.Tasks;
using JJMasterData.Commons.Tasks.Progress;
using Microsoft.Extensions.Localization;

namespace JJMasterData.Hangfire;

public sealed class BackgroundTaskManager: IBackgroundTaskManager
{
    internal static List<TaskWrapper> Tasks { get; }= new();

    internal static TaskWrapper GetTask(string key)
    {
        return Tasks.Find(x => x.Key.Equals(key));
    }

    
    public void Run(string key, IBackgroundTaskWorker worker)
    {
        if (IsRunning(key))
            throw new JJMasterDataException("Background task is already running.");

        var taskWrapper = new TaskWrapper
        {
            Key = key,
            TaskWorker = worker
        };

        var olderPipeline = GetTask(key);
        if (olderPipeline != null)
            Tasks.Remove(olderPipeline);

        //Workaround: Interfaces are not a good idea with Hangfire.
        var taskTrigger = new TaskTrigger();
        taskWrapper.JobId = taskTrigger.RunInBackground(key,worker);

        Tasks.Add(taskWrapper);
    }

    public bool IsRunning(string key)
    {
        var taskWrapper = GetTask(key);

        if (taskWrapper == null)
            return false;

        var connection = JobStorage.Current.GetConnection();
        var jobData = connection.GetJobData(taskWrapper.JobId);

        return jobData.State.Equals(ProcessingState.StateName);
    }

    public void Abort(string key)
    {
        var taskWrapper = GetTask(key);
        if (taskWrapper == null)
            return;

        BackgroundJob.Delete(taskWrapper.JobId);
    }

    public T GetProgress<T>(string key) where T : IProgressReporter
    {
        var task = GetTask(key);
        if (task != null)
            return (T)task.ProgressResult;

        return default;
    }

    internal static void SetProgress(string key, IProgressReporter progress)
    {
        var task = Tasks.Find(x => x.Key.Equals(key));
        if (task != null)
            task.ProgressResult = progress;
    }

}
