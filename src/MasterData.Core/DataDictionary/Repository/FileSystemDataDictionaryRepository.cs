using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using JJMasterData.Commons.Data.Entity.Repository;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.DataDictionary.Repository.Abstractions;
using JJMasterData.Core.DataDictionary.Structure;
using Microsoft.Extensions.Options;

namespace JJMasterData.Core.DataDictionary.Repository;

/// <summary>
/// The Data Dictionaries (metadata) are stored in files in a custom folder.
/// </summary>
public class FileSystemDataDictionaryRepository(IOptionsSnapshot<FileSystemDataDictionaryOptions> options) : IDataDictionaryRepository
{
    public List<FormElement> GetMetadataList(bool? sync = null)
    {
        var list = new List<FormElement>();
        foreach (var file in GetFiles())
        {
            var metadata = GetMetadata(file.Name);
            if (metadata != null && (!sync.HasValue || metadata.EnableSynchronism == sync.Value))
                list.Add(metadata);
        }

        return list;
    }

    public async Task<List<FormElement>> GetFormElementListAsync(bool? apiSync = null)
    {
        var list = new List<FormElement>();
        foreach (var file in GetFiles())
        {
            var metadata = await ReadMetadataAsync(file.FullName);
            if (metadata != null && (!apiSync.HasValue || metadata.EnableSynchronism == apiSync.Value))
                list.Add(metadata);
        }

        return list;
    }

    public List<string> GetElementNameList()
    {
        return GetFiles()
            .Select(file => Path.GetFileNameWithoutExtension(file.Name))
            .OrderBy(name => name, StringComparer.Ordinal)
            .ToList();
    }

    public ValueTask<List<string>> GetElementNameListAsync()
    {
        return new ValueTask<List<string>>(GetElementNameList());
    }

    public List<FormElement> GetFormElementList(bool? apiSync = null)
    {
        return GetMetadataList(apiSync);
    }

    public FormElement? GetMetadata(string elementName)
    {
        using var stream = OpenRead(GetFullFileName(elementName), false);
        return JsonSerializer.Deserialize<FormElement>(stream);
    }

    public FormElement? GetFormElement(string elementName)
    {
        return GetMetadata(elementName);
    }

    public async ValueTask<FormElement> GetFormElementAsync(string elementName)
    {
        return await ReadMetadataAsync(GetFullFileName(elementName))
               ?? throw new JsonException($"Dictionary '{elementName}' contains null metadata.");
    }

    private static async ValueTask<FormElement?> ReadMetadataAsync(string path)
    {
        await using var stream = OpenRead(path, true);
        return await JsonSerializer.DeserializeAsync<FormElement>(stream);
    }

    // Readers may finish reading the old file while a writer replaces its directory entry.
    private static FileStream OpenRead(string path, bool asynchronous)
    {
        return new FileStream(path, new FileStreamOptions
        {
            Mode = FileMode.Open,
            Access = FileAccess.Read,
            Share = FileShare.Read | FileShare.Delete,
            Options = asynchronous ? FileOptions.Asynchronous | FileOptions.SequentialScan : FileOptions.SequentialScan
        });
    }

    public void InsertOrReplace(FormElement formElement)
    {
        ArgumentNullException.ThrowIfNull(formElement);
        var path = GetFullFileName(formElement.Name);
        var temporaryPath = GetTemporaryPath(path);
        try
        {
            using (var stream = new FileStream(temporaryPath, FileMode.CreateNew, FileAccess.Write, FileShare.None))
                JsonSerializer.Serialize(stream, formElement);
            File.Move(temporaryPath, path, overwrite: true);
        }
        finally
        {
            File.Delete(temporaryPath);
        }
    }

    public async Task InsertOrReplaceAsync(IEnumerable<FormElement> formElements)
    {
        ArgumentNullException.ThrowIfNull(formElements);
        foreach (var formElement in formElements)
            await InsertOrReplaceAsync(formElement);
    }

    public async Task InsertOrReplaceAsync(FormElement metadata)
    {
        ArgumentNullException.ThrowIfNull(metadata);
        var path = GetFullFileName(metadata.Name);
        var temporaryPath = GetTemporaryPath(path);
        try
        {
            await using (var stream = new FileStream(temporaryPath, new FileStreamOptions
                         {
                             Mode = FileMode.CreateNew,
                             Access = FileAccess.Write,
                             Share = FileShare.None,
                             Options = FileOptions.Asynchronous
                         }))
                await JsonSerializer.SerializeAsync(stream, metadata);
            File.Move(temporaryPath, path, overwrite: true);
        }
        finally
        {
            File.Delete(temporaryPath);
        }
    }

    private static string GetTemporaryPath(string path)
    {
        return path + "." + Guid.NewGuid().ToString("N") + ".tmp";
    }

    public void Delete(string elementName)
    {
        File.Delete(GetFullFileName(elementName));
    }

