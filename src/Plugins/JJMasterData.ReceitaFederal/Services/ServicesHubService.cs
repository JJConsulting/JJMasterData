using JJMasterData.Commons.Language;
using JJMasterData.Commons.Logging;
using JJMasterData.Commons.Util;
using JJMasterData.ReceitaFederal.Abstractions;
using JJMasterData.ReceitaFederal.Models;
using System;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace JJMasterData.ReceitaFederal.Services;

/// <summary>
/// Procura por informações na receita federal
/// </summary>
/// <remarks>
/// Essa API utiliza conexão com a internet e serviços de terceiros
/// </remarks>
public class ServicesHubService : IReceitaFederal
{
    private ServicesHubSettings Settings { get; }
    
    public ServicesHubService(ServicesHubSettings settings)
    {
        Settings = settings;
    }
    
    public ServicesHubService(IOptions<ServicesHubSettings> options)
    {
        Settings = options.Value;
    }

    /// <summary>
    /// Habilita conexão https
    /// </summary>
    public bool IsSecureConnection { get; set; }

    /// <summary>
    /// Ignora o banco local forçando uma conexão 
    /// com a Receita para recuperar os dados atualizados
    /// </summary>
    public bool IgnoreDb { get; set; }

    /// <summary>
    /// Poocura os dados da empresa na Receita Federal
    /// </summary>
    /// <param name="cnpj">Numero do CNPJ</param>
    /// <returns>Informações da empresa</returns>
    public CnpjInfo SearchCnpj(string cnpj)
    {
        CnpjInfo info = null;
        try
        {
            if (string.IsNullOrEmpty(cnpj))
                throw new ArgumentNullException(nameof(cnpj));

            cnpj = StringManager.ClearCpfCnpjChars(cnpj);
            string sProtocol = IsSecureConnection ? "https://" : "http://";
            string sIgnoreDb = IgnoreDb ? "&ignore_db=1" : "";
            string url = $"{sProtocol}{Settings.Url}cnpj/?cnpj={cnpj}&token={Settings.Key}{sIgnoreDb}";

            using var http = new HttpClient();
            Task<HttpResponseMessage> result = http.GetAsync(url);
            Task<string> str = result.Result.Content.ReadAsStringAsync();
            info = JsonConvert.DeserializeObject<CnpjInfo>(str.Result);
        }
        catch (Exception ex)
        {
            string sErr = Translate.Key("Error searching for {0}.", "CNPJ");
            sErr += " ";
            if (!NetworkInterface.GetIsNetworkAvailable())
                sErr += Translate.Key("Internet unavailable");
            else
                sErr += ex.Message;

            info = new CnpjInfo();
            info.Return = "ERROR";
            info.Message = sErr;
            Log.AddError(sErr);
        }

        return info;
    }

    /// <summary>
    /// Poocura dados da pessoa física na Receita Federal
    /// </summary>
    /// <param name="cpf">Numero do CPF</param>
    /// <param name="birthDate">Data de Nascimento</param>
    /// <returns>Informações da pessoa física</returns>
    public CpfInfo SearchCpf(string cpf, DateTime birthDate)
    {
        CpfInfo info = null;
        try
        {
            if (string.IsNullOrEmpty(cpf))
                throw new ArgumentNullException(nameof(cpf));

            cpf = StringManager.ClearCpfCnpjChars(cpf);
            string sDate = birthDate.ToString("dd/MM/yyyy");
            string sProtocol = IsSecureConnection ? "https://" : "http://";
            string sIgnoreDb = IgnoreDb ? "&ignore_db=1" : "";
            string url = $"{sProtocol}{Settings.Url}cpf/?cpf={cpf}&data={sDate}&token={Settings.Key}{sIgnoreDb}";

            using var http = new HttpClient();
            Task<HttpResponseMessage> result = http.GetAsync(url);
            Task<string> str = result.Result.Content.ReadAsStringAsync();
            info = JsonConvert.DeserializeObject<CpfInfo>(str.Result);
        }
        catch (Exception ex)
        {
            string sErr = Translate.Key("Error searching for {0}.", "CPF");
            sErr += " ";
            if (!NetworkInterface.GetIsNetworkAvailable())
                sErr += Translate.Key("Internet unavailable");
            else
                sErr += ex.Message;

            info = new CpfInfo();
            info.Return = "ERROR";
            info.Message = sErr;
            Log.AddError(sErr);
        }
        return info;
    }

    /// <summary>
    /// Poocura endereço
    /// </summary>
    /// <param name="cep">Número do CEP</param>
    /// <returns>Informações de endereço</returns>
    public CepInfo SearchCep(string cep)
    {
        CepInfo info = null;
        try
        {
            if (string.IsNullOrEmpty(cep))
                throw new ArgumentNullException(nameof(cep));

            cep = StringManager.ClearCpfCnpjChars(cep);
            string sProtocol = IsSecureConnection ? "https://" : "http://";
            string sIgnoreDb = IgnoreDb ? "&ignore_db=1" : "";
            string url = $"{sProtocol}{Settings.Url}cep/?cep={cep}&token={Settings.Key}{sIgnoreDb}";

            using var http = new HttpClient();
            Task<HttpResponseMessage> result = http.GetAsync(url);
            Task<string> str = result.Result.Content.ReadAsStringAsync();
            info = JsonConvert.DeserializeObject<CepInfo>(str.Result);
        }
        catch (Exception ex)
        {
            string sErr = Translate.Key("Error searching for {0}.", "CEP");
            sErr += " ";
            if (!NetworkInterface.GetIsNetworkAvailable())
                sErr += Translate.Key("Internet unavailable");
            else
                sErr += ex.Message;

            info = new CepInfo();
            info.Return = "ERROR";
            info.Message = sErr;
            Log.AddError(sErr);
        }
        return info;
    }
        

        
}