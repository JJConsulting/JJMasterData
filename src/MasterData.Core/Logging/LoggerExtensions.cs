using System;
using System.Collections.Generic;
using JJMasterData.Core.DataDictionary.Models;
using Microsoft.Extensions.Logging;

namespace JJMasterData.Core.Logging;

public static class LoggerExtensions
{
    public static IDisposable BeginFormElementScope(this ILogger logger, FormElement formElement)
    {
        return logger.BeginScope(new Dictionary<string, object>
        {
            ["FormElement"] = formElement.Name
        });
    }
}