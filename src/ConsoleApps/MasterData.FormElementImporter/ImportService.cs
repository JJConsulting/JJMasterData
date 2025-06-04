using System.Text.Json;
using JJMasterData.Commons.Configuration;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.DataDictionary.Repository.Abstractions;
using Microsoft.Extensions.Configuration;

namespace JJMasterData.FormElementImportator;

public class ImportService(IDataDictionaryRepository dataDictionaryRepository, IConfiguration configuration)
{
    private readonly string? _dictionariesPath= configuration.GetJJMasterData().GetValue<string?>("DataDictionaryPath");

    public async Task Import()
    {
        Console.WriteLine(AppContext.BaseDirectory);
        Console.WriteLine("Starting Process...");

        var start = DateTime.Now;
        var databaseDictionaries = await dataDictionaryRepository.GetFormElementListAsync();
        var folderDictionaries = new List<FormElement>();

        if (_dictionariesPath != null)
        {
            foreach (var file in Directory.EnumerateFiles(_dictionariesPath, "*.json"))
            {
                await using var jsonStream = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read);

                var formElement = JsonSerializer.Deserialize<FormElement>(jsonStream);

                await dataDictionaryRepository.InsertOrReplaceAsync(formElement);
                
                Console.WriteLine($"Saving FormElement: {formElement.Name}");

                folderDictionaries.Add(formElement);
            }
        }

        foreach (var formElement in databaseDictionaries)
        {
            if (!folderDictionaries.Exists(dic => dic.Name.Equals(formElement.Name)))
            {
                Console.WriteLine($"Deleting FormElement: {formElement.Name}");
                await dataDictionaryRepository.DeleteAsync(formElement.Name);
            }
        }

        Console.WriteLine($"Process started: {start}");
        Console.WriteLine($"Process finished: {DateTime.Now}");
    }
}