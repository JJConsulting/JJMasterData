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

public class HubDevService(HttpClient httpClient,
        IMemoryCache memoryCache,
        IOptions<HubDevSettings> options)
    : IReceitaFederalService
{
    private HttpClient HttpClient { get; } = httpClient;
    private IMemoryCache MemoryCache { get; } = memoryCache;
    private HubDevSettings Settings { get; } = options.Value;


    public bool IsHttps { get; set; } = true;
    
    public bool IgnoreDb { get; set; }

    private async Task<T> Search<T>(string endpoint, string identifier, Dictionary<string,string>? additionalParameters = null)
    {
        try
        {
            if (string.IsNullOrEmpty(identifier))
                throw new ArgumentNullException(nameof(identifier));

            if (MemoryCache.TryGetValue(identifier, out T cacheResult) && !IgnoreDb)
                return cacheResult;
            
            var protocol = IsHttps ? "https://" : "http://";
            var ignoreDb = IgnoreDb ? "&ignore_db=1" : "";
            var url = $"{protocol}{Settings.Url}{endpoint}/?{endpoint}={identifier}&token={Settings.ApiKey}{ignoreDb}";
            
            if (additionalParameters is { Count: > 0 })
            {
                var additionalQueryString = string.Join("&", additionalParameters.Select(kv => $"{kv.Key}={kv.Value}"));
                url = $"{url}&{additionalQueryString}";
            }
            
            var message = await HttpClient.GetAsync(url);
            var content = await message.Content.ReadAsStringAsync();
            
            var apiResult = JsonConvert.DeserializeObject<JObject>(content, new JsonSerializerSettings()
            {
                DateFormatString = "dd/MM/yyyy"
            });
            
            var result = apiResult!["result"]!.ToObject<T>()!;

            MemoryCache.Set<T>(identifier, result, TimeSpan.FromMinutes(30));

            return result;
        }
        catch (Exception ex)
        {
            throw new ReceitaFederalException(ex.Message, ex);
        }
    }

    public async Task<CnpjResult> SearchCnpjAsync(string cnpj)
    {
        return await Search<CnpjResult>("cnpj", cnpj);
    }

    public async Task<CpfResult> SearchCpfAsync(string cpf, DateTime birthDate)
    {
        return await Search<CpfResult>("cpf", cpf, new Dictionary<string, string>
        {
            {"data", birthDate.ToString("ddMMyyyy")}
        });
    }


    public async Task<CepResult> SearchCepAsync(string cep)
    {
        return await Search<CepResult>("cep", cep);
    }
}