using System;
using System.Data;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using JJMasterData.Commons.Options;

namespace JJMasterData.Commons.Util;

public class FileIO
{
    public static string SanitizePath(string inputPath)
    {
        var invalidChars = Path.GetInvalidFileNameChars();
        
        var sanitizedPath = Regex.Replace(inputPath, "[" + Regex.Escape(new string(invalidChars)) + "]", "");

        return sanitizedPath;
    }
    
    /// <summary>
    /// Retorna o tipo do arquivo em minusculo
    /// </summary>
    public static string GetFileNameExtension(string filename)
    {
        string extension = string.Empty;
        int lastIndex = filename.LastIndexOf('.');
        if (lastIndex > 0)
        {
            extension = filename.Substring(lastIndex).ToLower();
        }
        return extension;
    }

    /// <summary>
    /// Returns physical path based on file name, also replaces DateTime templates.
    /// </summary>
    /// <param name="filepath">Part or full path of file</param>
    /// <returns>File full path</returns>
    public static string ResolveFilePath(string filepath)
    {
        var now = DateTime.Now;
        filepath = filepath.Replace("dd", now.ToString("dd"));
        filepath = filepath.Replace("MM", now.ToString("MM"));
        filepath = filepath.Replace("yyyy", now.ToString("yyyy"));
        filepath = filepath.Replace("HH", now.ToString("HH"));
        filepath = filepath.Replace("mm", now.ToString("mm"));
        filepath = filepath.Replace("ss", now.ToString("ss"));

        //We defaulted the path to never start with a slash
        if (filepath.StartsWith("\\") && !filepath.Substring(1, 1).Equals("\\"))
            filepath = filepath.Substring(1);

        //If the full path is not provided, we include the path from the application
        if (!filepath.Contains(":") && !filepath.StartsWith("\\\\") && !filepath.StartsWith("/"))
            filepath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, filepath);

        return filepath;
    }

    /// <summary>
    /// Returns the application path.
    /// .NET Framework: AppDomain.CurrentDomain.BaseDirectory
    /// .NET 6+: Environment.CurrentDirectory
    /// </summary>
    /// <returns></returns>
    public static string GetApplicationPath()
    {
        if (JJMasterDataCommonsOptions.IsNetFramework)
            return AppDomain.CurrentDomain.BaseDirectory;

        return Environment.CurrentDirectory;
    }


    ///<summary>
    ///Carrega os registros de um diretório em um DataTable
    ///</summary>
    ///<param name="fullPath">Caminho completo do diretório</param>
    ///<param name="searchPattern">Condição para filtro de arquivos (opcional)</param>
    ///<returns>
    ///DataTable contendo nome e tamanho dos arquivos localizados no diretório
    ///</returns>
    ///<remarks>
    ///Author: Lucio Pelinson 30-05-2017
    ///</remarks>
    public static DataTable GetDataTableFiles(string fullPath, string searchPattern = null)
    {
        var dir = new DirectoryInfo(fullPath);
        var dtFiles = new DataTable();
        dtFiles.Columns.Add("Id", typeof(string));
        dtFiles.Columns.Add("Nome", typeof(string));
        dtFiles.Columns.Add("Tamanho", typeof(string));
        dtFiles.Columns.Add("TamBytes", typeof(double));
        dtFiles.Columns.Add("LastWriteTime", typeof(string));
        dtFiles.Columns.Add("NomeCompleto", typeof(string));

        if (!dir.Exists)
            return dtFiles;

        int iId = 0;
        FileInfo[] files;

        if (searchPattern == null)
            files = dir.GetFiles();
        else
            files = dir.GetFiles(searchPattern);

        foreach (FileInfo oFile in files)
        {
            var row = dtFiles.NewRow();
            row["Id"] = iId.ToString();
            row["Nome"] = oFile.Name;
            row["Tamanho"] = Format.FormatFileSize(oFile.Length);
            row["TamBytes"] = oFile.Length;
            row["LastWriteTime"] = oFile.LastWriteTime.ToString(CultureInfo.CurrentCulture);
            dtFiles.Rows.Add(row);
            iId++;
        }
        return dtFiles;
    }

    public static bool IsFileLocked(string filePath) => IsFileLocked(new FileInfo(filePath));

    public static bool IsFileLocked(FileInfo file)
    {

        if (!File.Exists(file.FullName))
            return false;

        try
        {
            using var stream = file.Open(FileMode.Open, FileAccess.Read, FileShare.None);
            stream.Close();
        }
        catch (IOException)
        {
            //the file is unavailable because it is:
            //still being written to
            //or being processed by another thread
            //or does not exist (has already been processed)
            return true;
        }

        //file is not locked
        return false;
    }
}
