using JJMasterData.WebApi.Controllers;
using JJMasterData.WebApi.Services;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataDictionary.Repository.Abstractions;
using Moq;
using Xunit.Extensions.Ordering;

namespace JJMasterData.WebApi.Test.Controllers;

[Order(0)]
public class DictionariesControllerTest
{
    private readonly DictionariesController _controller;
    
    public DictionariesControllerTest()
    {
        var dataDictionaryRepository = new Mock<IDataDictionaryRepository>().Object;
        var dictionariesService = new Mock<DictionariesService>().Object;
        var accountsService = new Mock<AccountService>().Object;
        _controller = new DictionariesController(accountsService,dictionariesService, dataDictionaryRepository);
    }
    
    [Fact]
    public void GetAllTest()
    {
        var dictionaries = _controller.GetAll().Value;

        Assert.IsType<FormElement[]>(dictionaries);
    }
    
    [Theory]
    [InlineData("ApiTestDictionary")]
    public void GetTest(string id)
    {
        var dictionary = _controller.Get(id);

        Assert.IsType<FormElement>(dictionary);
        Assert.Equal(id,dictionary.Name);
    }
}