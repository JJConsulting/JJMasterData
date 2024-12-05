using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using JJMasterData.Commons.Data.Entity.Repository;
using JJMasterData.Commons.Extensions;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.DataDictionary.Repository.Abstractions;
using JJMasterData.Core.DataDictionary.Structure;
using Microsoft.Extensions.Options;


namespace JJMasterData.Core.DataDictionary.Repository;

/// <summary>
/// The Data Dictionaries (metadata) are stored in files in a custom folder
/// </summary>
public class FileSystemDataDictionaryRepository
    (IOptionsSnapshot<FileSystemDataDictionaryOptions> options) : IDataDictionaryRepository
{
    public string FolderPath { get; } = options.Value.FolderPath;

    ///<inheritdoc cref="IDataDictionaryRepository.GetMetadataList"/>
    public List<FormElement> GetMetadataList(bool? sync = null)
    {
        var list = new List<FormElement>();
        var dir = new DirectoryInfo(FolderPath);

        if (!dir.Exists)
            return list;
        
        var files = dir.GetFiles("*.json");
        foreach (var file in files)
        {
            var metadata = GetMetadata(file.Name);
            if (metadata == null)
                continue;

            if (!sync.HasValue)
            {
                list.Add(metadata);
                continue;
            }
            
            if (metadata.EnableSynchronism == sync.Value)
            {
                list.Add(metadata);
            }
        }

        return list;
    }

    public Task<List<FormElement>> GetFormElementListAsync(bool? apiSync = null)
    {
        var result = GetMetadataList();
        return Task.FromResult(result);
    }

    ///<inheritdoc cref="IDataDictionaryRepository.GetNameList"/>
    public List<string> GetNameList()
    {
        var list = new List<string>();
        var dir = new DirectoryInfo(FolderPath);

        if (!dir.Exists)
            return list;
        
        var files = dir.GetFiles("*.json").OrderBy(x => x.Name);
        foreach (var file in files)
        {
            string elementName = file.Name;
            if (!elementName.EndsWith(".json"))
                elementName += ".json";
            
            list.Add(elementName);
        }

        return list;
    }

    public Task<List<string>> GetNameListAsync()
    {
        var names = GetNameList();

        return Task.FromResult(names);
    }

    public List<FormElement> GetFormElementList(bool? apiSync = null) => GetMetadataList(apiSync);
    
    ///<inheritdoc cref="IDataDictionaryRepository.GetMetadata"/>
    public FormElement GetMetadata(string elementName)
    {
        string fileFullName = GetFullFileName(elementName);
        string json = File.ReadAllText(fileFullName);
        return JsonSerializer.Deserialize<FormElement>(json);
    }

    public FormElement GetFormElement(string elementName)
    {
        return GetMetadata(elementName);
    }

    public ValueTask<FormElement> GetFormElementAsync(string elementName)
    {
        var result = GetMetadata(elementName);
        return new ValueTask<FormElement>(result);
    }

    ///<inheritdoc cref="IDataDictionaryRepository.InsertOrReplace"/>
    public void InsertOrReplace(FormElement formElement)
    {
        if (formElement == null)
            throw new ArgumentNullException(nameof(formElement));
        
        
        if (string.IsNullOrEmpty(formElement.Name))
            throw new ArgumentNullException(nameof(formElement.Name));

        string json = JsonSerializer.Serialize(formElement);
        string fileFullName = GetFullFileName(formElement.Name);
        File.WriteAllText(fileFullName, json);
    }

    public Task InsertOrReplaceAsync(FormElement metadata)
    {
        InsertOrReplace(metadata);
        return Task.CompletedTask;
    }

    ///<inheritdoc cref="IDataDictionaryRepository.Delete"/>
    public void Delete(string elementName)
    {
        string fileFullName = GetFullFileName(elementName);
        File.Delete(fileFullName);
    }

    public Task DeleteAsync(string elementName)
    {
        Delete(elementName);
        return Task.CompletedTask;
    }

    public Task<ListResult<FormElementInfo>> GetFormElementInfoListAsync(DataDictionaryFilter filters, OrderByData orderBy, int recordsPerPage, int currentPage)
    {
        int total = 0;
        var result = GetMetadataInfoList(filters,orderBy,recordsPerPage,currentPage,ref total);
        return Task.FromResult(new ListResult<FormElementInfo>(result.ToList(),total));
    }

    ///<inheritdoc cref="IDataDictionaryRepository.Exists"/>
    public bool Exists(string elementName)
    {
        string fileFullName = GetFullFileName(elementName);
        return File.Exists(fileFullName);
    }
    
    public Task<bool> ExistsAsync(string elementName)
    {
        var result = Exists(elementName);

        return Task.FromResult(result);
    }

    ///<inheritdoc cref="IDataDictionaryRepository.CreateStructureIfNotExists"/>
    public void CreateStructureIfNotExists()
    {
        if (!Directory.Exists(FolderPath))
            Directory.CreateDirectory(FolderPath);
    }
    
    public Task CreateStructureIfNotExistsAsync()
    {
        CreateStructureIfNotExists();
        return Task.CompletedTask;
    }

    ///<inheritdoc cref="IDataDictionaryRepository.GetMetadataInfoList"/>
    public IEnumerable<FormElementInfo> GetMetadataInfoList(DataDictionaryFilter filter, OrderByData orderBy, int recordsPerPage, int currentPage, ref int totalRecords)
    {
        var list = new List<FormElementInfo>();
        
        var dir = new DirectoryInfo(FolderPath);
        if (!dir.Exists)
            return list;

        var files = dir.GetFiles("*.json");
        foreach (var file in files)
        {
            string fileFullName = GetFullFileName(file.Name);
            string json = File.ReadAllText(fileFullName);
            var formElement =  JsonSerializer.Deserialize<FormElement>(json);

            if (formElement == null)
                continue;

            if (filter != null)
            {
                if (!string.IsNullOrEmpty(filter.Name) && formElement.Name.IndexOf(filter.Name, StringComparison.OrdinalIgnoreCase) < 0)
                    continue;

                if (filter.ContainsTableName != null)
                {
                    bool containsName = filter.ContainsTableName.Any(tableName => formElement.TableName.IndexOf(tableName, StringComparison.OrdinalIgnoreCase) >= 0);
                    if (!containsName)
                        continue;
                }
                
                if (filter.LastModifiedFrom.HasValue && file.LastWriteTime < filter.LastModifiedFrom)
                    continue;
                    
                if (filter.LastModifiedTo.HasValue && file.LastWriteTime > filter.LastModifiedTo)
                    continue;
            }
            
            var metadataInfo = new FormElementInfo(formElement, file.LastWriteTime);
            list.Add(metadataInfo);
        }

        totalRecords = list.Count;
        return list.OrderBy(orderBy.ToQueryParameter()).Skip((currentPage - 1) * recordsPerPage).Take(recordsPerPage);
    }

    private string GetFullFileName(string elementName)
    {
        if (string.IsNullOrEmpty(elementName))
            throw new ArgumentNullException(nameof(elementName), @"Dictionary invalid");

        if (!elementName.EndsWith(".json"))
            elementName += ".json";
        
        return Path.Combine(FolderPath, elementName);
    }
}