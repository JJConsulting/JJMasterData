using JJMasterData.Commons.Configuration;

namespace JJMasterData.Hangfire;
public static class MasterDataServiceBuilderExtensions
{
    public static MasterDataServiceBuilder WithHangfire(this MasterDataServiceBuilder builder)
    {
        builder.WithBackgroundTaskManager<BackgroundTaskManager>();
        return builder;
    }
}