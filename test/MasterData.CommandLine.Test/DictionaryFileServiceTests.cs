using System.Text.Json;
using JJMasterData.Core.DataDictionary.Models;

namespace JJMasterData.CommandLine.Test;

public class DictionaryFileServiceTests
{
    [Fact]
    public async Task WriteAndLoadAsync_RoundTripsFormElementsUsingElementNameAsFileName()
    {
        var tempPath = CreateTempDirectory();

        try
        {
            var formElement = CreateFormElement("Customer", "Customer info");

            await DictionaryFileService.WriteAsync(tempPath, formElement, CancellationToken.None);

            var filePath = Path.Combine(tempPath, "Customer.json");
            Assert.True(File.Exists(filePath));

            var json = await File.ReadAllTextAsync(filePath);
            Assert.Contains(Environment.NewLine, json);

            var loaded = await DictionaryFileService.LoadAsync(tempPath, CancellationToken.None);

            Assert.Single(loaded);
            Assert.Equal("Customer", loaded[0].Name);
            Assert.Equal("Customer info", loaded[0].FormElement.Info);
        }
        finally
        {
            Directory.Delete(tempPath, recursive: true);
        }
    }

    [Fact]
    public async Task LoadAsync_ThrowsWhenTwoFilesContainTheSameElementName()
    {
        var tempPath = CreateTempDirectory();

        try
        {
            var first = CreateFormElement("Duplicate", "first");
            var second = CreateFormElement("Duplicate", "second");

            await File.WriteAllTextAsync(
                Path.Combine(tempPath, "first.json"),
                JsonSerializer.Serialize(first));
            await File.WriteAllTextAsync(
                Path.Combine(tempPath, "second.json"),
                JsonSerializer.Serialize(second));

            var exception = await Assert.ThrowsAsync<InvalidDataException>(
                () => DictionaryFileService.LoadAsync(tempPath, CancellationToken.None));

            Assert.Contains("Duplicate", exception.Message);
        }
        finally
        {
            Directory.Delete(tempPath, recursive: true);
        }
    }

    private static string CreateTempDirectory()
    {
        var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(path);
        return path;
    }

    private static FormElement CreateFormElement(string name, string info)
    {
        return new FormElement
        {
            Name = name,
            TableName = name,
            Info = info
        };
    }
}
