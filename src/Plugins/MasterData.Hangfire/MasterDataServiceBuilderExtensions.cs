using JJMasterData.Commons.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace JJMasterData.Hangfire;

public static class MasterDataServiceBuilderExtensions
{
    public static MasterDataServiceBuilder WithHangfire(this MasterDataServiceBuilder builder)
    {
        builder.WithBackgroundJobClient<HangfireBackgroundJobClient>();
        builder.Services.TryAddTransient(typeof(HangfireJobExecutor<>));
        return builder;
    }
}
