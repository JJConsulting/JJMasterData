using System.IO;
using System.Linq;
using JJMasterData.Commons.Configuration.Options.Abstractions;
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
        services.AddTransient<IWritableOptions<T>>(provider =>
        {
            var options = provider.GetService<IOptionsMonitor<T>>()!;
            var configuration = provider.GetService<IConfiguration>();

            if (configuration is IConfigurationBuilder builder)
            {
                var source = (JsonConfigurationSource)builder.Sources.FirstOrDefault(s=>s is JsonConfigurationSource);

                if (source?.FileProvider is PhysicalFileProvider physicalFileProvider)
                {
                    var path = Path.Combine(physicalFileProvider.Root,source.Path);
                
                    return new WritableJsonOptions<T>(options,sectionName , path);
                }
                
            }
            
            return null;
        });
    }
}