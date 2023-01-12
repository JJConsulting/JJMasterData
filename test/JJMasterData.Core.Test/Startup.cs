using JJMasterData.Commons.Extensions;
using JJMasterData.Core.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace JJMasterData.Core.Test;

public class Startup
{
    private static string GetAppSettingsPath(HostBuilderContext context)
    {
        var root = Path.Join(context.HostingEnvironment.ContentRootPath,  "..","..","..","..","..");
        return Path.GetFullPath(Path.Combine(root, "appsettings.json"));
    }
    
    public void ConfigureHost(IHostBuilder hostBuilder)
    {
        void ConfigureJsonFile(HostBuilderContext context, IConfigurationBuilder builder)
        {
            string path = GetAppSettingsPath(context);
            builder.AddJsonFile(path);
        }

        hostBuilder
            .ConfigureHostConfiguration(_ => { })
            .ConfigureAppConfiguration(ConfigureJsonFile)
            .ConfigureServices(ConfigureServices);

    }
    private void ConfigureServices(HostBuilderContext context, IServiceCollection services)
    {
        string path = GetAppSettingsPath(context);

        services.AddJJMasterDataCore(path);

        services.BuildServiceProvider()
            .UseJJMasterData();
    }
}