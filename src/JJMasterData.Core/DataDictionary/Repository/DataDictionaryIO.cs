using JJMasterData.Commons.Dao;
using JJMasterData.Commons.Dao.Entity;
using JJMasterData.Commons.DI;
using JJMasterData.Commons.Language;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;

namespace JJMasterData.Core.DataDictionary.Repository;

/// <summary>
/// The Data Dictionaries (metadata) are stored in files in a custom folder
/// </summary>
public class DataDictionaryIO : IDataDictionaryRepository
{
    private readonly IEntityRepository _entityRepository;

    public string FolderPath { get; }
    
    public DataDictionaryIO(IEntityRepository entityRepository)
    {
        _entityRepository = entityRepository;
        FolderPath = "Metadata";
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
        return Directory.GetFiles(FolderPath, "*.json");
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

    ///<inheritdoc cref="IDataDictionaryRepository.GetDataTable"/>
    public DataTable GetDataTable(DataDictionaryFilter filter, string orderBy, int recordsPerPage, int currentPage, ref int totalRecords)
    {
        var dtFiles = new DataTable();
        dtFiles.Columns.Add(DataDictionaryStructure.Type, typeof(string));
        dtFiles.Columns.Add(DataDictionaryStructure.Name, typeof(string));
        dtFiles.Columns.Add(DataDictionaryStructure.NameFilter, typeof(string));
        dtFiles.Columns.Add(DataDictionaryStructure.TableName, typeof(string));
        dtFiles.Columns.Add(DataDictionaryStructure.Info, typeof(string));
        dtFiles.Columns.Add(DataDictionaryStructure.Owner, typeof(string));
        dtFiles.Columns.Add(DataDictionaryStructure.Sync, typeof(string));
        dtFiles.Columns.Add(DataDictionaryStructure.LastModified, typeof(DateTime));
        dtFiles.Columns.Add(DataDictionaryStructure.Json, typeof(string));

        var dir = new DirectoryInfo(FolderPath);
        if (!dir.Exists)
            return dtFiles;
        
        var files = dir.GetFiles("*.json");
        foreach (var file in files)
        {
            var metadata = GetMetadata(file.Name);
            if (metadata == null)
                continue;

            if (filter != null)
            {
                if (!string.IsNullOrEmpty(filter.Name) && !metadata.Table.Name.Contains(filter.Name))
                    continue;

                if (filter.ContainsTableName != null)
                {
                    bool containsName = filter.ContainsTableName.Any(tableName => metadata.Table.TableName.Contains(tableName));
                    if (!containsName)
                        continue;
                }   
                
                if (filter.LastModifiedFrom.HasValue && file.LastWriteTime < filter.LastModifiedFrom)
                    continue;
                    
                if (filter.LastModifiedTo.HasValue && file.LastWriteTime > filter.LastModifiedTo)
                    continue;
            }
            
            var row = dtFiles.NewRow();
            row[DataDictionaryStructure.Type] = "F";
            row[DataDictionaryStructure.Name] = metadata.Table.Name;
            row[DataDictionaryStructure.NameFilter] = metadata.Table.Name;
            row[DataDictionaryStructure.TableName] = metadata.Table.Name;
            row[DataDictionaryStructure.Info] = metadata.Table.Name;
            row[DataDictionaryStructure.Owner] = metadata.Table.Name;
            row[DataDictionaryStructure.Sync] = metadata.Table.Name;
            row[DataDictionaryStructure.LastModified] = metadata.Table.Name;
            row[DataDictionaryStructure.Json] = metadata.Table.Name;
            
            row["Tamanho"] = Format.FormatFileSize(file.Length);
            row["TamBytes"] = file.Length;
            row["LastWriteTime"] = file.LastWriteTime.ToString(CultureInfo.CurrentCulture);
            dtFiles.Rows.Add(row);
           
        }
        
        return dtFiles;
      
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