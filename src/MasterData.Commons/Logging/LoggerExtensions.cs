using System;
using System.Collections.Generic;
using JJMasterData.Commons.Data;
using Microsoft.Extensions.Logging;

namespace JJMasterData.Commons.Logging;

public static class LoggerExtensions
{
    public static IDisposable BeginCommandScope<TCategoryName>(this ILogger<TCategoryName> logger, DataAccessCommand command)
    {
        return logger.BeginScope(new
        
            Dictionary<string,object>
            {
                {"@Command", command}
            }
        );
    }
    
    public static IDisposable BeginCommandListScope<TCategoryName>(this ILogger<TCategoryName> logger, List<DataAccessCommand> commandList)
    {
        return logger.BeginScope(new
        
            Dictionary<string,object>
            {
                {"@CommandList", commandList}
            }
        );
    }
}