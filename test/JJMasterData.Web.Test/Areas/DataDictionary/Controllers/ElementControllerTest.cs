using JJMasterData.Commons.Dao;
using JJMasterData.Commons.Dao.Entity;
using Xunit.Extensions.Ordering;

namespace JJMasterData.Web.Test.Areas.DataDictionary.Controllers;

[CollectionDefinition("ElementController", DisableParallelization = true)]
[Order(0)]
public class ElementControllerTest : IClassFixture<JJMasterDataWebExampleAppFactory>
{
    private readonly HttpClient _client;

    public ElementControllerTest(JJMasterDataWebExampleAppFactory factory)
    {
        factory.EnsureServer();
        _client = factory.CreateClient();
    }
    private static void CreateElement()
    {
        var element = new Element
        {
            Name = Constants.TestDataDictionaryName,
            TableName = Constants.TestDataDictionaryName,
            Fields = new ElementList
            {
                new()
                {
                    Name = "ID",
                    DataType = FieldType.Int,
                    IsPk = true,
                    Filter = new ElementFilter
                    {
                        Type  = FilterMode.Equal,
                        IsRequired = true
                    },
                    AutoNum = true
                },
                new()
                {
                    Name = "Name",
                    Size = 300
                }
            }
        };

        var dataAccess = new DataAccess();
        var provider = new Factory();
        
        string script = provider.GetScriptCreateTable(element);

        if (dataAccess.TableExists(element.TableName))
            dataAccess.SetCommand($"DROP TABLE {element.TableName}");
        
        dataAccess.SetCommand(script);
    }
        
    [Fact]
    [Order(0)]
    public async Task IndexTest()
    {
        var response = await _client.GetAsync("/en-us/DataDictionary/Element/Index");

        var responseString = await response.Content.ReadAsStringAsync();

        Assert.Contains("Data Dictionaries", responseString);
    }
        
    [Fact]
    [Order(1)]
    public async Task AddTest()
    {
        CreateElement();
            
        var response = await _client.GetAsync("/en-us/DataDictionary/Element/Add");

        var responseString = await response.Content.ReadAsStringAsync();

        Assert.Contains("Add Dictionary", responseString);

        var content = new Dictionary<string, string>
        {
            { "tableName", Constants.TestDataDictionaryName },
            { "importFields", "true" }
        };
            
        var postRequest = new HttpRequestMessage(HttpMethod.Post, "/en-us/DataDictionary/Element/Add");
        postRequest.Content = new FormUrlEncodedContent(content);

        response = await _client.SendAsync(postRequest);

        response.EnsureSuccessStatusCode();
    }
    [Fact]
    [Order(2)]
    public async Task ExecScriptsTest()
    {
        var postRequest = new HttpRequestMessage(HttpMethod.Post, "/en-us/DataDictionary/Element/Scripts");

        var content = new Dictionary<string, string>
        {
            { "dictionaryName", Constants.TestDataDictionaryName },
            { "scriptExec", "Exec" }
        };

        postRequest.Content = new FormUrlEncodedContent(content);

        var response = await _client.SendAsync(postRequest);

        response.EnsureSuccessStatusCode();
    }
}