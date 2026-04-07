using JJMasterData.Core.DataDictionary.Models;

namespace JJMasterData.CommandLine.Test;

public class DictionaryComparisonServiceTests
{
    [Fact]
    public void Compare_ClassifiesFolderAndDatabaseDifferences()
    {
        var createElement = CreateFormElement("CreateOnly", "create");
        var updateLocal = CreateFormElement("UpdateMe", "local");
        var unchanged = CreateFormElement("Unchanged", "same");

        var localFiles = new[]
        {
            CreateLocalFile(createElement),
            CreateLocalFile(updateLocal),
            CreateLocalFile(unchanged)
        };

        var databaseElements = new[]
        {
            CreateFormElement("UpdateMe", "database"),
            CreateFormElement("DeleteOnly", "delete"),
            CreateFormElement("Unchanged", "same")
        };

        var result = DictionaryComparisonService.Compare(localFiles, databaseElements);

        Assert.Single(result.FolderOnly);
        Assert.Equal("CreateOnly", result.FolderOnly[0].Name);

        Assert.Single(result.Changed);
        Assert.Equal("UpdateMe", result.Changed[0].Name);
        Assert.Equal("local", result.Changed[0].Local.FormElement.Info);
        Assert.Equal("database", result.Changed[0].Database.Info);

        Assert.Single(result.DatabaseOnly);
        Assert.Equal("DeleteOnly", result.DatabaseOnly[0].Name);

        Assert.Single(result.Unchanged);
        Assert.Equal("Unchanged", result.Unchanged[0]);
        Assert.True(result.HasChanges);
    }

    private static LocalFormElementFile CreateLocalFile(FormElement formElement)
    {
        return new(
            $"/tmp/{formElement.Name}.json",
            formElement,
            DictionaryFileService.SerializeCanonical(formElement));
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
