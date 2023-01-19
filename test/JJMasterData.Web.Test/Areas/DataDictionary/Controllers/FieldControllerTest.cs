using JJMasterData.Commons.Data.Entity;
using JJMasterData.Commons.Extensions;
using JJMasterData.Core.DataDictionary;
using Xunit.Extensions.Ordering;

namespace JJMasterData.Web.Test.Areas.DataDictionary.Controllers;

[CollectionDefinition("FieldController", DisableParallelization = true)]
[Order(1)]
public class FieldControllerTest : IClassFixture<JJMasterDataWebExampleAppFactory>
{
    private readonly HttpClient _client;

    public FieldControllerTest(JJMasterDataWebExampleAppFactory factory)
    {
        factory.EnsureServer();
        _client = factory.CreateClient();
    }

    [Fact]
    [Order(0)]
    public async Task DetailTest()
    {
        var response = await _client.GetAsync($"/en-us/DataDictionary/Field/Index/{Constants.TestDataDictionaryName}");

        response.EnsureSuccessStatusCode();
        
        var responseString = await response.Content.ReadAsStringAsync();

        Assert.Contains(Constants.TestDataDictionaryName, responseString);
    }

    [Fact]
    [Order(1)]
    public async Task SaveTest()
    {
        var postRequest = new HttpRequestMessage(HttpMethod.Post, "/en-us/DataDictionary/Field/Save");

        var content = new FormElementField()
        {
            Name = "Age",
            Label = "Age",
            DataType = FieldType.Int,
            Size = 100,
            MinValue = 1,
            MaxValue = 120,
            Component = FormComponent.Number
        }.ToDictionary();
        
        content.Add("name", "Age");
        content.Add("dictionaryName",Constants.TestDataDictionaryName);
        content.Add("originalName",Constants.TestDataDictionaryName);
        
        postRequest.Content = new FormUrlEncodedContent(content);

        var response = await _client.SendAsync(postRequest);

        response.EnsureSuccessStatusCode();
    }
    
    [Fact]
    [Order(2)]
    public async Task DeleteTest()
    {
        var postRequest = new HttpRequestMessage(HttpMethod.Post, "/en-us/DataDictionary/Field/Delete");

        var content = new Dictionary<string, string>
        {
            { "dictionaryName", Constants.TestDataDictionaryName },
            { "fieldName", "Age" }
        };

        postRequest.Content = new FormUrlEncodedContent(content);

        var response = await _client.SendAsync(postRequest);

        response.EnsureSuccessStatusCode();
    }
}