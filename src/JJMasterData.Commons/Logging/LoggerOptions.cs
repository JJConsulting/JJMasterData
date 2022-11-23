#nullable enable


using System.Text.Json.Serialization;


namespace JJMasterData.Commons.Logging;

public record LoggerOptions
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public LoggerOption WriteInTrace { get; set; } = LoggerOption.None;
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public LoggerOption WriteInConsole { get; set; } = LoggerOption.None;
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public LoggerOption WriteInEventViewer { get; set; } = LoggerOption.None;
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public LoggerOption WriteInDatabase { get; set; } = LoggerOption.None;
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public LoggerOption WriteInFile { get; set; } = LoggerOption.None;

    public string FileName { get; set; } = "application_log.txt";

    public LoggerTableOptions Table { get; set; } = new();

    public string ConnectionStringName { get; set; } = "ConnectionString";
}