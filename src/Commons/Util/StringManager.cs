#nullable enable

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using JJMasterData.Commons.Localization;
using Microsoft.Extensions.Localization;

namespace JJMasterData.Commons.Util;

public static class StringManager
{
    public static bool ParseBool(object? value) => ParseBool(value?.ToString());

    public static bool ParseBool(string? value)
    {
        return value?.ToLowerInvariant() switch
        {
            "true" => true,
            "1" => true,
            _ => false,
        };
    }
    
    /// <summary>
    /// Clear the text, preventing SQL injection
    /// </summary>
    /// <remarks>
    /// Author: Lucio Pelinson 21-05-2012
    /// </remarks>
    [Obsolete("This don't prevent SQL injection.")]
    public static string ClearText(string? input)
    {
        if (input == null)
            return string.Empty;

        string result = input.Replace("'", "`");
        result = result.Replace('<', '[');
        result = result.Replace('>', ']');
        result = result.Replace("--", "");

        return result;
    }
    
    /// <summary>
    /// Remove os acentos e caracteres especiais
    /// </summary>
    /// <param name="str">Valor</param>
    public static string GetStringWithoutAccents(string str)
    {
        //Troca os caracteres acentuados por não acentuados
        string[] acentos = ["ç", "Ç", "á", "é", "í", "ó", "ú", "ý", "Á", "É", "Í", "Ó", "Ú", "Ý", "à", "è", "ì", "ò", "ù", "À", "È", "Ì", "Ò", "Ù", "ã", "õ", "ñ", "ä", "ë", "ï", "ö", "ü", "ÿ", "Ä", "Ë", "Ï", "Ö", "Ü", "Ã", "Õ", "Ñ", "â", "ê", "î", "ô", "û", "Â", "Ê", "Î", "Ô", "Û"
        ];
        string[] semAcento = ["c", "C", "a", "e", "i", "o", "u", "y", "A", "E", "I", "O", "U", "Y", "a", "e", "i", "o", "u", "A", "E", "I", "O", "U", "a", "o", "n", "a", "e", "i", "o", "u", "y", "A", "E", "I", "O", "U", "A", "O", "N", "a", "e", "i", "o", "u", "A", "E", "I", "O", "U"
        ];
        for (int i = 0; i < acentos.Length; i++)
        {
            str = str.Replace(acentos[i], semAcento[i]);
        }
        //Troca os caracteres especiais da string por "" 
        //string[] caracteresEspeciais = { "\\.", ",", "-", ":", "\\(", "\\)", "ª", "\\|", "\\\\", "°" };
        string[] caracteresEspeciais = ["ª", "\\|", "°"];
        for (int i = 0; i < caracteresEspeciais.Length; i++)
        {
            str = str.Replace(caracteresEspeciais[i], "");
        }
        //Troca os espaços no início por "" 
        str = str.Replace("^\\s+", "");
        // Troca os espaços no início por "" 
        str = str.Replace("\\s+$", "");
        // Troca os espaços duplicados, tabulações e etc por  " " 
        str = str.Replace("\\s+", " ");
        return str;
    }

    /// <summary>
    /// Function to return greeting
    /// </summary>
    /// <returns>
    /// Good Morning, Good Afternoon or Good Night. i18n supported.
    /// </returns>
    public static string GetGreeting(IStringLocalizer<MasterDataResources> stringLocalizer)
    {
        var now = DateTime.Now;
        string greeting;
        if (now.Hour > 0 && (DateTime.Now.Hour <= 12))
            greeting = stringLocalizer["Good Morning"];
        else if ((DateTime.Now.Hour > 12) && (DateTime.Now.Hour <= 18))
            greeting = stringLocalizer["Good Afternoon"];
        else
            greeting = stringLocalizer["Good Night"];

        return greeting;
    }
    
    public static string ClearCpfCnpjChars(string value)
    {
        string cnpf = value;
        cnpf = cnpf.Replace("-", "");
        cnpf = cnpf.Replace("/", "");
        cnpf = cnpf.Replace(".", "");

        return cnpf;
    }
    
    public static string ClearTelChars(string value)
    {
        string phone = value;
        phone = phone.Replace("(", "");
        phone = phone.Replace(")", "");
        phone = phone.Replace("-", "");
        phone = phone.Replace(".", "");
        phone = phone.Replace(" ", "");
            
        return phone;
    }

