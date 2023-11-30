using JJMasterData.Commons.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace JJMasterData.Hangfire;
public static class MasterDataServiceBuilderExtensions
{
    public static MasterDataServiceBuilder WithHangfire(this MasterDataServiceBuilder builder)
    {
        builder.WithBackgroundTaskManager<BackgroundTaskManager>();
        return builder;
    }
}