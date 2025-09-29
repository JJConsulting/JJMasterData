// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using JJMasterData.Commons.Exceptions;

namespace JJMasterData.Commons.Logging;

/// <summary>
/// A provider of <see cref="BatchingLogger"/> instances.
/// </summary>
[Obsolete("Please use Serilog sinks.")]
public abstract class BatchingLoggerProvider : ILoggerProvider, ISupportExternalScope
{
    private readonly List<LogMessage> _currentBatch = [];
    private readonly TimeSpan _interval;
    private readonly int? _queueSize;
    private readonly int? _batchSize;
    private readonly IDisposable _optionsChangeToken;

    private int _messagesDropped;

    private BlockingCollection<LogMessage> _messageQueue;
    private Task _outputTask;
    private CancellationTokenSource _cancellationTokenSource;

    private bool _includeScopes;
    private IExternalScopeProvider _scopeProvider;

    internal IExternalScopeProvider ScopeProvider => _includeScopes ? _scopeProvider : null;

    internal BatchingLoggerProvider(IOptionsMonitor<BatchingLoggerOptions> options)
    {
        var loggerOptions = options.CurrentValue;
        if (loggerOptions.BatchSize <= 0)
        {
            throw new JJMasterDataException($"{nameof(loggerOptions.BatchSize)} must be a positive number.");
        }

        if (loggerOptions.FlushPeriod <= TimeSpan.Zero)
        {
            throw new JJMasterDataException($"{nameof(loggerOptions.FlushPeriod)} must be longer than zero.");
        }

        _interval = loggerOptions.FlushPeriod;
        _batchSize = loggerOptions.BatchSize;
        _queueSize = loggerOptions.BackgroundQueueSize;

        _optionsChangeToken = options.OnChange(UpdateOptions);
        UpdateOptions(options.CurrentValue);
    }

    /// <summary>
    /// Checks if the queue is enabled.
    /// </summary>
    public bool IsEnabled { get; private set; }

    private void UpdateOptions(BatchingLoggerOptions options)
    {
        var oldIsEnabled = IsEnabled;
        IsEnabled = options.IsEnabled;
        _includeScopes = options.IncludeScopes;

        if (oldIsEnabled != IsEnabled)
        {
            if (IsEnabled)
            {
                Start();
            }
            else
            {
                Stop();
            }
        }
    }

    protected abstract Task WriteMessagesAsync(List<LogMessage> messages, CancellationToken cancellationToken);

    private async Task ProcessLogQueue()
    {
        while (!_cancellationTokenSource.IsCancellationRequested)
        {
            var limit = _batchSize ?? int.MaxValue;

            while (limit > 0 && _messageQueue.TryTake(out var message))
            {
                _currentBatch.Add(message);
                limit--;
            }

            Interlocked.Exchange(ref _messagesDropped, 0);
            
            if (_currentBatch.Count > 0)
            {
                try
                {
                    await WriteMessagesAsync(_currentBatch, _cancellationTokenSource.Token).ConfigureAwait(false);
                }
                catch
                {
                    // ignored
                }

                _currentBatch.Clear();
            }
            else
            {
                await IntervalAsync(_interval, _cancellationTokenSource.Token).ConfigureAwait(false);
            }
        }
    }

    /// <summary>
    /// Wait for the given <see cref="TimeSpan"/>.
    /// </summary>
    /// <param name="interval">The amount of time to wait.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> that can be used to cancel the delay.</param>
    /// <returns>A <see cref="Task"/> which completes when the <paramref name="interval"/> has passed or the <paramref name="cancellationToken"/> has been canceled.</returns>
    private static Task IntervalAsync(TimeSpan interval, CancellationToken cancellationToken)
    {
        return Task.Delay(interval, cancellationToken);
    }

    internal void AddMessage(LogMessage logMessage)
    {
        if (_messageQueue.IsAddingCompleted)
            return;
        try
        {
            if (!_messageQueue.TryAdd(logMessage, millisecondsTimeout: 0,
                    cancellationToken: _cancellationTokenSource.Token))
            {
                Interlocked.Increment(ref _messagesDropped);
            }
        }
        catch
        {
            //cancellation token canceled or CompleteAdding called
        }
    }

    private void Start()
    {
        _messageQueue = _queueSize == null
            ? new BlockingCollection<LogMessage>(new ConcurrentQueue<LogMessage>())
            : new BlockingCollection<LogMessage>(new ConcurrentQueue<LogMessage>(), _queueSize.Value);

        _cancellationTokenSource = new CancellationTokenSource();
        _outputTask = Task.Run(ProcessLogQueue);
    }

    private void Stop()
    {
        _cancellationTokenSource.Cancel();
        _messageQueue.CompleteAdding();

        try
        {
            _outputTask.Wait(_interval);
        }
        catch (TaskCanceledException)
        {
        }
        catch (AggregateException ex) when (ex.InnerExceptions is [TaskCanceledException])
        {
        }
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        _optionsChangeToken?.Dispose();
        if (IsEnabled)
        {
            Stop();
        }
    }

    /// <summary>
    /// Creates a <see cref="BatchingLogger"/> with the given <paramref name="categoryName"/>.
    /// </summary>
    /// <param name="categoryName">The name of the category to create this logger with.</param>
    /// <returns>The <see cref="BatchingLogger"/> that was created.</returns>
    public ILogger CreateLogger(string categoryName)
    {
        return new BatchingLogger(this, categoryName);
    }

    /// <summary>
    /// Sets the scope on this provider.
    /// </summary>
    /// <param name="scopeProvider">Provides the scope.</param>
    void ISupportExternalScope.SetScopeProvider(IExternalScopeProvider scopeProvider)
    {
        _scopeProvider = scopeProvider;
    }
}