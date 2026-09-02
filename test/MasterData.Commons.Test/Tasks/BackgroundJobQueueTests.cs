using JJMasterData.Commons.Background;
using JJMasterData.Commons.Background.Queue;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace JJMasterData.Commons.Test.Tasks;

public sealed class BackgroundJobQueueTests
{
    [Fact]
    public async Task FullQueueIsRejected()
    {
        var queue = CreateQueue(capacity: 1);
        await queue.EnqueueAsync(new TestRequest("user", "first"), TestContext.Current.CancellationToken);

        await Assert.ThrowsAsync<BackgroundJobQueueFullException>(async () =>
            await queue.EnqueueAsync(new TestRequest("user", "second"), TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task StatusAndCancellationRequireTheOwner()
    {
        var queue = CreateQueue();
        var id = await queue.EnqueueAsync(new TestRequest("owner", "value"), TestContext.Current.CancellationToken);

        Assert.Null(queue.GetStatus(id, "other"));
        Assert.False(queue.Cancel(id, "other"));
        Assert.True(queue.Cancel(id, "owner"));
        Assert.Equal(BackgroundJobState.Cancelled, queue.GetStatus(id, "owner")!.State);
    }

    [Fact]
    public async Task StatusCanBeRecoveredByTheRequestedJobIdForTheOwner()
    {
        var queue = CreateQueue();
        var requestedId = BackgroundJobId.Create("export", "customers", "owner");
        var request = new TestRequest("owner", "value") { Id = requestedId };
        var id = await queue.EnqueueAsync(request, TestContext.Current.CancellationToken);

        Assert.Equal(requestedId, id);
        Assert.Equal(id, queue.GetStatus(requestedId, "owner")!.Id);
        Assert.Null(queue.GetStatus(requestedId, "other"));
    }

    [Fact]
    public async Task ActiveRequestedJobIdDoesNotQueueTheSameOperationTwice()
    {
        var queue = CreateQueue(capacity: 2);
        var requestedId = BackgroundJobId.Create("import", "customers", "owner");
        var firstId = await queue.EnqueueAsync(
            new TestRequest("owner", "first") { Id = requestedId },
            TestContext.Current.CancellationToken);
        var secondId = await queue.EnqueueAsync(
            new TestRequest("owner", "second") { Id = requestedId },
            TestContext.Current.CancellationToken);

        Assert.Equal(firstId, secondId);
        Assert.Equal(requestedId, queue.GetStatus(requestedId, "owner")!.Id);
    }

    [Fact]
    public async Task HostedServiceCreatesAndDisposesAScopeForEveryJob()
    {
        ScopedDependency.DisposeCount = 0;
        var queue = CreateQueue();
        var services = new ServiceCollection();
        services.AddScoped<ScopedDependency>();
        services.AddScoped<BackgroundJobHandler<TestRequest>, TestHandler>();
        await using var provider = services.BuildServiceProvider();
        var worker = CreateWorker(queue, provider);

        await worker.StartAsync(TestContext.Current.CancellationToken);
        var id = await queue.EnqueueAsync(new TestRequest("owner", "result"), TestContext.Current.CancellationToken);
        var status = await WaitForCompletionAsync(queue, id);
        await worker.StopAsync(TestContext.Current.CancellationToken);

        Assert.Equal(BackgroundJobState.Succeeded, status.State);
        Assert.Equal("result", status.Result);
        Assert.Equal(100, status.Progress!.Percentage);
        Assert.Equal(1, ScopedDependency.DisposeCount);
    }

    [Fact]
    public async Task HandlerFailureMarksJobAsFailed()
    {
        var queue = CreateQueue();
        var services = new ServiceCollection();
        services.AddScoped<BackgroundJobHandler<FailingRequest>, FailingHandler>();
        await using var provider = services.BuildServiceProvider();
        var worker = CreateWorker(queue, provider);

        await worker.StartAsync(TestContext.Current.CancellationToken);
        var id = await queue.EnqueueAsync(new FailingRequest("owner"), TestContext.Current.CancellationToken);
        var status = await WaitForCompletionAsync(queue, id);
        await worker.StopAsync(TestContext.Current.CancellationToken);

        Assert.Equal(BackgroundJobState.Failed, status.State);
        Assert.Equal("expected failure", status.Error);
    }

    [Fact]
    public async Task RunningJobCanBeCancelled()
    {
        var queue = CreateQueue();
        var control = new BlockingControl();
        var services = new ServiceCollection();
        services.AddSingleton(control);
        services.AddScoped<BackgroundJobHandler<BlockingRequest>, BlockingHandler>();
        await using var provider = services.BuildServiceProvider();
        var worker = CreateWorker(queue, provider);

        await worker.StartAsync(TestContext.Current.CancellationToken);
        var id = await queue.EnqueueAsync(new BlockingRequest("owner"), TestContext.Current.CancellationToken);
        await control.Started.Task.WaitAsync(TestContext.Current.CancellationToken);
        Assert.True(queue.Cancel(id, "owner"));
        var status = await WaitForCompletionAsync(queue, id);
        await worker.StopAsync(TestContext.Current.CancellationToken);

        Assert.Equal(BackgroundJobState.Cancelled, status.State);
    }

    private static BackgroundJobService CreateWorker(BackgroundJobQueue queue, ServiceProvider provider) =>
        new(queue, provider.GetRequiredService<IServiceScopeFactory>(), NullLogger<BackgroundJobService>.Instance);

    private static BackgroundJobQueue CreateQueue(int capacity = 10) => new(Options.Create(new BackgroundJobOptions
    {
        Capacity = capacity,
        MaxConcurrency = 1,
        CompletedJobRetention = TimeSpan.FromHours(1)
    }));

    private static async Task<BackgroundJobSnapshot> WaitForCompletionAsync(
        BackgroundJobQueue queue,
        Guid id)
    {
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        while (!timeout.IsCancellationRequested)
        {
            var status = queue.GetStatus(id, "owner")!;
            if (status.State is not (BackgroundJobState.Queued or BackgroundJobState.Running))
                return status;
            await Task.Delay(10, timeout.Token);
        }
        throw new TimeoutException();
    }

    private sealed class TestRequest(string userId, string value) : BackgroundJobRequest
    {
        public override string UserId { get; init; } = userId;
        public string Value { get; init; } = value;
    }

    private sealed class FailingRequest(string userId) : BackgroundJobRequest
    {
        public override string UserId { get; init; } = userId;
    }

    private sealed class BlockingRequest(string userId) : BackgroundJobRequest
    {
        public override string UserId { get; init; } = userId;
    }

    private sealed class ScopedDependency : IDisposable
    {
        public static int DisposeCount;
        public void Dispose() => Interlocked.Increment(ref DisposeCount);
    }

    private sealed class TestHandler(ScopedDependency dependency) : BackgroundJobHandler<TestRequest>
    {
        public override Task<object?> ExecuteAsync(TestRequest request, IProgress<BackgroundJobProgress> progress,
            CancellationToken cancellationToken)
        {
            _ = dependency;
            progress.Report(new BackgroundJobProgress(100, "done"));
            return Task.FromResult<object?>(request.Value);
        }
    }

    private sealed class FailingHandler : BackgroundJobHandler<FailingRequest>
    {
        public override Task<object?> ExecuteAsync(FailingRequest request, IProgress<BackgroundJobProgress> progress,
            CancellationToken cancellationToken) => throw new InvalidOperationException("expected failure");
    }

    private sealed class BlockingControl
    {
        public TaskCompletionSource Started { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
    }

    private sealed class BlockingHandler(BlockingControl control) : BackgroundJobHandler<BlockingRequest>
    {
        public override async Task<object?> ExecuteAsync(BlockingRequest request,
            IProgress<BackgroundJobProgress> progress, CancellationToken cancellationToken)
        {
            control.Started.TrySetResult();
            await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
            return null;
        }
    }
}
