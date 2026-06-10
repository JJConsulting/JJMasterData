using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JJMasterData.Commons.Exceptions;
using JJMasterData.Commons.Storage;
using JJMasterData.Commons.Tasks;
using JJMasterData.Commons.Util;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.DataManager.Models;
using JJMasterData.Core.UI.Events.Args;
using Microsoft.Extensions.Localization;

namespace JJMasterData.Core.DataManager.Services;

public class FormFileService(
    IHttpContextAccessor httpContext,
    IFileStorage fileStorage,
    IStringLocalizer<MasterDataResources> stringLocalizer)
{
    private const string RootPath = "{app.path}/MasterDataDraftFiles/";
    
    public event AsyncEventHandler<FormUploadFileEventArgs> OnBeforeCreateFileAsync;
    public event AsyncEventHandler<FormDeleteFileEventArgs> OnBeforeDeleteFileAsync;
    public event AsyncEventHandler<FormRenameFileEventArgs> OnBeforeRenameFileAsync;

    public async Task<List<FileStorageItem>> GetFilesAsync(string draftId, string folderPath)
    {
        var files = new List<FileStorageItem>();
        var draftFolderPath = GetDraftFolderPath(draftId);

        if (!string.IsNullOrEmpty(folderPath))
            files.AddRange(await GetStorageFilesAsync(folderPath));

        files.AddRange(await GetStorageFilesAsync(draftFolderPath));

        var mergedFiles = files
            .GroupBy(file => file.FileName)
            .Select(group => group.OrderByDescending(file => IsDraftFile(file, draftFolderPath)).First())
            .ToList();

        return mergedFiles;
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

        var currentFullPath = file.FullPath;
        var newFullPath = FileStoragePath.Combine(file.FolderPath, newName);
        await fileStorage.MoveAsync(currentFullPath, newFullPath);
    }

    public async Task<FileStorageItem> GetFileAsync(string draftId, string folderPath, string fileName)
    {
        fileName = Path.GetFileName(fileName);
        return (await GetFilesAsync(draftId, folderPath)).Find(x => fileName.Equals(x.FileName));
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
                throw new JJMasterDataException(errorMessage);
        }

        if (replaceIfExists && await CountFilesAsync(draftId, folderPath) > 0)
            await DeleteAllAsync(draftId, folderPath, autoSave);

        var storageFolderPath = autoSave && !string.IsNullOrEmpty(folderPath)
            ? folderPath
            : GetDraftFolderPath(draftId);

        var fullPath = FileStoragePath.Combine(storageFolderPath, fileContent.FileName);
        await fileStorage.SaveAsync(fullPath, fileContent.Stream);
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
                throw new JJMasterDataException(args.ErrorMessage);
        }

        var file = await GetFileAsync(draftId, folderPath, fileName);
        if (file == null)
            return;

        await fileStorage.DeleteAsync(file.FullPath);
    }

    public async Task DeleteAllAsync(string draftId, string folderPath, bool autoSave)
    {
        await fileStorage.DeleteFolderAsync(GetDraftFolderPath(draftId));

        if (autoSave && !string.IsNullOrEmpty(folderPath))
            await fileStorage.DeleteFolderAsync(folderPath);
    }

    public async Task<int> CountFilesAsync(string draftId, string folderPath)
    {
        return (await GetFilesAsync(draftId, folderPath)).Count;
    }
    
    public async Task PromoteDraftFilesAsync(FormElement formElement, Dictionary<string, object> values)
    {
        var uploadFields = formElement.Fields.FindAll(x => x.Component == FormComponent.File);
        if (uploadFields.Count == 0)
            return;

        foreach (var field in uploadFields)
        {
            var draftId = GetDraftId($"{field.Name}-upload-view-files");
            var folderPath = FileStoragePath.GetFolderPath(formElement, field, values);

            await PromoteDraftFilesAsync(draftId, folderPath);
        }
    }

    public async Task PromoteDraftFilesAsync(string draftId, string folderPath)
    {
        var draftFolderPath = GetDraftFolderPath(draftId);
        var files = await fileStorage.ListAsync(draftFolderPath);
        if (files.Count > 0)
        {
            foreach (var file in files)
            {
                var destination = FileStoragePath.Combine(folderPath, file.FileName);
                await fileStorage.MoveAsync(file.FullPath, destination);
            }

            await fileStorage.DeleteFolderAsync(draftFolderPath);
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

    private static bool IsDraftFile(FileStorageItem file, string draftFolderPath)
    {
        return string.Equals(file.FolderPath, draftFolderPath, StringComparison.Ordinal);
    }

    private async Task<IEnumerable<FileStorageItem>> GetStorageFilesAsync(string folderPath)
    {
        if (string.IsNullOrEmpty(folderPath))
            return [];

        return (await fileStorage.ListAsync(folderPath))
            .Select(file => new FileStorageItem
            {
                FileName = file.FileName,
                Length = file.Length,
                LastWriteTime = file.LastWriteTime,
                FolderPath = folderPath
            });
    }

    private string CreateDraftId()
    {
        _ = CleanupExpiredAsync(TimeSpan.FromHours(12));
        
        return Guid.NewGuid().ToString("N");
    }

    public static string GetDraftFolderPath(string draftId)
    {
        draftId = NormalizeDraftId(draftId);
        if (string.IsNullOrWhiteSpace(draftId))
            throw new ArgumentNullException(nameof(draftId));

        return ResolveDraftFolderPath(draftId);
    }

    public string GetDraftId(string uploadName)
    {
        var request = httpContext.HttpContext!.Request;
        var draftId = request.GetValue($"{uploadName}-draft-id");

        return string.IsNullOrWhiteSpace(draftId) ? CreateDraftId() : draftId;
    }
    
    private async Task CleanupExpiredAsync(TimeSpan maxAge)
    {
        var threshold = DateTime.Now.Subtract(maxAge);
        
        var files = await fileStorage.ListAsync(RootPath, isRecursive: true);

        foreach (var file in files)
        {
            if (file.LastWriteTime < threshold)
                await fileStorage.DeleteAsync(file.FullPath);
        }
    }

    private static string ResolveDraftFolderPath(string folderPath)
    {
        var draftId = NormalizeDraftId(folderPath);
        return Path.Combine(RootPath, draftId);
    }

    private static string NormalizeDraftId(string draftId)
    {
        if (string.IsNullOrWhiteSpace(draftId))
            return draftId;

        draftId = draftId.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)[0];
        draftId = Path.GetFileName(draftId);

        if (!Guid.TryParseExact(draftId, "N", out _))
            throw new ArgumentException("Invalid temporary upload draft id.", nameof(draftId));

        return draftId;
    }
}
