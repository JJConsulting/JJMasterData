using JJMasterData.Brasil.Actions;
using JJMasterData.Brasil.Abstractions;
using JJMasterData.Brasil.Configuration;
using JJMasterData.Brasil.Models;
using JJMasterData.Brasil.Services;
using JJMasterData.Commons.Configuration;
using JJMasterData.Core.DataDictionary.Models.Actions;
using JJMasterData.Core.DataManager.Expressions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace JJMasterData.Brasil.Test;

public class BrasilConfigurationTests
{
    [Theory]
    [InlineData("JJMasterData:HubDev:ApiKey", "test-hubdev-key", typeof(HubDevService))]
    [InlineData("JJMasterData:Sintegra:ApiKey", "test-sintegra-key", typeof(SintegraService))]
    public void WithBrasilActionPlugins_RegistersSelectedReceitaFederalService(string selectedKey, string selectedValue, Type expectedType)
    {
        var services = new ServiceCollection();
        services.AddLogging();

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                [selectedKey] = selectedValue
            })
            .Build();

        var builder = new MasterDataServiceBuilder(services);

        builder.WithBrasilActionPlugins(configuration);

        var serviceDescriptor = services.Last(sd => sd.ServiceType == typeof(IReceitaFederalService));

        Assert.Equal(expectedType, serviceDescriptor.ImplementationType);
    }

    [Fact]
    public void CpfPluginActionHandler_HidesIgnoreDbField_WhenServiceDoesNotSupportIt()
    {
        var handler = new CpfPluginActionHandler(new FakeReceitaFederalService(false), null!);

        var fieldNames = handler.ConfigurationFields!.Select(field => field.Name).ToArray();

        Assert.Contains("BirthDate", fieldNames);
        Assert.DoesNotContain("IgnoreDb", fieldNames);
    }

    [Fact]
    public void CpfPluginActionHandler_ShowsIgnoreDbField_WhenServiceSupportsIt()
    {
        var handler = new CpfPluginActionHandler(new FakeReceitaFederalService(true), null!);

        var fieldNames = handler.ConfigurationFields!.Select(field => field.Name).ToArray();

        Assert.Contains("BirthDate", fieldNames);
        Assert.Contains("IgnoreDb", fieldNames);
    }

    private sealed class FakeReceitaFederalService(bool supportsIgnoreDb) : IReceitaFederalService
    {
        public bool IgnoreDb { get; set; }
        public bool SupportsIgnoreDb { get; } = supportsIgnoreDb;
        public int TimeoutSeconds { get; set; }
        public bool IsHttps { get; set; }

        public Task<CnpjResult> SearchCnpjAsync(string cnpj) => Task.FromResult(new CnpjResult
        {
            Nome = string.Empty,
            Fantasia = string.Empty,
            Logradouro = string.Empty,
            Numero = string.Empty,
            Complemento = string.Empty,
            Cep = string.Empty,
            Bairro = string.Empty,
            Municipio = string.Empty,
            Uf = string.Empty,
            Email = string.Empty,
            Telefone = string.Empty,
            Situacao = string.Empty,
            AtividadePrincipal = new CnaeResult
            {
                Code = string.Empty,
                Text = string.Empty
            },
            CapitalSocial = string.Empty,
            Abertura = DateTime.MinValue
        });

        public Task<CpfResult> SearchCpfAsync(string cpf, DateTime birthDate) => Task.FromResult(new CpfResult
        {
            NomeDaPf = string.Empty,
            SituacaoCadastral = string.Empty,
            ComprovanteEmitido = string.Empty
        });

        public Task<CepResult> SearchCepAsync(string cep) => Task.FromResult(new CepResult
        {
            Logradouro = string.Empty,
            Bairro = string.Empty,
            Localidade = string.Empty,
            Uf = string.Empty
        });
    }
}
