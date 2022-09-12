using JJMasterData.Commons.DI;

namespace JJMasterData.Hangfire;
public static class Extensions
{
    public static JJServiceBuilder WithHangfire(this JJServiceBuilder builder)
    {
        builder.WithBackgroundTask<BackgroundTask>();
        return builder;
    }
}