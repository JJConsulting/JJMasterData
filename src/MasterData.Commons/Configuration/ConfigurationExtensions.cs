using Microsoft.Extensions.Configuration;

namespace JJMasterData.Commons.Configuration;

public static class ConfigurationExtensions
{
    extension(IConfiguration configuration)
    {
        public IConfigurationSection GetJJMasterData()
        {
            return configuration.GetSection("JJMasterData");
        }

        public string GetJJMasterData(string key)
        {
            return configuration.GetJJMasterData().GetSection(key)?.Value;
        }
    }
}
