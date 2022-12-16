using System;
using System.Configuration;
using System.IO;
using JJMasterData.Commons.DI;
using JJMasterData.Commons.Logging;
using JJMasterData.Commons.Options;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Configuration;

namespace JJMasterData.Commons.Extensions;

public static class ServiceExtensions
{
    public static JJServiceBuilder AddJJMasterDataCommons(this IServiceCollection services, string filePath = "appsettings.json")
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile(filePath, optional: false, reloadOnChange: true)
            .Build();

        var builder = new JJServiceBuilder(services);
        builder.AddDefaultServices(configuration);
        return builder;
    }

    public static JJServiceBuilder AddJJMasterDataCommons(this IServiceCollection services, IConfiguration configuration)
    {
        var builder = new JJServiceBuilder(services);
        builder.AddDefaultServices(configuration);
        return builder;
    }

    public static IServiceProvider UseJJMasterData(this IServiceProvider provider)
    {
        JJService.Provider = provider;
        return JJService.Provider;
    }

    public static IApplicationBuilder UseJJMasterData(this IApplicationBuilder app)
    {
        app.ApplicationServices.UseJJMasterData();
        return app;
    }

    public static ILoggingBuilder AddJJMasterDataLogger(this ILoggingBuilder builder)
    {
        builder.AddConfiguration();

        builder.Services.TryAddEnumerable(
            ServiceDescriptor.Singleton<ILoggerProvider, LoggerProvider>());

        LoggerProviderOptions.RegisterProviderOptions
            <JJMasterDataOptions, LoggerProvider>(builder.Services);

        return builder;
    }
}