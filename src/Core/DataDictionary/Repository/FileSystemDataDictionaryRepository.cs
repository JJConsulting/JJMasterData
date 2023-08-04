using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using JJMasterData.Commons.Data.Entity;
using JJMasterData.Commons.Extensions;
using Newtonsoft.Json;
using JJMasterData.Commons.Localization;
using JJMasterData.Core.DataDictionary.Repository.Abstractions;
using Microsoft.Extensions.Options;

namespace JJMasterData.Core.DataDictionary.Repository;

/// <summary>
/// The Data Dictionaries (metadata) are stored in files in a custom folder
/// </summary>
public class FileSystemDataDictionaryRepository : IDataDictionaryRepository
{
    public string FolderPath { get; }
    
    public FileSystemDataDictionaryRepository(IOptions<FileSystemDataDictionaryOptions> options)
    {
        FolderPath = options.Value.FolderPath;
    }

    ///<inheritdoc cref="IDataDictionaryRepository.GetMetadataList"/>
    public IEnumerable<FormElement> GetMetadataList(bool? sync = null)
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
            
            if (metadata.Sync == sync.Value)
            {
                list.Add(metadata);
            }
        }

        return list;
    }

    public async Task<IEnumerable<FormElement>> GetMetadataListAsync(bool? sync = null)
    {
        var result = GetMetadataList();
        return await Task.FromResult(result);
    }

    ///<inheritdoc cref="IDataDictionaryRepository.GetNameList"/>
    public IEnumerable<string> GetNameList()
    {
        var list = new List<string>();
        var dir = new DirectoryInfo(FolderPath);

        if (!dir.Exists)
            return list;
        
        var files = dir.GetFiles("*.json").OrderBy(x => x.Name);
        foreach (var file in files)
        {
            string dictionaryName = file.Name;
            if (!dictionaryName.EndsWith(".json"))
                dictionaryName += ".json";
            
            list.Add(dictionaryName);
        }

        return list;
    }

    public async IAsyncEnumerable<string> GetNameListAsync()
    {
        var names = GetNameList();

        foreach (var name in names)
        {
            yield return await Task.FromResult(name);
        }
    }

    ///<inheritdoc cref="IDataDictionaryRepository.GetMetadata"/>
    public FormElement GetMetadata(string dictionaryName)
    {
        string fileFullName = GetFullFileName(dictionaryName);
        string json = File.ReadAllText(fileFullName);
        return JsonConvert.DeserializeObject<FormElement>(json);
    }
    
    public Task<FormElement> GetMetadataAsync(string dictionaryName)
    {
        var result = GetMetadata(dictionaryName);
        return Task.FromResult(result);
    }

    ///<inheritdoc cref="IDataDictionaryRepository.InsertOrReplace"/>
    public void InsertOrReplace(FormElement formElement)
    {
        if (formElement == null)
            throw new ArgumentNullException(nameof(formElement));
        
        
        if (string.IsNullOrEmpty(formElement.Name))
            throw new ArgumentNullException(nameof(formElement.Name));

        string json = JsonConvert.SerializeObject(formElement);
        string fileFullName = GetFullFileName(formElement.Name);
        File.WriteAllText(fileFullName, json);
    }

    public Task InsertOrReplaceAsync(FormElement metadata)
    {
        InsertOrReplace(metadata);
        return Task.CompletedTask;
    }

    ///<inheritdoc cref="IDataDictionaryRepository.Delete"/>
    public void Delete(string dictionaryName)
    {
        string fileFullName = GetFullFileName(dictionaryName);
        File.Delete(fileFullName);
    }

    public Task DeleteAsync(string dictionaryName)
    {
        Delete(dictionaryName);
        return Task.CompletedTask;
    }

    public async Task<EntityResult<IEnumerable<FormElementInfo>>> GetFormElementInfoListAsync(DataDictionaryFilter filters, string orderBy, int recordsPerPage, int currentPage)
    {
        int total = 0;
        var result = GetMetadataInfoList(filters,orderBy,recordsPerPage,currentPage,ref total);
        return await Task.FromResult(new EntityResult<IEnumerable<FormElementInfo>>(result,total));
    }

    ///<inheritdoc cref="IDataDictionaryRepository.Exists"/>
    public bool Exists(string dictionaryName)
    {
        string fileFullName = GetFullFileName(dictionaryName);
        return File.Exists(fileFullName);
    }
    
    public Task<bool> ExistsAsync(string dictionaryName)
    {
        var result = Exists(dictionaryName);

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
    public IEnumerable<FormElementInfo> GetMetadataInfoList(DataDictionaryFilter filter, string orderBy, int recordsPerPage, int currentPage, ref int totalRecords)
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
            var formElement =  JsonConvert.DeserializeObject<FormElement>(json);

            if (formElement == null)
                continue;

            if (filter != null)
            {
                if (!string.IsNullOrEmpty(filter.Name) && !formElement.Name.ToLower().Contains(filter.Name.ToLower()))
                    continue;

                if (filter.ContainsTableName != null)
                {
                    bool containsName = filter.ContainsTableName.Any(tableName => formElement.TableName.ToLower().Contains(tableName.ToLower()));
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
        return list.OrderBy(orderBy).Skip((currentPage - 1) * recordsPerPage).Take(recordsPerPage);
    }

    private string GetFullFileName(string dictionaryName)
    {
        if (string.IsNullOrEmpty(dictionaryName))
            throw new ArgumentNullException(nameof(dictionaryName), "Dictionary invalid");

        if (!dictionaryName.EndsWith(".json"))
            dictionaryName += ".json";
        
        return Path.Combine(FolderPath, dictionaryName);
    }
}