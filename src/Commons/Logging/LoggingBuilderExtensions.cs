using JJMasterData.Commons.Logging.Db;
using JJMasterData.Commons.Logging.File;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Configuration;

namespace JJMasterData.Commons.Logging;

public static class LoggingBuilderExtensions
{
    public static ILoggingBuilder AddDbLoggerProvider(this ILoggingBuilder builder)
    {
        var services = builder.Services;
        
        services.TryAddEnumerable(ServiceDescriptor.Singleton<ILoggerProvider, DbLoggerProvider>());

        LoggerProviderOptions.RegisterProviderOptions<DbLoggerOptions, DbLoggerProvider>(builder.Services);
        
        return builder;
    }
    
    public static ILoggingBuilder AddFileLoggerProvider(this ILoggingBuilder builder)
    {
        var services = builder.Services;

        services.TryAddEnumerable(ServiceDescriptor.Singleton<ILoggerProvider, FileLoggerProvider>());

        LoggerProviderOptions.RegisterProviderOptions<FileLoggerOptions, FileLoggerProvider>(builder.Services);
        
        return builder;
    }
}