using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using JJMasterData.Commons.Exceptions;
using JJMasterData.Commons.Localization;
using JJMasterData.Commons.Util;
using JJMasterData.Core.DataManager.Models;
using JJMasterData.Core.Http.Abstractions;
using JJMasterData.Core.UI.Events.Args;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace JJMasterData.Core.DataManager.IO;

public class FormFileManager(string memoryFilesSessionName,
    IHttpContext httpContext,
    IStringLocalizer<MasterDataResources> stringLocalizer, 
    ILogger<FormFileManager> logger)
{
    private IHttpContext HttpContext { get; } = httpContext;
    public event EventHandler<FormUploadFileEventArgs> OnBeforeCreateFile;
    public event EventHandler<FormDeleteFileEventArgs> OnBeforeDeleteFile;
    public event EventHandler<FormRenameFileEventArgs> OnBeforeRenameFile;

    /// <summary>
    /// Session variable name
    /// </summary>
    private string MemoryFilesSessionName { get; } = $"{memoryFilesSessionName}_files";

    /// <summary>
    /// Always apply changes from files on disk,
    /// if it is false, keep it in memory
    /// Default: true
    /// </summary>
    public bool AutoSave { get; set; } = true;

    /// <summary>
    /// Full Directory Path.<para></para>
    /// (Optional) If the path is not given, all files will be stored in the session.
    /// </summary>
    /// <remarks>
    /// The path is OS agnostic, you can use for example C:\Temp\Files\ or /home/gumbarros/Documents/Files,
    /// but beware where you're deploying your application.
    /// </remarks>
    public string FolderPath { get; set; }

    private IStringLocalizer<MasterDataResources> StringLocalizer { get; } = stringLocalizer;

    private ILogger<FormFileManager> Logger { get; } = logger;

    public List<FormFileInfo> MemoryFiles
    {
        get => HttpContext.Session.GetSessionValue<List<FormFileInfo>>(MemoryFilesSessionName);
        set => HttpContext.Session.SetSessionValue(MemoryFilesSessionName, value);
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
            throw new ArgumentNullException(StringLocalizer["Required file name"]);

        if (!FileIO.GetFileNameExtension(currentName).Equals(FileIO.GetFileNameExtension(newName)))
            throw new JJMasterDataException(StringLocalizer["The file extension must remain the same"]);

        var files = GetFiles();
        if (files.Exists(x => x.Content.FileName.Equals(newName)))
            throw new JJMasterDataException(StringLocalizer["A file with the name {0} already exists", newName]);

        if (OnBeforeRenameFile != null)
        {
            var args = new FormRenameFileEventArgs(currentName, newName);
            OnBeforeRenameFile.Invoke(this, args);

            if (!string.IsNullOrEmpty(args.ErrorMessage))
                throw new JJMasterDataException(args.ErrorMessage);
        }

        if (AutoSave && !string.IsNullOrEmpty(FolderPath))
        {
            File.Move(Path.Combine(FolderPath,currentName), Path.Combine(FolderPath, newName));
        }
        else
        {
            var file = files.Find(x => x.Content.FileName.Equals(currentName));
            if (file == null)
                throw new JJMasterDataException(StringLocalizer["file {0} not found!", currentName]);

            files.Remove(file);
            
            file.Content.FileName = newName;
            file.OldName = currentName;

            files.Add(file);
            
            MemoryFiles = files;
        }
    }

    public FormFileInfo GetFile(string fileName)
    {
        var files = GetFiles();
        var file = files.FirstOrDefault(x => fileName.Equals(x.Content.FileName) || fileName.Equals(x.OldName));
        
        return file;
    }

    public void CreateFile(FormFileContent fileContent, bool replaceIfExists)
    {
        if (fileContent == null)
            throw new ArgumentNullException(nameof(fileContent));

        string fileName = fileContent.FileName;

        if (OnBeforeCreateFile != null)
        {
            var args = new FormUploadFileEventArgs(fileContent);
            OnBeforeCreateFile.Invoke(this, args);
            string errorMessage = args.ErrorMessage;

            if (!string.IsNullOrEmpty(errorMessage))
            {
                var exception = new JJMasterDataException(errorMessage);
                Logger.LogError(exception,"Error OnBeforeCreateFile");
                throw exception;
            }
                
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
            {
                var exception = new JJMasterDataException(args.ErrorMessage);
                Logger.LogError(exception, "Error OnBeforeDeleteFile");
                throw exception;
            }
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

        FolderPath = folderPath;
        
        foreach (var file in MemoryFiles)
        {
            string fileName = file.Content.FileName;
            if (file.Deleted)
            {
                string filename = string.IsNullOrEmpty(file.OldName) ? fileName : file.OldName;
                File.Delete(folderPath + filename);
            }
            else if (!string.IsNullOrEmpty(file.OldName))
            {
                File.Move(folderPath + file.OldName, folderPath + fileName);
            }
            else if (file.Content.Bytes != null && file.IsInMemory)
            {
                SavePhysicalFile(file.Content);
            }
        }
        
        MemoryFiles = null;
    }

    private List<FormFileInfo> GetPhysicalFiles()
    {
        var formfiles = new List<FormFileInfo>();
        if (string.IsNullOrEmpty(FolderPath))
            return formfiles;

        var directory = new DirectoryInfo(FolderPath);
        if (directory.Exists)
        {
            var files = directory.GetFiles();
            foreach (var file in files)
            {
                var formfile = new FormFileInfo
                {
                    Content =
                    {
                        FileName = file.Name,
                        Length = file.Length,
                        LastWriteTime = file.LastWriteTime,
                    }
                };
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
        var ms = new MemoryStream(file.Bytes);
        var fileStream = File.Create(fileFullName);
        ms.Seek(0, SeekOrigin.Begin);
        ms.CopyTo(fileStream);
        fileStream.Close();
    }
}
