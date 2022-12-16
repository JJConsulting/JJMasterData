using System;
using System.Data;
using System.Globalization;
using System.IO;
using JJMasterData.Commons.Options;

namespace JJMasterData.Commons.Util;

public class FileIO
    {
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
                filepath = Path.Combine(GetApplicationPath(), filepath);

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
            if(JJMasterDataOptions.IsNetFramework)
                return AppDomain.CurrentDomain.BaseDirectory;
            return Environment.CurrentDirectory;
        }


        ///<summary>
        ///Carrega os registros de um diretório em um DataTable
        ///</summary>
        ///<param name="fullPath">Caminho completo do diretório</param>
        ///<returns>
        ///DataTable contendo nome e tamanho dos arquivos localizados no diretório
        ///</returns>
        ///<remarks>
        ///Author: Lucio Pelinson 21-05-2012
        ///</remarks>
        public static DataTable GetDataTableFiles(string fullPath)
        {
            return GetDataTableFiles(fullPath, null);
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
        public static DataTable GetDataTableFiles(string fullPath, string searchPattern)
        {
            DirectoryInfo oDir = new DirectoryInfo(fullPath);
            DataTable dtFiles = new DataTable();
            DataRow oRow;
            dtFiles.Columns.Add("Id", typeof(string));
            dtFiles.Columns.Add("Nome", typeof(string));
            dtFiles.Columns.Add("Tamanho", typeof(string));
            dtFiles.Columns.Add("TamBytes", typeof(double));
            dtFiles.Columns.Add("LastWriteTime", typeof(string));
            dtFiles.Columns.Add("NomeCompleto", typeof(string));
            if (oDir.Exists)
            {
                int iId = 0;
                FileInfo[] files;
                if (searchPattern == null)
                    files = oDir.GetFiles();
                else
                    files = oDir.GetFiles(searchPattern);

                foreach (FileInfo oFile in files)
                {
                    oRow = dtFiles.NewRow();
                    oRow["Id"] = iId.ToString();
                    oRow["Nome"] = oFile.Name;
                    oRow["Tamanho"] = Format.FormatFileSize(oFile.Length);
                    oRow["TamBytes"] = oFile.Length;
                    oRow["LastWriteTime"] = oFile.LastWriteTime.ToString(CultureInfo.CurrentCulture);
                    dtFiles.Rows.Add(oRow);
                    iId++;
                }
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
