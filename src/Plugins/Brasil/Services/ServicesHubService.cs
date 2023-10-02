using System;
using System.Net.Http;
using System.Threading.Tasks;
using JJMasterData.Brasil.Abstractions;
using JJMasterData.Brasil.Exceptions;
using JJMasterData.Brasil.Models;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace JJMasterData.Brasil.Services;

public class ServicesHubService : IReceitaFederalService
{
    private HttpClient HttpClient { get; }
    private ServicesHubSettings Settings { get; }
    
    
    public ServicesHubService(
        HttpClient httpClient,
        IOptions<ServicesHubSettings> options)
    {
        HttpClient = httpClient;
        Settings = options.Value;
    }
    
    public bool IsHttps { get; set; }
    
    public bool IgnoreDb { get; set; }
    
    public async Task<T> Search<T>(string identifier, string endpoint)
    {
        try
        {
            if (string.IsNullOrEmpty(identifier))
                throw new ArgumentNullException(nameof(identifier));
        
            string protocol = IsHttps ? "https://" : "http://";
            string ignoreDb = IgnoreDb ? "&ignore_db=1" : "";
            string url = $"{protocol}{Settings.Url}{endpoint}/{identifier}?token={Settings.Key}{ignoreDb}";
        
            var message = await HttpClient.GetAsync(url);
            var content = await message.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<JObject>(content);
            return result["result"]!.ToObject<T>();
        }
        catch (Exception ex)
        {
            throw new ReceitaFederalException(ex.Message, ex);
        }
    }

    public async Task<CnpjResult> SearchCnpjAsync(string cnpj)
    {
        return await Search<CnpjResult>(cnpj, "cnpj");
    }

    public async Task<CpfResult> SearchCpfAsync(string cpf, DateTime birthDate)
    {
        return await Search<CpfResult>(cpf, "cpf");
    }


    public async Task<CepResult> SearchCepAsync(string cep)
    {
        return await Search<CepResult>(cep, "cep");
    }
    
        
}