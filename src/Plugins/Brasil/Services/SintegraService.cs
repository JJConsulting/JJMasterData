using System;
using System.Net.Http;
using System.Threading.Tasks;
using JJMasterData.Commons.Util;
using JJMasterData.Brasil.Abstractions;
using JJMasterData.Brasil.Configuration;
using JJMasterData.Brasil.Exceptions;
using JJMasterData.Brasil.Models;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace JJMasterData.Brasil.Services;

public class SintegraService(HttpClient httpClient, ICepService cepService, IOptions<SintegraSettings> options)
    : IReceitaFederalService
{
    private HttpClient HttpClient { get; } = httpClient;
    private ICepService CepService { get; } = cepService;
    private SintegraSettings Settings { get; } = options.Value;
    public bool IsHttps { get; set; } = true;

    public bool IgnoreDb { get; set; }

    public async Task<CnpjResult> SearchCnpjAsync(string cnpj)
    {
        try
        {
            if (string.IsNullOrEmpty(cnpj))
                throw new ArgumentNullException(nameof(cnpj));

            cnpj = StringManager.ClearCpfCnpjChars(cnpj);
            var protocol = IsHttps ? "https://" : "http://";
            var url = $"{protocol}{Settings.Url}?cnpj={cnpj}&token={Settings.ApiKey}&plugin=RF";
            
            var message = await HttpClient.GetAsync(url);
            var content = await message.Content.ReadAsStringAsync();
            var sintegraDto = JsonConvert.DeserializeObject<SintegraCnpjDto>(content)!;
            return sintegraDto.ToCnpjResult();
        }
        catch (Exception ex)
        {
            throw new ReceitaFederalException(ex.Message, ex);
        }
    }
    
    public async Task<CpfResult> SearchCpfAsync(string cpf, DateTime birthDate)
    {
        try
        {
            if (string.IsNullOrEmpty(cpf))
                throw new ArgumentNullException(nameof(cpf));
            
            cpf = StringManager.ClearCpfCnpjChars(cpf);
            var date = birthDate.ToString("dd/MM/yyyy");
            var protocol = IsHttps ? "https://" : "http://";
            var url = $"{protocol}{Settings.Url}?cpf={cpf}&data-nascimento={date}&token={Settings.ApiKey}&plugin=CPF";
            
            var message = await HttpClient.GetAsync(url);
            var content = await message.Content.ReadAsStringAsync();

            var dto = JsonConvert.DeserializeObject<SintegraCpfDto>(content)!;
            return dto.ToCpfResult();
        }
        catch (Exception ex)
        {
            throw new ReceitaFederalException(ex.Message, ex);
        }
    }
    
    public Task<CepResult> SearchCepAsync(string cep)
    {
        return CepService.SearchCepAsync(cep);
    }
}