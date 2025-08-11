using System;
using System.Net.Http;
using System.Threading.Tasks;
using JJMasterData.Brasil.Abstractions;
using JJMasterData.Brasil.Exceptions;
using JJMasterData.Brasil.Models;
using JJMasterData.Commons.Util;
using Microsoft.Extensions.Logging;

namespace JJMasterData.Brasil.Services;

public class ViaCepService(HttpClient httpClient, ILogger<ViaCepService> logger) : ICepService
{
    private const string ViaCepUrl = "https://viacep.com.br/ws/";

    public async Task<CepResult> SearchCepAsync(string cep)
    {
        logger.LogInformation("Searching CEP: {Cep}", cep);

        try
        {
            if (string.IsNullOrEmpty(cep))
            {
                logger.LogWarning("CEP is null or empty.");
                throw new ArgumentNullException(nameof(cep));
            }

            var url = $"{ViaCepUrl}{StringManager.ClearCpfCnpjChars(cep)}/json";

            var response = await httpClient.GetAsync(url);
            var content = await response.Content.ReadAsStringAsync();
            var result = CepResult.FromJson(content);

            logger.LogInformation("Returnerd by VIACEP for the: {Cep}: {Content}", cep, content);

            if (result.Erro)
            {
                logger.LogWarning("CEP {Cep} not found in the ViaCep database.", cep);
                throw new ViaCepException("Invalid CEP.");
            }

                logger.LogInformation("CEP {Cep} found successfully.", cep);
            return result;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error when checking CEP {cep}", cep);
            throw new ViaCepException(ex.Message, ex);
        }
    }
}