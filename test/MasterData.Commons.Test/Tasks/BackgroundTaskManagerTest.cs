
using JJMasterData.Commons.Tasks;
using JJMasterData.Commons.Tasks.Progress;

namespace JJMasterData.Commons.Test.Tasks;

public class TaskWorkerTest : IBackgroundTaskWorker
{
    public event EventHandler<IProgressReporter>? OnProgressChanged;

    public Task RunWorkerAsync(CancellationToken cancellationToken)
    {
        return Task.Run(() =>
        {
            var reporter = new ProgressReporter();
            for (int i = 0; i < 10; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                Console.WriteLine(@"Running Worker...");
                reporter.Percentage = i * 10;
                OnProgressChanged?.Invoke(this, new ProgressReporter());
                Task.Delay(50, cancellationToken).Wait(cancellationToken);
            }
        }, cancellationToken);
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
        var manager = BackgroundTaskManager;
        var exception = Record.Exception(() => manager.Run("RunTaskTest", Worker));
        manager.Abort("RunTaskTest");
        Assert.Null(exception);
    }

    [Fact]
    public void TaskIsNotRunningTest()
    {
        var manager = BackgroundTaskManager;
        Assert.False(manager.IsRunning("NonExistentTask"));
    }
        
    [Fact(Timeout=3000)]
    public async Task GetProgressTest()
    {
        const string key = "TestProgressTask";
        var manager = BackgroundTaskManager;
        manager.Run(key, Worker);
        ProgressReporter? progress = null;
        while (progress == null)
        {
            progress = manager.GetProgress<ProgressReporter>(key);
            await Task.Delay(10, TestContext.Current.CancellationToken);
        }

        manager.Abort(key);
        Assert.NotNull(progress);
    }
           
        
    [Fact]
    public async Task AbortTest()
    {
        const string key = "TaskToBeAborted";
        var manager = BackgroundTaskManager;
        manager.Run(key, Worker);
        manager.Abort(key);

        //Task needs a delay to cancel itself.
        await Task.Delay(100, TestContext.Current.CancellationToken);
        Assert.False(manager.IsRunning(key));
    }
}
