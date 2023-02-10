using System;
using JJMasterData.Commons.DI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.IO;
using JJMasterData.Commons.Options;

namespace JJMasterData.Commons.Extensions;

public static class JJServiceBuilderExtensions
{
    public static JJServiceBuilder AddJJMasterDataCommons(this IServiceCollection services, string filePath = "appsettings.json")
    {
        string basePath = Directory.GetCurrentDirectory();
        var builder = new JJServiceBuilder(services);
        
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

    public static JJServiceBuilder AddJJMasterDataCommons(this IServiceCollection services, IConfiguration configuration)
    {
        var builder = new JJServiceBuilder(services);
        
        builder.AddDefaultServices(configuration);
        builder.Services.Configure<JJMasterDataCommonsOptions>(configuration.GetJJMasterData());
        
        return builder;
    }
    
    public static JJServiceBuilder AddJJMasterDataCommons(this IServiceCollection services, Action<JJMasterDataCommonsOptions> configure, IConfiguration loggingConfiguration = null)
    {
        var builder = new JJServiceBuilder(services);
        
        builder.AddDefaultServices(loggingConfiguration);
        builder.Services.Configure(configure);
        
        return builder;
    }
}