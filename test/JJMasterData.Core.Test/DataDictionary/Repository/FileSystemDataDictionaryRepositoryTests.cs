using JJMasterData.Commons.Data.Entity;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataDictionary.Repository;
using JJMasterData.Core.DataDictionary.Repository.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using Newtonsoft.Json;

namespace JJMasterData.Core.Test.DataDictionary.Repository;

public class FileSystemDataDictionaryRepositoryTests : IDisposable
{
    private readonly IDataDictionaryRepository _repository;

    private const string FolderPath = "Metadata";
    private const string DictionaryName = "Example";
    private static string DictionaryFilePath { get; } = Path.Combine(FolderPath, DictionaryName + ".json");
    
    public FileSystemDataDictionaryRepositoryTests()
    {
        var optionsMock = new Mock<IOptions<FileSystemDataDictionaryOptions>>();
        optionsMock.SetupGet(o => o.Value).Returns(new FileSystemDataDictionaryOptions { FolderPath = "Metadata" });

        _repository = new FileSystemDataDictionaryRepository(optionsMock.Object);
    }

    private static void CreateMetadataIfNotExists()
    {
        var metadata = new FormElement()
        {
            Name = "Example"
        };

        if (!File.Exists(DictionaryFilePath))
        {
            if (!Directory.Exists(FolderPath))
                Directory.CreateDirectory(FolderPath);
            
            File.WriteAllText(DictionaryFilePath, JsonConvert.SerializeObject(metadata));
        }
    }

    [Fact]
    public void GetMetadataListTest()
    {
        // Arrange
        CreateMetadataIfNotExists();
        
        // Act
        var metadataList = _repository.GetMetadataList();
        
        // Assert
        Assert.True(metadataList.Any());
    }
    
    [Fact]
    public void GetMetadataTest()
    {
        // Arrange
        CreateMetadataIfNotExists();
        
        // Act
        var metadata = _repository.GetMetadata(DictionaryName);
        
        // Assert
        Assert.Equal(DictionaryName, metadata.Name);
    }
    
    [Fact]
    public void InsertOrReplaceTest()
    {
        // Arrange
        CreateMetadataIfNotExists();
        var testData = "Test: " + DateTime.Now;
        var metadata = _repository.GetMetadata(DictionaryName);
        metadata.Info = testData;
        
        // Act
        _repository.InsertOrReplace(metadata);
        metadata = _repository.GetMetadata(DictionaryName);
        
        // Assert
        Assert.Equal(testData, metadata.Info);
    }

    public void Dispose()
    {
        if(Directory.Exists(FolderPath))
            Directory.Delete(FolderPath, true);
        
        GC.SuppressFinalize(this);
    }
}