using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.DataDictionary.Repository.Abstractions;

namespace JJMasterData.Core.DataDictionary.Services;

public class ElementImportService(IDataDictionaryRepository dataDictionaryRepository)
{
    public async Task<bool> Import(Stream file)
    {
        file.Seek(0, SeekOrigin.Begin);
        
        var dicParser = await JsonSerializer.DeserializeAsync<FormElement>(file);

        //TODO: Validation
        //FormElement.Validate()

        await dataDictionaryRepository.InsertOrReplaceAsync(dicParser);

        return true;
    }
}