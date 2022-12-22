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
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile(filePath, optional: false, reloadOnChange: true)
            .Build();

        var builder = new JJServiceBuilder(services);
        
        builder.AddDefaultServices();
        builder.Services.Configure<JJMasterDataCommonsOptions>(configuration.GetJJMasterData());
        
        return builder;
    }

    public static JJServiceBuilder AddJJMasterDataCommons(this IServiceCollection services, IConfigurationSection configuration)
    {
        var builder = new JJServiceBuilder(services);
        

        builder.AddDefaultServices();
        builder.Services.Configure<JJMasterDataCommonsOptions>(configuration);
        
        return builder;
    }
    
    public static JJServiceBuilder AddJJMasterDataCommons(this IServiceCollection services, Action<JJMasterDataCommonsOptions> configure)
    {
        var builder = new JJServiceBuilder(services);
        
        builder.AddDefaultServices();
        builder.Services.Configure(configure);
        
        return builder;
    }
}