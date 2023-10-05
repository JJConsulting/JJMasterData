using JJMasterData.Brasil.Services;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace JJMasterData.Brasil.Test;

public class ViaCepServiceTest
{
    [Fact]
    public async Task SearchCepAsync_ValidCep_ReturnsExpectedAddress()
    {
        // Arrange
        const string expectedCep = "12941650";
        const string expectedLogradouro = "Alameda Professor Lucas Nogueira Garcez";

        var httpClient = new HttpClient();
        var viaCepService = new ViaCepService(httpClient, new MemoryCache(Options.Create(new MemoryCacheOptions())));

        // Act
        var cepResult = await viaCepService.SearchCepAsync(expectedCep);

        // Assert
        Assert.NotNull(cepResult);
        Assert.Equal(expectedLogradouro, cepResult.Logradouro);
    }
}