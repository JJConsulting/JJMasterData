using JJMasterData.Api.Controllers;
using JJMasterData.Api.Services;
using JJMasterData.Commons.Dao;
using JJMasterData.Commons.DI;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataDictionary.Repository;
using Xunit.Extensions.Ordering;

namespace JJMasterData.Api.Test.Controllers;

[Order(0)]
public class DictionariesControllerTest
{
    private readonly DictionariesController _controller;
    
    public DictionariesControllerTest()
    {
        IEntityRepository entityRepository = JJService.EntityRepository; 
        IDictionaryRepository dictionaryRepository = new DictionaryDao(entityRepository);
        var dictionariesService = new DictionariesService(dictionaryRepository, entityRepository);
        var accountService = new AccountService();
        _controller = new DictionariesController(accountService, dictionariesService, dictionaryRepository);
    }
    
    [Fact]
    public void GetAllTest()
    {
        var dictionaries = _controller.GetAll().Value;

        Assert.IsType<DataDictionary[]>(dictionaries);
    }
    
    [Theory]
    [InlineData("ApiTestDictionary")]
    public void GetTest(string id)
    {
        var dictionary = _controller.Get(id);

        Assert.IsType<DataDictionary>(dictionary);
        Assert.Equal(id,dictionary.Table.Name);
    }
}