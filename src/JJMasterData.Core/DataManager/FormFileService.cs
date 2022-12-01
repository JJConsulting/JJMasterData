using JJMasterData.Commons.Language;
using JJMasterData.Commons.Util;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.FormEvents.Args;
using JJMasterData.Core.Http;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace JJMasterData.Core.DataManager;

internal class FormFileService
{
    public EventHandler<FormUploadFileEventArgs> OnBeforeCreateFile;
    public EventHandler<FormDeleteFileEventArgs> OnBeforeDeleteFile;
    public EventHandler<FormRenameFileEventArgs> OnBeforeRenameFile;

    /// <summary>
    /// Nome da variavél de sessão
    /// </summary>
    public string MemoryFilesSessionName { get; private set; }

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

    public List<FormFileInfo> MemoryFiles
    {
        get => JJSession.GetSessionValue<List<FormFileInfo>>(MemoryFilesSessionName);
        set => JJSession.SetSessionValue(MemoryFilesSessionName, value);
    }

    public FormFileService(string memoryFilesSessionName)
    {
        MemoryFilesSessionName = $"{memoryFilesSessionName}_files";
        AutoSave = true;
    }

    public List<FormFileInfo> GetFiles()
    {
        List<FormFileInfo> files = null;

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
        if (files.Exists(x => x.Content.FileName.Equals(newName)))
            throw new Exception(Translate.Key("A file with the name {0} already exists", newName));

        if (OnBeforeRenameFile != null)
        {
            var args = new FormRenameFileEventArgs(currentName, newName);
            OnBeforeRenameFile.Invoke(this, args);

            if (!string.IsNullOrEmpty(args.ErrorMessage))
                throw new Exception(args.ErrorMessage);
        }

        if (AutoSave & !string.IsNullOrEmpty(FolderPath))
        {
            File.Move(FolderPath + currentName, FolderPath + newName);
        }
        else
        {
            var file = files.Find(x => x.Content.FileName.Equals(currentName));
            if (file == null)
                throw new Exception(Translate.Key("file {0} not found!", currentName));

            file.Content.FileName = newName;
            if (file.Content.FileStream == null & string.IsNullOrEmpty(file.OriginName))
                file.OriginName = currentName;

            MemoryFiles = files;
        }
    }

    public FormFileInfo GetFile(string fileName)
    {
        var files = GetFiles();
        return files.Find(x => x.Content.FileName.Equals(fileName));
    }

    public void CreateFile(FormFileContent fileContent, bool replaceIfExists)
    {
        if (fileContent == null)
            throw new ArgumentNullException(nameof(FormFileContent));

        string fileName = fileContent.FileName;

        if (OnBeforeCreateFile != null)
        {
            var evt = new FormUploadFileEventArgs(fileContent);
            OnBeforeCreateFile.Invoke(this, evt);
            string errorMessage = evt.ErrorMessage;

            if (!string.IsNullOrEmpty(errorMessage))
                throw new Exception(errorMessage);
        }

        if (replaceIfExists && CountFiles() > 0)
            DeleteAll();

        if (fileName?.LastIndexOf("\\") > 0)
            // ReSharper disable once ReplaceSubstringWithRangeIndexer
            fileName = fileName.Substring(fileName.LastIndexOf("\\", StringComparison.Ordinal) + 1);

        if (AutoSave & !string.IsNullOrEmpty(FolderPath))
        {
            SavePhysicalFile(fileContent);
        }
        else
        {
            var files = GetFiles();
            var currentFile = files.Find(x => x.Content.FileName.Equals(fileName));
            if (currentFile == null)
            {
                var file = new FormFileInfo
                {
                    Content = fileContent
                };
                files.Add(file);
            }
            else
            {
                currentFile.Content = fileContent;
                currentFile.Deleted = false;
            }

            MemoryFiles = files;
        }
    }

    public void DeleteFile(string fileName)
    {
        if (OnBeforeDeleteFile != null)
        {
            var args = new FormDeleteFileEventArgs(fileName);
            OnBeforeDeleteFile.Invoke(this, args);

            if (!string.IsNullOrEmpty(args.ErrorMessage))
                throw new Exception(args.ErrorMessage);
        }

        if (AutoSave & !string.IsNullOrEmpty(FolderPath))
        {
            File.Delete(Path.Combine(FolderPath, fileName));
        }
        else
        {
            var files = GetFiles();
            var file = files.Find(x => x.Content.FileName.Equals(fileName));
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
            throw new ArgumentNullException(nameof(folderPath));

        if (MemoryFiles == null)
            return;

        if (!Directory.Exists(folderPath))
            Directory.CreateDirectory(folderPath);

        foreach (var file in MemoryFiles)
        {
            string fileName = file.Content.FileName;
            if (file.Deleted)
            {
                string filename = string.IsNullOrEmpty(file.OriginName) ? fileName : file.OriginName;
                File.Delete(folderPath + filename);
            }
            else if (!string.IsNullOrEmpty(file.OriginName))
            {
                File.Move(folderPath + file.OriginName, folderPath + fileName);
            }
            else if (file.Content.FileStream != null && file.IsInMemory)
            {
                SavePhysicalFile(file.Content);
            }
        }

        FolderPath = folderPath;
        MemoryFiles = null;
    }

    private List<FormFileInfo> GetPhysicalFiles()
    {
        var formfiles = new List<FormFileInfo>();
        if (string.IsNullOrEmpty(FolderPath))
            return formfiles;

        var oDir = new DirectoryInfo(FolderPath);
        if (oDir.Exists)
        {
            FileInfo[] files = oDir.GetFiles();
            foreach (FileInfo oFile in files)
            {
                var formfile = new FormFileInfo();
                formfile.Content.FileName = oFile.Name;
                formfile.Content.SizeBytes = oFile.Length;
                formfile.Content.LastWriteTime = oFile.LastWriteTime;
                formfiles.Add(formfile);
            }
        }
        return formfiles;
    }

    private void SavePhysicalFile(FormFileContent file)
    {
        if (file == null)
            throw new ArgumentNullException(nameof(file));

        if (string.IsNullOrEmpty(FolderPath))
            throw new ArgumentNullException(nameof(FolderPath));

        if (!Directory.Exists(FolderPath))
            Directory.CreateDirectory(FolderPath);

        string fileFullName = Path.Combine(FolderPath, file.FileName);
        var ms = file.FileStream;
        var fileStream = File.Create(fileFullName);
        ms.Seek(0, SeekOrigin.Begin);
        ms.CopyTo(fileStream);
        fileStream.Close();
    }

    internal static void SaveFormMemoryFiles(FormElement FormElement, Hashtable primaryKeys)
    {
        var uploadFields = FormElement.Fields.ToList().FindAll(x => x.Component == FormComponent.File);
        if (uploadFields.Count == 0)
            return;

        var pathBuilder = new FormFilePathBuilder(FormElement);
        foreach (var field in uploadFields)
        {
            string folderPath = pathBuilder.GetFolderPath(field, primaryKeys);
            var fileService = new FormFileService(field.Name + "_formupload");
            fileService.SaveMemoryFiles(folderPath);
        }
    }

    internal static void DeleteFiles(FormElement FormElement, Hashtable primaryKeys)
    {
        var uploadFields = FormElement.Fields.ToList().FindAll(x => x.Component == FormComponent.File);
        if (uploadFields.Count == 0)
            return;
        
        foreach (var field in uploadFields)
        {
            var fileService = new FormFileService(field.Name + "_formupload");
            fileService.DeleteAll();
        }
    }

}
