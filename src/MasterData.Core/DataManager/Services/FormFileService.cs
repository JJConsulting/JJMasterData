using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using JJMasterData.Commons.Exceptions;
using JJMasterData.Commons.Tasks;
using JJMasterData.Commons.Util;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.DataManager.Models;
using JJMasterData.Core.DataManager.Storage;
using JJMasterData.Core.UI.Events.Args;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;

namespace JJMasterData.Core.DataManager.IO;

public class FormFileService(
    IHttpContextAccessor httpContext,
    ITemporaryUploadStore temporaryUploadStore,
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
            ? temporaryUploadStore.CreateDraftId()
            : temporaryUploadStore.GetDraftFolderKey(draftId);
    }

    public async Task<List<FormFileInfo>> GetFilesAsync(string draftId, string folderKey,
        bool preferTemporaryFiles = false)
    {
        var files = new List<FormFileInfo>();

        if (!string.IsNullOrEmpty(folderKey))
            files.AddRange(await GetStorageFilesAsync(fileStorage, folderKey, false));

        files.AddRange(await GetStorageFilesAsync(temporaryUploadStore, temporaryUploadStore.GetDraftFolderKey(draftId),
            true));

        var mergedFiles = files
            .GroupBy(file => file.Content.FileName)
            .Select(group => group.OrderByDescending(file => file.IsTemporary).First())
            .ToList();

        return preferTemporaryFiles && mergedFiles.Exists(file => file.IsTemporary)
            ? mergedFiles.Where(file => file.IsTemporary).ToList()
            : mergedFiles;
    }

    public async Task RenameFileAsync(
        string draftId,
        string folderKey,
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

        if ((await GetFilesAsync(draftId, folderKey)).Exists(x => x.Content.FileName.Equals(newName)))
            throw new JJMasterDataException(stringLocalizer["A file with the name {0} already exists", newName]);

        if (OnBeforeRenameFileAsync != null)
        {
            var args = new FormRenameFileEventArgs(currentName, newName);
            await OnBeforeRenameFileAsync(this, args);

            if (!string.IsNullOrEmpty(args.ErrorMessage))
                throw new JJMasterDataException(args.ErrorMessage);
        }

        var file = await GetFileAsync(draftId, folderKey, currentName)
                   ?? throw new JJMasterDataException(stringLocalizer["file {0} not found!", currentName]);

        var storage = file.IsTemporary || string.IsNullOrEmpty(folderKey)
            ? temporaryUploadStore
            : fileStorage;
        var storageFolderKey = file.IsTemporary || string.IsNullOrEmpty(folderKey)
            ? temporaryUploadStore.GetDraftFolderKey(draftId)
            : folderKey;

        await storage.RenameAsync(storageFolderKey, currentName, newName);
    }

    public async Task<FormFileInfo> GetFileAsync(string draftId, string folderKey, string fileName)
    {
        fileName = Path.GetFileName(fileName);
        return (await GetFilesAsync(draftId, folderKey))
            .Find(x => fileName.Equals(x.Content.FileName) || fileName.Equals(x.OldName));
    }

    public async Task<FileStorageReference> GetFileReferenceAsync(string draftId, string folderKey, string fileName)
    {
        var file = await GetFileAsync(draftId, folderKey, fileName);
        if (file == null)
            throw new JJMasterDataException(stringLocalizer["file {0} not found!", fileName]);

        return FileStorageReference.Create(
            file.IsTemporary ? temporaryUploadStore.GetDraftFolderKey(draftId) : folderKey,
            file.Content.FileName,
            file.IsTemporary);
    }

    public async Task CreateFileAsync(
        string draftId,
        string folderKey,
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

        if (replaceIfExists && await CountFilesAsync(draftId, folderKey) > 0)
            await DeleteAllAsync(draftId, folderKey, autoSave);

        var storage = autoSave && !string.IsNullOrEmpty(folderKey)
            ? fileStorage
            : temporaryUploadStore;
        var storageFolderKey = autoSave && !string.IsNullOrEmpty(folderKey)
            ? folderKey
            : temporaryUploadStore.GetDraftFolderKey(draftId);

        await storage.SaveAsync(storageFolderKey, fileContent.FileName, fileContent.Stream, true);
    }

    public async Task DeleteFileAsync(
        string draftId,
        string folderKey,
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

        var file = await GetFileAsync(draftId, folderKey, fileName);
        if (file == null)
            return;

        var storage = file.IsTemporary ? temporaryUploadStore : fileStorage;
        var storageFolderKey = file.IsTemporary ? temporaryUploadStore.GetDraftFolderKey(draftId) : folderKey;
        await storage.DeleteAsync(storageFolderKey, fileName);
    }

    public async Task DeleteAllAsync(string draftId, string folderKey, bool autoSave)
    {
        await temporaryUploadStore.DeleteFolderAsync(temporaryUploadStore.GetDraftFolderKey(draftId));

        if (autoSave && !string.IsNullOrEmpty(folderKey))
            await fileStorage.DeleteFolderAsync(folderKey);
    }

    public async Task<int> CountFilesAsync(string draftId, string folderKey)
    {
        return (await GetFilesAsync(draftId, folderKey)).Count(x => !x.Deleted);
    }

    public async Task PromoteTemporaryFilesAsync(string draftId, string folderKey, bool deleteExistingFiles = false)
    {
        if (string.IsNullOrEmpty(folderKey))
            throw new ArgumentNullException(nameof(folderKey));

        await temporaryUploadStore.PromoteAsync(draftId, fileStorage, folderKey, deleteExistingFiles);
    }

    public Task<Stream> OpenReadAsync(FileStorageReference reference)
    {
        var storage = reference.IsTemporary ? temporaryUploadStore : fileStorage;
        return storage.OpenReadAsync(reference.FolderKey, reference.FileName);
    }

    public async Task SaveFormTemporaryFilesAsync(FormElement formElement, Dictionary<string, object> values)
    {
        var uploadFields = formElement.Fields.FindAll(x => x.Component == FormComponent.File);
        if (uploadFields.Count == 0)
            return;

        foreach (var field in uploadFields)
        {
            var draftId = GetDraftId($"{field.Name}-upload-view-files");
            var folderKey = fileStorage.GetFolderKey(formElement, field, values);

            await PromoteTemporaryFilesAsync(draftId, folderKey, deleteExistingFiles: !field.DataFile.MultipleFile);
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
            var folderKey = fileStorage.GetFolderKey(formElement, field, primaryKeys);
            await DeleteAllAsync(draftId, folderKey, autoSave: true);
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

    private static async Task<IEnumerable<FormFileInfo>> GetStorageFilesAsync(
        IFileStorage storage,
        string folderKey,
        bool temporary)
    {
        if (string.IsNullOrEmpty(folderKey))
            return [];

        return (await storage.ListAsync(folderKey))
            .Select(file => new FormFileInfo
            {
                IsTemporary = temporary,
                Content = new FormFileContent
                {
                    FileName = file.FileName,
                    Length = file.Length,
                    LastWriteTime = file.LastWriteTime
                }
            });
    }
}