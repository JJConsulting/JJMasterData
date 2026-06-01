using System.Text.RegularExpressions;

namespace JJMasterData.IntegrationTests.Web;

[Collection(IntegrationTestCollection.Name)]
public sealed class DataDictionaryUiTests(IntegrationTestFixture fixture) : MasterDataWebPageTest(fixture)
{
    [Fact]
    public async Task Should_Create_Data_Dictionary_From_Playwright()
    {
        var elementName = $"Playwright{Guid.NewGuid():N}";

        await Page.GotoAsync("/DataDictionary/Element/Add");
        await Page.Locator("#Name").FillAsync(elementName);
        await Page.Locator("form button[type='submit']").ClickAsync();

        await Page.WaitForURLAsync(new Regex($".*/DataDictionary/Entity/Index/{Regex.Escape(elementName)}$", RegexOptions.IgnoreCase));
        await Page.Locator("#Entity_Name").WaitForAsync();

        Assert.Contains($"/DataDictionary/Entity/Index/{elementName}", new Uri(Page.Url).AbsolutePath, StringComparison.OrdinalIgnoreCase);
        await Expect(Page.Locator("#Entity_Name")).ToHaveValueAsync(elementName);
    }
}
