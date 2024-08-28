using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using JJMasterData.Brasil.Abstractions;
using JJMasterData.Brasil.Configuration;
using JJMasterData.Brasil.Exceptions;
using JJMasterData.Brasil.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace JJMasterData.Brasil.Services;

public class HubDevService(HttpClient httpClient, IOptions<HubDevSettings> options)
    : IReceitaFederalService
{
    private readonly HubDevSettings _settings = options.Value;
    
    public bool IsHttps { get; set; } = true;
    
    public bool IgnoreDb { get; set; }

    private async Task<T> Search<T>(string endpoint, string identifier, Dictionary<string,string>? additionalParameters = null)
    {
        try
        {
            if (string.IsNullOrEmpty(identifier))
                throw new ArgumentNullException(nameof(identifier));
            
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
            
            var apiResult = JsonConvert.DeserializeObject<JObject>(content, new JsonSerializerSettings()
            {
                DateFormatString = "dd/MM/yyyy"
            });
            
            var result = apiResult!["result"]!.ToObject<T>()!;
            
            return result;
        }
        catch (Exception ex)
        {
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