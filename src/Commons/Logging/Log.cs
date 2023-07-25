#if NET48
using System;
using JJMasterData.Commons.Configuration;
using JJMasterData.Commons.DI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace JJMasterData.Commons.Logging;

/// <summary>
/// Static accessor to the ILogger interface.
/// When possible, use ILogger via constructor injection.
/// </summary>
[Obsolete("Please use ILogger. This class uses a static service locator and don't have <T> categories support.")]
public static class Log
{
    private static ILogger _logger;

    static Log()
    {
        _logger = JJService.Provider.GetRequiredService<ILogger<JJServiceBuilder>>();
    }

    public static void Configure(ILogger logger)
    {
        _logger = logger;
    }

    public static void AddError(string value, string source = "System")
    {
        _logger.LogError(value, new EventId(0, source));
    }
    
    public static void AddError(Exception exception, string message)
    {
        _logger.LogError(exception, message);
    }
    
    public static void AddInfo(string value, string source = "System")
    {
        _logger.LogInformation(value, new EventId(0, source));
    }

    public static void AddWarning(string value, string source = "System")
    {
        _logger.LogWarning(value, new EventId(0, source));
    }
}
#endif
