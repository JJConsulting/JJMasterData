using JJMasterData.Core.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace JJMasterData.Core.Test;

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
    private static void ConfigureServices(HostBuilderContext host, IServiceCollection services)
    {
        services.AddJJMasterDataCore()
            .WithFileSystemDataDictionary();

        services.BuildServiceProvider();
    }
}