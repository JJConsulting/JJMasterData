using IntegrationTests.Utils;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;

namespace IntegrationTests.PageObjects
{
    internal class tela_renderizacao_dicionarios_PO
    {
        private IWebDriver driver;

        private By ByFiltroElemento;
        private By ByBotaoFiltrarElemento;
        private By ByBotaoRenderizarDicionario;
        private By ByBotaoAdicionarRegistros;
        private By ByBotaoExportarRegistros;
        private By ByBotaoExportacaoRegistros;
        private By ByLinkExportacaoRegistros;
        private By ByBotaoExportar;
        private WebDriverWait wait;


        public tela_renderizacao_dicionarios_PO(IWebDriver driver)
        {
            this.driver = driver;

            ByFiltroElemento = By.Id("filter_name");
            ByBotaoFiltrarElemento = By.XPath("/html/body/div/form/div/div/div[2]/div/div/div/div/div/div[2]/div/button");
            ByBotaoRenderizarDicionario = By.XPath("/html/body/div[1]/form/div/div/div[4]/div[1]/table/tbody/tr/td[7]/a/span");
            ByBotaoAdicionarRegistros = By.XPath("/html/body/div[1]/form/div/div/div[2]/div/div/a[1]");
            ByBotaoExportarRegistros = MasterDataUtils.AbrirPopUpExportar();
            ByBotaoExportar = By.CssSelector("a.float-end:nth-child(3)");
            ByBotaoExportacaoRegistros = MasterDataUtils.ExecutarExportacao();
            ByLinkExportacaoRegistros = MasterDataUtils.GetLinkExportacao("export_link_testecampostring1");
            wait = new WebDriverWait(driver, TimeSpan.FromSeconds(15));


        }


        public void RenderizarDicionario()
        {
            driver.FindElement(ByFiltroElemento).SendKeys("TesteCampoString1");
            driver.FindElement(ByBotaoFiltrarElemento).Click();
            driver.FindElement(ByBotaoRenderizarDicionario).Click();
        }

        public void CadastrarRegistros()
        {
            driver.FindElement(ByBotaoAdicionarRegistros).Click();
        }

        public void ExportarRegistros()
        {
         //   MasterDataUtils.AbrirPopUpExportar();

 //           driver.FindElement(By.("div>a[class='float-end btn btn-secondary'][data-bs-original-title='Exportar']")).Click(); 

            driver.FindElement(ByBotaoExportar).Click();

            wait.Until(ExpectedConditions.ElementIsVisible(MasterDataUtils.ExecutarExportacao()));

            driver.FindElement(MasterDataUtils.ExecutarExportacao()).Click();

            try
            {
                wait.Until(ExpectedConditions.ElementIsVisible(MasterDataUtils.GetLinkExportacao("")));
                Console.WriteLine("O link de exportação está presente. Exportação feita.");
            }
            catch (WebDriverTimeoutException)
            {
                Console.WriteLine("O link de exportação não está visível. Exportação falhou");
                throw;
            }

        }

    }
}