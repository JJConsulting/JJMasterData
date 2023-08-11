using System;
using System.IO;
using JJMasterData.Commons.Configuration.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace JJMasterData.Commons.Configuration;

public static class ServiceCollectionExtensions
{
    public static JJMasterDataServiceBuilder AddJJMasterDataCommons(this IServiceCollection services, string filePath = "appsettings.json")
    {
        string basePath = Directory.GetCurrentDirectory();
        var builder = new JJMasterDataServiceBuilder(services);

        if (File.Exists(Path.Combine(basePath, filePath)))
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile(filePath, optional: false, reloadOnChange: true)
                .Build();

            builder.AddDefaultServices(configuration);
            builder.Services.Configure<JJMasterDataCommonsOptions>(configuration.GetJJMasterData());
        }

        return builder;
    }

    public static JJMasterDataServiceBuilder AddJJMasterDataCommons(this IServiceCollection services, IConfiguration configuration)
    {
        var builder = new JJMasterDataServiceBuilder(services);

        builder.AddDefaultServices(configuration);
        builder.Services.Configure<JJMasterDataCommonsOptions>(configuration.GetJJMasterData());

        return builder;
    }

    public static JJMasterDataServiceBuilder AddJJMasterDataCommons(this IServiceCollection services, Action<JJMasterDataCommonsOptions> configure, IConfiguration loggingConfiguration = null)
    {
        var builder = new JJMasterDataServiceBuilder(services);

        builder.AddDefaultServices(loggingConfiguration);
        builder.Services.Configure(configure);

        return builder;
    }
}