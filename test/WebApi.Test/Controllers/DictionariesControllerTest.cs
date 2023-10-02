using System.Threading.Tasks;
using JJMasterData.WebApi.Controllers;
using JJMasterData.WebApi.Services;
using JJMasterData.Core.DataDictionary.Models;
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
    public async Task GetAllTest()
    {
        var dictionaries = (await _controller.GetAll()).Value;

        Assert.IsType<FormElement[]>(dictionaries);
    }
    
    [Theory]
    [InlineData("ApiTestDictionary")]
    public async Task GetTest(string id)
    {
        var dictionary = await _controller.Get(id);

        Assert.IsType<FormElement>(dictionary);
        Assert.Equal(id,dictionary.Name);
    }
}