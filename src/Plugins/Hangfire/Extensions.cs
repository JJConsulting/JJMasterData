using JJMasterData.Commons.Configuration;

namespace JJMasterData.Hangfire;
public static class Extensions
{
    public static JJMasterDataServiceBuilder WithHangfire(this JJMasterDataServiceBuilder builder)
    {
        builder.WithBackgroundTask<BackgroundTask>();
        return builder;
    }
}