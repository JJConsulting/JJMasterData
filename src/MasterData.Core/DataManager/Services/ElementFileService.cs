using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using JJConsulting.MasterData.Storage.Abstractions;
using JJMasterData.Commons.Data.Entity.Repository.Abstractions;
using JJMasterData.Commons.Storage;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.DataDictionary.Repository.Abstractions;

namespace JJMasterData.Core.DataManager.Services;

public class ElementFileService(
    IDataDictionaryRepository dictionaryRepository,
    IEntityRepository entityRepository,
    IFileStorage fileStorage,
    FileValidationService fileValidationService,
    IMasterDataUser? masterDataUser)
{
    public ElementFileService(
        IDataDictionaryRepository dictionaryRepository,
        IEntityRepository entityRepository,
        IFileStorage fileStorage,
        FileValidationService fileValidationService)
        : this(dictionaryRepository, entityRepository, fileStorage, fileValidationService, null)
    {
    }

    public async Task SaveFileAsync(string folderPath, IFormFile file, bool overwrite = true, string? allowedTypes = null)
    {
        var fileName = FileStoragePath.GetFileName(file.FileName);
        fileValidationService.Validate(file, allowedTypes);

        var fullPath = FileStoragePath.Combine(folderPath, fileName);
        await using var uploadStream = file.OpenReadStream();
        await fileStorage.SaveAsync(fullPath, uploadStream, overwrite);
    }

    public async Task DeleteFileAsync(string folderPath, string fileName)
    {
        fileName = Path.GetFileName(fileName);
        var fullPath = FileStoragePath.Combine(folderPath, fileName);
        await fileStorage.DeleteAsync(fullPath);
    }

    public async Task RenameFileAsync(string folderPath, string oldName, string newName)
    {
        oldName = Path.GetFileName(oldName);
        newName = Path.GetFileName(newName);
        fileValidationService.ValidateFileName(newName);

        var oldFullPath = FileStoragePath.Combine(folderPath, oldName);
        var newFullPath = FileStoragePath.Combine(folderPath, newName);
        await fileStorage.MoveAsync(oldFullPath, newFullPath);
    }

    public Task<Stream?> GetElementFileAsync(string elementName, string pkValues, string fieldName, string? fileName)
    {
        return GetElementFileAsync(elementName, pkValues, fieldName, fileName, requireDetailPermission: false);
    }

    public async Task<Stream?> GetElementFileAsync(
        string elementName, string pkValues, string fieldName, string? fileName, bool requireDetailPermission)
    {
        var formElement = await dictionaryRepository.GetFormElementAsync(elementName);
        if (requireDetailPermission && !formElement.ApiOptions.EnableGetDetail)
            return null;

        var field = formElement.Fields.FirstOrDefault(field => field.Name == fieldName && field.DataFile is not null);
        if (field is null)
            return null;

        var primaryKeys = DataHelper.GetPkValues(formElement, pkValues, ',');
        var values = await GetAccessibleRecordAsync(formElement, primaryKeys);
        if (values is null)
            return null;

        var files = GetReferencedNames(field, values);
        var requestedName = string.IsNullOrEmpty(fileName)
            ? files.FirstOrDefault()
            : FileStoragePath.GetFileName(fileName);
        if (requestedName is null || !files.Contains(requestedName, StringComparer.Ordinal))
            return null;

        var folderPath = GetRecordFolderPath(formElement, field, values);
        return await fileStorage.OpenReadAsync(FileStoragePath.Combine(folderPath, requestedName));
    }

    public async Task SetElementFileAsync(string elementName, string fieldName, string pkValues, IFormFile file)
    {
        var formElement = await dictionaryRepository.GetFormElementAsync(elementName);
        if (!formElement.ApiOptions.EnableAdd)
            throw new UnauthorizedAccessException();

        var field = GetFileField(formElement, fieldName);
        var primaryKeys = DataHelper.GetPkValues(formElement, pkValues, ',');
        var values = await GetAccessibleRecordAsync(formElement, primaryKeys) ?? throw new KeyNotFoundException();

        var fileName = FileStoragePath.GetFileName(file.FileName);
        var folderPath = GetRecordFolderPath(formElement, field, values);
        await SaveFileAsync(folderPath, file, true, field.DataFile!.AllowedTypes);

        if (field.DataFile.MultipleFile)
        {
            var files = GetReferencedNames(field, values);
            if (!files.Contains(fileName, StringComparer.Ordinal))
                files.Add(fileName);

            values[field.Name] = string.Join(',', files);
        }
        else
        {
            values[field.Name] = fileName;
        }

        await entityRepository.UpdateAsync(formElement, values);
    }

    public async Task DeleteFileAsync(string elementName, string fieldName, string pkValues, string fileName)
    {
        var formElement = await dictionaryRepository.GetFormElementAsync(elementName);
        if (!formElement.ApiOptions.EnableDel)
            throw new UnauthorizedAccessException();

        var field = GetFileField(formElement, fieldName);
        var primaryKeys = DataHelper.GetPkValues(formElement, pkValues, ',');
        var values = await GetAccessibleRecordAsync(formElement, primaryKeys) ?? throw new KeyNotFoundException();
        var files = GetReferencedNames(field, values);

        fileName = FileStoragePath.GetFileName(fileName);
        if (!files.Remove(fileName))
            throw new KeyNotFoundException();

        var folderPath = GetRecordFolderPath(formElement, field, values);
        await DeleteFileAsync(folderPath, fileName);
        values[field.Name] = field.DataFile!.MultipleFile ? string.Join(',', files) : null;
        await entityRepository.UpdateAsync(formElement, values);
    }

    public async Task RenameFileAsync(string elementName, string fieldName, string pkValues, string oldName, string newName)
    {
        var formElement = await dictionaryRepository.GetFormElementAsync(elementName);
        if (!formElement.ApiOptions.EnableUpdatePart)
            throw new UnauthorizedAccessException();

        var field = GetFileField(formElement, fieldName);
        var primaryKeys = DataHelper.GetPkValues(formElement, pkValues, ',');
        var values = await GetAccessibleRecordAsync(formElement, primaryKeys) ?? throw new KeyNotFoundException();
        var files = GetReferencedNames(field, values);

        oldName = FileStoragePath.GetFileName(oldName);
        newName = FileStoragePath.GetFileName(newName);
        if (!files.Remove(oldName))
            throw new KeyNotFoundException();

        fileValidationService.ValidateAllowedExtensions(newName, field.DataFile!.AllowedTypes);
        var folderPath = GetRecordFolderPath(formElement, field, values);
        await RenameFileAsync(folderPath, oldName, newName);
        files.Add(newName);
        values[field.Name] = string.Join(',', files);
        await entityRepository.UpdateAsync(formElement, values);
    }

    private async Task<Dictionary<string, object?>?> GetAccessibleRecordAsync(
        FormElement formElement, Dictionary<string, object> primaryKeys)
    {
        var values = await entityRepository.GetFieldsAsync(formElement, primaryKeys);
        if (values is null || values.Count == 0)
            return null;

        var ownershipField = formElement.ApiOptions.ApplyUserIdOn;
        if (string.IsNullOrEmpty(ownershipField))
            return values;

        var userId = masterDataUser?.Id;
        if (string.IsNullOrWhiteSpace(userId) ||
            !formElement.Fields.Contains(ownershipField) ||
            !values.TryGetValue(formElement.Fields[ownershipField].Name, out var owner) ||
            owner is null or DBNull ||
            !string.Equals(owner.ToString(), userId, StringComparison.Ordinal))
            return null;

        return values;
    }

    private static FormElementField GetFileField(FormElement formElement, string fieldName)
    {
        return formElement.Fields.FirstOrDefault(field => field.Name == fieldName && field.DataFile is not null)
               ?? throw new KeyNotFoundException();
    }

    private static string GetRecordFolderPath(FormElement formElement, FormElementField field,
        Dictionary<string, object?> values)
    {
        return FileStoragePath.GetFolderPath(formElement, field, DataHelper.GetPkValues(formElement, values)!);
    }

    private static List<string> GetReferencedNames(FormElementField field, Dictionary<string, object?> values)
    {
        if (!values.TryGetValue(field.Name, out var storedFile) || storedFile is not string names ||
            string.IsNullOrEmpty(names))
            return [];

        return field.DataFile!.MultipleFile
            ? names.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList()
            : [names];
    }
}
