using System;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using JJMasterData.Brasil.Abstractions;
using JJMasterData.Brasil.Configuration;
using JJMasterData.Brasil.Exceptions;
using JJMasterData.Brasil.Models;
using JJMasterData.Commons.Util;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace JJMasterData.Brasil.Services;

public class CpfCnpjService(
    HttpClient httpClient,
    ICepService cepService,
    IOptions<CpfCnpjSettings> options,
    ILogger<CpfCnpjService> logger)
    : IReceitaFederalService
{
    private readonly CpfCnpjSettings _settings = options.Value;

    private static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        NumberHandling = JsonNumberHandling.AllowReadingFromString
    };

    public bool IsHttps { get; set; } = true;

    public int TimeoutSeconds { get; set; } = 5;

    public bool IgnoreDb { get; set; }

    public bool SupportsIgnoreDb => false;

    public async Task<CnpjResult> SearchCnpjAsync(string cnpj)
    {
        if (string.IsNullOrEmpty(cnpj))
        {
            logger.LogWarning("CNPJ is null or empty.");
            throw new ArgumentNullException(nameof(cnpj));
        }

        try
        {
            var document = StringManager.ClearCpfCnpjChars(cnpj);
            var content = await GetAsync(_settings.CnpjPackage, document);

            var dto = Deserialize<CpfCnpjCnpjDto>(content);
            EnsureSuccess(dto.Status, dto.Erro, dto.ErroCodigo);

            return dto.ToCnpjResult();
        }
        catch (ReceitaFederalException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new ReceitaFederalException(ex.Message, ex);
        }
    }

    public async Task<CpfResult> SearchCpfAsync(string cpf, DateTime birthDate)
    {
        if (string.IsNullOrEmpty(cpf))
        {
            logger.LogWarning("CPF is null or empty.");
            throw new ArgumentNullException(nameof(cpf));
        }

        // The cpfcnpj.com.br CPF endpoint is path based ({token}/{package}/{document}) and does not
        // take a birth date. Sending one makes the API reject the request, so birthDate (required by
        // the IReceitaFederalService contract for providers such as HubDev/Sintegra) is not forwarded.
        _ = birthDate;

        try
        {
            var document = StringManager.ClearCpfCnpjChars(cpf);
            var content = await GetAsync(_settings.CpfPackage, document);

            var dto = Deserialize<CpfCnpjCpfDto>(content);
            EnsureSuccess(dto.Status, dto.Erro, dto.ErroCodigo);

            return dto.ToCpfResult();
        }
        catch (ReceitaFederalException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new ReceitaFederalException(ex.Message, ex);
        }
    }

    public Task<CepResult> SearchCepAsync(string cep)
    {
        return cepService.SearchCepAsync(cep);
    }

    private async Task<string> GetAsync(int package, string document)
    {
        var protocol = IsHttps ? "https://" : "http://";
        var url = $"{protocol}{_settings.Url}{_settings.ApiKey}/{package}/{document}";

        using var cts = new CancellationTokenSource(
            TimeSpan.FromSeconds(TimeoutSeconds == 0 ? 5 : TimeoutSeconds));

        var message = await httpClient.GetAsync(url, cts.Token);

#if NET
        var content = await message.Content.ReadAsStringAsync(cts.Token);
#else
        var content = await message.Content.ReadAsStringAsync();
#endif

        if (!message.IsSuccessStatusCode)
        {
            var exception = new ReceitaFederalException($"Invalid status code ({message.StatusCode}).");
            logger.LogError(exception, "Error at CpfCnpjService. Response: {Response}", content);
            throw exception;
        }

        return content;
    }

    private static T Deserialize<T>(string content) where T : class
    {
        var result = JsonSerializer.Deserialize<T>(content, JsonSerializerOptions);

        if (result is null)
            throw new ReceitaFederalException("The cpfcnpj.com.br API returned an empty or invalid response.");

        return result;
    }

    private static void EnsureSuccess(int status, string? error, int? errorCode)
    {
        if (status == 1)
            return;

        var message = string.IsNullOrWhiteSpace(error)
            ? "The cpfcnpj.com.br API returned an error."
            : error;

        if (errorCode.HasValue)
            message = $"{message} (code {errorCode.Value})";

        throw new ReceitaFederalException(message);
    }
}
