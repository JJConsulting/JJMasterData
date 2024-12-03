using JJMasterData.Commons.Logging.Db;
using JJMasterData.Commons.Logging.File;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Configuration;
using Microsoft.Extensions.Options;

namespace JJMasterData.Commons.Logging;

public static class LoggingBuilderExtensions
{
    
    public static ILoggingBuilder AddDbLoggerProvider(this ILoggingBuilder builder, IConfiguration configuration)
    {
        var services = builder.Services;

        services.TryAddSingleton<IOptionsChangeTokenSource<LoggerFilterOptions>>(
            new ConfigurationChangeTokenSource<LoggerFilterOptions>(configuration));

        services.TryAddEnumerable(ServiceDescriptor.Singleton<ILoggerProvider, DbLoggerProvider>());

        services.AddSingleton<IOptionsChangeTokenSource<DbLoggerOptions>>(
            new ConfigurationChangeTokenSource<DbLoggerOptions>(configuration));

        LoggerProviderOptions.RegisterProviderOptions<DbLoggerOptions, DbLoggerProvider>(builder.Services);
        
        return builder;
    }
    
    public static ILoggingBuilder AddFileLoggerProvider(this ILoggingBuilder builder, IConfiguration configuration)
    {
        var services = builder.Services;

        services.TryAddSingleton<IOptionsChangeTokenSource<LoggerFilterOptions>>(
            new ConfigurationChangeTokenSource<LoggerFilterOptions>(configuration));

        services.TryAddEnumerable(ServiceDescriptor.Singleton<ILoggerProvider, FileLoggerProvider>());
        
        services.AddSingleton<IOptionsChangeTokenSource<FileLoggerOptions>>(
            new ConfigurationChangeTokenSource<FileLoggerOptions>(configuration));

        LoggerProviderOptions.RegisterProviderOptions<FileLoggerOptions, FileLoggerProvider>(builder.Services);
        
        return builder;
    }
}