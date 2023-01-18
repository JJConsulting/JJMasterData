using JJMasterData.Commons.Data.Entity;
using JJMasterData.Commons.Data.Entity.Abstractions;
using JJMasterData.Commons.Localization;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataDictionary.Repository;
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

    public FileStream GetDictionaryFile(string elementName, string pkValues, string fieldName, string fileName)
    {
        var metadata = _dictionaryRepository.GetMetadata(elementName);
        if (!metadata.MetadataApiOptions.EnableGetDetail)
            throw new UnauthorizedAccessException();
        
        var formElement = metadata.GetFormElement();
        
        var field = formElement.Fields.First(f => f.Name == fieldName);

        var builder = new FormFilePathBuilder(formElement);

        var path = builder.GetFolderPath(field, DataHelper.GetPkValues(formElement, pkValues, ','));

        string? file = Directory.GetFiles(path).FirstOrDefault(f => f.EndsWith(fileName));

        if (file == null)
            throw new KeyNotFoundException(Translate.Key("File not found"));

        var fileStream = new FileStream(Path.Combine(path, file), FileMode.Open, FileAccess.Read, FileShare.Read);
        
        return fileStream;
    }
    public void SetDictionaryFile(string elementName, string fieldName, string pkValues, IFormFile file)
    {
        var metadata = _dictionaryRepository.GetMetadata(elementName);
        
        if (!metadata.MetadataApiOptions.EnableAdd)
            throw new UnauthorizedAccessException();
        
        var formElement = metadata.GetFormElement();
        var field = formElement.Fields.First(f => f.Name == fieldName);

        SetPhysicalFile(formElement, field, pkValues, file);

        SetEntityFile(formElement, field, pkValues, file.FileName);
    }
    private void SetEntityFile(FormElement formElement, FormElementField field, string pkValues, string fileName)
    {
        var primaryKeys = DataHelper.GetPkValues(formElement, pkValues, ',');

        var values = _entityRepository.GetFields(formElement, primaryKeys);

        if (field.DataFile.MultipleFile)
        {
            var currentFiles = values[field.Name]!.ToString()!.Split(",").ToList();

            if (!currentFiles.Contains(fileName))
            {
                currentFiles.Add(fileName);
                values[field.Name] = string.Join(",", currentFiles);
            }
        }
        else
        {
            values[field.Name] = fileName;
        }

        _entityRepository.SetValues(formElement, values);
    }
    private static void SetPhysicalFile(FormElement formElement, FormElementField field, string pkValues,
        IFormFile file)
    {
        var builder = new FormFilePathBuilder(formElement);

        var path = builder.GetFolderPath(field, DataHelper.GetPkValues(formElement, pkValues, ','));
        
        if (!Directory.Exists(path))
            Directory.CreateDirectory(path);

        if (!field.DataFile.MultipleFile)
        {
            foreach (var fileInfo in new DirectoryInfo(path).EnumerateFiles())
            {
                fileInfo.Delete();
            }
        }

        using var fileStream =
            new FileStream(Path.Combine(path, file.FileName), FileMode.OpenOrCreate, FileAccess.ReadWrite);

        file.CopyToAsync(fileStream);
    }
    public void DeleteFile(string elementName, string fieldName, string pkValues, string fileName)
    {
        var metadata = _dictionaryRepository.GetMetadata(elementName);
        
        if (!metadata.MetadataApiOptions.EnableDel)
            throw new UnauthorizedAccessException();
        
        var formElement = metadata.GetFormElement();
        
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
            throw new KeyNotFoundException(Translate.Key("File not found"));
    }
    private void DeleteEntityFile(Element element, FormElementField field, string pkValues, string fileName)
    {
        var primaryKeys = DataHelper.GetPkValues(element, pkValues, ',');

        var values = _entityRepository.GetFields(element, primaryKeys);

        if (field.DataFile.MultipleFile)
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


        _entityRepository.SetValues(element, values);
    }
    public void RenameFile(string elementName, string fieldName, string pkValues, string oldName, string newName)
    {
        var metadata = _dictionaryRepository.GetMetadata(elementName);
        
        if (!metadata.MetadataApiOptions.EnableUpdatePart)
            throw new UnauthorizedAccessException();
        
        var formElement = metadata.GetFormElement();
        
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
            throw new KeyNotFoundException(Translate.Key("File not found"));
    }
    private void RenameEntityFile(FormElement formElement, FormElementField field, string pkValues, string oldName,
        string newName)
    {
        var primaryKeys = DataHelper.GetPkValues(formElement, pkValues, ',');

        var values = _entityRepository.GetFields(formElement, primaryKeys);

        if (field.DataFile.MultipleFile)
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

        _entityRepository.SetValues(formElement, values);
    }
}