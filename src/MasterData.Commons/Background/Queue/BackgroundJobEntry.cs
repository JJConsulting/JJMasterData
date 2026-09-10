#nullable enable
using System;
using System.Threading;

namespace JJMasterData.Commons.Background.Queue;

internal sealed class BackgroundJobEntry(Guid id, string userId)
{
    private readonly Lock _lock = new();
    private readonly CancellationTokenSource _cancellation = new();
    private readonly DateTimeOffset _createdAt = DateTimeOffset.UtcNow;
    private BackgroundJobState _state = BackgroundJobState.Queued;
    private DateTimeOffset? _startedAt;
    private DateTimeOffset? _completedAt;
    private BackgroundJobProgress? _progress;
    private object? _result;
    private string? _error;

    public string UserId { get; } = userId;
    public CancellationToken CancellationToken => _cancellation.Token;

    public bool TryStart()
    {
        lock (_lock)
        {
            if (_state != BackgroundJobState.Queued)
                return false;
            _state = BackgroundJobState.Running;
            _startedAt = DateTimeOffset.UtcNow;
            return true;
        }
    }

    public void Report(BackgroundJobProgress progress)
    {
        lock (_lock)
        {
            if (_state != BackgroundJobState.Queued)
                _progress = progress;
        }
    }

    public void Succeed(object? result)
    {
        lock (_lock)
        {
            if (_state != BackgroundJobState.Running)
                return;
            _state = BackgroundJobState.Succeeded;
            _result = result;
            _completedAt = DateTimeOffset.UtcNow;
        }
    }

    public void Fail(Exception exception)
    {
        lock (_lock)
        {
            if (_state is BackgroundJobState.Succeeded or BackgroundJobState.Cancelled)
                return;
            _state = BackgroundJobState.Failed;
            _error = exception.Message;
            _completedAt = DateTimeOffset.UtcNow;
        }
    }

    public bool Cancel()
    {
        lock (_lock)
        {
            if (_state is BackgroundJobState.Succeeded or BackgroundJobState.Failed or BackgroundJobState.Cancelled)
                return false;
            _cancellation.Cancel();
            if (_state == BackgroundJobState.Queued)
                MarkCancelledUnsafe();
            return true;
        }
    }

    public void MarkCancelled()
    {
        lock (_lock)
            MarkCancelledUnsafe();
    }

    private void MarkCancelledUnsafe()
    {
        _state = BackgroundJobState.Cancelled;
        _completedAt = DateTimeOffset.UtcNow;
    }

    public BackgroundJobSnapshot Snapshot()
    {
        lock (_lock)
        {
            return new BackgroundJobSnapshot
            {
                Id = id,
                UserId = UserId,
                State = _state,
                CreatedAt = _createdAt,
                StartedAt = _startedAt,
                CompletedAt = _completedAt,
                Progress = _progress,
                Result = _result,
                Error = _error
            };
        }
    }
}
