using System;
using JJMasterData.ReceitaFederal.Models;

namespace JJMasterData.ReceitaFederal.Abstractions;

public interface IReceitaFederal
{
    bool IgnoreDb { get; set; }
    bool IsSecureConnection { get; set; }

    CepInfo SearchCep(string cep);
    CnpjInfo SearchCnpj(string cnpj);
    CpfInfo SearchCpf(string cpf, DateTime birthDate);
}