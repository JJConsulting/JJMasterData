using System;
using System.Globalization;
using System.IO;
using System.Linq;
using JJMasterData.Commons.Util;

namespace JJMasterData.Commons.Validations;

public static class Validations
{
    /// <summary>
    /// Valida se o CNPJ é válido
    /// </summary>
    /// <returns>Retorna Verdadeiro caso o CNPJ seja valido</returns> 
    public static bool ValidateCnpj(string cnpj)
    {
        return BrazilValidations.ValidateCnpj(cnpj);
    }

    /// <summary>
    /// Verifica se o e-mail é valido
    /// </summary>
    /// <param name="email">E-Mail</param>
    public static bool ValidateEmail(string email)
    {
        if (email.Contains("'"))
            return false;

        if (email.Contains('@') && email.Contains('.') && !email.Contains(".."))
            return true;
        return false;
    }

    /// <summary>
    /// Verifica se o numero de telefone é valido 
    /// </summary>
    /// <param name="telNumber">Numero de Telefone</param>
    public static bool ValidatePhone(string telNumber)
    {
        bool validTel = false;
        double nTel;
        string sTel = StringManager.ClearPhoneChars(telNumber);
            
        if (double.TryParse(sTel, out nTel))
        {
            if (nTel.ToString(CultureInfo.CurrentCulture).Length > 7)
                validTel = true;
        }
        return validTel;
    }

    /// <summary>
    /// Valida Inscrição estadual do cliente
    /// </summary>
    /// <param name="uf">Estado</param>
    /// <param name="inscr">Inscrição Estadual</param>
    public static bool ValidateIE(string uf, string inscr)
    {
        return BrazilValidations.ValidateIE(uf, inscr);
    }

    /// <summary>
    /// Valida Hora
    /// </summary>
    public static bool ValidateHour(string value)
    {
        bool bRet = true;
        try
        {
            int nHora = Convert.ToInt32(value.Substring(0, 2));
            int nMin = Convert.ToInt32(value.Substring(3, 2));

            if (nHora is > 24 or < 0)
            {
                bRet = false;
            }

            if (nMin is > 59 or < 0)
            {
                bRet = false;
            }

        }
        catch
        {
            bRet = false;
        }

        return bRet;
    }

    /// <summary>
    /// Valida se o CPF é valido
    /// </summary>
    /// <returns>Retorna Verdadeiro caso o CPF seja valido</returns> 
    public static bool ValidateCpf(string cpf)
    {
        return BrazilValidations.ValidateCpf(cpf);
    }


    /// <summary>
    /// Valida se o CPF ou CNPJ é valido
    /// </summary>
    public static bool ValidateCpfCnpj(string value)
    {
        bool bRet = false;
        if (!string.IsNullOrEmpty(value))
        {
            value = value.Replace(".", "");
            value = value.Replace("-", "");
            if (value.Trim().Length == 11)
                bRet = ValidateCpf(value);
            else
                bRet = ValidateCnpj(value);
        }

        return bRet; 
    }

    /// <summary>
    /// Valida se a chave da NFe é valida
    /// </summary>
    public static bool ValidateNFe(string chaveNFe)
    {
        string chave = chaveNFe.Substring(0, chaveNFe.Length - 1);
        int soma = 0;
        int resto = 0;
        int digitoRetorno;
        int[] peso = [4, 3, 2, 9, 8, 7, 6, 5];

        for (int i = 0; i < chave.Length; i++)
        {
            soma += peso[i % 8] * (int.Parse(chave.Substring(i, 1)));
        }

        resto = soma % 11;
        if (resto == 0 || resto == 1)
        {
            digitoRetorno = 0;
        }
        else
        {
            digitoRetorno = 11 - resto;
        }

        return chaveNFe.EndsWith(digitoRetorno.ToString());
    }

    
    public static bool IsFilePathValid(string filename)
    {
        if (string.IsNullOrEmpty(filename))
            return false;
        
        var invalidChars = Path.GetInvalidFileNameChars();

        return invalidChars.All(invalidChar => !filename.Contains(invalidChar));
    }

    public static bool IsDatabaseKeyword(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return false;

        var reservedKeywords = ReservedWords.GetReservedDatabaseKeywords();
        return reservedKeywords.Contains(value, StringComparer.InvariantCultureIgnoreCase);
    }

    public static bool IsMasterDataKeyword(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return false;

        var reservedKeywords = ReservedWords.GetReservedMasterDataKeywords();
        return reservedKeywords.Contains(value, StringComparer.InvariantCultureIgnoreCase);
    }

}