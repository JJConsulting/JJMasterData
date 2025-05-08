// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using JJMasterData.Commons.Util;
using Microsoft.Extensions.Options;

namespace JJMasterData.Commons.Logging.File;

/// <summary>
/// A <see cref="BatchingLoggerProvider"/> which writes out to a file.
/// </summary>
[ProviderAlias(ProviderAlias)]
public sealed class FileLoggerProvider : BatchingLoggerProvider
{
    private const string ProviderAlias = "File";
    private readonly string _fileName;
    private readonly int? _maxFileSize;
    private readonly int? _maxRetainedFiles;
    private readonly FileLoggerFormatting _formatting;

    /// <summary>
    /// Creates a new instance of <see cref="FileLoggerProvider"/>.
    /// </summary>
    /// <param name="options">The options to use when creating a provider.</param>
    public FileLoggerProvider(IOptionsMonitor<FileLoggerOptions> options) : base(options)
    {
        var loggerOptions = options.CurrentValue;
        _fileName = loggerOptions.FileName;
        _maxFileSize = loggerOptions.FileSizeLimit;
        _maxRetainedFiles = loggerOptions.RetainedFileCountLimit;
        _formatting = loggerOptions.Formatting;
    }

    protected override async Task WriteMessagesAsync(List<LogMessage> messages, CancellationToken cancellationToken)
    {
        var path = FileIO.ResolveFilePath(_fileName);
        var directory = Path.GetDirectoryName(path);

        Directory.CreateDirectory(directory!);
        using var streamWriter = System.IO.File.AppendText(path);
        foreach (var message in messages)
        {
            var fileInfo = new FileInfo(path);
            if (_maxFileSize > 0 && fileInfo.Exists && fileInfo.Length > _maxFileSize)
            {
                return;
            }

            await streamWriter.WriteAsync(GetMessage(message)).ConfigureAwait(false);
        }

        RollFiles(directory);
    }
    
    private void RollFiles(string directory)
    {
        if (_maxRetainedFiles > 0)
        {
            var files = new DirectoryInfo(directory)
                .GetFiles(_fileName + "*")
                .OrderByDescending(f => f.Name)
                .Skip(_maxRetainedFiles.Value);

            foreach (var item in files)
            {
                item.Delete();
            }
        }
    }

    private string GetMessage(LogMessage message)
    {
        var log = new StringBuilder();

        switch (_formatting)
        {
            case FileLoggerFormatting.Compact:
            {
                log.Append(message.Timestamp.ToString("yyyy-MM-dd HH:mm:ss.fff zzz", CultureInfo.InvariantCulture));
                log.Append(message.Category);

                var scopeProvider = ScopeProvider;
                if (scopeProvider != null)
                {
                    scopeProvider.ForEachScope(
                        (scope, stringBuilder) => stringBuilder.Append(" => ").Append(scope), log);

                    log.AppendLine(":");
                }
                else
                {
                    log.Append(": ");
                }

                log.AppendLine(message.Message);
                break;
            }
            default:
                log.Append(DateTime.Now);
                log.Append(' ');
                log.Append('(');
                log.Append(message.LogLevel);
                log.AppendLine(")");
                log.Append(message.Message);
                log.AppendLine();
                log.AppendLine();
                break;
        }

        return log.ToString();
    }
}