using System;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using JJMasterData.Commons.Localization;
using JJMasterData.Commons.Logging;
using JJMasterData.Commons.Util;
using JJMasterData.ReceitaFederal.Abstractions;
using JJMasterData.ReceitaFederal.Models;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace JJMasterData.ReceitaFederal.Services;

public class SintegraService : IReceitaFederal
{
    private SintegraSettings Settings { get; }
    
    /// <summary>
    /// Habilita conexão https
    /// </summary>
    public bool IsSecureConnection { get; set; }

    /// <summary>
    /// Ignora o banco local forçando uma conexão 
    /// com a Receita para recuperar os dados atualizados
    /// </summary>
    public bool IgnoreDb { get; set; }


    public SintegraService(SintegraSettings settings)
    {
        Settings = settings;
    }

    public SintegraService(IOptions<SintegraSettings> options)
    {
        Settings = options.Value;
    }

    /// <summary>
    /// Procura os dados da empresa na Receita Federal
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
            string sIgnoreDb = "&plugin=RF";
            string url = $"{sProtocol}{Settings.Url}?cnpj={cnpj}&token={Settings.Key}{sIgnoreDb}";

            using var http = new HttpClient();
            Task<HttpResponseMessage> result = http.GetAsync(url);
            Task<string> str = result.Result.Content.ReadAsStringAsync();
            var sintegraDto = JsonConvert.DeserializeObject<SintegraCnpjDto>(str.Result);
            info = CnpjInfo.GetInstance(sintegraDto);
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
            string sIgnoreDb = "&plugin=CPF";
            string url = $"{sProtocol}{Settings.Url}?cpf={cpf}&data-nascimento={sDate}&token={Settings.Key}{sIgnoreDb}";

            using var http = new HttpClient();

            Task<HttpResponseMessage> result = http.GetAsync(url);
            Task<string> str = result.Result.Content.ReadAsStringAsync();

            var dto = JsonConvert.DeserializeObject<SintegraCpfDto>(str.Result);
            info = CpfInfo.GetInstance(dto);
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
            string sUrl = "viacep.com.br/ws/";
            string url = $"{sProtocol}{sUrl}{cep}/json";

            using var http = new HttpClient();
            Task<HttpResponseMessage> result = http.GetAsync(url);
            Task<string> str = result.Result.Content.ReadAsStringAsync();
            var dto = JsonConvert.DeserializeObject<CepInfo.CepResult>(str.Result);
            info = new CepInfo
            {
                Return = "OK",
                Result = dto
            };
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