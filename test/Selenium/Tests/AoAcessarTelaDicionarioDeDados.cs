using IntegrationTests.PageObjects;
using OpenQA.Selenium;
using TestesMasterData.Fixture;
using TestesMasterData.PageObjects;

namespace TestesMasterData.Tests;
public class AoAcessarTelaDicionarioDeDados : TestFixture
{

    private tela_home_dicionarios_PO _telahomedicionariosPo;
    private tela_renderizacao_dicionarios_PO _telarederizacaodicionariosPo;

    public void Setup(TestFixture fixture)
    {
        fixture.Driver = Driver;

    }

    private void SetupMasterData()
    {

        _telahomedicionariosPo = new tela_home_dicionarios_PO(Driver);
        _telarederizacaodicionariosPo = new tela_renderizacao_dicionarios_PO(Driver);

    }

    [Test]

    public void NavegarParaURL()
    {
        SetupMasterData();

        _telahomedicionariosPo.NavegarParaURL();

    }

    [Test]
    public void AdicionarDicionario()
    {
        SetupMasterData();

        _telahomedicionariosPo.NavegarParaURL(); 
        _telahomedicionariosPo.AdicionarDicionario();
        _telahomedicionariosPo.AdicionarCampoID();
        _telahomedicionariosPo.AdicionarCampoDesc();
        _telahomedicionariosPo.AdicionarCampoDataTeste();
        _telahomedicionariosPo.AdicionarCampoValor();
        _telahomedicionariosPo.AdicionarCampoCep();
        _telahomedicionariosPo.AdicionarCampoCNPJ();
        _telahomedicionariosPo.AdicionarCampoCNPJ_CPF();
        _telahomedicionariosPo.AdicionarCampoCPF();
        _telahomedicionariosPo.AdicionarCampoEmail();
        _telahomedicionariosPo.AdicionarCampoPassword();
        _telahomedicionariosPo.AdicionarCampoPhone();
        _telahomedicionariosPo.Renderizar();
    }


    [Test]

    public void AdicionarRegistros()
    {
        SetupMasterData();

        _telahomedicionariosPo.NavegarParaURL();
        _telarederizacaodicionariosPo.RenderizarDicionario();
    }


    [Test]

    public void ExportarRegistros()
    {
        SetupMasterData();

        _telahomedicionariosPo.NavegarParaURL();
        _telarederizacaodicionariosPo.RenderizarDicionario();
        _telarederizacaodicionariosPo.ExportarRegistros();
    }

}









