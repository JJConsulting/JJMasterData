using Microsoft.Playwright.Xunit.v3;

namespace JJMasterData.IntegrationTests.Web;

public abstract class MasterDataWebPageTest(IntegrationTestFixture fixture) : PlaywrightTest
{
    private IBrowserContext? Context { get; set; }

    protected IPage Page { get; private set; } = null!;

    public override async ValueTask InitializeAsync()
    {
        await base.InitializeAsync();

        var browser = await fixture.GetBrowserAsync();

        Context = await browser.NewContextAsync(new BrowserNewContextOptions
        {
            BaseURL = fixture.WebRootUri.ToString()
        });

        Page = await Context.NewPageAsync();
    }

    public override async ValueTask DisposeAsync()
    {
        if (Page is not null)
        {
            await Page.CloseAsync();
        }

        if (Context is not null)
        {
            await Context.CloseAsync();
        }

        await base.DisposeAsync();

        GC.SuppressFinalize(this);
    }
}
