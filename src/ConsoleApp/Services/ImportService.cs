using JJMasterData.Commons.Configuration;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.DataDictionary.Repository.Abstractions;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using ConfigurationExtensions = JJMasterData.Commons.Configuration.ConfigurationExtensions;

namespace JJMasterData.ConsoleApp.Services;

public class ImportService(IDataDictionaryRepository dataDictionaryRepository, IConfiguration configuration)
{
    private IDataDictionaryRepository DataDictionaryRepository { get; } = dataDictionaryRepository;
    private string? DictionariesPath { get; } = ConfigurationExtensions.GetJJMasterData(configuration).GetValue<string?>("DataDictionaryPath");

    public void Import()
    {
        Console.WriteLine(AppContext.BaseDirectory);
        Console.WriteLine(@"Starting Process...
");

        var start = DateTime.Now;
        
        var databaseDictionaries = DataDictionaryRepository.GetFormElementListAsync(false).GetAwaiter().GetResult();
        var folderDictionaries = new List<FormElement>();

        if (DictionariesPath != null)
        {
            foreach (string file in Directory.EnumerateFiles(DictionariesPath, "*.json"))
            {
                var json = new StreamReader(file).ReadToEnd();

                var dicParser = JsonConvert.DeserializeObject<FormElement>(json);

                if (dicParser != null)
                {
                    Console.WriteLine($@"SetDictionary: {dicParser.Name}");
                    DataDictionaryRepository.InsertOrReplaceAsync(dicParser).GetAwaiter().GetResult();

                    folderDictionaries.Add(dicParser);
                }
            }
        }

        foreach (var dicParser in databaseDictionaries)
        {
            if (!folderDictionaries.Exists(dic => dic.Name.Equals(dicParser.Name)))
            {
                Console.WriteLine($@"DelDictionary: {dicParser.Name}");
                DataDictionaryRepository.DeleteAsync(dicParser.Name).GetAwaiter().GetResult();

            }
        }

        Console.WriteLine($@"
Process started: {start}");
        Console.WriteLine($@"Process finished: {DateTime.Now}");
    }
}