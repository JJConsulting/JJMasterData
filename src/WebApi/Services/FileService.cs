using JJMasterData.Commons.Data.Entity;
using JJMasterData.Commons.Data.Entity.Abstractions;
using JJMasterData.Commons.Localization;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataDictionary.Repository.Abstractions;
using JJMasterData.Core.DataManager;

namespace JJMasterData.WebApi.Services;

public class FileService
{
    private readonly IDataDictionaryRepository _dictionaryRepository;
    private readonly IEntityRepository _entityRepository;

    public FileService(IDataDictionaryRepository dictionaryRepository, IEntityRepository entityRepository)
    {
        _dictionaryRepository = dictionaryRepository;
        _entityRepository = entityRepository;
    }

    public async Task<FileStream> GetDictionaryFileAsync(string elementName, string pkValues, string fieldName, string fileName)
    {
        var formElement =await _dictionaryRepository.GetMetadataAsync(elementName);
        if (!formElement.ApiOptions.EnableGetDetail)
            throw new UnauthorizedAccessException();

        var field = formElement.Fields.First(f => f.Name == fieldName);

        var builder = new FormFilePathBuilder(formElement);

        var path = builder.GetFolderPath(field, DataHelper.GetPkValues(formElement, pkValues, ','));

        string? file = Directory.GetFiles(path).FirstOrDefault(f => f.EndsWith(fileName));

        if (file == null)
            throw new KeyNotFoundException("File not found");

        var fileStream = new FileStream(Path.Combine(path, file), FileMode.Open, FileAccess.Read, FileShare.Read);
        
        return fileStream;
    }
    
    public async Task SetDictionaryFileAsync(string elementName, string fieldName, string pkValues, IFormFile file)
    {
        var formElement = await _dictionaryRepository.GetMetadataAsync(elementName);
        
        if (!formElement.ApiOptions.EnableAdd)
            throw new UnauthorizedAccessException();
        
        var field = formElement.Fields.First(f => f.Name == fieldName);

        await SetPhysicalFileAsync(formElement, field, pkValues, file);

        await SetEntityFileAsync(formElement, field, pkValues, file.FileName);
    }
    
    private async Task SetEntityFileAsync(FormElement formElement, FormElementField field, string pkValues, string fileName)
    {
        var primaryKeys = DataHelper.GetPkValues(formElement, pkValues, ',');
        var values = await _entityRepository.GetFieldsAsync(formElement, primaryKeys);

        if (field.DataFile!.MultipleFile)
        {
            var currentFiles = new List<string>();

            if (values[field.Name] != null && values[field.Name] is not DBNull)
            {
                currentFiles = values[field.Name]!.ToString()!.Split(",").ToList();
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

        await _entityRepository.SetValuesAsync(formElement, values);
    }
    
    private static async Task SetPhysicalFileAsync(FormElement formElement, FormElementField field, string pkValues, IFormFile file)
    {
        var builder = new FormFilePathBuilder(formElement);

        var hashValues = DataHelper.GetPkValues(formElement, pkValues, ',');
        
        var path = builder.GetFolderPath(field, hashValues);
        
        if (!Directory.Exists(path))
            Directory.CreateDirectory(path);

        if (field.DataFile!.MultipleFile)
        {
            foreach (var fileInfo in new DirectoryInfo(path).EnumerateFiles())
            {
                if (fileInfo.Name == file.FileName)
                {
                    fileInfo.Delete();
                }
            }
        }

        await using var fileStream =
            new FileStream(Path.Combine(path, file.FileName), FileMode.OpenOrCreate, FileAccess.ReadWrite);

        await file.CopyToAsync(fileStream);
    }
    
    public async Task DeleteFileAsync(string elementName, string fieldName, string pkValues, string fileName)
    {
        var formElement = await _dictionaryRepository.GetMetadataAsync(elementName);
        
        if (!formElement.ApiOptions.EnableDel)
            throw new UnauthorizedAccessException();
        
        var field = formElement.Fields.First(f => f.Name == fieldName);
        
        DeletePhysicalFile(formElement, field, pkValues, fileName);
        DeleteEntityFile(formElement, field, pkValues, fileName);
    }
    
    private void DeletePhysicalFile(FormElement formElement, FormElementField field, string pkValues, string fileName)
    {
        var builder = new FormFilePathBuilder(formElement);

        var path = builder.GetFolderPath(field, DataHelper.GetPkValues(formElement, pkValues, ','));

        var filePath = Path.Combine(path, fileName);
        
        if (File.Exists(filePath))
            File.Delete(filePath);
        else
            throw new KeyNotFoundException("File not found");
    }
    
    private async Task DeleteEntityFile(Element element, FormElementField field, string pkValues, string fileName)
    {
        var primaryKeys = DataHelper.GetPkValues(element, pkValues, ',');

        var values = await _entityRepository.GetFieldsAsync(element, primaryKeys);

        if (field.DataFile!.MultipleFile)
        {
            var currentFiles = values[field.Name]!.ToString()!.Split(",").ToList();

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


        await _entityRepository.SetValuesAsync(element, values);
    }
    
    public async Task RenameFileAsync(string elementName, string fieldName, string pkValues, string oldName, string newName)
    {
        var formElement = await _dictionaryRepository.GetMetadataAsync(elementName);
        
        if (!formElement.ApiOptions.EnableUpdatePart)
            throw new UnauthorizedAccessException();

        var field = formElement.Fields.First(f => f.Name == fieldName);
        
        RenamePhysicalFile(formElement, field, pkValues, oldName, newName);
        RenameEntityFile(formElement,field, pkValues, oldName, newName);
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
    
    private async Task RenameEntityFile(FormElement formElement, FormElementField field, string pkValues, string oldName,
        string newName)
    {
        var primaryKeys = DataHelper.GetPkValues(formElement, pkValues, ',');

        var values = await _entityRepository.GetFieldsAsync(formElement, primaryKeys);

        if (field.DataFile!.MultipleFile)
        {
            var currentFiles = values[field.Name]!.ToString()!.Split(",").ToList();

            currentFiles.Remove(oldName);
            currentFiles.Add(newName);
            
            values[field.Name] = string.Join(",", currentFiles);
        }
        else
        {
            values[field.Name] = newName;
        }

        await _entityRepository.SetValuesAsync(formElement, values);
    }
}