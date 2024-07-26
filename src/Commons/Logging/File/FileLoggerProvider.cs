using System;
using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace JJMasterData.Commons.Logging.File;

[ProviderAlias(ProviderName)]
internal sealed class FileLoggerProvider(FileLoggerBuffer buffer, IOptionsMonitor<FileLoggerOptions> options)
    : ILoggerProvider
{
    private const string ProviderName = "File";

    private readonly ConcurrentDictionary<string, FileLogger> _loggers =
        new(StringComparer.OrdinalIgnoreCase);


    public ILogger CreateLogger(string categoryName) =>
        _loggers.GetOrAdd(categoryName, name => new FileLogger(name, buffer,options));

    public void Dispose()
    {
        _loggers.Clear();
    }
}