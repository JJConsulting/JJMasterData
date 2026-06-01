using JJMasterData.Core.DataDictionary.Models;

namespace JJMasterData.IntegrationTests.WebApi;

[Collection(IntegrationTestCollection.Name)]
public sealed class DataDictionaryApiTests(IntegrationTestFixture fixture)
{
    [Fact]
    public async Task Should_Return_Created_Data_Dictionary_From_HttpClient()
    {
        var elementName = $"HttpClient{Guid.NewGuid():N}";

        var created = await fixture.CreateDataDictionaryAsync(elementName);

        Assert.NotNull(created);
        Assert.Equal(elementName, created!.Name);

        var retrieved = await fixture.Client.GetFromJsonAsync<FormElement>(
            $"data-dictionary/{elementName}",
            TestContext.Current.CancellationToken);

        Assert.NotNull(retrieved);
        Assert.Equal(elementName, retrieved!.Name);
        Assert.Equal(elementName, retrieved.TableName);
    }
}
