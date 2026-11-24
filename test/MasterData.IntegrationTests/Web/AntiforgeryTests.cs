namespace JJMasterData.IntegrationTests.Web;

[Collection(IntegrationTestCollection.Name)]
public sealed class AntiforgeryTests(IntegrationTestFixture fixture) : MasterDataWebPageTest(fixture)
{
    [Theory]
    [InlineData("/DataDictionary/Element/Add")]
    [InlineData("/MasterData/Form/Render/AntiforgeryTest")]
    public async Task Should_Reject_Missing_And_Invalid_Tokens(string url)
    {
        await Page.GotoAsync("/DataDictionary/Element/Add");

        var missingToken = await Page.APIRequest.PostAsync(url);
        Assert.Equal(400, missingToken.Status);

        var invalidToken = await Page.APIRequest.PostAsync(url, new APIRequestContextOptions
        {
            Headers = new Dictionary<string, string> { ["RequestVerificationToken"] = "invalid" }
        });
        Assert.Equal(400, invalidToken.Status);
    }

    [Theory]
    [InlineData("/DataDictionary/Element/Index")]
    [InlineData("/MasterData/Icons/Index?inputId=test")]
    public async Task Should_Accept_Ajax_With_Valid_Token(string url)
    {
        await Page.GotoAsync("/DataDictionary/Element/Index");
        await Expect(Page.Locator("#masterdata-form")).ToHaveAttributeAsync("method", "post");
        await Expect(Page.Locator("#masterdata-form input[name='__RequestVerificationToken']")).ToHaveCountAsync(1);

        var status = await Page.EvaluateAsync<int>("""
            async url => {
                const options = getRequestOptions();
                options.body.delete('__RequestVerificationToken');
                return (await fetch(url, options)).status;
            }
            """, url);

        Assert.Equal(200, status);
    }

    [Theory]
    [InlineData("Element")]
    [InlineData("Field")]
    [InlineData("Panel")]
    [InlineData("Rules")]
    public async Task Should_Reject_Delete_Via_Get(string controller)
    {
        var response = await Page.APIRequest.GetAsync($"/DataDictionary/{controller}/Delete");

        Assert.Equal(405, response.Status);
    }
}
