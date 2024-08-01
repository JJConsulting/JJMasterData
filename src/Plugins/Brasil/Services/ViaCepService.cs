using System;
using System.Net.Http;
using System.Threading.Tasks;
using JJMasterData.Brasil.Abstractions;
using JJMasterData.Brasil.Exceptions;
using JJMasterData.Brasil.Models;
using JJMasterData.Commons.Util;
using Microsoft.Extensions.Caching.Memory;

namespace JJMasterData.Brasil.Services;

public class ViaCepService(HttpClient httpClient) : ICepService
{
    private const string ViaCepUrl = "https://viacep.com.br/ws/";

    public async Task<CepResult> SearchCepAsync(string cep)
    {
        try
        {
            if (string.IsNullOrEmpty(cep))
                throw new ArgumentNullException(nameof(cep));

            var url = $"{ViaCepUrl}{StringManager.ClearCpfCnpjChars(cep)}/json";

            var response = await httpClient.GetAsync(url);
            var content = await response.Content.ReadAsStringAsync();
            var result = CepResult.FromJson(content);

            if (result.Erro)
                throw new ViaCepException("CEP inválido.");

            return result;
        }
        catch (Exception ex)
        {
            throw new ViaCepException(ex.Message, ex);
        }
    }
}