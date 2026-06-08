using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using JJMasterData.Commons.Exceptions;
using JJMasterData.Commons.Tasks;
using JJMasterData.Commons.Util;
using JJMasterData.Core.DataManager.IO.Storage;
using JJMasterData.Core.DataManager.Models;
using JJMasterData.Core.UI.Events.Args;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace JJMasterData.Core.DataManager.IO;

public class FormFileManager(
    string draftId,
    ITemporaryUploadStore temporaryUploadStore,
    IFileStorage fileStorage,
    IStringLocalizer<MasterDataResources> stringLocalizer,
    ILogger<FormFileManager> logger)
{
    public event AsyncEventHandler<FormUploadFileEventArgs> OnBeforeCreateFileAsync;
    public event AsyncEventHandler<FormDeleteFileEventArgs> OnBeforeDeleteFileAsync;
    public event AsyncEventHandler<FormRenameFileEventArgs> OnBeforeRenameFileAsync;

    public string DraftId { get; } = string.IsNullOrWhiteSpace(draftId)
        ? temporaryUploadStore.CreateDraftId()
        : temporaryUploadStore.GetDraftFolderKey(draftId);

    public bool AutoSave { get; set; } = true;
    public string FolderKey { get; set; }

    private string DraftFolderKey => temporaryUploadStore.GetDraftFolderKey(DraftId);

    public async Task<List<FormFileInfo>> GetFilesAsync(bool preferTemporaryFiles = false)
    {
        var files = new List<FormFileInfo>();

        if (!string.IsNullOrEmpty(FolderKey))
            files.AddRange(await GetStorageFilesAsync(fileStorage, FolderKey, false));

        files.AddRange(await GetStorageFilesAsync(temporaryUploadStore, DraftFolderKey, true));

        var mergedFiles = files
            .GroupBy(file => file.Content.FileName)
            .Select(group => group.OrderByDescending(file => file.IsTemporary).First())
            .ToList();

        return preferTemporaryFiles && mergedFiles.Exists(file => file.IsTemporary)
            ? mergedFiles.Where(file => file.IsTemporary).ToList()
            : mergedFiles;
    }

    public async Task RenameFileAsync(string currentName, string newName)
    {
        if (string.IsNullOrEmpty(currentName))
            throw new ArgumentNullException(nameof(currentName));

        if (string.IsNullOrWhiteSpace(newName))
            throw new ArgumentNullException(stringLocalizer["Required file name"]);

        currentName = Path.GetFileName(currentName);
        newName = Path.GetFileName(newName);

        if (!FileIO.GetFileNameExtension(currentName).Equals(FileIO.GetFileNameExtension(newName)))
            throw new JJMasterDataException(stringLocalizer["The file extension must remain the same"]);

        if ((await GetFilesAsync()).Exists(x => x.Content.FileName.Equals(newName)))
            throw new JJMasterDataException(stringLocalizer["A file with the name {0} already exists", newName]);

        if (OnBeforeRenameFileAsync != null)
        {
            var args = new FormRenameFileEventArgs(currentName, newName);
            await OnBeforeRenameFileAsync(this, args);

            if (!string.IsNullOrEmpty(args.ErrorMessage))
                throw new JJMasterDataException(args.ErrorMessage);
        }

        var file = await GetFileAsync(currentName) ?? throw new JJMasterDataException(stringLocalizer["file {0} not found!", currentName]);
        var storage = file.IsTemporary || string.IsNullOrEmpty(FolderKey)
            ? temporaryUploadStore
            : fileStorage;
        var folderKey = file.IsTemporary || string.IsNullOrEmpty(FolderKey)
            ? DraftFolderKey
            : FolderKey;

        await storage.RenameAsync(folderKey, currentName, newName);
    }

    public async Task<FormFileInfo> GetFileAsync(string fileName)
    {
        fileName = Path.GetFileName(fileName);
        return (await GetFilesAsync()).Find(x => fileName.Equals(x.Content.FileName) || fileName.Equals(x.OldName));
    }

    public async Task<FileStorageReference> GetFileReferenceAsync(string fileName)
    {
        var file = await GetFileAsync(fileName);
        if (file == null)
            throw new JJMasterDataException(stringLocalizer["file {0} not found!", fileName]);

        return FileStorageReference.Create(
            file.IsTemporary ? DraftFolderKey : FolderKey,
            file.Content.FileName,
            file.IsTemporary);
    }

    public async Task CreateFileAsync(FormFileContent fileContent, bool replaceIfExists)
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
                logger.LogError(exception, "Error OnBeforeCreateFile");
                throw exception;
            }
        }

        if (replaceIfExists && await CountFilesAsync() > 0)
            await DeleteAllAsync();

        var storage = AutoSave && !string.IsNullOrEmpty(FolderKey)
            ? fileStorage
            : temporaryUploadStore;
        var folderKey = AutoSave && !string.IsNullOrEmpty(FolderKey)
            ? FolderKey
            : DraftFolderKey;

        await storage.SaveAsync(folderKey, fileContent.FileName, fileContent.Stream, true);
    }

    public async Task DeleteFileAsync(string fileName)
    {
        fileName = Path.GetFileName(fileName);

        if (OnBeforeDeleteFileAsync != null)
        {
            var args = new FormDeleteFileEventArgs(fileName);
            await OnBeforeDeleteFileAsync(this, args);

            if (!string.IsNullOrEmpty(args.ErrorMessage))
            {
                var exception = new JJMasterDataException(args.ErrorMessage);
                logger.LogError(exception, "Error OnBeforeDeleteFile");
                throw exception;
            }
        }

        var file = await GetFileAsync(fileName);
        if (file == null)
            return;

        var storage = file.IsTemporary ? temporaryUploadStore : fileStorage;
        var folderKey = file.IsTemporary ? DraftFolderKey : FolderKey;
        await storage.DeleteAsync(folderKey, fileName);
    }

    public async Task DeleteAllAsync()
    {
        await temporaryUploadStore.DeleteFolderAsync(DraftFolderKey);

        if (AutoSave && !string.IsNullOrEmpty(FolderKey))
            await fileStorage.DeleteFolderAsync(FolderKey);
    }

    public async Task<int> CountFilesAsync()
    {
        return (await GetFilesAsync()).Count(x => !x.Deleted);
    }

    public async Task PromoteTemporaryFilesAsync(string folderKey, bool deleteExistingFiles = false)
    {
        if (string.IsNullOrEmpty(folderKey))
            throw new ArgumentNullException(nameof(folderKey));

        FolderKey = folderKey;
        await temporaryUploadStore.PromoteAsync(DraftId, fileStorage, folderKey, deleteExistingFiles);
    }

    public Task<Stream> OpenReadAsync(FileStorageReference reference)
    {
        var storage = reference.IsTemporary ? temporaryUploadStore : fileStorage;
        return storage.OpenReadAsync(reference.FolderKey, reference.FileName);
    }

    private static async Task<IEnumerable<FormFileInfo>> GetStorageFilesAsync(IFileStorage storage, string folderKey, bool temporary)
    {
        if (string.IsNullOrEmpty(folderKey))
            return [];

        return (await storage.ListAsync(folderKey))
            .Select(file => new FormFileInfo
            {
                IsTemporary = temporary,
                Content =
                {
                    FileName = file.FileName,
                    Length = file.Length,
                    LastWriteTime = file.LastWriteTime
                }
            });
    }
}
