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
    [EnumDataType(typeof(LoggerOption))]
    public LoggerOption WriteInTrace { get; set; }

    [EnumDataType(typeof(LoggerOption))]
    public LoggerOption WriteInConsole { get; set; } 

    [EnumDataType(typeof(LoggerOption))]
    public LoggerOption WriteInEventViewer { get; set; } 

    [EnumDataType(typeof(LoggerOption))]
    public LoggerOption WriteInDatabase { get; set; }

    [EnumDataType(typeof(LoggerOption))]
    public LoggerOption WriteInFile { get; set; } 

    public string FileName { get; set; }
    public LoggerTableSettings? Table { get; set; }
    public string ConnectionStringName { get; set; } = JJService.Settings.ConnectionString;
    public string ConnectionProvider { get; set; } = JJService.Settings.ConnectionProvider;


    public LoggerSettings()
    {
        WriteInTrace = GetLoggerOption(nameof(WriteInTrace));
        WriteInConsole = GetLoggerOption(nameof(WriteInConsole));
        WriteInEventViewer = GetLoggerOption(nameof(WriteInEventViewer));
        WriteInDatabase = GetLoggerOption(nameof(WriteInDatabase));
        WriteInFile = GetLoggerOption(nameof(WriteInFile));
        FileName = GetOption(nameof(FileName));

        Table = new LoggerTableSettings();
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
        if (JJMasterDataSettings.IsNetFramework)
        {
            string key = GetNetFrameworkLegacyKey(option);

            string value = ConfigurationManager.AppSettings[key];

            return value;
        }

        var configuration = JJMasterDataSettings.Configuration;

        return configuration.GetJJMasterDataLogger(option);
            
    }

    private LoggerOption GetLoggerOption(string option)
    {
        return (LoggerOption)Enum.Parse(typeof(LoggerOption), GetOption(option) ?? "None");
    }
}