    public Task DeleteAsync(string elementName)
    {
        Delete(elementName);
        return Task.CompletedTask;
    }

    public async Task<ListResult<FormElementInfo>> GetFormElementInfoListAsync(DataDictionaryFilter filters,
        OrderByData? orderBy, int recordsPerPage, int currentPage)
    {
        var list = new List<FormElementInfo>();
        foreach (var file in GetFiles(filters))
        {
            var formElement = await ReadMetadataAsync(file.FullName);
            if (formElement != null && MatchesFilter(formElement, filters))
                list.Add(new FormElementInfo(formElement, file.LastWriteTime));
        }

        return new ListResult<FormElementInfo>(GetPage(list, orderBy, recordsPerPage, currentPage).ToList(),
            list.Count);
    }

    public bool Exists(string elementName)
    {
        return File.Exists(GetFullFileName(elementName));
    }

    public Task<bool> ExistsAsync(string elementName)
    {
        return Task.FromResult(Exists(elementName));
    }

    public void CreateStructureIfNotExists()
    {
        Directory.CreateDirectory(options.Value.FolderPath);
    }

    public Task CreateStructureIfNotExistsAsync()
    {
        CreateStructureIfNotExists();
        return Task.CompletedTask;
    }

    public IEnumerable<FormElementInfo> GetMetadataInfoList(DataDictionaryFilter? filter, OrderByData? orderBy,
        int recordsPerPage, int currentPage, ref int totalRecords)
    {
        var list = new List<FormElementInfo>();
        foreach (var file in GetFiles(filter))
        {
            var formElement = GetMetadata(file.Name);
            if (formElement != null && MatchesFilter(formElement, filter))
                list.Add(new FormElementInfo(formElement, file.LastWriteTime));
        }

        totalRecords = list.Count;
        return GetPage(list, orderBy, recordsPerPage, currentPage);
    }

    private IEnumerable<FileInfo> GetFiles(DataDictionaryFilter? filter = null)
    {
        var directory = new DirectoryInfo(options.Value.FolderPath);
        if (!directory.Exists)
            return [];

        // Check dates before opening or deserializing any files.
        var from = filter?.LastModifiedFrom;
        var to = filter?.LastModifiedTo;
        return directory.EnumerateFiles("*.json").Where(file =>
            (!from.HasValue || file.LastWriteTime >= from.Value) &&
            (!to.HasValue || file.LastWriteTime <= to.Value));
    }

    private static bool MatchesFilter(FormElement element, DataDictionaryFilter? filter)
    {
        return filter == null ||
               ((string.IsNullOrEmpty(filter.Name) ||
                 element.Name.Contains(filter.Name, StringComparison.OrdinalIgnoreCase)) &&
                (filter.ContainsTableName is not { Count: > 0 } || filter.ContainsTableName.Any(table =>
                    element.TableName.Contains(table, StringComparison.OrdinalIgnoreCase))));
    }

    private static IEnumerable<FormElementInfo> GetPage(List<FormElementInfo> list, OrderByData? orderBy,
        int recordsPerPage, int currentPage)
    {
        IOrderedEnumerable<FormElementInfo>? ordered = null;
        var query = orderBy?.ToQueryParameter();
        if (!string.IsNullOrEmpty(query))
        {
            foreach (var field in query.Split(','))
            {
                var parts = field.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                Func<FormElementInfo, object?> selector = parts[0].ToLowerInvariant() switch
                {
                    DataDictionaryStructure.Name => item => item.Name,
                    DataDictionaryStructure.TableName => item => item.TableName,
                    DataDictionaryStructure.Info => item => item.Info,
                    DataDictionaryStructure.LastModified => item => item.Modified,
                    DataDictionaryStructure.EnableSynchronism or "enableapi" => item => item.EnableApi,
                    _ => throw new ArgumentException($"Invalid dictionary sort field: {parts[0]}", nameof(orderBy))
                };
                var descending = parts.Length > 1 && parts[1].Equals("DESC", StringComparison.OrdinalIgnoreCase);
                ordered = ordered == null
                    ? descending ? list.OrderByDescending(selector) : list.OrderBy(selector)
                    : descending
                        ? ordered.ThenByDescending(selector)
                        : ordered.ThenBy(selector);
            }
        }

        return (ordered == null
                ? list.OrderBy(item => item.Name, StringComparer.Ordinal)
                : ordered.ThenBy(item => item.Name, StringComparer.Ordinal))
            .Skip((currentPage - 1) * recordsPerPage).Take(recordsPerPage);
    }

    private string GetFullFileName(string elementName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(elementName);
        if (Path.IsPathRooted(elementName) || elementName.IndexOfAny(['/', '\\', ':']) >= 0 ||
            elementName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0 || elementName is "." or "..")
            throw new ArgumentException("Dictionary name must be a simple file name.", nameof(elementName));

        if (!elementName.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
            elementName += ".json";
        return Path.Combine(options.Value.FolderPath, elementName);
    }
}