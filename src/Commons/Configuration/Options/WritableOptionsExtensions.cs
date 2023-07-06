using JJMasterData.Commons.Configuration.Options.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace JJMasterData.Commons.Configuration.Options;

public static class WritableOptionsExtensions
{
    public static void ConfigureWritableOptions<T>(
        this IServiceCollection services,
        IConfigurationSection section,
        string file) where T : class, new()
    {
        services.Configure<T>(section);
        services.AddTransient<IWritableOptions<T>>(provider =>
        {
            var options = provider.GetService<IOptionsMonitor<T>>()!;
            return new WritableJsonOptions<T>(options, section.Key, file);
        });
    }
}