    /// <summary>
    /// Converts the date from yyyyMMdd to DateTime
    /// </summary>
    /// <param name="dataString">Data at yyyyMMdd format</param>
    /// <returns></returns>
    public static DateTime GetDataSiga(string dataString)
    {
        var date = DateTime.MinValue;

        if (dataString.Trim().Length == 8)
        {
            int day = int.Parse(dataString.Substring(6, 2));
            int month = int.Parse(dataString.Substring(4, 2));
            int year = int.Parse(dataString.Substring(0, 4));
            date = new DateTime(year, month, day);
        }
        return date;
    }

    /// <summary>
    /// Calcula a idade de uma pessoa com base na data atual
    /// </summary>
    /// <param name="data2">Data como um string em formato portugues (dd/MM/yyyy)</param>
    /// <returns>
    /// Devolve um inteiro com a idade. 
    /// Devolve false em caso de que a data seja incorreta ou maior que o dia atual
    /// </returns>
    public static string GetAge(string data2)
    {

        string dt = data2;
        int idade;
        //calculo a data de hoje 
        DateTime hoje = DateTime.Now;
        //alert(hoje) 

        //calculo a data que recebo 
        //descomponho a data em um array 
        string[] arrayData = dt.Split('/');

        //se o array nao tem tres partes, a data eh incorreta 
        if (arrayData.Length != 3)
            return "";

        //comprovo que o ano, mes, dia são corretos 
        int ano;
        ano = int.Parse(arrayData[2]);
        //if ( isNaN(ano) )
        //  return "";

        int mes;
        mes = int.Parse(arrayData[1]);
        // if (isNaN(mes))
        //    return false

        int dia;
        dia = int.Parse(arrayData[0]);
        //if (isNaN(dia))
        //    return false*/


        //se o ano da data que recebo so tem 2 cifras temos que muda-lo a 4 
        if (ano <= 99)
            ano += 1900;

        //subtraio os anos das duas datas 
        idade = hoje.Year - ano - 1; //-1 porque ainda nao fez anos durante este ano

        //se subtraio os meses e for menor que 0 entao nao cumpriu anos. Se for maior sim ja cumpriu
        if (hoje.Month - mes < 0) //+ 1 porque os meses comecam em 0 
            return idade.ToString();

        if (hoje.Month - mes > 0)
            return (idade + 1).ToString();

        //entao eh porque sao iguais. Vejo os dias 
        //se subtraio os dias e der menor que 0 entao nao cumpriu anos. Se der maior ou igual sim que já cumpriu
        if ((hoje.Day - dia) >= 0)
            return (idade + 1).ToString();

        return idade.ToString();
    }

