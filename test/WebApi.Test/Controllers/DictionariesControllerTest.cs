using JJMasterData.WebApi.Controllers;
using JJMasterData.WebApi.Services;

using JJMasterData.Commons.DI;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataDictionary.Repository;
using JJMasterData.Core.DataDictionary.Repository.Abstractions;
using JJMasterData.Core.DI;
using JJMasterData.Core.Options;
using Microsoft.Extensions.Configuration;
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
        var entityRepository = JJService.EntityRepository; 
        var dataDictionaryRepository = new SqlDataDictionaryRepository(entityRepository, JJService.Provider.GetRequiredService<IConfiguration>());
        var dictionariesService = new DictionariesService(dataDictionaryRepository, entityRepository);
        _controller = new DictionariesController(dictionariesService, dataDictionaryRepository);
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