using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text.Json;
using System.Threading.Tasks;
using JJMasterData.Core.DataDictionary.Repository.Abstractions;

namespace JJMasterData.Core.DataDictionary.Services;

public class ElementExportService(IDataDictionaryRepository dataDictionaryRepository)
{
    private static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        WriteIndented = true
    };

    public async Task<MemoryStream> ExportSingleRowAsync(Dictionary<string, object> row)
    {
        var elementName = row["name"].ToString();
        var formElement = await dataDictionaryRepository.GetFormElementAsync(elementName);

        var memoryStream = new MemoryStream();
        await JsonSerializer.SerializeAsync(memoryStream, formElement, JsonSerializerOptions);

        memoryStream.Seek(0, SeekOrigin.Begin);
        
        return memoryStream;
    }

    public async Task<MemoryStream> ExportMultipleRowsAsync(List<Dictionary<string, object>> selectedRows)
    {
        var memoryStream = new MemoryStream();
        
        using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
        {
            foreach (var element in selectedRows)
            {
                var elementName = element["name"].ToString();
                var formElement = await dataDictionaryRepository.GetFormElementAsync(elementName);

                var jsonFile = archive.CreateEntry($"{elementName}.json");
                using var jsonFileStream = jsonFile.Open();
                await JsonSerializer.SerializeAsync(jsonFileStream, formElement);
            }
        }
        
        memoryStream.Seek(0, SeekOrigin.Begin);
        return memoryStream;
    }
}