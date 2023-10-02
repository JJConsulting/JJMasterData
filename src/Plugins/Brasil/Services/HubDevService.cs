using System;
using System.Net.Http;
using System.Threading.Tasks;
using JJMasterData.Brasil.Abstractions;
using JJMasterData.Brasil.Configuration;
using JJMasterData.Brasil.Exceptions;
using JJMasterData.Brasil.Models;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace JJMasterData.Brasil.Services;

public class HubDevService : IReceitaFederalService
{
    private HttpClient HttpClient { get; }
    private HubDevSettings Settings { get; }
    
    
    public HubDevService(
        HttpClient httpClient,
        IOptions<HubDevSettings> options)
    {
        HttpClient = httpClient;
        Settings = options.Value;
    }
    
    public bool IsHttps { get; set; } = true;
    
    public bool IgnoreDb { get; set; }
    
    public async Task<T> Search<T>(string endpoint, string identifier)
    {
        try
        {
            if (string.IsNullOrEmpty(identifier))
                throw new ArgumentNullException(nameof(identifier));
        
            string protocol = IsHttps ? "https://" : "http://";
            string ignoreDb = IgnoreDb ? "&ignore_db=1" : "";
            string url = $"{protocol}{Settings.Url}{endpoint}/?{endpoint}={identifier}&token={Settings.ApiKey}{ignoreDb}";
        
            var message = await HttpClient.GetAsync(url);
            var content = await message.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<JObject>(content, new JsonSerializerSettings()
            {
                DateFormatString = "dd/MM/yyyy"
            });
            return result!["result"]!.ToObject<T>()!;
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

    public async Task<CpfResult> SearchCpfAsync(string cpf, DateTime? birthDate)
    {
        return await Search<CpfResult>("cpf", cpf);
    }


    public async Task<CepResult> SearchCepAsync(string cep)
    {
        return await Search<CepResult>("cep", cep);
    }
    
        
}