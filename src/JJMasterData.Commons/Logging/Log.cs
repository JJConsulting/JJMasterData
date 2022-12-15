using JJMasterData.Commons.DI;
using Microsoft.Extensions.Logging;

namespace JJMasterData.Commons.Logging;

/// <summary>
/// Static accessor to the ILogger interface.
/// </summary>
public static class Log
{
    private static ILogger _logger;

    static Log()
    {
        _logger = JJService.Logger;
    }


    public static void Configure(ILogger logger)
    {
        _logger = logger;
    }
    
    public static void AddError(string value)
    {
        _logger.LogError(value); }
    
    public static void AddError(string value, string source)
    {
        _logger.LogError(value, source);
    }
    
    public static void AddInfo(string value)
    {
        _logger.LogInformation(value);
    }
    
    public static void AddInfo(string value, string source)
    {
        _logger.LogInformation(value, source);
    }
    
    public static void AddWarning(string value)
    {
        _logger.LogWarning(value);
    }
    
    public static void AddWarning(string value, string source)
    {
        _logger.LogWarning(value, source);
    }
}



