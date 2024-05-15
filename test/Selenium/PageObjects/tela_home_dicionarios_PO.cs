using IntegrationTests.Utils;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace TestesMasterData.PageObjects
{
    internal class tela_home_dicionarios_PO
    {
        private IWebDriver driver;

        private By ByBotaoAdicionarDicionario;
        private By ByNomeTabela;
        private By ByBotaoAdicionarTabela;
        private By ByBotaoSelecionarDicionario;
        private By ByBotaoExluirSelecionados;
        private By ByFiltroElemento;
        private By ByBotaoFiltrarElemento;
        private By ByBotaoAlterar;
        private By ByAlterarNomeTabela;
        private By ByBotaoSalvar;
        private By ByBotaoCampos;
        private By ByCampoNome;
        private By ByCampoLabel;
        private By ByCampoModoDoFiltro;
        private By ByCampoComportamento;
        private By ByCampoTipoDeDados;
        private By ByCampoTamanho;
        private By ByCampoPK;
        private By ByBotaoCampoSalvar;
        private By ByBotaoAdicionarCampo;
        private By ByComponente;
        private By ByCampoComponente;
        private By ByCampoGeral;
        private By ByCampoNumberCasasDecimais;
        private By ByBotaoMais;
        private By ByBotaoRenderizar;




        public tela_home_dicionarios_PO(IWebDriver driver)
        {
            this.driver = driver;

            ByBotaoAdicionarDicionario = By.CssSelector(".mb-1 > div:nth-child(1) > div:nth-child(1) > a:nth-child(1)");
            ByNomeTabela = By.Id("Name");
            ByBotaoAdicionarTabela = By.XPath("/html/body/div/form/div[4]/div/button[2]");
            ByFiltroElemento = By.Id("filter_name");
            ByBotaoExluirSelecionados = By.XPath("/html/body/div[1]/form/div/div/div[2]/div/div/button");
            ByBotaoFiltrarElemento = By.CssSelector("button.btn.btn-secondary[onclick*='GridViewFilterHelper.filter']");
            ByBotaoSelecionarDicionario = By.XPath("//*[@id=\"jjchk_0-checkbox\"]");
            ByBotaoAlterar = By.CssSelector(".fa-pencil");
            ByAlterarNomeTabela = By.Id("Entity_TableName");
            ByBotaoSalvar = By.CssSelector("button.btn");
            ByBotaoCampos = By.CssSelector("a[href*='/DataDictionary/Field/Index']");
            ByCampoNome = By.Id("Name");
            ByCampoLabel = By.Id("Label");
            ByCampoModoDoFiltro = By.Id("Filter_Type");
            ByCampoComportamento = By.Id("DataBehavior");
            ByCampoTipoDeDados = By.Id("DataType");
            ByCampoTamanho = By.Id("Size");
            ByCampoPK = By.CssSelector("#IsPk-checkbox");
            ByBotaoCampoSalvar = By.XPath("/html/body/div/form/div/div[2]/div/div[2]/a[1]");
            ByBotaoAdicionarCampo = By.CssSelector(".input-group-text");
            ByComponente = By.CssSelector("#nav-component > a:nth-child(1)");
            ByCampoComponente = By.Id("Component");
            ByCampoGeral = By.CssSelector("#nav-general");
            ByCampoNumberCasasDecimais = By.CssSelector("#NumberOfDecimalPlaces");
            ByBotaoMais = By.CssSelector("a.nav-link.dropdown-toggle");
            ByBotaoRenderizar = By.XPath("/html/body/div/ul/li[9]/ul/li[3]/a");

        }



        public void NavegarParaURL()
        {
            driver.Navigate().GoToUrl("http://localhost/masterdataINFINITY/pt-BR/DataDictionary/Element/Index");           
        }

        public void AdicionarDicionario()
        {
            driver.FindElement(ByFiltroElemento).SendKeys("TesteCampoString1");
            driver.FindElement(ByBotaoFiltrarElemento).Click();
            driver.FindElement(ByBotaoSelecionarDicionario).Click();
            driver.FindElement(ByBotaoExluirSelecionados).Click();
            driver.FindElement(ByBotaoAdicionarDicionario).Click();
            driver.FindElement(ByNomeTabela).SendKeys("TesteCampoString1");
            driver.FindElement(ByBotaoAdicionarTabela).Click();
            driver.FindElement(ByBotaoAlterar).Click();
            driver.FindElement(ByAlterarNomeTabela).Clear();
            driver.FindElement(ByAlterarNomeTabela).SendKeys("tb_teste");
            driver.FindElement(ByBotaoSalvar).Click();

                      
        }

        public void AdicionarCampoID()
        {
            driver.FindElement(ByBotaoCampos).Click();
            driver.FindElement(ByCampoNome).Click();
            driver.FindElement(ByCampoNome).SendKeys("ID");
            driver.FindElement(ByCampoLabel).SendKeys("ID");
            var selectModoDoFiltro = new SelectElement(driver.FindElement(ByCampoModoDoFiltro));
            selectModoDoFiltro.SelectByText("Equal"); 
            var selectComportamento = new SelectElement(driver.FindElement(ByCampoComportamento));
            selectComportamento.SelectByText("Real");
            var selectTipoDeDados = new SelectElement(driver.FindElement(ByCampoTipoDeDados));
            selectTipoDeDados.SelectByText("Int");
            driver.FindElement(ByCampoTamanho).Clear();
            driver.FindElement(ByCampoTamanho).SendKeys("0");
            driver.FindElement(ByCampoPK).Click();
            driver.FindElement(ByBotaoCampoSalvar).Click();
            driver.FindElement(ByComponente).Click();
            var selectComponent = new SelectElement(driver.FindElement(ByCampoComponente));
            selectComponent.SelectByText("Number");
            driver.FindElement(ByBotaoCampoSalvar).Click();
            driver.FindElement(ByCampoGeral).Click();

        }

        public void AdicionarCampoDesc()
        {

            driver.FindElement(ByBotaoAdicionarCampo).Click();
            driver.FindElement(ByCampoNome).Click();
            driver.FindElement(ByCampoNome).SendKeys("Descricao");
            driver.FindElement(ByCampoLabel).SendKeys("Descrição");
            var selectModoDoFiltro = new SelectElement(driver.FindElement(ByCampoModoDoFiltro));
            selectModoDoFiltro.SelectByText("None");
            var selectComportamento = new SelectElement(driver.FindElement(ByCampoComportamento));
            selectComportamento.SelectByText("Real");
            var selectTipoDeDados = new SelectElement(driver.FindElement(ByCampoTipoDeDados));
            selectTipoDeDados.SelectByText("Varchar");
            driver.FindElement(ByCampoTamanho).Clear();
            driver.FindElement(ByCampoTamanho).SendKeys("50");
            driver.FindElement(ByBotaoCampoSalvar).Click();
            driver.FindElement(ByComponente).Click();
            var selectComponent = new SelectElement(driver.FindElement(ByCampoComponente));
            selectComponent.SelectByText("TextBox");
            driver.FindElement(ByBotaoCampoSalvar).Click();
            driver.FindElement(ByCampoGeral).Click();
        }

        public void AdicionarCampoDataTeste()
        {

            driver.FindElement(ByBotaoAdicionarCampo).Click();
            driver.FindElement(ByCampoNome).Click();
            driver.FindElement(ByCampoNome).SendKeys("DataTeste");
            driver.FindElement(ByCampoLabel).SendKeys("Data Teste");
            var selectModoDoFiltro = new SelectElement(driver.FindElement(ByCampoModoDoFiltro));
            selectModoDoFiltro.SelectByText("None");
            var selectComportamento = new SelectElement(driver.FindElement(ByCampoComportamento));
            selectComportamento.SelectByText("Real");
            var selectTipoDeDados = new SelectElement(driver.FindElement(ByCampoTipoDeDados));
            selectTipoDeDados.SelectByText("Date");
            driver.FindElement(ByCampoTamanho).Clear();
            driver.FindElement(ByCampoTamanho).SendKeys("0");
            driver.FindElement(ByBotaoCampoSalvar).Click();
            driver.FindElement(ByComponente).Click();
            var selectComponent = new SelectElement(driver.FindElement(ByCampoComponente));
            selectComponent.SelectByText("Date");
            driver.FindElement(ByBotaoCampoSalvar).Click();
            driver.FindElement(ByCampoGeral).Click();
        }

        public void AdicionarCampoValor()
        {

            driver.FindElement(ByBotaoAdicionarCampo).Click();
            driver.FindElement(ByCampoNome).Click();
            driver.FindElement(ByCampoNome).SendKeys("Valor");
            driver.FindElement(ByCampoLabel).SendKeys("Valor");
            var selectModoDoFiltro = new SelectElement(driver.FindElement(ByCampoModoDoFiltro));
            selectModoDoFiltro.SelectByText("None");
            var selectComportamento = new SelectElement(driver.FindElement(ByCampoComportamento));
            selectComportamento.SelectByText("Real");
            var selectTipoDeDados = new SelectElement(driver.FindElement(ByCampoTipoDeDados));
            selectTipoDeDados.SelectByText("Float");
            driver.FindElement(ByCampoTamanho).Clear();
            driver.FindElement(ByCampoTamanho).SendKeys("8");
            driver.FindElement(ByBotaoCampoSalvar).Click();
            driver.FindElement(ByComponente).Click();
            var selectComponent = new SelectElement(driver.FindElement(ByCampoComponente));
            selectComponent.SelectByText("Number");
            driver.FindElement(ByCampoNumberCasasDecimais).Clear();
            driver.FindElement(ByCampoNumberCasasDecimais).SendKeys("2");
            driver.FindElement(ByBotaoCampoSalvar).Click();
            driver.FindElement(ByCampoGeral).Click();

        }

        public void AdicionarCampoCep()
        {

            driver.FindElement(ByBotaoAdicionarCampo).Click();
            driver.FindElement(ByCampoNome).Click();
            driver.FindElement(ByCampoNome).SendKeys("CEP");
            driver.FindElement(ByCampoLabel).SendKeys("CEP");
            var selectModoDoFiltro = new SelectElement(driver.FindElement(ByCampoModoDoFiltro));
            selectModoDoFiltro.SelectByText("None");
            var selectComportamento = new SelectElement(driver.FindElement(ByCampoComportamento));
            selectComportamento.SelectByText("Real");
            var selectTipoDeDados = new SelectElement(driver.FindElement(ByCampoTipoDeDados));
            selectTipoDeDados.SelectByText("Varchar");
            driver.FindElement(ByCampoTamanho).Clear();
            driver.FindElement(ByCampoTamanho).SendKeys("8");
            driver.FindElement(ByBotaoCampoSalvar).Click();
            driver.FindElement(ByComponente).Click();
            var selectComponent = new SelectElement(driver.FindElement(ByCampoComponente));
            selectComponent.SelectByText("CEP");
            driver.FindElement(ByBotaoCampoSalvar).Click();
            driver.FindElement(ByCampoGeral).Click();
        }

        public void AdicionarCampoCNPJ()
        {

            driver.FindElement(ByBotaoAdicionarCampo).Click();
            driver.FindElement(ByCampoNome).Click();
            driver.FindElement(ByCampoNome).SendKeys("CNPJ");
            driver.FindElement(ByCampoLabel).SendKeys("CNPJ");
            var selectModoDoFiltro = new SelectElement(driver.FindElement(ByCampoModoDoFiltro));
            selectModoDoFiltro.SelectByText("None");
            var selectComportamento = new SelectElement(driver.FindElement(ByCampoComportamento));
            selectComportamento.SelectByText("Real");
            var selectTipoDeDados = new SelectElement(driver.FindElement(ByCampoTipoDeDados));
            selectTipoDeDados.SelectByText("Varchar");
            driver.FindElement(ByCampoTamanho).Clear();
            driver.FindElement(ByCampoTamanho).SendKeys("14");
            driver.FindElement(ByBotaoCampoSalvar).Click();
            driver.FindElement(ByComponente).Click();
            var selectComponent = new SelectElement(driver.FindElement(ByCampoComponente));
            selectComponent.SelectByText("CNPJ");
            driver.FindElement(ByBotaoCampoSalvar).Click();
            driver.FindElement(ByCampoGeral).Click();
        }

        public void AdicionarCampoCNPJ_CPF()
        {

            driver.FindElement(ByBotaoAdicionarCampo).Click();
            driver.FindElement(ByCampoNome).Click();
            driver.FindElement(ByCampoNome).SendKeys("CNPJ_CPF");
            driver.FindElement(ByCampoLabel).SendKeys("CNPJ/CPF");
            var selectModoDoFiltro = new SelectElement(driver.FindElement(ByCampoModoDoFiltro));
            selectModoDoFiltro.SelectByText("None");
            var selectComportamento = new SelectElement(driver.FindElement(ByCampoComportamento));
            selectComportamento.SelectByText("Real");
            var selectTipoDeDados = new SelectElement(driver.FindElement(ByCampoTipoDeDados));
            selectTipoDeDados.SelectByText("Varchar");
            driver.FindElement(ByCampoTamanho).Clear();
            driver.FindElement(ByCampoTamanho).SendKeys("14");
            driver.FindElement(ByBotaoCampoSalvar).Click();
            driver.FindElement(ByComponente).Click();
            var selectComponent = new SelectElement(driver.FindElement(ByCampoComponente));
            selectComponent.SelectByText("CNPJ/CPF");
            driver.FindElement(ByBotaoCampoSalvar).Click();
            driver.FindElement(ByCampoGeral).Click();
        }

        public void AdicionarCampoCPF()
        {

            driver.FindElement(ByBotaoAdicionarCampo).Click();
            driver.FindElement(ByCampoNome).Click();
            driver.FindElement(ByCampoNome).SendKeys("CPF");
            driver.FindElement(ByCampoLabel).SendKeys("CPF");
            var selectModoDoFiltro = new SelectElement(driver.FindElement(ByCampoModoDoFiltro));
            selectModoDoFiltro.SelectByText("None");
            var selectComportamento = new SelectElement(driver.FindElement(ByCampoComportamento));
            selectComportamento.SelectByText("Real");
            var selectTipoDeDados = new SelectElement(driver.FindElement(ByCampoTipoDeDados));
            selectTipoDeDados.SelectByText("Varchar");
            driver.FindElement(ByCampoTamanho).Clear();
            driver.FindElement(ByCampoTamanho).SendKeys("20");
            driver.FindElement(ByBotaoCampoSalvar).Click();
            driver.FindElement(ByComponente).Click();
            var selectComponent = new SelectElement(driver.FindElement(ByCampoComponente));
            selectComponent.SelectByText("CPF");
            driver.FindElement(ByBotaoCampoSalvar).Click();
            driver.FindElement(ByCampoGeral).Click();
        }

        public void AdicionarCampoEmail()
        {

            driver.FindElement(ByBotaoAdicionarCampo).Click();
            driver.FindElement(ByCampoNome).Click();
            driver.FindElement(ByCampoNome).SendKeys("Email");
            driver.FindElement(ByCampoLabel).SendKeys("Email");
            var selectModoDoFiltro = new SelectElement(driver.FindElement(ByCampoModoDoFiltro));
            selectModoDoFiltro.SelectByText("None");
            var selectComportamento = new SelectElement(driver.FindElement(ByCampoComportamento));
            selectComportamento.SelectByText("Real");
            var selectTipoDeDados = new SelectElement(driver.FindElement(ByCampoTipoDeDados));
            selectTipoDeDados.SelectByText("Varchar");
            driver.FindElement(ByCampoTamanho).Clear();
            driver.FindElement(ByCampoTamanho).SendKeys("100");
            driver.FindElement(ByBotaoCampoSalvar).Click();
            driver.FindElement(ByComponente).Click();
            var selectComponent = new SelectElement(driver.FindElement(ByCampoComponente));
            selectComponent.SelectByText("Email");
            driver.FindElement(ByBotaoCampoSalvar).Click();
            driver.FindElement(ByCampoGeral).Click();
        }

        public void AdicionarCampoPassword()
        {

            driver.FindElement(ByBotaoAdicionarCampo).Click();
            driver.FindElement(ByCampoNome).Click();
            driver.FindElement(ByCampoNome).SendKeys("Password");
            driver.FindElement(ByCampoLabel).SendKeys("Password");
            var selectModoDoFiltro = new SelectElement(driver.FindElement(ByCampoModoDoFiltro));
            selectModoDoFiltro.SelectByText("None");
            var selectComportamento = new SelectElement(driver.FindElement(ByCampoComportamento));
            selectComportamento.SelectByText("Real");
            var selectTipoDeDados = new SelectElement(driver.FindElement(ByCampoTipoDeDados));
            selectTipoDeDados.SelectByText("Varchar");
            driver.FindElement(ByCampoTamanho).Clear();
            driver.FindElement(ByCampoTamanho).SendKeys("50");
            driver.FindElement(ByBotaoCampoSalvar).Click();
            driver.FindElement(ByComponente).Click();
            var selectComponent = new SelectElement(driver.FindElement(ByCampoComponente));
            selectComponent.SelectByText("Password");
            driver.FindElement(ByBotaoCampoSalvar).Click();
            driver.FindElement(ByCampoGeral).Click();
        }

        public void AdicionarCampoPhone()
        {

            driver.FindElement(ByBotaoAdicionarCampo).Click();
            driver.FindElement(ByCampoNome).Click();
            driver.FindElement(ByCampoNome).SendKeys("Phone");
            driver.FindElement(ByCampoLabel).SendKeys("Phone");
            var selectModoDoFiltro = new SelectElement(driver.FindElement(ByCampoModoDoFiltro));
            selectModoDoFiltro.SelectByText("None");
            var selectComportamento = new SelectElement(driver.FindElement(ByCampoComportamento));
            selectComportamento.SelectByText("Real");
            var selectTipoDeDados = new SelectElement(driver.FindElement(ByCampoTipoDeDados));
            selectTipoDeDados.SelectByText("Varchar");
            driver.FindElement(ByCampoTamanho).Clear();
            driver.FindElement(ByCampoTamanho).SendKeys("10");
            driver.FindElement(ByBotaoCampoSalvar).Click();
            driver.FindElement(ByComponente).Click();
            var selectComponent = new SelectElement(driver.FindElement(ByCampoComponente));
            selectComponent.SelectByText("Phone");
            driver.FindElement(ByBotaoCampoSalvar).Click();
            driver.FindElement(ByCampoGeral).Click();
        }

        public void Renderizar()
        {
            driver.FindElement(ByBotaoMais).Click();
            driver.FindElement(ByBotaoRenderizar).Click();
        }

    }
}
