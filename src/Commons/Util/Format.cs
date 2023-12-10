using System;
using System.Globalization;
using System.Text;

namespace JJMasterData.Commons.Util;

public static class Format
{
    
    public static string DateFormat => DateTimeFormatInfo.CurrentInfo.ShortDatePattern;

    public static string TimeFormat => DateTimeFormatInfo.CurrentInfo.LongTimePattern;
    
    public static string DateTimeFormat => $"{DateFormat} {DateTimeFormatInfo.CurrentInfo.LongTimePattern}";
    public static string FormatTimeSpan(DateTime dtFrom, DateTime dtTo)
    {
        var ts = dtTo - dtFrom;
        return FormatTimeSpan(ts);
    }

    /// <summary>
    /// Escreve por extenso o intervalo de datas para o período mais próximo
    /// </summary>
    /// <param name="ts">Time Interval</param>
    /// <returns>Texto com a hora</returns>
    public static string FormatTimeSpan(TimeSpan ts)
    {
        var timeSpanResult = new StringBuilder();
        if (ts.Days == 1)
        {
            timeSpanResult.Append(ts.Days);
            timeSpanResult.Append(" dia");
        }
        else if (ts.Days > 1)
        {
            timeSpanResult.Append(ts.Days);
            timeSpanResult.Append(" dias");
        }

        int horas = ts.Hours;
        if (horas == 1)
        {
            if (timeSpanResult.Length > 0)
                timeSpanResult.Append(" e ");

            timeSpanResult.Append(ts.Hours);
            timeSpanResult.Append(" hora");
        }
        else if (horas > 1)
        {
            if (timeSpanResult.Length > 0)
                timeSpanResult.Append(" e ");

            timeSpanResult.Append(horas);
            timeSpanResult.Append(" horas");
        }

        var minutes = ts.Minutes;
        if (minutes == 1)
        {
            if (timeSpanResult.Length > 0)
                timeSpanResult.Append(" e ");

            timeSpanResult.Append(ts.Minutes);
            timeSpanResult.Append(" minuto");
        }
        else if (minutes > 1)
        {
            if (timeSpanResult.Length > 0)
                timeSpanResult.Append(" e ");

            timeSpanResult.Append(minutes);
            timeSpanResult.Append(" minutos");
        }

        int seconds = ts.Seconds;
        if (seconds > 0)
        {
            if (timeSpanResult.Length > 0)
                timeSpanResult.Append(" e ");

            timeSpanResult.Append(seconds);
            timeSpanResult.Append(" segundos");
        }
        
        if (timeSpanResult.Length == 0)
        {
            timeSpanResult.Append(ts.TotalMilliseconds.ToString("N2"));
            timeSpanResult.Append(" Milissegundos");
        }
        
        return timeSpanResult.ToString();
    }

    /// <summary>
    /// Formata o tamanho de bytes para a medida mais proxima
    /// </summary>
    /// <param name="value">Bytes</param>
    /// <returns>Tamanho formatado para ser exibido</returns> 
    /// <remarks>
    /// Author: Lucio Pelinson 21-05-2012
    /// </remarks>
    public static string FormatFileSize(long value)
    {
        const Decimal oneKiloByte = 1024M;
        const Decimal oneMegaByte = oneKiloByte * 1024M;
        const Decimal oneGigaByte = oneMegaByte * 1024M;
        Decimal size = Convert.ToDecimal(value);
        string suffix = "";
        if (size > oneGigaByte)
        {
            size /= oneGigaByte;
            suffix = "GB";
        }
        else if (size > oneMegaByte)
        {
            size /= oneMegaByte;
            suffix = "MB";
        }
        else if (size > oneKiloByte)
        {
            size /= oneKiloByte;
            suffix = "kB";
        }
        else
        {
            suffix = "B";
        }

        return $"{size:N2} {suffix}";

    }

    /// <summary>
    /// Formata um CNPJ válido
    /// </summary>
    public static string FormatCnpj(string cnpj)
    {
        string sRet = "";
        double nCnpj;
        if (Double.TryParse(cnpj, out nCnpj))
        {
            sRet = nCnpj.ToString(@"#00\.000\.000\/0000\-00");
        }

        return sRet;
    }

    /// <summary>
    /// Formata um CPF válido
    /// </summary>
    public static string FormatCpf(string cpf)
    {
        string sRet = "";
        double nCpf;
        if (Double.TryParse(cpf, out nCpf))
        {
            sRet = nCpf.ToString(@"#000\.000\.000\-00");
        }

        return sRet;
    }

    /// <summary>
    /// Formata um CNPJ ou CPF válido
    /// </summary>
    public static string FormatCnpjCpf(string value)
    {
        value = StringManager.ClearCpfCnpjChars(value);
        var formattedCnpf = value.Trim().Length > 11 ? FormatCnpj(value) : FormatCpf(value);

        return formattedCnpf;
    }

    /// <summary>
    /// Formata CEP
    /// </summary>
    public static string FormatCep(string cep)
    {
        var cepString = cep.Trim().Replace("-", "");
        if (int.TryParse(cepString, out var numericCep))
        {
            cepString = numericCep.ToString("00000-000");
        }
        return cepString;
    }

    /// <summary>
    /// Formata número de telefone
    /// </summary>
    /// <param name="tel">Número de Telefone em qualquer formato</param>
    /// <returns>Número telefone formatado</returns>
    /// <remarks>Lucio Pelinson 21-02-2013</remarks> 
    public static string FormatPhone(string tel)
    {
        double nTel;
        string sTel = StringManager.ClearTelChars(tel);
        
        if (sTel.Length > 9 && sTel.StartsWith("0"))
        {
            sTel = sTel.Substring(1);
        }

        if (double.TryParse(sTel, out nTel))
        {
            if (sTel.Length == 8)
            {
                sTel = nTel.ToString("0000-0000");
            }
            else if (sTel.Length == 9)
            {
                sTel = nTel.ToString("00000-0000");
            }
            else if (sTel.Length == 10)
            {
                sTel = nTel.ToString("(00) 0000-0000");
            }
            else if (sTel.Length == 11)
            {
                sTel = nTel.ToString("(00) 00000-0000");
            }
            else
            {
                sTel = tel;
            }
        }
        else
        {
            sTel = tel;
        }

        return sTel;
    }

    public static string FormatDecBr2Usa(double value)
    {
        return FormatDecBr2Usa(value.ToString(CultureInfo.CurrentCulture));
    }

    public static string FormatDecBr2Usa(string text)
    {
        String result = text.Replace(".", "");
        result = result.Replace(",", ".");
        return result;
    }

}
