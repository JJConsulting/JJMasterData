using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using JJConsulting.MasterData.Storage.Abstractions;
using JJMasterData.Commons.Exceptions;
using JJMasterData.Commons.Storage;
using JJMasterData.Commons.Tasks;
using JJMasterData.Commons.Util;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.DataManager;
using JJMasterData.Core.DataManager.Services;
using JJMasterData.Web.Events.Args;
using Microsoft.Extensions.Localization;

namespace JJMasterData.Web.Components;

public class UploadViewManager(
    IHttpContextAccessor currentContext,
    ElementFileService elementFileService,
    IFileStorage fileStorage,
    IStringLocalizer<MasterDataResources> stringLocalizer)
{
    public event AsyncEventHandler<FormUploadFileEventArgs>? OnBeforeCreateFileAsync;
    public event AsyncEventHandler<FormDeleteFileEventArgs>? OnBeforeDeleteFileAsync;
    public event AsyncEventHandler<FormRenameFileEventArgs>? OnBeforeRenameFileAsync;
    
    
    public async Task<List<FileStorageItem>> GetFilesAsync(string tempPath, string? folderPath)
    {
        var files = new List<FileStorageItem>();
        files.AddRange(await fileStorage.ListAsync(tempPath));

        if (string.IsNullOrEmpty(folderPath))
            return files;

        var savedFiles = await fileStorage.ListAsync(folderPath);
        foreach (var file in savedFiles)
        {
            if (files.Any(x => x.FileName.Equals(file.FileName)))
                continue;

            files.Add(file);
        }

        return files;
    }

    public async Task RenameFileAsync(
        string tempPath,
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

        if ((await GetFilesAsync(tempPath, folderPath)).Exists(x => x.FileName.Equals(newName)))
            throw new JJMasterDataException(stringLocalizer["A file with the name {0} already exists", newName]);

        if (OnBeforeRenameFileAsync != null)
        {
            var args = new FormRenameFileEventArgs(currentName, newName);
            await OnBeforeRenameFileAsync(this, args);

            if (!string.IsNullOrEmpty(args.ErrorMessage))
                throw new JJMasterDataException(args.ErrorMessage);
        }

        var file = await GetFileAsync(tempPath, folderPath, currentName);
        if (file == null)
            throw new JJMasterDataException(stringLocalizer["file {0} not found!", currentName]);
        
        await elementFileService.RenameFileAsync(file.FolderPath, currentName, newName);
    }

    public async Task<FileStorageItem?> GetFileAsync(string tempPath, string folderPath, string fileName)
    {
        fileName = Path.GetFileName(fileName);
        var files = await GetFilesAsync(tempPath, folderPath);
        return files.Find(x => fileName.Equals(x.FileName));
    }

    public async Task CreateFileAsync(
        string tempPath,
        string folderPath,
        bool autoSave,
        IFormFile file,
        bool isMultipleFiles)
    {
        if (file == null)
            throw new ArgumentNullException(nameof(file));

        if (OnBeforeCreateFileAsync != null)
        {
            var args = new FormUploadFileEventArgs(file);
            await OnBeforeCreateFileAsync(this, args);
            string errorMessage = args.ErrorMessage;

            if (!string.IsNullOrEmpty(errorMessage))
                throw new JJMasterDataException(errorMessage);

            file = args.File;
        }
        
        if (!isMultipleFiles)
            await DeleteAllAsync(tempPath, folderPath, autoSave);

        var storageFolderPath = autoSave
            ? folderPath
            : tempPath;

        await using var stream = file.OpenReadStream();
        await elementFileService.SaveFileAsync(storageFolderPath, file.FileName, stream);
    }

    public async Task DeleteFileAsync(
        string tempPath,
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

        var files = await GetFilesAsync(tempPath, folderPath);
        foreach (var file in files)
        {
            if (file.FileName.Equals(fileName))
                await elementFileService.DeleteFileAsync(file.FolderPath, file.FileName);
        }
    }

    public async Task DeleteAllAsync(string tempPath, string? folderPath, bool autoSave)
    {
        await fileStorage.DeleteFolderAsync(tempPath);

        if (autoSave && !string.IsNullOrEmpty(folderPath))
            await fileStorage.DeleteFolderAsync(folderPath);
    }

    public async Task ClearTemporaryFilesAsync(FormElement formElement)
    {
        foreach (var field in GetFileFields(formElement))
        {
            var tempPath = GetTempPath(field);
            if (string.IsNullOrEmpty(tempPath))
                continue;

            await DeleteAllAsync(tempPath, null, false);
        }
    }
    
    public async Task PromoteDraftFilesAsync(string tempPath, string folderPath, HashSet<string> deletedFiles)
    {
        foreach (var fileName in deletedFiles)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                continue;

            await elementFileService.DeleteFileAsync(folderPath, Path.GetFileName(fileName));
        }

        var files = await fileStorage.ListAsync(tempPath);
        foreach (var file in files)
        {
            var destination = FileStoragePath.Combine(folderPath, file.FileName);
            await fileStorage.MoveAsync(file.FullPath, destination);
        }

        await fileStorage.DeleteFolderAsync(tempPath);
    }

    public async Task PromoteDraftFilesAsync(FormElement formElement, Dictionary<string, object?> values)
    {
        if (!DataHelper.ContainsPkValues(formElement, values!))
            return;

        foreach (var field in GetFileFields(formElement))
        {
            var tempPath = GetTempPath(field);
            if (string.IsNullOrEmpty(tempPath))
                continue;

            var folderPath = FileStoragePath.GetFolderPath(formElement, field, values);
            await PromoteDraftFilesAsync(tempPath, folderPath, GetDeletedFiles(field));
        }
    }

    private string? GetTempPath(FormElementField field)
    {
        var draftIdStr = currentContext.HttpContext?.Request.GetValue($"{GetUploadViewName(field)}-draft-id");
        if (!Guid.TryParse(draftIdStr, out var draftId))
            return null;

        return JJUploadView.TempPathFolder + draftId.ToString("N") + "/";
    }

    private HashSet<string> GetDeletedFiles(FormElementField field)
    {
        var values = currentContext.HttpContext?.Request.GetValue(GetDeletedFilesInputName(field));
        if (string.IsNullOrWhiteSpace(values))
            return [];

        return values
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(Path.GetFileName)
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .Select(name => name!)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
    }

    private static string GetUploadViewName(FormElementField field) =>
        $"{field.Name}-upload-view";

    internal static string GetDeletedFilesInputName(FormElementField field) =>
        $"{GetUploadViewName(field)}-files-deleted";

    private static IEnumerable<FormElementField> GetFileFields(FormElement formElement)
    {
        foreach (var field in formElement.Fields)
        {
            if (field.Component is FormComponent.File && field.DataFile is not null)
                yield return field;
        }
    }
}
