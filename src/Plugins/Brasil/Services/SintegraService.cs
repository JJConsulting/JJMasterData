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

public class SintegraService : IReceitaFederalService
{
    private HttpClient HttpClient { get; }
    private ICepService CepService { get; }
    private SintegraSettings Settings { get; }
    public bool IsHttps { get; set; } = true;

    public bool IgnoreDb { get; set; }

    public SintegraService(HttpClient httpClient,ICepService cepService,IOptions<SintegraSettings> options)
    {
        HttpClient = httpClient;
        CepService = cepService;
        Settings = options.Value;
    }
    
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
    
    public async Task<CpfResult> SearchCpfAsync(string cpf, DateTime? birthDate)
    {
        try
        {
            if (string.IsNullOrEmpty(cpf))
                throw new ArgumentNullException(nameof(cpf));

            if (birthDate is null)
                throw new ArgumentNullException(nameof(birthDate));
            
            cpf = StringManager.ClearCpfCnpjChars(cpf);
            var date = birthDate.Value.ToString("dd/MM/yyyy");
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
    
    public async Task<CepResult> SearchCepAsync(string cep)
    {
        return await CepService.SearchCepAsync(cep);
    }
}