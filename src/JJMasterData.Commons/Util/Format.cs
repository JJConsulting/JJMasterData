using System;
using System.Globalization;
using System.Text;

namespace JJMasterData.Commons.Util;

public static class Format
{
    
    public static string DateFormat => DateTimeFormatInfo.CurrentInfo.ShortDatePattern;

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
        StringBuilder sLog = new StringBuilder();
        if (ts.Days == 1)
        {
            sLog.Append(ts.Days);
            sLog.Append(" dia");
        }
        else if (ts.Days > 1)
        {
            sLog.Append(ts.Days);
            sLog.Append(" dias");
        }

        int horas = ts.Hours;
        if (horas == 1)
        {
            if (sLog.Length > 0)
                sLog.Append(" e ");

            sLog.Append(ts.Hours);
            sLog.Append(" hora");
        }
        else if (horas > 1)
        {
            if (sLog.Length > 0)
                sLog.Append(" e ");

            sLog.Append(horas);
            sLog.Append(" horas");
        }

        int minutes = ts.Minutes;
        if (minutes == 1)
        {
            if (sLog.Length > 0)
                sLog.Append(" e ");

            sLog.Append(ts.Minutes);
            sLog.Append(" minuto");
        }
        else if (minutes > 1)
        {
            if (sLog.Length > 0)
                sLog.Append(" e ");

            sLog.Append(minutes);
            sLog.Append(" minutos");
        }

        int seconds = ts.Seconds;
        if (seconds > 0)
        {
            if (sLog.Length > 0)
                sLog.Append(" e ");

            sLog.Append(seconds);
            sLog.Append(" segundos");
        }
        
        if (sLog.Length == 0)
        {
            sLog.Append(ts.TotalMilliseconds.ToString("N2"));
            sLog.Append(" Milissegundos");
        }
        
        return sLog.ToString();
    }

    /// <summary>
    /// Formata o tamanho de bytes para a medida mais proxima
    /// </summary>
    /// <param name="value">Bytes</param>
    /// <returns>Tamanho formatado para ser exibido</returns> 
    /// <remarks>
    /// Autor: Lucio Pelinson 21-05-2012
    /// </remarks>
    public static string FormatFileSize(long value)
    {
        const Decimal OneKiloByte = 1024M;
        const Decimal OneMegaByte = OneKiloByte * 1024M;
        const Decimal OneGigaByte = OneMegaByte * 1024M;
        Decimal size = Convert.ToDecimal(value);
        string suffix = "";
        if (size > OneGigaByte)
        {
            size /= OneGigaByte;
            suffix = "GB";
        }
        else if (size > OneMegaByte)
        {
            size /= OneMegaByte;
            suffix = "MB";
        }
        else if (size > OneKiloByte)
        {
            size /= OneKiloByte;
            suffix = "kB";
        }
        else
        {
            suffix = "B";
        }

        return String.Format("{0:N2} {1}", size, suffix);

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
    public static string FormatCnpj_Cpf(string value)
    {
        value = StringManager.ClearCpfCnpjChars(value);
        string sRet = "";
        if (value.Trim().Length > 11)
            sRet = FormatCnpj(value);
        else
            sRet = FormatCpf(value);

        return sRet;
    }

    /// <summary>
    /// Formata CEP
    /// </summary>
    public static string FormatCep(string cep)
    {
        string sCep = cep.Trim().Replace("-", "");
        int nCep;
        if (int.TryParse(sCep, out nCep))
        {
            sCep = nCep.ToString("00000-000");
        }
        return sCep;
    }

    /// <summary>
    /// Formata número de telefone
    /// </summary>
    /// <param name="tel">Número de Telefone em qualquer formato</param>
    /// <returns>Número telefone formatado</returns>
    /// <remarks>Lucio Pelinson 21-02-2013</remarks> 
    public static string FormatTel(string tel)
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

    public static String FormatDecBR2USA(double value)
    {
        return FormatDecBR2USA(value.ToString());
    }

    public static String FormatDecBR2USA(string text)
    {
        String result = text.Replace(".", "");
        result = result.Replace(",", ".");
        return result;
    }

}
