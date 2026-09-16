# Background Jobs

Import and export operations run as background jobs. By default an in-memory queue (`BackgroundJobQueue`) is consumed
by a .NET `BackgroundService`. Each submission returns a job identifier; status and cancellation calls must include
both that identifier and the current user. Queued jobs and status records do not survive an application restart, and
generated files remain in the configured `IFileStorage`.

## Interface

Jobs are submitted through `IBackgroundJobClient`:

```csharp
public interface IBackgroundJobClient
{
    ValueTask<Guid> EnqueueAsync<TRequest>(
        TRequest request,
        CancellationToken cancellationToken = default) where TRequest : BackgroundJobRequest;

    BackgroundJobSnapshot? GetStatus(Guid id, string userId);

    bool Cancel(Guid id, string userId);
}
```

Requests derive from `BackgroundJobRequest`, which carries an optional `Id` and the `UserId` that owns the job.
Handlers derive from `BackgroundJobHandler<TRequest>` and receive progress reporting and cancellation:

```csharp
public abstract class BackgroundJobHandler<TRequest> where TRequest : BackgroundJobRequest
{
    public abstract Task<object?> ExecuteAsync(
        TRequest request,
        IProgress<BackgroundJobProgress> progress,
        CancellationToken cancellationToken);
}
```

`GetStatus` returns a `BackgroundJobSnapshot` with the job `State` (`Queued`, `Running`, `Succeeded`, `Failed`, or
`Cancelled`), timestamps, progress, result, and error.

### Deterministic identifiers

Import and export generate the job id before enqueueing with
`BackgroundJobId.Create(operation, elementName, userId)`, so each form has at most one active job per user. If a job
with the same id is already queued or running, `EnqueueAsync` returns the existing id instead of creating a new job.

## Configuration

Configure capacity, concurrency, and completed-job retention under `JJMasterData:BackgroundJobs`:

```json
{
  "JJMasterData": {
    "BackgroundJobs": {
      "Capacity": 1000,
      "MaxConcurrency": 100,
      "CompletedJobRetention": "01:00:00"
    }
  }
}
```

## See also

Replace the in-memory queue with a durable backend using the [Hangfire plugin](plugins/hangfire.md).
