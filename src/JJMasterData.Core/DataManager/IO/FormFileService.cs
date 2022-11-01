using JJMasterData.Commons.Language;
using JJMasterData.Commons.Util;
using JJMasterData.Core.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace JJMasterData.Core.DataManager.IO;

public class FormFileService
{
    /// <summary>
    /// Nome da variavél de sessão
    /// </summary>
    public string MemoryFilesSessionName { get; set; }

    /// <summary>
    /// Sempre aplica as alterações dos arquivos em disco, 
    /// se for falso mantem na memoria
    /// Default: true
    /// </summary>
    public bool AutoSave { get; set; }

    /// <summary>
    /// Caminho Completo do Diretório.<para></para>
    /// (Opcional) Se o caminho não for informado, todos os arquivos serão armazenado na sessão.
    /// </summary>
    /// <remarks>
    /// Exemplo: c:\temp\files\
    /// </remarks>
    public string FolderPath { get; set; }

    public List<FileDetail> MemoryFiles
    {
        get => JJSession.GetSessionValue<List<FileDetail>>(MemoryFilesSessionName);
        set => JJSession.SetSessionValue(MemoryFilesSessionName, value);
    }

    public FormFileService()
    {
        AutoSave = true;
    }

    public List<FileDetail> GetFiles()
    {
        List<FileDetail> files = null;

        if (!AutoSave || string.IsNullOrEmpty(FolderPath))
            files = MemoryFiles;

        return files ?? GetPhysicalFiles();
    }

    public void RenameFile(string currentName, string newName)
    {
        if (string.IsNullOrEmpty(currentName))
            throw new ArgumentNullException(nameof(currentName));

        if (string.IsNullOrWhiteSpace(newName))
            throw new Exception(Translate.Key("Required file name"));

        if (!Validate.ValidFileName(newName))
            throw new Exception(Translate.Key("file name cannot contain [{0}] characters", "* < > | : ? \" / \\"));

        if (!FileIO.GetFileNameExtension(currentName).Equals(FileIO.GetFileNameExtension(newName)))
            throw new Exception(Translate.Key("The file extension must remain the same"));

        var files = GetFiles();
        if (files.Exists(x => x.FileName.Equals(newName)))
            throw new Exception(Translate.Key("A file with the name {0} already exists", newName));

        if (AutoSave & !string.IsNullOrEmpty(FolderPath))
        {
            File.Move(FolderPath + currentName, FolderPath + newName);
        }
        else
        {
            var file = files.Find(x => x.FileName.Equals(currentName));
            if (file == null)
                throw new Exception(Translate.Key("file {0} not found!", currentName));

            file.FileName = newName;
            if (file.FileStream == null & string.IsNullOrEmpty(file.OriginName))
                file.OriginName = currentName;

            MemoryFiles = files;
        }
    }

    public FileDetail GetFile(string fileName)
    {
        var files = GetFiles();
        return files.Find(x => x.FileName.Equals(fileName));
    }

    public void CreateFile(string fileName, MemoryStream memoryStream)
    {
        if (fileName?.LastIndexOf("\\") > 0)
            fileName = fileName.Substring(fileName.LastIndexOf("\\") + 1);

        if (AutoSave & !string.IsNullOrEmpty(FolderPath))
        {
            SavePhysicalFile(FolderPath, fileName, memoryStream);
        }
        else
        {
            var files = GetFiles();
            var currentFile = files.Find(x => x.FileName.Equals(fileName));
            if (currentFile == null)
            {
                var file = new FileDetail
                {
                    FileName = fileName,
                    FileStream = memoryStream,
                    LastWriteTime = DateTime.Now,
                    SizeBytes = memoryStream.Length
                };
                files.Add(file);
            }
            else
            {
                currentFile.Deleted = false;
                currentFile.FileStream = memoryStream;
                currentFile.LastWriteTime = DateTime.Now;
                currentFile.SizeBytes = memoryStream.Length;
            }

            MemoryFiles = files;

        }
    }

    public void DeleteFile(string fileName)
    {
        if (AutoSave & !string.IsNullOrEmpty(FolderPath))
        {
            File.Delete(FolderPath + fileName);
        }
        else
        {
            var files = GetFiles();
            var file = files.Find(x => x.FileName.Equals(fileName));
            if (file != null)
            {
                if (!file.IsInMemory)
                    file.Deleted = true;
                else
                    files.Remove(file);
            }

            MemoryFiles = files;
        }
    }

    public void ClearMemoryFiles()
    {
        MemoryFiles = null;
    }

    public void DeleteAll()
    {
        if (!string.IsNullOrEmpty(FolderPath))
        {
            if (Directory.Exists(FolderPath))
                Directory.Delete(FolderPath, true);
        }

        MemoryFiles = null;
    }

    public int CountFiles()
    {
        var listFiles = GetFiles();
        return listFiles.Count(x => !x.Deleted);
    }

    public void SaveMemoryFiles(string folderPath)
    {
        if (string.IsNullOrEmpty(folderPath))
        {
            throw new ArgumentNullException(nameof(folderPath));
        }

        if (MemoryFiles == null)
            return;

        if (!Directory.Exists(folderPath))
            Directory.CreateDirectory(folderPath);

        foreach (var file in MemoryFiles)
        {
            if (file.Deleted)
            {
                string filename = string.IsNullOrEmpty(file.OriginName) ? file.FileName : file.OriginName;
                File.Delete(folderPath + filename);
            }
            else if (!string.IsNullOrEmpty(file.OriginName))
            {
                File.Move(folderPath + file.OriginName, folderPath + file.FileName);
            }
            else if (file.FileStream != null && file.IsInMemory)
            {
                SavePhysicalFile(folderPath, file.FileName, file.FileStream);
            }
        }

        FolderPath = folderPath;
        MemoryFiles = null;
    }

    private List<FileDetail> GetPhysicalFiles()
    {
        var formfiles = new List<FileDetail>();
        if (string.IsNullOrEmpty(FolderPath))
            return formfiles;

        var oDir = new DirectoryInfo(FolderPath);
        if (oDir.Exists)
        {
            FileInfo[] files = oDir.GetFiles();
            foreach (FileInfo oFile in files)
            {
                var formfile = new FileDetail();
                formfile.FileName = oFile.Name;
                formfile.SizeBytes = oFile.Length;
                formfile.LastWriteTime = oFile.LastWriteTime;
                formfiles.Add(formfile);
            }
        }
        return formfiles;
    }

    private void SavePhysicalFile(string folderPath, string fileName, MemoryStream ms)
    {
        if (!Directory.Exists(folderPath))
            Directory.CreateDirectory(folderPath);

        string fileFullName = folderPath + fileName;

        var fileStream = File.Create(fileFullName);
        ms.Seek(0, SeekOrigin.Begin);
        ms.CopyTo(fileStream);
        fileStream.Close();
    }

}
