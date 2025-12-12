using System;
using System.Collections.Generic;
using JJMasterData.Commons.Data;
using Microsoft.Extensions.Logging;

namespace JJMasterData.Commons.Logging;

public static class LoggerExtensions
{
    extension<TCategoryName>(ILogger<TCategoryName> logger)
    {
        public IDisposable BeginCommandScope(DataAccessCommand command)
        {
            return logger.BeginScope(new
        
                Dictionary<string,object>
                {
                    {"@Command", command}
                }
            );
        }

        public IDisposable BeginCommandListScope(List<DataAccessCommand> commandList)
        {
            return logger.BeginScope(new
        
                Dictionary<string,object>
                {
                    {"@CommandList", commandList}
                }
            );
        }
    }
}