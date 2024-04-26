using OpenQA.Selenium;
using OpenQA.Selenium.Firefox;
using WebDriverManager.DriverConfigs.Impl;

namespace TestesMasterData.Fixture
{
    public class TestFixture : IDisposable
    {
        public IWebDriver Driver;

        [SetUp]
        public void InicializarDriver()
        {
            new WebDriverManager.DriverManager().SetUpDriver(new FirefoxConfig());
            Driver = new FirefoxDriver();

            Driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(20);
            Driver.Manage().Window.Maximize();
        }

        public void Dispose()
        {
            Driver.Quit();
        }

    }
}
