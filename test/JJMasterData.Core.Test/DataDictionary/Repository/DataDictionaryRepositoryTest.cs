using JJMasterData.Commons.Dao;
using JJMasterData.Commons.Dao.Entity;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataDictionary.Repository;
using Newtonsoft.Json;

namespace JJMasterData.Core.Test.DataDictionary.Repository;

public class DataDictionaryRepositoryTest
{
    private IDataDictionaryRepository _repository;
    
    public DataDictionaryRepositoryTest(IEntityRepository entityRepository)
    {
        _repository = new DataDictionaryIO(entityRepository);
    }
    
    
    [Fact]
    public void GetMetadataListTest()
    {
        var metadataList = _repository.GetMetadataList();
        Assert.True(metadataList.Any());
    }
    
    [Theory]
    [InlineData("Example")]
    public void GetMetadataTest(string dictionaryName)
    {
        var metadata = _repository.GetMetadata(dictionaryName);
        Assert.Equal(dictionaryName, metadata.Table.Name);
    }
    
    [Fact]
    public void InsertOrReplaceTest()
    {
        const string dictionaryName = "Example";
        string testData = "Test: " + DateTime.Now;
        var metadata = _repository.GetMetadata(dictionaryName);
        metadata.Table.Info = testData;
        _repository.InsertOrReplace(metadata);
        metadata = _repository.GetMetadata(dictionaryName);
        Assert.Equal(testData, metadata.Table.Info);
    }
    
}