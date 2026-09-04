using System;
using System.Threading;
using System.Threading.Tasks;
using Hangfire;
using Hangfire.Common;
using Hangfire.Server;
using JJMasterData.Commons.Background;
using Microsoft.Extensions.Options;

namespace JJMasterData.Hangfire;

public sealed class HangfireJobExecutor<TRequest>(
    BackgroundJobHandler<TRequest> handler,
    IOptions<BackgroundJobOptions> options)
    where TRequest : BackgroundJobRequest
{
    private const string ProgressParameter = "JJMasterData.Progress";
    private const string ResultParameter = "JJMasterData.Result";
    private const string ResultTypeParameter = "JJMasterData.ResultType";

    [AutomaticRetry(Attempts = 0)]
    public async Task ExecuteAsync(
        Guid publicId,
        TRequest request,
        PerformContext context,
        CancellationToken cancellationToken)
    {
        context.Connection.SetJobParameter(
            context.BackgroundJob.Id,
            HangfireBackgroundJobClient.StartedAtParameter,
            SerializationHelper.Serialize(DateTimeOffset.UtcNow, SerializationOption.User));

        var jobId = context.BackgroundJob.Id;
        var progress = new Progress<BackgroundJobProgress>(value =>
        {
            using var connection = JobStorage.Current.GetConnection();
            connection.SetJobParameter(
                jobId,
                ProgressParameter,
                SerializationHelper.Serialize(value, SerializationOption.User));
        });

        try
        {
            var result = await handler.ExecuteAsync(request, progress, cancellationToken);
            if (result is null)
                return;

            context.Connection.SetJobParameter(
                context.BackgroundJob.Id,
                ResultParameter,
                SerializationHelper.Serialize(result, result.GetType(), SerializationOption.User));
            context.Connection.SetJobParameter(
                context.BackgroundJob.Id,
                ResultTypeParameter,
                SerializationHelper.Serialize(result.GetType().AssemblyQualifiedName, SerializationOption.User));
        }
        finally
        {
            HangfireBackgroundJobClient.ScheduleMappingRemoval(
                publicId, context.BackgroundJob.Id, options.Value.CompletedJobRetention);
        }
    }
}

public static class HangfireMappingCleaner
{
    [AutomaticRetry(Attempts = 3)]
    public static void Delete(Guid publicId, string hangfireId)
    {
        using var connection = JobStorage.Current.GetConnection();
        var mapping = connection.GetAllEntriesFromHash(HangfireBackgroundJobClient.GetMappingKey(publicId));
        if (mapping is null || !mapping.TryGetValue(HangfireBackgroundJobClient.HangfireIdField, out var currentHangfireId) ||
            !string.Equals(currentHangfireId, hangfireId, StringComparison.Ordinal))
            return;

        using var transaction = connection.CreateWriteTransaction();
        transaction.RemoveHash(HangfireBackgroundJobClient.GetMappingKey(publicId));
        transaction.Commit();
    }
}
