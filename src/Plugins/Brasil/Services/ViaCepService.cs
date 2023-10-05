using System;
using System.Net.Http;
using System.Threading.Tasks;
using JJMasterData.Brasil.Abstractions;
using JJMasterData.Brasil.Exceptions;
using JJMasterData.Brasil.Models;
using JJMasterData.Commons.Util;
using Microsoft.Extensions.Caching.Memory;

namespace JJMasterData.Brasil.Services;

public class ViaCepService : ICepService
{
    private const string ViaCepUrl = "https://viacep.com.br/ws/";
    private HttpClient HttpClient { get; }
    private IMemoryCache MemoryCache { get; }

    public ViaCepService(HttpClient httpClient, IMemoryCache memoryCache)
    {
        HttpClient = httpClient;
        MemoryCache = memoryCache;
    }
    public async Task<CepResult> SearchCepAsync(string cep)
    {
         try
         {
             if (string.IsNullOrEmpty(cep))
                 throw new ArgumentNullException(nameof(cep));

             if (MemoryCache.TryGetValue(cep, out CepResult cepResult))
                 return cepResult;
             
             var url = $"{ViaCepUrl}{StringManager.ClearCpfCnpjChars(cep)}/json";
             
             var response = await HttpClient.GetAsync(url);
             var content = await response.Content.ReadAsStringAsync();
             var result =  CepResult.FromJson(content);

             if (result.Erro)
                 throw new ViaCepException("CEP inválido.");

             MemoryCache.Set(cep, cepResult, TimeSpan.FromMinutes(30));
             
             return result;
         }
         catch (Exception ex)
         {
             throw new ViaCepException(ex.Message, ex);
         }
    }
}