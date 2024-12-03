using JJMasterData.Commons.Util;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace JJMasterData.Commons.Logging;

internal class BatchingLoggerConfigureOptions(IConfiguration configuration, string isEnabledKey)
    : IConfigureOptions<BatchingLoggerOptions>
{
    public void Configure(BatchingLoggerOptions options)
    {
        var section = configuration.GetSection(isEnabledKey);
        if (section.Exists())
        {
            options.IsEnabled = StringManager.ParseBool(section.Value);
        }
    }
}
