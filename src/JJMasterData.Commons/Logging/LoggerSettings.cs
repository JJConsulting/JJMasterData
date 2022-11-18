#nullable enable
using System;
using System.ComponentModel.DataAnnotations;
using System.Configuration;
using JJMasterData.Commons.DI;
using JJMasterData.Commons.Extensions;
using JJMasterData.Commons.Settings;

namespace JJMasterData.Commons.Logging;

public class LoggerSettings
{
    private LoggerOption? _writeInTrace;
    private LoggerOption? _writeInConsole;
    private LoggerOption? _writeInEventViewer;
    private LoggerOption? _writeInDatabase;
    private LoggerOption? _writeInFile;
    private string? _fileName;
    private LoggerTableSettings? _table;

    [EnumDataType(typeof(LoggerOption))]
    public LoggerOption WriteInTrace
    {
        get => _writeInTrace ??= GetLoggerOption(nameof(WriteInTrace));
        set => _writeInTrace = value;
    }

    [EnumDataType(typeof(LoggerOption))]
    public LoggerOption WriteInConsole
    {
        get => _writeInConsole ??= GetLoggerOption(nameof(WriteInConsole));
        set => _writeInConsole = value;
    }

    [EnumDataType(typeof(LoggerOption))]
    public LoggerOption WriteInEventViewer
    {
        get => _writeInEventViewer ??= GetLoggerOption(nameof(WriteInEventViewer));
        set => _writeInEventViewer = value;
    }

    [EnumDataType(typeof(LoggerOption))]
    public LoggerOption WriteInDatabase
    {
        get => _writeInDatabase ??= GetLoggerOption(nameof(WriteInDatabase));
        set => _writeInDatabase = value;
    }

    [EnumDataType(typeof(LoggerOption))]
    public LoggerOption WriteInFile
    {
        get => _writeInFile ??= GetLoggerOption(nameof(WriteInFile));
        set => _writeInFile = value;
    }

    public string FileName
    {
        get => _fileName ??=  GetOption(nameof(FileName));
        set => _fileName = value;
    }

    public LoggerTableSettings? Table
    {
        get => _table ??= new LoggerTableSettings();
        set => _table = value;
    }

    public string? ConnectionStringName { get; set; }
    
    public LoggerSettings()
    {
    }

    private string GetNetFrameworkLegacyKey(string key)
    {
        return key switch
        {
            nameof(WriteInTrace) => "LOG_WRITEINTRACE",
            nameof(WriteInConsole) => "LOG_WRITEINCONSOLE",
            nameof(WriteInEventViewer) => "LOG_WRITEINEVENTVIEWER",
            nameof(WriteInDatabase) => "LOG_WRITEINDATABASE",
            nameof(WriteInFile) => "LOG_WRITEINFILE",
            nameof(FileName) => "LOG_FILENAME",
            nameof(ConnectionStringName) => "LOG_CONNECTNAME",
            _ => throw new SettingsPropertyNotFoundException("Logger option not implemented."),
        };
    }

    private string GetOption(string option)
    {
        if (JJMasterDataOptions.IsNetFramework)
        {
            string key = GetNetFrameworkLegacyKey(option);

            string value = ConfigurationManager.AppSettings[key];

            return value;
        }

        var configuration = JJMasterDataOptions.Configuration;

        return configuration.GetJJMasterDataLogger(option);
            
    }

    private LoggerOption GetLoggerOption(string option)
    {
        return (LoggerOption)Enum.Parse(typeof(LoggerOption), GetOption(option) ?? "None");
    }
}