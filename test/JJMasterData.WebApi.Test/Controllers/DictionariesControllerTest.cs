using JJMasterData.WebApi.Controllers;
using JJMasterData.WebApi.Services;
using JJMasterData.Commons.Dao;
using JJMasterData.Commons.DI;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataDictionary.Repository;
using JJMasterData.Core.DI;
using JJMasterData.Core.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit.Extensions.Ordering;

namespace JJMasterData.WebApi.Test.Controllers;

[Order(0)]
public class DictionariesControllerTest
{
    private readonly DictionariesController _controller;
    
    public DictionariesControllerTest()
    {
        IEntityRepository entityRepository = JJService.EntityRepository; 
        IDataDictionaryRepository dataDictionaryRepository = new DatabaseDataDictionaryRepository(entityRepository, JJService.Provider.GetRequiredService<IOptions<JJMasterDataCoreOptions>>());
        var dictionariesService = new DictionariesService(dataDictionaryRepository, entityRepository);
        _controller = new DictionariesController(dictionariesService, dataDictionaryRepository);
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