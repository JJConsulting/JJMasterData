using JJMasterData.Commons.Configuration.Options;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Newtonsoft.Json.Linq;

namespace JJMasterData.Commons.Test.Options;

public class LoggingOptions
{
    public Dictionary<string, LogLevel> LogLevel { get; set; } = new();
    public DatabaseLoggingOptions Database { get; set; } = new();
    public FileLoggingOptions File { get; set; } = new();
    public ConsoleLoggingOptions Console { get; set; } = new();
}

public class DatabaseLoggingOptions
{
    public Dictionary<string, LogLevel> LogLevel { get; set; } = new();
}

public class FileLoggingOptions
{
    public string FileName { get; set; } = "log.txt";
    public Dictionary<string, LogLevel> LogLevel { get; set; } = new();
}

public class ConsoleLoggingOptions
{
    public Dictionary<string, LogLevel> LogLevel { get; set; } = new();
}

public class WritableJsonOptionsTests
{
    private readonly Mock<IOptionsMonitor<LoggingOptions>> _optionsMock = new();
    private readonly Mock<IMemoryCache> _memoryCacheMock = new();
    private const string Section = "Logging";
    private const string FilePath = "settings.json";

    private static void Cleanup()
    {
        if (File.Exists(FilePath))
        {
            File.Delete(FilePath);
        }
    }

    [Fact]
    public async Task UpdateAsync_WhenFileDoesNotExist_CreatesNewFileWithCorrectSection()
    {
        Cleanup();

        var writableOptions = new WritableJsonOptions<LoggingOptions>(_optionsMock.Object, _memoryCacheMock.Object,Section, FilePath);

        var applyChanges = new Action<LoggingOptions>(foo =>
        {
            foo.Console.LogLevel = new Dictionary<string, LogLevel>()
            {
                { "Bar", LogLevel.Critical }
            };
        });

        await writableOptions.UpdateAsync(applyChanges);

        Assert.True(File.Exists(FilePath));

        var content = await File.ReadAllTextAsync(FilePath);
        var jObject = JObject.Parse(content);

        Assert.True(jObject.ContainsKey("Logging"));
        Assert.True(jObject["Logging"]!.HasValues);
        Assert.True(jObject["Logging"]!.ToObject<JObject>()!.ContainsKey("Console"));

        var section = jObject["Logging"]!["Console"];
        Assert.NotNull(section);
        Assert.NotEmpty(section.ToString());

        var options = jObject["Logging"]!.ToObject<LoggingOptions>();
        Assert.NotNull(options);
        Assert.Equal(LogLevel.Critical, options.Console.LogLevel["Bar"]);

        Cleanup();
    }

    [Fact]
    public async Task UpdateAsync_WhenFileExists_UpdatesCorrectSection()
    {
        Cleanup();

        var initialContent = "{\"Logging\":{\"Console\":{\"Bar\":\"None\"}}}";
        await File.WriteAllTextAsync(FilePath, initialContent);

        var writableOptions = new WritableJsonOptions<LoggingOptions>(_optionsMock.Object, _memoryCacheMock.Object,Section, FilePath);

        var applyChanges = new Action<LoggingOptions>(foo =>
        {
            foo.Console.LogLevel["Bar"] = LogLevel.Critical;
        });


        await writableOptions.UpdateAsync(applyChanges);

        Assert.True(File.Exists(FilePath));

        var content = await File.ReadAllTextAsync(FilePath);
        var jObject = JObject.Parse(content);

        Assert.True(jObject.ContainsKey("Logging"));
        Assert.True(jObject["Logging"]!.HasValues);
        Assert.True(jObject["Logging"]!.ToObject<JObject>()!.ContainsKey("Console"));

        var section = jObject["Logging"]!["Console"];
        Assert.NotNull(section);
        Assert.NotEmpty(section.ToString());

        var options = jObject["Logging"]!.ToObject<LoggingOptions>();
        Assert.NotNull(options);
        Assert.Equal(LogLevel.Critical, options.Console.LogLevel["Bar"]);

        Cleanup();
    }
}