    /// <summary>
    /// Function to write a number in full in a brazilian format.
    /// </summary>
    /// <param name="valor">Value, limited to (999.999.999.999.999,00)</param>
    /// <param name="isCurrency">Write in Reais</param>
    /// <param name="isFemale">Use a female substantive</param>
    /// <returns>Número por extenso</returns>
    /// <remarks>Lucio Pelinson 21-02-2013</remarks> 
    public static string NumToWords(decimal valor, bool isCurrency = true, bool isFemale = false)
    {
        if (valor >= 1000000000000000)
            throw new FormatException("Value not supported.");
        bool isNegativo = false;
        if (valor < 0)
        {
            isNegativo = true;
            valor *= -1;
        }
        string strValor = valor.ToString("000000000000000.00");
        string valorPorExtenso = string.Empty;

        for (int i = 0; i <= 15; i += 3)
        {
            //Escreve valor
            decimal vlrCentena = Convert.ToDecimal(strValor.Substring(i, 3));
            if (vlrCentena > 0)
            {
                string montagem = string.Empty;
                if (vlrCentena > 0 & vlrCentena < 1)
                {
                    vlrCentena *= 100;
                }
                string strVlrCentena = vlrCentena.ToString("000");
                int a = Convert.ToInt32(strVlrCentena.Substring(0, 1));
                int b = Convert.ToInt32(strVlrCentena.Substring(1, 1));
                int c = Convert.ToInt32(strVlrCentena.Substring(2, 1));

                if (a == 1) montagem += (b + c == 0) ? "cem" : "cento";
                else if (a == 2) montagem += isFemale ? "duzentas" : "duzentos";
                else if (a == 3) montagem += isFemale ? "trezentas" : "trezentos";
                else if (a == 4) montagem += isFemale ? "quatrocentas" : "quatrocentos";
                else if (a == 5) montagem += isFemale ? "quinhentas" : "quinhentos";
                else if (a == 6) montagem += isFemale ? "seiscentas" : "seiscentos";
                else if (a == 7) montagem += isFemale ? "setecentas" : "setecentos";
                else if (a == 8) montagem += isFemale ? "oitocentas" : "oitocentos";
                else if (a == 9) montagem += isFemale ? "novecentas" : "novecentos";

                if (b == 1)
                {
                    if (c == 0) montagem += $"{((a > 0) ? " e " : string.Empty)}dez";
                    else if (c == 1) montagem += $"{((a > 0) ? " e " : string.Empty)}onze";
                    else if (c == 2) montagem += $"{((a > 0) ? " e " : string.Empty)}doze";
                    else if (c == 3) montagem += $"{((a > 0) ? " e " : string.Empty)}treze";
                    else if (c == 4) montagem += $"{((a > 0) ? " e " : string.Empty)}quatorze";
                    else if (c == 5) montagem += $"{((a > 0) ? " e " : string.Empty)}quinze";
                    else if (c == 6) montagem += $"{((a > 0) ? " e " : string.Empty)}dezesseis";
                    else if (c == 7) montagem += $"{((a > 0) ? " e " : string.Empty)}dezessete";
                    else if (c == 8) montagem += $"{((a > 0) ? " e " : string.Empty)}dezoito";
                    else if (c == 9) montagem += $"{((a > 0) ? " e " : string.Empty)}dezenove";
                }
                else if (b == 2) montagem += $"{((a > 0) ? " e " : string.Empty)}vinte";
                else if (b == 3) montagem += $"{((a > 0) ? " e " : string.Empty)}trinta";
                else if (b == 4) montagem += $"{((a > 0) ? " e " : string.Empty)}quarenta";
                else if (b == 5) montagem += $"{((a > 0) ? " e " : string.Empty)}cinquenta";
                else if (b == 6) montagem += $"{((a > 0) ? " e " : string.Empty)}sessenta";
                else if (b == 7) montagem += $"{((a > 0) ? " e " : string.Empty)}setenta";
                else if (b == 8) montagem += $"{((a > 0) ? " e " : string.Empty)}oitenta";
                else if (b == 9) montagem += $"{((a > 0) ? " e " : string.Empty)}noventa";

                if (strVlrCentena.Substring(1, 1) != "1" & c != 0 & montagem != string.Empty) montagem += " e ";

                if (strVlrCentena.Substring(1, 1) != "1")
                    if (c == 1) montagem += isFemale ? "uma" : "um";
                    else if (c == 2) montagem += isFemale ? "duas" : "dois";
                    else if (c == 3) montagem += "três";
                    else if (c == 4) montagem += "quatro";
                    else if (c == 5) montagem += "cinco";
                    else if (c == 6) montagem += "seis";
                    else if (c == 7) montagem += "sete";
                    else if (c == 8) montagem += "oito";
                    else if (c == 9) montagem += "nove";

                valorPorExtenso += montagem;
            }


            if (i == 0 & valorPorExtenso != string.Empty)
            {
                if (Convert.ToInt32(strValor.Substring(0, 3)) == 1)
                    valorPorExtenso +=
                        $" trilhão{((Convert.ToDecimal(strValor.Substring(3, 12)) > 0) ? " e " : string.Empty)}";
                else if (Convert.ToInt32(strValor.Substring(0, 3)) > 1)
                    valorPorExtenso +=
                        $" trilhões{((Convert.ToDecimal(strValor.Substring(3, 12)) > 0) ? " e " : string.Empty)}";
            }
            else if (i == 3 & valorPorExtenso != string.Empty)
            {
                if (Convert.ToInt32(strValor.Substring(3, 3)) == 1)
                    valorPorExtenso +=
                        $" bilhão{((Convert.ToDecimal(strValor.Substring(6, 9)) > 0) ? " e " : string.Empty)}";
                else if (Convert.ToInt32(strValor.Substring(3, 3)) > 1)
                    valorPorExtenso +=
                        $" bilhões{((Convert.ToDecimal(strValor.Substring(6, 9)) > 0) ? " e " : string.Empty)}";
            }
            else if (i == 6 & valorPorExtenso != string.Empty)
            {
                if (Convert.ToInt32(strValor.Substring(6, 3)) == 1)
                    valorPorExtenso +=
                        $" milhão{((Convert.ToDecimal(strValor.Substring(9, 6)) > 0) ? " e " : string.Empty)}";
                else if (Convert.ToInt32(strValor.Substring(6, 3)) > 1)
                    valorPorExtenso +=
                        $" milhões{((Convert.ToDecimal(strValor.Substring(9, 6)) > 0) ? " e " : string.Empty)}";
            }
            else if (i == 9 & valorPorExtenso != string.Empty)
                if (Convert.ToInt32(strValor.Substring(9, 3)) > 0)
                    valorPorExtenso +=
                        $" mil{((Convert.ToDecimal(strValor.Substring(12, 3)) > 0) ? " e " : string.Empty)}";

            if (i == 12)
            {
                if (valorPorExtenso.Length > 8)
                    if (valorPorExtenso.Substring(valorPorExtenso.Length - 6, 6) == "bilhão" | valorPorExtenso.Substring(valorPorExtenso.Length - 6, 6) == "milhão")
                        valorPorExtenso += " de";
                    else
                    if (valorPorExtenso.Substring(valorPorExtenso.Length - 7, 7) == "bilhões" | valorPorExtenso.Substring(valorPorExtenso.Length - 7, 7) == "milhões" | valorPorExtenso.Substring(valorPorExtenso.Length - 8, 7) == "trilhões")
                        valorPorExtenso += " de";
                    else
                    if (valorPorExtenso.Substring(valorPorExtenso.Length - 8, 8) == "trilhões")
                        valorPorExtenso += " de";

                if (isCurrency)
                {
                    if (Convert.ToInt64(strValor.Substring(0, 15)) == 1)
                        valorPorExtenso += " real";
                    else if (Convert.ToInt64(strValor.Substring(0, 15)) > 1)
                        valorPorExtenso += " reais";

                    if (Convert.ToInt32(strValor.Substring(16, 2)) > 0 && valorPorExtenso != string.Empty)
                        valorPorExtenso += " e ";
                }
                else
                {
                    if (Convert.ToInt32(strValor.Substring(16, 2)) > 0 && valorPorExtenso != string.Empty)
                        valorPorExtenso += " virgula ";
                }
            }

            if (i == 15 && isCurrency)
            {
                if (Convert.ToInt32(strValor.Substring(16, 2)) == 1)
                    valorPorExtenso += " centavo";
                else if (Convert.ToInt32(strValor.Substring(16, 2)) > 1)
                    valorPorExtenso += " centavos";
            }
        }

        if (isNegativo)
        {
            valorPorExtenso = $"menos {valorPorExtenso}";
        }
        else if (valor == 0)
        {
            valorPorExtenso = "zero";
        }


        return valorPorExtenso;
    }

