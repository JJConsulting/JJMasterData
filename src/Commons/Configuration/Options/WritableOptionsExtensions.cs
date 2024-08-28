using System.IO;
using JJMasterData.Commons.Configuration.Options.Abstractions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;

namespace JJMasterData.Commons.Configuration.Options;

public static class WritableOptionsExtensions
{
    public static void ConfigureWritableOptions<T>(
        this IServiceCollection services, string sectionName) where T : class, new()
    {
        services.AddTransient<IWritableOptions<T>>(svp =>
        {
            var options = svp.GetRequiredService<IOptionsMonitor<T>>()!;
            var memoryCache = svp.GetRequiredService<IMemoryCache>()!;
            var configuration = svp.GetService<IConfiguration>();

            if (configuration is not IConfigurationBuilder builder)
                return null;

            foreach (var source in builder.Sources)
            {
                if (source is not JsonConfigurationSource jsonSource)
                    continue;
                
                if (jsonSource.FileProvider is PhysicalFileProvider physicalFileProvider && jsonSource.Path != "secrets.json")
                {
                    var path = Path.Combine(physicalFileProvider.Root,jsonSource.Path);
                    if (File.Exists(path))
                        return new WritableJsonOptions<T>(options, memoryCache,sectionName, path);
                }
            }
            return null;
        });
    }
}