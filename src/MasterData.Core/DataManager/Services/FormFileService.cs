using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using JJMasterData.Commons.Exceptions;
using JJMasterData.Commons.Storage;
using JJMasterData.Commons.Tasks;
using JJMasterData.Commons.Util;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.DataManager.Models;
using JJMasterData.Core.UI.Events.Args;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;

namespace JJMasterData.Core.DataManager.Services;

public class FormFileService(
    IHttpContextAccessor httpContext,
    IFileStorage fileStorage,
    IStringLocalizer<MasterDataResources> stringLocalizer,
    ILoggerFactory loggerFactory)
{
    public event AsyncEventHandler<FormUploadFileEventArgs> OnBeforeCreateFileAsync;
    public event AsyncEventHandler<FormDeleteFileEventArgs> OnBeforeDeleteFileAsync;
    public event AsyncEventHandler<FormRenameFileEventArgs> OnBeforeRenameFileAsync;

    private ILogger<FormFileService> Logger { get; } = loggerFactory.CreateLogger<FormFileService>();

    public string GetDraftId(string uploadName)
    {
        var request = httpContext.HttpContext?.Request;
        var draftId = GetFormValue(request, $"{uploadName}-draft-id");
        if (string.IsNullOrEmpty(draftId))
            draftId = request == null ? null : GetFirstValue(request.Query["draftId"]);

        return string.IsNullOrWhiteSpace(draftId)
            ? fileStorage.CreateDraftId()
            : fileStorage.GetDraftFolderPath(draftId);
    }

    public async Task<List<FileStorageItem>> GetFilesAsync(string draftId, string folderPath, bool preferTemporaryFiles = false)
    {
        var files = new List<FileStorageItem>();

        if (!string.IsNullOrEmpty(folderPath))
            files.AddRange(await GetStorageFilesAsync(fileStorage, folderPath, false));

        files.AddRange(await GetStorageFilesAsync(fileStorage, fileStorage.GetDraftFolderPath(draftId), true));

        var mergedFiles = files
            .GroupBy(file => file.FileName)
            .Select(group => group.OrderByDescending(file => file.IsTemporary).First())
            .ToList();

        return preferTemporaryFiles && mergedFiles.Exists(file => file.IsTemporary)
            ? mergedFiles.Where(file => file.IsTemporary).ToList()
            : mergedFiles;
    }

    public async Task RenameFileAsync(
        string draftId,
        string folderPath,
        string currentName,
        string newName)
    {
        if (string.IsNullOrEmpty(currentName))
            throw new ArgumentNullException(nameof(currentName));

        if (string.IsNullOrWhiteSpace(newName))
            throw new ArgumentNullException(stringLocalizer["Required file name"]);

        currentName = Path.GetFileName(currentName);
        newName = Path.GetFileName(newName);

        if (!FileIO.GetFileNameExtension(currentName).Equals(FileIO.GetFileNameExtension(newName)))
            throw new JJMasterDataException(stringLocalizer["The file extension must remain the same"]);

        if ((await GetFilesAsync(draftId, folderPath)).Exists(x => x.FileName.Equals(newName)))
            throw new JJMasterDataException(stringLocalizer["A file with the name {0} already exists", newName]);

        if (OnBeforeRenameFileAsync != null)
        {
            var args = new FormRenameFileEventArgs(currentName, newName);
            await OnBeforeRenameFileAsync(this, args);

            if (!string.IsNullOrEmpty(args.ErrorMessage))
                throw new JJMasterDataException(args.ErrorMessage);
        }

        var file = await GetFileAsync(draftId, folderPath, currentName)
                   ?? throw new JJMasterDataException(stringLocalizer["file {0} not found!", currentName]);

        var storageFolderPath = file.IsTemporary || string.IsNullOrEmpty(folderPath)
            ? fileStorage.GetDraftFolderPath(draftId)
            : folderPath;

        var currentFullPath = FileStoragePath.Combine(storageFolderPath, currentName);
        var newFullPath = FileStoragePath.Combine(storageFolderPath, newName);
        await fileStorage.MoveAsync(currentFullPath, newFullPath);
    }

    public async Task<FileStorageItem> GetFileAsync(string draftId, string folderPath, string fileName)
    {
        fileName = Path.GetFileName(fileName);
        return (await GetFilesAsync(draftId, folderPath))
            .Find(x => fileName.Equals(x.FileName));
    }

    public async Task<FileStorageItemKey> GetFileKeyAsync(string draftId, string folderPath, string fileName)
    {
        var file = await GetFileAsync(draftId, folderPath, fileName);
        if (file == null)
            throw new JJMasterDataException(stringLocalizer["file {0} not found!", fileName]);

        return new FileStorageItemKey(
            file.FullPath,
            file.IsTemporary);
    }

    public async Task CreateFileAsync(
        string draftId,
        string folderPath,
        bool autoSave,
        FormFileContent fileContent,
        bool replaceIfExists)
    {
        if (fileContent == null)
            throw new ArgumentNullException(nameof(fileContent));

        fileContent.FileName = Path.GetFileName(fileContent.FileName);

        if (OnBeforeCreateFileAsync != null)
        {
            var args = new FormUploadFileEventArgs(fileContent);
            await OnBeforeCreateFileAsync(this, args);
            string errorMessage = args.ErrorMessage;

            if (!string.IsNullOrEmpty(errorMessage))
            {
                var exception = new JJMasterDataException(errorMessage);
                Logger.LogError(exception, "Error OnBeforeCreateFile");
                throw exception;
            }
        }

        if (replaceIfExists && await CountFilesAsync(draftId, folderPath) > 0)
            await DeleteAllAsync(draftId, folderPath, autoSave);

        var storageFolderPath = autoSave && !string.IsNullOrEmpty(folderPath)
            ? folderPath
            : fileStorage.GetDraftFolderPath(draftId);

        var fullPath = FileStoragePath.Combine(storageFolderPath, fileContent.FileName);
        await fileStorage.SaveAsync(fullPath, fileContent.Stream, true);
    }

    public async Task DeleteFileAsync(
        string draftId,
        string folderPath,
        string fileName)
    {
        fileName = Path.GetFileName(fileName);

        if (OnBeforeDeleteFileAsync != null)
        {
            var args = new FormDeleteFileEventArgs(fileName);
            await OnBeforeDeleteFileAsync(this, args);

            if (!string.IsNullOrEmpty(args.ErrorMessage))
            {
                var exception = new JJMasterDataException(args.ErrorMessage);
                Logger.LogError(exception, "Error OnBeforeDeleteFile");
                throw exception;
            }
        }

        var file = await GetFileAsync(draftId, folderPath, fileName);
        if (file == null)
            return;

        var storageFolderPath = file.IsTemporary ? fileStorage.GetDraftFolderPath(draftId) : folderPath;
        var fullPath = FileStoragePath.Combine(storageFolderPath, fileName);
        await fileStorage.DeleteAsync(fullPath);
    }

    public async Task DeleteAllAsync(string draftId, string folderPath, bool autoSave)
    {
        await fileStorage.DeleteFolderAsync(fileStorage.GetDraftFolderPath(draftId));

        if (autoSave && !string.IsNullOrEmpty(folderPath))
            await fileStorage.DeleteFolderAsync(folderPath);
    }

    public async Task<int> CountFilesAsync(string draftId, string folderPath)
    {
        return (await GetFilesAsync(draftId, folderPath)).Count;
    }

    public async Task PromoteTemporaryFilesAsync(string draftId, string folderPath, bool deleteExistingFiles = false)
    {
        if (string.IsNullOrEmpty(folderPath))
            throw new ArgumentNullException(nameof(folderPath));

        await fileStorage.PromoteAsync(draftId, fileStorage, folderPath, deleteExistingFiles);
    }

    public Task<Stream> OpenReadAsync(FileStorageItem reference)
    {
        return fileStorage.OpenReadAsync(reference.FullPath);
    }

    public async Task SaveFormTemporaryFilesAsync(FormElement formElement, Dictionary<string, object> values)
    {
        var uploadFields = formElement.Fields.FindAll(x => x.Component == FormComponent.File);
        if (uploadFields.Count == 0)
            return;

        foreach (var field in uploadFields)
        {
            var draftId = GetDraftId($"{field.Name}-upload-view-files");
            var folderPath = FileStoragePath.GetFolderPath(formElement, field, values);

            await PromoteTemporaryFilesAsync(draftId, folderPath, deleteExistingFiles: !field.DataFile.MultipleFile);
        }
    }

    public async Task DeleteFilesAsync(FormElement formElement, Dictionary<string, object> primaryKeys)
    {
        var fileFields = formElement.Fields.FindAll(x => x.Component == FormComponent.File);
        if (fileFields.Count == 0)
            return;

        foreach (var field in fileFields)
        {
            var draftId = GetDraftId($"{field.Name}-upload-view-files");
            var folderPath = FileStoragePath.GetFolderPath(formElement, field, primaryKeys);
            await DeleteAllAsync(draftId, folderPath, autoSave: true);
        }
    }

    private static string GetFirstValue(StringValues values)
    {
        return values.Count == 0 ? null : values[0];
    }

    private static string GetFormValue(HttpRequest request, string key)
    {
        if (request?.HasFormContentType != true)
            return null;

        return GetFirstValue(request.Form[key]);
    }

    private static async Task<IEnumerable<FileStorageItem>> GetStorageFilesAsync(
        IFileStorage storage,
        string folderPath,
        bool temporary)
    {
        if (string.IsNullOrEmpty(folderPath))
            return [];

        return (await storage.ListAsync(folderPath))
            .Select(file => new FileStorageItem
            {
                IsTemporary = temporary,
                FileName = file.FileName,
                Length = file.Length,
                LastWriteTime = file.LastWriteTime,
                FolderPath = folderPath
            });
    }

}
