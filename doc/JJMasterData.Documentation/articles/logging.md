# Logging

JJMasterData uses Microsoft [ILogger](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/logging/?view=aspnetcore-7.0). 
We have two implementations of `ILogger`, these are `DbLogger` and `FileLogger`. These two implementations can work alongside any `ILogger` source, including the default ones.

## Configuration
```
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    },
    "Console": {
      "LogLevel": {
        "Default": "Warning",
        "Microsoft.AspNetCore": "None"
      }
    },
    "Database": {
      "LogLevel": {
        "Default": "Warning",
        "Microsoft.AspNetCore": "None"
      },
      "TableName": "JJMasterDataLogger"
    },
    "File": {
      "FileName": "Log/AppLog-yyyyMMdd.txt"
    }
  }
```

At the root [LogLevel](https://learn.microsoft.com/en-us/dotnet/api/microsoft.extensions.logging.loglevel?view=dotnet-plat-ext-7.0) property, 
you can set the default LogLevel to log for all `ILogger` sources. If you set `LogLevel` inside a property, only that `ILogger` source will be affected.
You can also specify the settings of each `ILogger` source in their respective properties. Check `DbLogger` and `FileLogger` API reference.
