using System.Net;
using JJMasterData.Brasil.Abstractions;
using JJMasterData.Brasil.Configuration;
using JJMasterData.Brasil.Exceptions;
using JJMasterData.Brasil.Models;
using JJMasterData.Brasil.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace JJMasterData.Brasil.Test;

public class CpfCnpjServiceTest
{
    private const string Cnpj = "33000167000101";
    private const string Cpf = "11144477735";

    private const string CnpjPackage6Json = """
        {
            "status": 1,
            "cnpj": "33.000.167/0001-01",
            "tipo": "Matriz",
            "razao": "TOKEN TEST LTDA",
            "fantasia": "TOKEN TEST",
            "capitalSocial": 95000,
            "inicioAtividade": "31/12/1999",
            "email": "contato@empresa.com",
            "matrizEndereco": {
                "cep": "0000-111",
                "logradouro": "Rua A",
                "numero": "1",
                "complemento": "Sala 1",
                "bairro": "Centro",
                "cidade": "Montes Claros",
                "uf": "MG"
            },
            "telefones": [ { "ddd": "11", "numero": "22334454" } ],
            "situacao": { "id": 4, "nome": "Inapta" },
            "cnae": { "fiscal": "6202300", "descricao": "Desenvolvimento e licenciamento de programas de computador customizaveis" },
            "porte": { "id": "03", "descricao": "Empresa de Pequeno Porte" },
            "socios": [ { "nome": "Joao Jose" }, { "nome": "Yakov Sever" } ],
            "pacoteUsado": 6
        }
        """;

    private const string CnpjPackage5Json = """
        {
            "status": 1,
            "cnpj": "33.000.167/0001-01",
            "tipo": "Matriz",
            "razao": "TOKEN TEST LTDA",
            "fantasia": "TOKEN TEST",
            "matrizEndereco": {
                "cep": "0000-111",
                "logradouro": "Rua A",
                "numero": "1",
                "complemento": "Sala 1",
                "bairro": "Centro",
                "cidade": "Montes Claros",
                "uf": "MG"
            },
            "pacoteUsado": 5
        }
        """;

    private const string CnpjErrorJson = """
        { "status": 0, "razao": null, "erro": "CNPJ invalido!", "pacoteUsado": 6, "erroCodigo": 200 }
        """;

    private const string CpfJson = """
        {
            "status": 1,
            "cpf": "111.444.777-35",
            "nome": "Test Token",
            "situacao": "REGULAR",
            "consultaID": "11bb22cc33dd44ee",
            "pacoteUsado": 2
        }
        """;

    [Fact]
    public async Task SearchCnpjAsync_Package6_MapsFullRegistration()
    {
        var handler = new StubHttpMessageHandler(CnpjPackage6Json);
        var service = CreateService(handler);

        var result = await service.SearchCnpjAsync(Cnpj);

        Assert.Equal("TOKEN TEST LTDA", result.Nome);
        Assert.Equal("TOKEN TEST", result.Fantasia);
        Assert.Equal("Rua A", result.Logradouro);
        Assert.Equal("0000111", result.Cep);
        Assert.Equal("Montes Claros", result.Municipio);
        Assert.Equal("MG", result.Uf);
        Assert.Equal("Inapta", result.Situacao);
        Assert.Equal("6202300", result.AtividadePrincipal.Code);
        Assert.Equal("95000", result.CapitalSocial);
        Assert.Equal("(11) 22334454", result.Telefone);
        Assert.Equal(new DateTime(1999, 12, 31), result.Abertura);
        Assert.NotNull(result.QuadroSocios);
        Assert.Equal(["Joao Jose", "Yakov Sever"], result.QuadroSocios);
    }

