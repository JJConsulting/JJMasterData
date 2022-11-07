using JJMasterData.Commons.Dao;
using JJMasterData.Commons.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;

namespace JJMasterData.Commons.Test;

public class Startup
{
    public void ConfigureHost(IHostBuilder hostBuilder)
    {
        void ConfigureJsonFile(HostBuilderContext context, IConfigurationBuilder builder)
        {
            var root = Path.Join(context.HostingEnvironment.ContentRootPath,  "..","..","..","..","..");
            var sharedSettings = Path.Combine(root, "appsettings.json");
            builder.AddJsonFile(sharedSettings);
        }

        hostBuilder
            .ConfigureHostConfiguration(_ => { })
            .ConfigureAppConfiguration(ConfigureJsonFile)
            .ConfigureServices(ConfigureServices);
    }
    
    private void ConfigureServices(HostBuilderContext host, IServiceCollection services)
    {
        services.BuildServiceProvider().UseJJMasterData();
    }
}