    /// <summary>
    /// Retorna o primeiro nome
    /// </summary>
    /// <param name="value">Nome</param>
    public static string GetFirstName(string value)
    {
        string sret = value;
        if (value.Length > 2)
        {
            sret = value.Split(' ')[0];
            sret = sret.ToLower();
            sret = sret.Substring(0, 1).ToUpper() + sret.Substring(1);
        }
        return sret;
    }

    /// <summary>
    /// Retorna o primeiro e o ultimo nome
    /// </summary>
    /// <param name="value">Nome</param>
    public static string GetShortName(string value)
    {
        string sret = value;
        if (value.Length > 2)
        {
            string[] aNome = value.Split(' ');
            sret = aNome[0];
            sret = sret.ToLower();
            sret = sret.Substring(0, 1).ToUpper() + sret.Substring(1);

            if (aNome.Length > 0)
            {
                string lastname = aNome[aNome.Length - 1];
                lastname = lastname.ToLower();
                lastname = lastname.Substring(0, 1).ToUpper() + lastname.Substring(1);
                sret += $" {lastname}";
            }
        }
        return sret;
    }

    /// <summary>
    /// Remove letras e caracteres especiais
    /// </summary>
    /// <param name="value">Texto</param>
    /// <returns>Retorna somente númetos</returns>
    public static string OnlyNumbers(string value)
    {
        StringBuilder sRet = new StringBuilder();
        char[] chars = value.ToCharArray();
        foreach (char c in chars)
        {
            if (c.Equals('0') ||
                c.Equals('1') || 
                c.Equals('2') || 
                c.Equals('3') || 
                c.Equals('4') || 
                c.Equals('5') || 
                c.Equals('6') || 
                c.Equals('7') || 
                c.Equals('8') ||
                c.Equals('9'))
            {
                sRet.Append(c);  
            }
        }
        return sRet.ToString();   
    }

