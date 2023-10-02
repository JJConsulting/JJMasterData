using System;
using System.Net.Http;
using System.Threading.Tasks;
using JJMasterData.Brasil.Abstractions;
using JJMasterData.Brasil.Exceptions;
using JJMasterData.Brasil.Models;
using JJMasterData.Commons.Util;

namespace JJMasterData.Brasil.Services;

public class ViaCepService : ICepService
{
    public const string ViaCepUrl = "https://viacep.com.br/ws/";
    private HttpClient HttpClient { get; }

    public ViaCepService(HttpClient httpClient)
    {
        HttpClient = httpClient;
    }
    public async Task<CepResult> SearchCepAsync(string cep)
    {
         try
         {
             if (string.IsNullOrEmpty(cep))
                 throw new ArgumentNullException(nameof(cep));
             
             var url = $"{ViaCepUrl}{StringManager.ClearCpfCnpjChars(cep)}/json";
             
             var response = await HttpClient.GetAsync(url);
             var content = await response.Content.ReadAsStringAsync();
             return CepResult.FromJson(content);
         }
         catch (Exception ex)
         {
             throw new ViaCepException(ex.Message, ex);
         }
    }
}