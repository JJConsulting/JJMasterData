using System;
using System.Collections.Generic;

namespace JJMasterData.Commons.Logging.Db;

internal class DbLogEntry
{
    public required DateTime Created { get; init; }
    public required int LogLevel { get; init; }
    public required string Event { get; init; }
    public required string Message { get; init; }
    
    public Dictionary<string, dynamic> ToDictionary(DbLoggerOptions options)
    {
        return new Dictionary<string, dynamic>
        {
            [options.CreatedColumnName] = Created,
            [options.LevelColumnName] = LogLevel,
            [options.EventColumnName] = Event,
            [options.MessageColumnName] = Message,
        };
    }
    
    public char[] ToSeparatedCharArray()
    {
        return $"{Created};{LogLevel};{Event};{Message}".ToCharArray();
    }

    public static DbLogEntry FromSeparatedString(string input)
    {
        var values = input.Split(';');

        var created = DateTime.Parse(values[0]);
        var logLevel = int.Parse(values[1]);
        var @event = values[2];
        var message = values[3];

        return new DbLogEntry
        {
            Created = created,
            LogLevel = logLevel,
            Event = @event,
            Message = message
        };
    }
}