    /// <summary>
    /// Returns a list with all characters between the specified begin and end characters.
    /// Example:
    /// StringManager.FindValuesByInterval("test[a]foo [b] test" ,'[',']') returns ['a','b'].
    /// </summary>
    public static List<string> FindValuesByInterval(string text, char begin, char end)
    {
        var result = new List<string>();
        var valueBuilder = new StringBuilder();
        var isReading = false;

        foreach (var c in text)
        {
            if (c == begin)
            {
                valueBuilder.Clear();
                isReading = true;
            }
            else if (c == end)
            {
                result.Add(valueBuilder.ToString());
                isReading = false;
            }
            else if (isReading)
            {
                valueBuilder.Append(c);
            }
        }

        return result;
    }

    public static string Soma1(string baseVal)
    {
        if (string.IsNullOrEmpty(baseVal))
        {
            return "0";
        }
        return Soma1(baseVal, baseVal.Length);
    }

    private static string Soma1(string baseVal, int size)
    {
        long nAux;
        if (long.TryParse(baseVal, out nAux))
        {
            nAux++;
            string sRet = nAux.ToString().PadLeft(size, '0');
            if (sRet.Length <= size)
                return sRet;
        }

        baseVal = baseVal.Trim().ToUpper();
        char lastDigit = baseVal.ToCharArray()[baseVal.Length - 1];
        lastDigit = (char)(lastDigit + 1);
        if (lastDigit > '9' && lastDigit < 'A')
        {
            lastDigit = 'A';
            baseVal = (baseVal.Substring(0, (baseVal.Length - 1)) + lastDigit);
        }
        else if ((lastDigit > 'Z'))
        {
            string lastDigitVal = Soma1(baseVal.Substring(0,baseVal.Length - 1), size-1);
            baseVal = $"{lastDigitVal}0";
        }
        else
        {
            baseVal = (baseVal.Substring(0, (baseVal.Length - 1)) + lastDigit);
        }

        return baseVal;
    }

    public static string FirstCharToUpper(this string input)
    {
        //Since .NET Core 3.0 / .NET Standard 2.1 String.Concat()
        //supports ReadonlySpan<char> which saves one allocation if we use .AsSpan(1) instead of .Substring(1).
#if NET
        return input switch
        {
            null => throw new ArgumentNullException(nameof(input)),
            "" => throw new ArgumentException($"{nameof(input)} cannot be empty", nameof(input)),
            _ => string.Concat(input[0].ToString().ToUpper(), input.AsSpan(1))
        };
#else
        if (!string.IsNullOrEmpty(input))
            return input[0].ToString().ToUpper() + input[1..].ToLower();
        return input;
#endif

    }
    
    public static string ToParamCase(string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return input;
        }

        var paramCaseString = new StringBuilder();
        bool isFirstChar = true;

        foreach (var c in input.Where(char.IsLetterOrDigit))
        {
            if (char.IsUpper(c))
            {
                if (!isFirstChar)
                {
                    paramCaseString.Append('-');
                }

                paramCaseString.Append(char.ToLower(c, CultureInfo.InvariantCulture));
            }
            else
            {
                paramCaseString.Append(c);
            }

            isFirstChar = false;
        }

        return paramCaseString.ToString();
    }
    
    public static string ToPascalCase(string input)
    {
        var normalizedInput = input.ToLower().Replace("_", " ");
        var info = CultureInfo.CurrentCulture.TextInfo;
        return info.ToTitleCase(normalizedInput).Replace(" ", string.Empty);
    }
}
