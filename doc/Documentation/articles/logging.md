# JJMasterData Logging System

The **JJMasterData Logging System** integrates with `Microsoft.Extensions.Logging` to provide custom logging providers for database and file-based logging. These providers offer robust, configurable, and extendable options to handle application logging needs. Below is a detailed explanation of the options, configuration, and usage.

---

## Logging Providers Overview

### File Logger Provider
The **File Logger Provider** writes log messages to files with support for log rotation and formatting options.

#### Features
- **Log Size Limit**: Configurable limit to the size of a log file before rotation.
- **Retained Files**: Specifies how many old log files to keep.
- **Custom File Names**: Supports templated filenames (e.g., `Log/AppLog-yyyyMMdd.txt`).
- **Formatting**: Customize log output format.

### Database Logger Provider
The **Database Logger Provider** stores log messages in a relational database table.

#### Features
- **Customizable Columns**: Maps log properties to database table columns.
- **Connection Management**: Uses a `ConnectionStringId` for database connection identification.

---

## Configuration in `appsettings.json`

To use these providers, configure them in your application's `appsettings.json`. Below is an example configuration.

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning",
      "System": "Warning"
    },
    "File": {
      "FileSizeLimit": 10485760,
      "RetainedFileCountLimit": 5,
      "FileName": "Logs/MyApp-yyyyMMdd.log",
      "Formatting": "Default"
    },
    "Database": {
      "TableName": "tb_masterdata_log",
      "Microsoft.EntityFrameworkCore.Database.Command": "None"
    }
  }
}
```

---

## Programmatic Integration in `Program.cs`

To enable these custom logging providers in your application, update the `Program.cs` file as follows:

```csharp
var builder = WebApplication.CreateBuilder(args);

// Add logging providers
builder.Logging.AddFileLoggerProvider(builder.Configuration);
builder.Logging.AddDbLoggerProvider(builder.Configuration);

var app = builder.Build();
app.Run();
```

---

## File Logger Configuration Options

| Property                | Description                                                                                  | Default                  |
|-------------------------|----------------------------------------------------------------------------------------------|--------------------------|
| `FileSizeLimit`         | Maximum size of a single log file in bytes.                                                  | `10485760` (10 MB)       |
| `RetainedFileCountLimit`| Number of old log files to retain.                                                           | `2`                      |
| `FileName`              | Templated name for log files.                                                               | `Log/AppLog-yyyyMMdd.txt`|
| `Formatting`            | Determines log entry format (e.g., `Default`, `Json`).                                       | `Default`                |

---

## Database Logger Configuration Options

| Property              | Description                                   | Default               |
|-----------------------|-----------------------------------------------|-----------------------|
| `ConnectionStringId`  | Identifier for database connection string.    | `null`               |
| `TableName`           | Target table name for logs.                   | `tb_masterdata_log`  |
| `IdColumnName`        | Column for unique log IDs.                    | `Id`                |
| `CreatedColumnName`   | Column for timestamp of log creation.         | `log_dat_evento`     |
| `LevelColumnName`     | Column for log level (e.g., Info, Error).     | `log_txt_tipo`       |
| `MessageColumnName`   | Column for log message.                       | `log_txt_message`    |
| `CategoryColumnName`  | Column for log category/source.               | `log_txt_source`     |


### Common Options for `BatchingLoggerOptions`

The `BatchingLoggerOptions` class provides configurable settings applicable to both **File Logger Provider** and **Database Logger Provider**. These options control how log messages are batched, queued, and flushed to their respective destinations.

---

#### Common Properties

| **Property**           | **Description**                                                                                  | **Default Value**         |
|------------------------|--------------------------------------------------------------------------------------------------|---------------------------|
| `FlushPeriod`          | Specifies the interval at which logs are flushed to the store. Must be a positive `TimeSpan`.    | `1 second`                |
| `BackgroundQueueSize`  | Maximum size of the log message queue before the system starts blocking.                         | `1000`                    |
| `BatchSize`            | Maximum number of log entries to include in a single batch. Must be a positive integer or null.  | `null` (no limit)         |
| `IsEnabled`            | Indicates if the logger is enabled and accepting log messages.                                   | `true`                    |
| `IncludeScopes`        | Determines whether scope information is included in log entries.                                 | `false`                   |

---

#### Usage Details

- **FlushPeriod**: Controls the frequency of log writes. This ensures logs are periodically sent to the file or database, reducing memory usage.
- **BackgroundQueueSize**: Prevents overloading by limiting the number of pending log messages.
- **BatchSize**: Optimizes write performance by batching multiple log entries.
- **IsEnabled**: Allows toggling logging on or off without modifying other configurations.
- **IncludeScopes**: Useful for structured logging, providing context about the log source or operation scope.

---

#### Example Configuration in `appsettings.json`

```json
{
  "Logging": {
    "FileLogger": {
      "FileSizeLimit": 10485760,
      "RetainedFileCountLimit": 3,
      "FileName": "Logs/App-yyyyMMdd.log"
    },
    "DbLogger": {
      "FlushPeriod": "00:00:02",
      "BackgroundQueueSize": 500,
      "BatchSize": 50,
      "IsEnabled": true,
      "IncludeScopes": true
      "TableName": "ApplicationLogs"
    }
  }
}
```

By combining these **common options** with specific settings for file or database logging, you can ensure consistent behavior while tailoring the output destination. These options ensure scalability and control for high-throughput logging scenarios.