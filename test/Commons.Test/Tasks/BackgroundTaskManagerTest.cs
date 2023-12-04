
using JJMasterData.Commons.Tasks;
using JJMasterData.Commons.Tasks.Progress;

namespace JJMasterData.Commons.Test.Tasks;

public class TaskWorkerTest : IBackgroundTaskWorker
{
    public event EventHandler<IProgressReporter>? OnProgressChanged;

    public Task RunWorkerAsync(CancellationToken token)
    {
        return Task.Run(() =>
        {
            var reporter = new ProgressReporter();
            for (int i = 0; i < 10; i++)
            {
                token.ThrowIfCancellationRequested();
                Console.WriteLine(@"Running Worker...");
                reporter.Percentage = i * 10;
                OnProgressChanged?.Invoke(this, new ProgressReporter());
                Task.Delay(1000, token).Wait(token);
            }
        }, token);
    }
}

 
public class BackgroundTaskManagerTest
{

    public static IBackgroundTaskWorker Worker => new TaskWorkerTest();

    //Implement your own IBackgroundTaskManager here if you want a specific test.
    public static IBackgroundTaskManager BackgroundTaskManager => new BackgroundTaskManager();

    [Fact]
    public void RunTaskTest()
    {
        var exception = Record.Exception(() => BackgroundTaskManager.Run("RunTaskTest", Worker));
        Assert.Null(exception);
    }

    [Fact]
    public void TaskIsNotRunningTest()
    {
        Assert.False(BackgroundTaskManager.IsRunning("NonExistentTask"));
    }
        
    [Fact(Timeout=3000)]
    public void GetProgressTest()
    {
        const string key = "TestProgressTask";
        BackgroundTaskManager.Run(key, Worker);
        ProgressReporter progress;
        do
        {
            progress = BackgroundTaskManager.GetProgress<ProgressReporter>(key);
                
        } while (progress == null);
            

        Assert.NotNull(progress);
    }
           
        
    [Fact]
    public async void AbortTest()
    {
        const string key = "TaskToBeAborted";
        BackgroundTaskManager.Run(key, Worker);
        BackgroundTaskManager.Abort(key);

        //Task needs a delay to cancel itself.
        await Task.Delay(3000);
        Assert.False(BackgroundTaskManager.IsRunning(key));
    }
}