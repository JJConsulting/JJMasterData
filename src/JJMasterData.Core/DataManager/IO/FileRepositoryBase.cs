using JJMasterData.Commons.Language;
using JJMasterData.Commons.Util;
using JJMasterData.Core.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace JJMasterData.Core.DataManager.IO;

internal abstract class FileRepositoryBase 
{
    /// <summary>
    /// Nome da variavél de sessão
    /// </summary>
    public string MemoryFilesSessionName { get; }

    public List<FileDetail> MemoryFiles
    {
        get => JJSession.GetSessionValue<List<FileDetail>>(MemoryFilesSessionName);
        set => JJSession.SetSessionValue(MemoryFilesSessionName, value);
    }

    public FileRepositoryBase(string formName)
    {
        MemoryFilesSessionName = $"{formName}_jjfiles";
    }
    public abstract List<FileDetail> GetFiles();

    public FileDetail GetFile(string fileName)
    {
        var files = GetFiles();
        return files.Find(x => x.FileName.Equals(fileName));
    }

    public int CountFiles()
    {
        var listFiles = GetFiles();
        return listFiles.Count(x => !x.Deleted);
    }

    public string ValidateRename(string currentName, string newName)
    {
        if (string.IsNullOrEmpty(currentName))
            return Translate.Key("Current file name can not be null");

        if (string.IsNullOrWhiteSpace(newName))
            return Translate.Key("Required file name");

        if (!Validate.ValidFileName(newName))
            return Translate.Key("file name cannot contain [{0}] characters", "* < > | : ? \" / \\");

        if (!FileIO.GetFileNameExtension(currentName).Equals(FileIO.GetFileNameExtension(newName)))
            return Translate.Key("The file extension must remain the same");

        return null;
    }

    public void SaveMemoryFiles(string folderPath)
    {
        if (string.IsNullOrEmpty(folderPath))
            throw new ArgumentNullException(nameof(folderPath));
        
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

        MemoryFiles = null;
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
