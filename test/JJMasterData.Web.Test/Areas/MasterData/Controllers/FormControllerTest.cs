using JJMasterData.Web.Example;
using Xunit.Extensions.Ordering;

namespace JJMasterData.Web.Test.Areas.MasterData.Controllers;

[CollectionDefinition("FormController", DisableParallelization = true)]
[Order(2)]
public class FormControllerTest : IClassFixture<JJMasterDataWebExampleAppFactory<Program>>
{
    private readonly HttpClient _client;

    public FormControllerTest(JJMasterDataWebExampleAppFactory<Program> factory)
    {
        factory.EnsureServer();
        _client = factory.CreateClient();
    }
  
    [Fact]
    [Order(0)]
    public async Task RenderTest()
    {
        var response = await _client.GetAsync($"/en-us/MasterData/Form/Render/{Constants.TestDataDictionaryName}");

        response.EnsureSuccessStatusCode();
    }
}