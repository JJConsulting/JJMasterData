using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Hangfire;
using Hangfire.Common;
using Hangfire.States;
using Hangfire.Storage;
using JJMasterData.Commons.Background;
using Microsoft.Extensions.Options;

//Conflito com o Hangfire
using IBackgroundJobClient = JJMasterData.Commons.Background.IBackgroundJobClient;

namespace JJMasterData.Hangfire;

public sealed class HangfireBackgroundJobClient(IOptions<BackgroundJobOptions> options) : IBackgroundJobClient
{
    internal const string HangfireIdField = "HangfireId";
    private const string UserField = "UserId";
    private const string ProgressParameter = "JJMasterData.Progress";
    private const string ResultParameter = "JJMasterData.Result";
    private const string ResultTypeParameter = "JJMasterData.ResultType";
    internal const string StartedAtParameter = "JJMasterData.StartedAt";
    private readonly Func<JobStorage> _getStorage = static () => JobStorage.Current;
    private readonly TimeSpan _completedJobRetention = options.Value.CompletedJobRetention;

    public ValueTask<Guid> EnqueueAsync<TRequest>(
        TRequest request,
        CancellationToken cancellationToken = default) where TRequest : BackgroundJobRequest
    {
        ArgumentNullException.ThrowIfNull(request);
        cancellationToken.ThrowIfCancellationRequested();

        var publicId = request.Id ?? Guid.NewGuid();
        using (var validationConnection = _getStorage().GetConnection())
        {
            var mapping = validationConnection.GetAllEntriesFromHash(GetMappingKey(publicId));
            if (mapping is not null && mapping.TryGetValue(UserField, out var mappedUser) &&
                !string.Equals(mappedUser, request.UserId, StringComparison.Ordinal))
                throw new InvalidOperationException("The requested background job identifier is already in use.");
        }

        var current = GetStatus(publicId, request.UserId);
        if (current?.State is BackgroundJobState.Queued or BackgroundJobState.Running)
            return ValueTask.FromResult(publicId);

        var hangfireId = BackgroundJob.Enqueue<HangfireJobExecutor<TRequest>>(
            executor => executor.ExecuteAsync(publicId, request, null!, CancellationToken.None));
        using var connection = _getStorage().GetConnection();
        connection.SetRangeInHash(GetMappingKey(publicId), new Dictionary<string, string>
        {
            [HangfireIdField] = hangfireId,
            [UserField] = request.UserId
        });
        return ValueTask.FromResult(publicId);
    }

    public BackgroundJobSnapshot? GetStatus(Guid id, string userId)
    {
        using var connection = _getStorage().GetConnection();
        var mapping = connection.GetAllEntriesFromHash(GetMappingKey(id));
        if (mapping is null || !mapping.TryGetValue(HangfireIdField, out var hangfireId) ||
            !mapping.TryGetValue(UserField, out var mappedUser) ||
            !string.Equals(mappedUser, userId, StringComparison.Ordinal))
            return null;
        var job = connection.GetJobData(hangfireId);
        if (job is null)
            return null;

        var state = connection.GetStateData(hangfireId);
        var mappedState = MapState(state?.Name);
        var stateDate = GetStateDate(state);
        var startedAt = GetParameter<DateTimeOffset?>(connection, hangfireId, StartedAtParameter);
        var completedAt = mappedState is BackgroundJobState.Succeeded or BackgroundJobState.Failed or
            BackgroundJobState.Cancelled ? stateDate : null;
        string? error = null;
        state?.Data.TryGetValue("ExceptionMessage", out error);

        return new BackgroundJobSnapshot
        {
            Id = id,
            UserId = userId,
            State = mappedState,
            CreatedAt = job.CreatedAt,
            StartedAt = startedAt,
            CompletedAt = completedAt,
            Progress = GetParameter<BackgroundJobProgress>(connection, hangfireId, ProgressParameter),
            Result = GetResult(connection, hangfireId),
            Error = error
        };
    }

    public bool Cancel(Guid id, string userId)
    {
        using var connection = _getStorage().GetConnection();
        var mapping = connection.GetAllEntriesFromHash(GetMappingKey(id));
        if (mapping is null || !mapping.TryGetValue(HangfireIdField, out var hangfireId) ||
            !mapping.TryGetValue(UserField, out var mappedUser) ||
            !string.Equals(mappedUser, userId, StringComparison.Ordinal))
            return false;
        var cancelled = BackgroundJob.Delete(hangfireId);
        if (cancelled)
            ScheduleMappingRemoval(id, hangfireId, _completedJobRetention);
        return cancelled;
    }

    internal static string GetMappingKey(Guid id) => $"jjmasterdata:background-job:{id:N}";

    internal static void ScheduleMappingRemoval(Guid id, string hangfireId, TimeSpan retention) =>
        BackgroundJob.Schedule(() => HangfireMappingCleaner.Delete(id, hangfireId), retention);

    private static object? GetResult(IStorageConnection connection, string jobId)
    {
        var serializedType = GetParameter<string>(connection, jobId, ResultTypeParameter);
        var serializedResult = connection.GetJobParameter(jobId, ResultParameter);
        if (serializedType is null || serializedResult is null)
            return null;
        var resultType = Type.GetType(serializedType, throwOnError: false);
        return resultType is null
            ? null
            : SerializationHelper.Deserialize(serializedResult, resultType, SerializationOption.User);
    }

    private static T? GetParameter<T>(IStorageConnection connection, string jobId, string name)
    {
        var value = connection.GetJobParameter(jobId, name);
        return value is null ? default : SerializationHelper.Deserialize<T>(value, SerializationOption.User);
    }

    private static DateTimeOffset? GetStateDate(StateData? state)
    {
        if (state is null)
            return null;
        foreach (var key in new[] { "StartedAt", "SucceededAt", "FailedAt", "DeletedAt", "EnqueuedAt", "ScheduledAt" })
        {
            if (state.Data.TryGetValue(key, out var value) &&
                DateTimeOffset.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var date))
                return date;
        }
        return null;
    }

    private static BackgroundJobState MapState(string? state)
    {
        if (state == ProcessingState.StateName)
            return BackgroundJobState.Running;
        if (state == SucceededState.StateName)
            return BackgroundJobState.Succeeded;
        if (state == FailedState.StateName)
            return BackgroundJobState.Failed;
        if (state == DeletedState.StateName)
            return BackgroundJobState.Cancelled;
        return BackgroundJobState.Queued;
    }
}
