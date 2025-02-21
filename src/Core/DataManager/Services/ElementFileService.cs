#if NET

#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using JJMasterData.Commons.Data.Entity.Models;
using JJMasterData.Commons.Data.Entity.Repository.Abstractions;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.DataDictionary.Repository.Abstractions;
using JJMasterData.Core.DataManager.IO;
using Microsoft.AspNetCore.Http;

namespace JJMasterData.Core.DataManager.Services;

public class ElementFileService(IDataDictionaryRepository dictionaryRepository, IEntityRepository entityRepository)
{
    public async Task<FileStream?> GetElementFileAsync(string elementName, string pkValues, string fieldName, string fileName)
    {
        var formElement = await dictionaryRepository.GetFormElementAsync(elementName);

        fileName = Path.GetFileName(fileName);
        
        var field = formElement.Fields.First(f => f.Name == fieldName);

        var builder = new FormFilePathBuilder(formElement);

        var path = builder.GetFolderPath(field, DataHelper.GetPkValues(formElement, pkValues, ','));

        var file = Directory.GetFiles(path).FirstOrDefault(f => f.EndsWith(fileName));

        if (file == null)
            return null;

        var fileStream = new FileStream(Path.Combine(path, file), FileMode.Open, FileAccess.Read, FileShare.Read);
        
        return fileStream;
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
    
    private static async Task SetPhysicalFileAsync(
        FormElement formElement,
        FormElementField field,
        string pkValues,
        IFormFile file)
    {
        var builder = new FormFilePathBuilder(formElement);

        var hashValues = DataHelper.GetPkValues(formElement, pkValues, ',');
        
        var path = builder.GetFolderPath(field, hashValues);
        
        if (!Directory.Exists(path))
            Directory.CreateDirectory(path);

        var fileName = Path.GetFileName(file.FileName);
        
        if (field.DataFile!.MultipleFile)
        {
            foreach (var fileInfo in new DirectoryInfo(path).EnumerateFiles())
            {
                if (fileInfo.Name == fileName)
                {
                    fileInfo.Delete();
                }
            }
        }
        
        await using var fileStream =
            new FileStream(Path.Combine(path, fileName), FileMode.OpenOrCreate, FileAccess.ReadWrite);

        await file.CopyToAsync(fileStream);
    }
    
    public async Task DeleteFileAsync(string elementName, string fieldName, string pkValues, string fileName)
    {
        var formElement = await dictionaryRepository.GetFormElementAsync(elementName);
        
        if (!formElement.ApiOptions.EnableDel)
            throw new UnauthorizedAccessException();
        
        var field = formElement.Fields.First(f => f.Name == fieldName);
        
        fileName = Path.GetFileName(fileName);
        
        DeletePhysicalFile(formElement, field, pkValues, fileName);
        await DeleteEntityFileAsync(formElement, field, pkValues, fileName);
    }
    
    private static void DeletePhysicalFile(FormElement formElement, FormElementField field, string pkValues, string fileName)
    {
        var builder = new FormFilePathBuilder(formElement);

        var path = builder.GetFolderPath(field, DataHelper.GetPkValues(formElement, pkValues, ','));

        fileName = Path.GetFileName(fileName);
        
        var filePath = Path.Combine(path, fileName);
        
        if (File.Exists(filePath))
            File.Delete(filePath);
        else
            throw new KeyNotFoundException("File not found");
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
        
        RenamePhysicalFile(formElement, field, pkValues, oldName, newName);
        await RenameEntityFileAsync(formElement,field, pkValues, oldName, newName);
    }
    
    private static void RenamePhysicalFile(FormElement formElement, FormElementField field, string pkValues, string oldName, string newName)
    {
        var builder = new FormFilePathBuilder(formElement);

        var path = builder.GetFolderPath(field, DataHelper.GetPkValues(formElement, pkValues, ','));
        
        var oldFilePath = Path.Combine(path, oldName);
        var newFilePath = Path.Combine(path, newName);

        if (File.Exists(oldFilePath))
            File.Move(oldFilePath, newFilePath);
        else
            throw new KeyNotFoundException("File not found");
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