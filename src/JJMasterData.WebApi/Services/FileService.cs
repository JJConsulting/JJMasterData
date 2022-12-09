using JJMasterData.Commons.Dao;
using JJMasterData.Commons.Dao.Entity;
using JJMasterData.Commons.Language;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataDictionary.Repository;
using JJMasterData.Core.DataManager;

namespace JJMasterData.WebApi.Services;

public class FileService
{

    private readonly IDictionaryRepository _dictionaryRepository;
    private readonly IEntityRepository _entityRepository;

    public FileService(IDictionaryRepository dictionaryRepository, IEntityRepository entityRepository)
    {
        _dictionaryRepository = dictionaryRepository;
        _entityRepository = entityRepository;
    }

    public FileStream GetDictionaryFile(string elementName, string pkValues, string fieldName, string fileName)
    {
        var metadata = _dictionaryRepository.GetMetadata(elementName);
        if (!metadata.Api.EnableGetDetail)
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
        var formElement = metadata.GetFormElement();
        var field = formElement.Fields.First(f => f.Name == fieldName);

        SetPhysicalFile(pkValues, file, formElement, field);

        SetEntityFile(pkValues, file, formElement, field);
    }
    private void SetEntityFile(string pkValues, IFormFile file, FormElement formElement, FormElementField field)
    {
        var primaryKeys = DataHelper.GetPkValues(formElement, pkValues, ',');

        var values = _entityRepository.GetFields(formElement, primaryKeys);

        if (field.DataFile.MultipleFile)
        {
            var currentFiles = values[field.Name]!.ToString()!.Split(",").ToList();

            if (!currentFiles.Contains(file.FileName))
            {
                currentFiles.Add(file.FileName);
                values[field.Name] = string.Join(",", currentFiles);
            }
        }
        else
        {
            values[field.Name] = file.FileName;
        }

        _entityRepository.SetValues(formElement, values);
    }
    private static void SetPhysicalFile(string pkValues, IFormFile file, FormElement formElement, FormElementField field)
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
}