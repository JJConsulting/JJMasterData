using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text.Json;
using System.Threading.Tasks;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.DataDictionary.Repository.Abstractions;

namespace JJMasterData.Core.DataDictionary.Services;

public class ElementImportService(IDataDictionaryRepository dataDictionaryRepository)
{
    public async Task<bool> Import(MemoryStream file)
    {
        file.Seek(0, SeekOrigin.Begin);
        
        var formElement = await JsonSerializer.DeserializeAsync<FormElement>(file);

        await dataDictionaryRepository.InsertOrReplaceAsync(formElement);

        return true;
    }

    public async Task ImportZipFile(MemoryStream ms)
    {
        using var zip = new ZipArchive(ms, ZipArchiveMode.Read, leaveOpen: true);
        
        var formElements = GetFormElements(zip);

        await dataDictionaryRepository.InsertOrReplaceAsync(formElements);
    }

    private static IEnumerable<FormElement> GetFormElements(ZipArchive zip)
    {
        foreach (var entry in zip.Entries)
        {
            if (!entry.Name.EndsWith(".json"))
                continue;
            
            using var entryStream = entry.Open();
        
            var formElement = JsonSerializer.Deserialize<FormElement>(entryStream);

            yield return formElement;
        }
    }
}