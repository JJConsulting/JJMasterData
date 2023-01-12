using Microsoft.Extensions.Configuration;

namespace JJMasterData.Commons.Extensions;

public static class ConfigurationExtensions
{
    public static IConfigurationSection GetJJMasterData(this IConfiguration configuration)
    {
        return configuration.GetSection("JJMasterData");
    }
    public static string GetJJMasterData(this IConfiguration configuration, string key)
    {
        return configuration.GetJJMasterData().GetSection(key)?.Value;
    }
}
