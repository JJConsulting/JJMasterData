using System;
using System.Globalization;
using System.IO;
using System.Linq;
using JJMasterData.Commons.Util;

namespace JJMasterData.Commons.Validations;

public static class Validate
{
    /// <summary>
    /// Valida se o CNPJ é válido
    /// </summary>
    /// <returns>Retorna Verdadeiro caso o CNPJ seja valido</returns> 
    public static bool ValidCnpj(string cnpj)
    {
        int[] multiplicador1 = { 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };
        int[] multiplicador2 = { 6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };

        cnpj = cnpj.Trim();
        cnpj = cnpj.Replace(".", "").Replace("-", "").Replace("/", "").Replace("_", "");

        if (cnpj.Length != 14)
            return false;

        var tempCnpj = cnpj.Substring(0, 12);
        var soma = 0; for (int i = 0; i < 12; i++) soma += int.Parse(tempCnpj[i].ToString()) * multiplicador1[i];
        var resto = (soma % 11);

        if (resto < 2)
        {
            resto = 0;
        }
        else
        {
            resto = 11 - resto;
        }


        var digito = resto.ToString();
        tempCnpj += digito;
        soma = 0;

        for (int i = 0; i < 13; i++)
            soma += int.Parse(tempCnpj[i].ToString()) * multiplicador2[i];

        resto = (soma % 11);

        if (resto < 2)
            resto = 0;
        else
            resto = 11 - resto;

        digito += resto;

        return cnpj.EndsWith(digito);
    }

    /// <summary>
    /// Verifica se o e-mail é valido
    /// </summary>
    /// <param name="email">E-Mail</param>
    public static bool ValidEmail(string email)
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
    public static bool ValidTel(string telNumber)
    {
        bool validTel = false;
        double nTel;
        string sTel = StringManager.ClearTelChars(telNumber);
            
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
    public static bool ValidIE(string uf, string inscr)
    {
        return ValidateBrazil.ValidIE(uf, inscr);
    }

    /// <summary>
    /// Valida Hora
    /// </summary>
    public static bool ValidHour(string value)
    {
        bool bRet = true;
        try
        {
            int nHora = Convert.ToInt32(value.Substring(0, 2));
            int nMin = Convert.ToInt32(value.Substring(3, 2));

            if (nHora > 24 | nHora < 0)
            {
                bRet = false;
            }

            if (nMin > 59 | nMin < 0)
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
    public static bool ValidCpf(string cpf)
    {
        int[] multiplicador1 = { 10, 9, 8, 7, 6, 5, 4, 3, 2 };
        int[] multiplicador2 = { 11, 10, 9, 8, 7, 6, 5, 4, 3, 2 };

        cpf = cpf.Trim();
        cpf = StringManager.ClearCpfCnpjChars(cpf);

        // Se o tamanho for < 11 entao retorna como inválido
        if (cpf.Length != 11)
            return false;

        // Caso coloque todos os numeros iguais
        switch (cpf)
        {
            case "11111111111":
                return false;
            case "00000000000":
                return false;
            case "22222222222":
                return false;
            case "33333333333":
                return false;
            case "44444444444":
                return false;
            case "55555555555":
                return false;
            case "66666666666":
                return false;
            case "77777777777":
                return false;
            case "88888888888":
                return false;
            case "99999999999":
                return false;
        }

        var tempCpf = cpf.Substring(0, 9);
        var soma = 0;

        for (int i = 0; i < 9; i++)
            soma +=

                int.Parse(tempCpf[i].ToString()) * multiplicador1[i];
        var resto = soma % 11;

        if (resto < 2)
            resto = 0;
        else
            resto = 11 - resto;

        var digito = resto.ToString();
        tempCpf += digito;
        soma = 0;

        for (int i = 0; i < 10; i++)
            soma +=
                int.Parse(tempCpf[i].ToString()) * multiplicador2[i];

        resto = soma % 11;
        if (resto < 2)
            resto = 0;
        else
            resto = 11 - resto;

        digito += resto;

        return cpf.EndsWith(digito);

    }


    /// <summary>
    /// Valida se o CPF ou CNPJ é valido
    /// </summary>
    public static bool ValidCpfCnpj(string value)
    {
        bool bRet = false;
        if (!String.IsNullOrEmpty(value))
        {
            value = value.Replace(".", "");
            value = value.Replace("-", "");
            if (value.Trim().Length == 11)
                bRet = ValidCpf(value);
            else
                bRet = ValidCnpj(value);
        }

        return bRet; 
    }

    /// <summary>
    /// Valida se a chave da NFe é valida
    /// </summary>
    public static bool ValidNFe(string chaveNFe)
    {
        string chave = chaveNFe.Substring(0, chaveNFe.Length - 1);
        int soma = 0;
        int resto = 0;
        int digitoRetorno;
        int[] peso = { 4, 3, 2, 9, 8, 7, 6, 5 };

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
        return reservedKeywords.Contains(value.ToUpper());
    }

    public static bool IsMasterDataKeyword(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return false;

        var reservedKeywords = ReservedWords.GetReservedMasterDataKeywords();
        return reservedKeywords.Contains(value.ToUpper());
    }

}