using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using JJMasterData.Brasil.Abstractions;
using JJMasterData.Brasil.Configuration;
using JJMasterData.Brasil.Exceptions;
using JJMasterData.Brasil.Helpers;
using JJMasterData.Brasil.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace JJMasterData.Brasil.Services;

public class HubDevService(HttpClient httpClient, IOptions<HubDevSettings> options)
    : IReceitaFederalService
{
    private readonly HubDevSettings _settings = options.Value;
    private readonly HttpClient _httpClient;
    private readonly ILogger<HubDevService> _logger;

    public HubDevService(HubDevSettings settings, HttpClient httpClient, ILogger<HubDevService> logger, bool isHttps, bool ignoreDb)
        : this(httpClient, Options.Create(settings))
    {
        _settings = settings;
        _httpClient = httpClient;
        _logger = logger;
        IsHttps = isHttps;
        IgnoreDb = ignoreDb;
    }

    private static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters =
        {
            new CustomDateConverter("dd/MM/yyyy")
        }
    };

    public bool IsHttps { get; set; } = true;

    public bool IgnoreDb { get; set; }

    private async Task<T> Search<T>(string endpoint, string identifier, Dictionary<string, string>? additionalParameters = null)
    {
        _logger.LogInformation("Searching search for {Endpoint} with identifier {Identifier}", endpoint, identifier);

        try
        {
            if (string.IsNullOrEmpty(identifier))
            {
                _logger.LogWarning("Identifier is null or empty for endpoint {Endpoint}", endpoint);
                throw new ArgumentNullException(nameof(identifier));
            }

            var protocol = IsHttps ? "https://" : "http://";
            var ignoreDb = IgnoreDb ? "&ignore_db=1" : "";
            var url = $"{protocol}{_settings.Url}{endpoint}/?{endpoint}={identifier}&token={_settings.ApiKey}{ignoreDb}";

            if (additionalParameters is { Count: > 0 })
            {
                var additionalQueryString = string.Join("&", additionalParameters.Select(kv => $"{kv.Key}={kv.Value}"));
                url = $"{url}&{additionalQueryString}";
            }

            var message = await httpClient.GetAsync(url);
            var content = await message.Content.ReadAsStringAsync();

            _logger.LogInformation("JSON returned by HubDev for {Endpoint} with identifier {Identifier}: {Content}", endpoint, identifier, content);

            var apiResult = JsonSerializer.Deserialize<JsonObject>(content, JsonSerializerOptions);

            var result = JsonSerializer.Deserialize<T>(apiResult!["result"]!.ToString(), JsonSerializerOptions)!;

            _logger.LogInformation("{Enpoint} found successfully", endpoint);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during search for {Endpoint} with identifier {Identifier}", endpoint, identifier);
            throw new ReceitaFederalException(ex.Message, ex);
        }
    }

    public Task<CnpjResult> SearchCnpjAsync(string cnpj)
    {
        return Search<CnpjResult>("cnpj", cnpj);
    }

    public Task<CpfResult> SearchCpfAsync(string cpf, DateTime birthDate)
    {
        return Search<CpfResult>("cpf", cpf, new Dictionary<string, string>
        {
            {"data", birthDate.ToString("dd/MM/yyyy")}
        });
    }


    public Task<CepResult> SearchCepAsync(string cep)
    {
        return Search<CepResult>("cep", cep);
    }
}