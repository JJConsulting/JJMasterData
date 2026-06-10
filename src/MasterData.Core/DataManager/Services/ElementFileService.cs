#if NET

#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using JJMasterData.Commons.Data.Entity.Models;
using JJMasterData.Commons.Data.Entity.Repository.Abstractions;
using JJMasterData.Commons.Storage;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.DataDictionary.Repository.Abstractions;

namespace JJMasterData.Core.DataManager.Services;

public class ElementFileService(
    IDataDictionaryRepository dictionaryRepository,
    IEntityRepository entityRepository,
    IFileStorage fileStorage)
{
    public async Task<Stream?> GetElementFileAsync(string elementName, string pkValues, string fieldName, string? fileName)
    {
        var formElement = await dictionaryRepository.GetFormElementAsync(elementName);

        fileName = Path.GetFileName(fileName);

        var field = formElement.Fields.First(f => f.Name == fieldName);
        var folderPath = FileStoragePath.GetFolderPath(formElement, field, DataHelper.GetPkValues(formElement, pkValues, ','));

        if (string.IsNullOrEmpty(fileName))
            fileName = (await fileStorage.ListAsync(folderPath)).FirstOrDefault()?.FileName;

        if (string.IsNullOrEmpty(fileName))
            return null;

        var fullPath = FileStoragePath.Combine(folderPath, fileName);
        return await fileStorage.OpenReadAsync(fullPath);
    }

    
    public async Task SetElementFileAsync(
        string elementName,
        string fieldName,
        string pkValues,
        IFormFile file)
    {
        var formElement = await dictionaryRepository.GetFormElementAsync(elementName);
        
        if (!formElement.ApiOptions.EnableAdd)
            throw new UnauthorizedAccessException();
        
        var field = formElement.Fields.First(f => f.Name == fieldName);

        await SetPhysicalFileAsync(formElement, field, pkValues, file);

        var fileName = Path.GetFileName(file.FileName);
        
        await SetEntityFileAsync(formElement, field, pkValues, fileName);
    }
    
    private async Task SetEntityFileAsync(FormElement formElement, FormElementField field, string pkValues, string fileName)
    {
        var primaryKeys = DataHelper.GetPkValues(formElement, pkValues, ',');
        var values = await entityRepository.GetFieldsAsync(formElement, primaryKeys);
        fileName = Path.GetFileName(fileName);
        
        if (values == null)
            throw new KeyNotFoundException();
        
        if (field.DataFile!.MultipleFile)
        {
            var currentFiles = new List<string>();
            var dbValue = values[field.Name];
            if (dbValue != null && dbValue is not DBNull)
            {
                currentFiles = ((string)dbValue).Split(',').ToList();
            }
            
            if (!currentFiles.Contains(fileName))
            {
                currentFiles.Add(fileName);
                values[field.Name] = string.Join(",", currentFiles).TrimStart(',');
            }
        }
        else
        {
            values[field.Name] = fileName.TrimStart(',');
        }

        await entityRepository.SetValuesAsync(formElement, values);
    }
    
    private async Task SetPhysicalFileAsync(
        FormElement formElement,
        FormElementField field,
        string pkValues,
        IFormFile file)
    {
        var hashValues = DataHelper.GetPkValues(formElement, pkValues, ',');
        var folderPath = FileStoragePath.GetFolderPath(formElement, field, hashValues);

        var fileName = Path.GetFileName(file.FileName);
        
        await using var uploadStream = file.OpenReadStream();
        var fullPath = FileStoragePath.Combine(folderPath, fileName);
        await fileStorage.SaveAsync(fullPath, uploadStream, true);
    }
    
    public async Task DeleteFileAsync(string elementName, string fieldName, string pkValues, string fileName)
    {
        var formElement = await dictionaryRepository.GetFormElementAsync(elementName);
        
        if (!formElement.ApiOptions.EnableDel)
            throw new UnauthorizedAccessException();
        
        var field = formElement.Fields.First(f => f.Name == fieldName);
        
        fileName = Path.GetFileName(fileName);
        
        await DeletePhysicalFileAsync(formElement, field, pkValues, fileName);
        await DeleteEntityFileAsync(formElement, field, pkValues, fileName);
    }
    
    private async Task DeletePhysicalFileAsync(FormElement formElement, FormElementField field, string pkValues, string fileName)
    {
        fileName = Path.GetFileName(fileName);
        var folderPath = FileStoragePath.GetFolderPath(formElement, field, DataHelper.GetPkValues(formElement, pkValues, ','));

        var fullPath = FileStoragePath.Combine(folderPath, fileName);
        await fileStorage.DeleteAsync(fullPath);
    }
    
    private async Task DeleteEntityFileAsync(Element element, FormElementField field, string pkValues, string fileName)
    {
        var primaryKeys = DataHelper.GetPkValues(element, pkValues, ',');
        var values = await entityRepository.GetFieldsAsync(element, primaryKeys);
        if (values == null)
            throw new KeyNotFoundException();
        
        fileName = Path.GetFileName(fileName);
        
        if (field.DataFile!.MultipleFile)
        {
            var currentFiles = values[field.Name]!.ToString()!.Split(',').ToList();

            if (currentFiles.Contains(fileName))
            {
                currentFiles.Remove(fileName);
                values[field.Name] = string.Join(",", currentFiles);
            }
        }
        else
        {
            values[field.Name] = null;
        }


        await entityRepository.SetValuesAsync(element, values);
    }
    
    public async Task RenameFileAsync(string elementName, string fieldName, string pkValues, string oldName, string newName)
    {
        var formElement = await dictionaryRepository.GetFormElementAsync(elementName);
        
        if (!formElement.ApiOptions.EnableUpdatePart)
            throw new UnauthorizedAccessException();

        var field = formElement.Fields.First(f => f.Name == fieldName);
        
        oldName = Path.GetFileName(oldName);
        newName = Path.GetFileName(newName);
        
        await RenamePhysicalFileAsync(formElement, field, pkValues, oldName, newName);
        await RenameEntityFileAsync(formElement,field, pkValues, oldName, newName);
    }
    
    private async Task RenamePhysicalFileAsync(FormElement formElement, FormElementField field, string pkValues, string oldName, string newName)
    {
        var folderPath = FileStoragePath.GetFolderPath(formElement, field, DataHelper.GetPkValues(formElement, pkValues, ','));
        var oldFullPath = FileStoragePath.Combine(folderPath, oldName);
        var newFullPath = FileStoragePath.Combine(folderPath, newName);
        await fileStorage.MoveAsync(oldFullPath, newFullPath);
    }
    
    private async Task RenameEntityFileAsync(FormElement formElement, FormElementField field, string pkValues, string oldName,
        string newName)
    {
        var primaryKeys = DataHelper.GetPkValues(formElement, pkValues, ',');
        var values = await entityRepository.GetFieldsAsync(formElement, primaryKeys);

        if (values == null)
            throw new KeyNotFoundException();
        
        if (field.DataFile!.MultipleFile)
        {
            var currentFiles = values[field.Name]!.ToString()!.Split(',').ToList();

            currentFiles.Remove(oldName);
            currentFiles.Add(newName);
            
            values[field.Name] = string.Join(",", currentFiles);
        }
        else
        {
            values[field.Name] = newName;
        }

        await entityRepository.SetValuesAsync(formElement, values);
    }
}
#endif
