#nullable enable
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;

namespace JJMasterData.Commons.Background.Queue;

internal sealed class BackgroundJobQueue : IBackgroundJobClient
{
    private readonly Channel<IBackgroundJobWorkItem> _channel;
    private readonly ConcurrentDictionary<Guid, BackgroundJobEntry> _jobs = new();
    private readonly BackgroundJobOptions _options;

    public BackgroundJobQueue(IOptions<BackgroundJobOptions> options)
    {
        _options = options.Value;
        if (_options.Capacity <= 0)
            throw new ArgumentOutOfRangeException(nameof(options), "Background job capacity must be greater than zero.");
        if (_options.MaxConcurrency <= 0)
            throw new ArgumentOutOfRangeException(nameof(options), "Background job concurrency must be greater than zero.");

        _channel = Channel.CreateBounded<IBackgroundJobWorkItem>(new BoundedChannelOptions(_options.Capacity)
        {
            FullMode = BoundedChannelFullMode.Wait,
            SingleReader = _options.MaxConcurrency == 1,
            SingleWriter = false
        });
    }

    internal ChannelReader<IBackgroundJobWorkItem> Reader => _channel.Reader;
    internal int MaxConcurrency => _options.MaxConcurrency;
    public ValueTask<Guid> EnqueueAsync<TRequest>(
        TRequest request,
        CancellationToken cancellationToken = default) where TRequest : BackgroundJobRequest
    {
        ArgumentNullException.ThrowIfNull(request);
        cancellationToken.ThrowIfCancellationRequested();
        CleanupExpired();

        var id = Guid.NewGuid();
        var entry = new BackgroundJobEntry(id, request.UserId);
        if (!_jobs.TryAdd(id, entry))
            throw new InvalidOperationException("Unable to allocate a background job identifier.");

        if (!_channel.Writer.TryWrite(new BackgroundJobWorkItem<TRequest>(id, request)))
        {
            _jobs.TryRemove(id, out _);
            throw new BackgroundJobQueueFullException("The background job queue is full.");
        }

        return ValueTask.FromResult(id);
    }

    public BackgroundJobSnapshot? GetStatus(Guid id, string userId)
    {
        CleanupExpired();
        return _jobs.TryGetValue(id, out var entry) && entry.UserId == userId ? entry.Snapshot() : null;
    }

    public bool Cancel(Guid id, string userId)
    {
        if (!_jobs.TryGetValue(id, out var entry) || entry.UserId != userId)
            return false;

        return entry.Cancel();
    }

    internal bool TryGetEntry(Guid id, out BackgroundJobEntry entry) => _jobs.TryGetValue(id, out entry!);
    internal void Complete() => _channel.Writer.TryComplete();

    internal void CancelQueuedJobs()
    {
        while (_channel.Reader.TryRead(out var workItem))
        {
            if (_jobs.TryGetValue(workItem.Id, out var entry))
                entry.MarkCancelled();
        }
    }

    private void CleanupExpired()
    {
        var threshold = DateTimeOffset.UtcNow - _options.CompletedJobRetention;
        foreach (var pair in _jobs)
        {
            var snapshot = pair.Value.Snapshot();
            if (snapshot.CompletedAt < threshold)
                _jobs.TryRemove(pair.Key, out _);
        }
    }
}