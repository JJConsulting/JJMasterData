using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using JJMasterData.Commons.Util;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace JJMasterData.Commons.Logging.File;

public class FileLoggerBackgroundService : LoggerBackgroundService<FileLoggerBuffer>
{
    private readonly IOptionsMonitor<FileLoggerOptions> _optionsMonitor;

    public FileLoggerBackgroundService(FileLoggerBuffer loggerBuffer, IOptionsMonitor<FileLoggerOptions> optionsMonitor) : base(loggerBuffer)
    {
        _optionsMonitor = optionsMonitor;
    }
    protected override async Task LogAsync(LogMessage logMessage, CancellationToken cancellationToken)
    {
        var options = _optionsMonitor.CurrentValue;
        var path = FileIO.ResolveFilePath(options.FileName);
        var directory = Path.GetDirectoryName(path);

        if (!Directory.Exists(directory))
            Directory.CreateDirectory(directory!);

        var formatting = options.Formatting;
        var record = GetLogRecord(logMessage, formatting);
        
        using var writer = new StreamWriter(path, true);
        await writer.WriteAsync(record);
        
    }

    private static string GetLogRecord(LogMessage message, FileLoggerFormatting formatting)
    {
        var log = new StringBuilder();

        switch (formatting)
        {
            case FileLoggerFormatting.Default:

                log.Append(DateTime.Now);
                log.Append(" ");
                if (!string.IsNullOrWhiteSpace(message.EventId.Name))
                {
                    log.AppendFormat(" [{0}] ", message.EventId.Name);
                }

                log.Append("(");
                log.Append(message.LogLevel.ToString());
                log.AppendLine(")");
                log.Append(message.Formatter(message.State, message.Exception));
                log.AppendLine();
                log.AppendLine();
                break;
            case FileLoggerFormatting.Compact:
            {
                log.AppendFormat("{0:yyyy-MM-dd HH:mm:ss+00:00} -", DateTime.Now);
                log.AppendFormat(" [{0}] ", message.LogLevel);

                if (!string.IsNullOrWhiteSpace(message.EventId.Name))
                {
                    log.AppendFormat(" [{0}] ", message.EventId.Name);
                }

                log.AppendFormat(" {0} ", message.Formatter(message.State, message.Exception));

                if (message.Exception != null)
                {
                    log.AppendLine(message.Exception.Message);
                    log.AppendLine(message.Exception.StackTrace);
                    log.AppendFormat("Source: {0}", message.Exception.Source);
                }
                log.AppendLine();
                break;
            }
            case FileLoggerFormatting.Json:
                log.Append(
                    JsonConvert.SerializeObject(new
                    {   Date = DateTime.Now,
                        Event = message.EventId.Name,
                        message.LogLevel,
                        Message = message.Formatter(message.State, message.Exception),
                        message.Exception
                    }, new JsonSerializerSettings()
                    {
                        NullValueHandling = NullValueHandling.Ignore,
                        Formatting = Formatting.Indented
                    }));
                log.AppendLine(",");
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(formatting), formatting, null);
        }

        return log.ToString();
    }
}
