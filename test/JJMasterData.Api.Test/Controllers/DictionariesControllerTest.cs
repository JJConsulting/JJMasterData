using JJMasterData.Api.Controllers;
using JJMasterData.Api.Services;
using JJMasterData.Core.DataDictionary.DictionaryDAL;
using Xunit.Extensions.Ordering;

namespace JJMasterData.Api.Test.Controllers;

[Order(0)]
public class DictionariesControllerTest
{
    private readonly DictionariesController _controller;
    
    public DictionariesControllerTest()
    {
        _controller = new DictionariesController(new AccountService());
    }
    
    [Fact]
    public void GetAllTest()
    {
        var dictionaries = _controller.GetAll().Value;

        Assert.IsType<DicParser[]>(dictionaries);
    }
    
    [Theory]
    [InlineData("ApiTestDictionary")]
    public void GetTest(string id)
    {
        var dictionary = _controller.Get(id);

        Assert.IsType<DicParser>(dictionary);
        Assert.Equal(id,dictionary.Table.Name);
    }
}