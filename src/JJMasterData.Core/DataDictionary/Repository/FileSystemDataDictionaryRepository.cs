using JJMasterData.Commons.Language;
using JJMasterData.Commons.Extensions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
    public IEnumerable<Metadata> GetMetadataList(bool? sync = null)
    {
        var list = new List<Metadata>();
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
            
            if (metadata.Table.Sync == sync.Value)
            {
                list.Add(metadata);
            }
        }

        return list;
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

    ///<inheritdoc cref="IDataDictionaryRepository.GetMetadata"/>
    public Metadata GetMetadata(string dictionaryName)
    {
        string fileFullName = GetFullFileName(dictionaryName);
        string json = File.ReadAllText(fileFullName);
        return JsonConvert.DeserializeObject<Metadata>(json);
    }

    ///<inheritdoc cref="IDataDictionaryRepository.InsertOrReplace"/>
    public void InsertOrReplace(Metadata metadata)
    {
        if (metadata == null)
            throw new ArgumentNullException(nameof(metadata));

        if (metadata.Table == null)
            throw new ArgumentNullException(nameof(metadata.Table));

        if (string.IsNullOrEmpty(metadata.Table.Name))
            throw new ArgumentNullException(nameof(metadata.Table.Name));

        string json = JsonConvert.SerializeObject(metadata);
        string fileFullName = GetFullFileName(metadata.Table.Name);
        File.WriteAllText(fileFullName, json);
    }

    ///<inheritdoc cref="IDataDictionaryRepository.Delete"/>
    public void Delete(string dictionaryName)
    {
        string fileFullName = GetFullFileName(dictionaryName);
        File.Delete(fileFullName);
    }

    ///<inheritdoc cref="IDataDictionaryRepository.Exists"/>
    public bool Exists(string dictionaryName)
    {
        string fileFullName = GetFullFileName(dictionaryName);
        return File.Exists(fileFullName);
    }

    ///<inheritdoc cref="IDataDictionaryRepository.CreateStructureIfNotExists"/>
    public void CreateStructureIfNotExists()
    {
        if (!Directory.Exists(FolderPath))
            Directory.CreateDirectory(FolderPath);
    }

    ///<inheritdoc cref="IDataDictionaryRepository.GetMetadataInfoList"/>
    public IEnumerable<MetadataInfo> GetMetadataInfoList(DataDictionaryFilter filter, string orderBy, int recordsPerPage, int currentPage, ref int totalRecords)
    {
        var list = new List<MetadataInfo>();
        
        var dir = new DirectoryInfo(FolderPath);
        if (!dir.Exists)
            return list;

        var files = dir.GetFiles("*.json");
        foreach (var file in files)
        {
            string fileFullName = GetFullFileName(file.Name);
            string json = File.ReadAllText(fileFullName);
            var metadata =  JsonConvert.DeserializeObject<Metadata>(json);

            if (metadata == null)
                continue;

            if (filter != null)
            {
                if (!string.IsNullOrEmpty(filter.Name) && !metadata.Table.Name.ToLower().Contains(filter.Name.ToLower()))
                    continue;

                if (filter.ContainsTableName != null)
                {
                    bool containsName = filter.ContainsTableName.Any(tableName => metadata.Table.TableName.ToLower().Contains(tableName.ToLower()));
                    if (!containsName)
                        continue;
                }   
                
                if (filter.LastModifiedFrom.HasValue && file.LastWriteTime < filter.LastModifiedFrom)
                    continue;
                    
                if (filter.LastModifiedTo.HasValue && file.LastWriteTime > filter.LastModifiedTo)
                    continue;
            }
            
            var metadataInfo = new MetadataInfo(metadata, file.LastWriteTime);
            list.Add(metadataInfo);
        }

        totalRecords = list.Count;
        return list.OrderBy(orderBy).Skip((currentPage - 1) * recordsPerPage).Take(recordsPerPage);
    }

    private string GetFullFileName(string dictionaryName)
    {
        if (string.IsNullOrEmpty(dictionaryName))
            throw new ArgumentNullException(nameof(dictionaryName), Translate.Key("Dictionary invalid"));

        if (!dictionaryName.EndsWith(".json"))
            dictionaryName += ".json";
        
        return Path.Combine(FolderPath, dictionaryName);
    }
}