    [Fact]
    public async Task SearchCnpjAsync_BuildsTokenPackageDocumentUrl()
    {
        var handler = new StubHttpMessageHandler(CnpjPackage6Json);
        var service = CreateService(handler, new CpfCnpjSettings { ApiKey = "my-token", CnpjPackage = 6 });

        await service.SearchCnpjAsync("33.000.167/0001-01");

        Assert.Equal("https://api.cpfcnpj.com.br/my-token/6/33000167000101", handler.LastRequestUri);
    }

    [Fact]
    public async Task SearchCnpjAsync_Package5_MapsBasicDataWithoutStatus()
    {
        var handler = new StubHttpMessageHandler(CnpjPackage5Json);
        var service = CreateService(handler);

        var result = await service.SearchCnpjAsync(Cnpj);

        Assert.Equal("TOKEN TEST LTDA", result.Nome);
        Assert.Equal("Centro", result.Bairro);
        Assert.Equal(string.Empty, result.Situacao);
        Assert.Equal(string.Empty, result.CapitalSocial);
        Assert.Equal(string.Empty, result.AtividadePrincipal.Code);
        Assert.Equal(DateTime.MinValue, result.Abertura);
    }

    [Fact]
    public async Task SearchCnpjAsync_ErrorStatus_ThrowsReceitaFederalException()
    {
        var handler = new StubHttpMessageHandler(CnpjErrorJson);
        var service = CreateService(handler);

        var exception = await Assert.ThrowsAsync<ReceitaFederalException>(() => service.SearchCnpjAsync(Cnpj));

        Assert.Contains("CNPJ invalido!", exception.Message);
        Assert.Contains("200", exception.Message);
    }

    [Fact]
    public async Task SearchCnpjAsync_NonJsonResponse_ThrowsReceitaFederalException()
    {
        var handler = new StubHttpMessageHandler("<html>rate limited</html>");
        var service = CreateService(handler);

        await Assert.ThrowsAsync<ReceitaFederalException>(() => service.SearchCnpjAsync(Cnpj));
    }

    [Fact]
    public async Task SearchCpfAsync_MapsName()
    {
        var handler = new StubHttpMessageHandler(CpfJson);
        var service = CreateService(handler);

        var result = await service.SearchCpfAsync(Cpf, new DateTime(1900, 12, 31));

        Assert.Equal("Test Token", result.NomeDaPf);
        Assert.Equal("REGULAR", result.SituacaoCadastral);
        Assert.Equal("11bb22cc33dd44ee", result.ComprovanteEmitido);
    }

    [Fact]
    public async Task SearchCepAsync_DelegatesToCepService()
    {
        var handler = new StubHttpMessageHandler(CpfJson);
        var expected = new CepResult
        {
            Logradouro = "Alameda X",
            Bairro = "Centro",
            Localidade = "Campinas",
            Uf = "SP"
        };
        var service = CreateService(handler, cepService: new FakeCepService(expected));

        var result = await service.SearchCepAsync("13000000");

        Assert.Same(expected, result);
    }

    private static CpfCnpjService CreateService(
        StubHttpMessageHandler handler,
        CpfCnpjSettings? settings = null,
        ICepService? cepService = null)
    {
        var httpClient = new HttpClient(handler);
        var options = Options.Create(settings ?? new CpfCnpjSettings { ApiKey = "test-token" });
        return new CpfCnpjService(
            httpClient,
            cepService ?? new FakeCepService(null),
            options,
            NullLogger<CpfCnpjService>.Instance);
    }

    private sealed class StubHttpMessageHandler(string payload) : HttpMessageHandler
    {
        public string? LastRequestUri { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            LastRequestUri = request.RequestUri?.ToString();

            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(payload)
            };

            return Task.FromResult(response);
        }
    }

    private sealed class FakeCepService(CepResult? result) : ICepService
    {
        public Task<CepResult> SearchCepAsync(string cep)
        {
            return Task.FromResult(result ?? new CepResult
            {
                Logradouro = string.Empty,
                Bairro = string.Empty,
                Localidade = string.Empty,
                Uf = string.Empty
            });
        }
    }
}
