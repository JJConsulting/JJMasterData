using System;
using System.Threading.Tasks;
using JJMasterData.Brasil.Models;

namespace JJMasterData.Brasil.Abstractions;

public interface IReceitaFederalService : ICepService
{
    /// <summary>
    /// Ignora o banco local forçando uma conexão com a Receita Federal para recuperar os dados atualizados.
    /// </summary>
    bool IgnoreDb { get; set; }
    bool IsHttps { get; set; }
    Task<CnpjResult> SearchCnpjAsync(string cnpj);
    Task<CpfResult> SearchCpfAsync(string cpf, DateTime? birthDate = null);
}