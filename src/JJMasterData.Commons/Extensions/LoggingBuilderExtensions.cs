using JJMasterData.Commons.Logging.Db;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Configuration;

namespace JJMasterData.Commons.Extensions;

public static class LoggingBuilderExtensions
{
    public static ILoggingBuilder AddDbLoggerProvider(this ILoggingBuilder builder)
    {
        builder.Services.AddSingleton<ILoggerProvider, DbLoggerProvider>();
        LoggerProviderOptions.RegisterProviderOptions<DbLoggerOptions, DbLoggerProvider>(builder.Services);
        return builder;
    }
}