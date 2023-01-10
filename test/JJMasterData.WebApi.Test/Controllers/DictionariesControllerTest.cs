using JJMasterData.WebApi.Controllers;
using JJMasterData.WebApi.Services;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataDictionary.Repository;
using Xunit.Extensions.Ordering;

namespace JJMasterData.WebApi.Test.Controllers;

[Order(0)]
public class DictionariesControllerTest
{
    private readonly DictionariesController _controller;
    
    public DictionariesControllerTest(DictionariesService service, DatabaseDataDictionaryRepository dataDictionaryRepository)
    {
        _controller = new DictionariesController(service, dataDictionaryRepository);
    }
    
    [Fact]
    public void GetAllTest()
    {
        var dictionaries = _controller.GetAll().Value;

        Assert.IsType<Metadata[]>(dictionaries);
    }
    
    [Theory]
    [InlineData("ApiTestDictionary")]
    public void GetTest(string id)
    {
        var dictionary = _controller.Get(id);

        Assert.IsType<Metadata>(dictionary);
        Assert.Equal(id,dictionary.Table.Name);
    }
}