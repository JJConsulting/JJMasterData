using JJMasterData.Commons.Extensions;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataDictionary.Repository.Abstractions;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace JJMasterData.ConsoleApp.Services;

public class ImportService
{
    private IDataDictionaryRepository DataDictionaryRepository { get; }
    private string? DictionariesPath { get; }

    public ImportService(IDataDictionaryRepository dataDictionaryRepository, IConfiguration configuration)
    {
        DataDictionaryRepository = dataDictionaryRepository;
        DictionariesPath = configuration.GetJJMasterData().GetValue<string?>("DictionaryPath");
    }

    public void Import()
    {
        Console.WriteLine(AppContext.BaseDirectory);
        Console.WriteLine(@"Starting Process...
");

        var start = DateTime.Now;
        
        var databaseDictionaries = DataDictionaryRepository.GetMetadataList(false);
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
                    DataDictionaryRepository.InsertOrReplace(dicParser);

                    folderDictionaries.Add(dicParser);
                }
            }
        }

        foreach (var dicParser in databaseDictionaries)
        {
            if (!folderDictionaries.Exists(dic => dic.Name.Equals(dicParser.Name)))
            {
                Console.WriteLine($@"DelDictionary: {dicParser.Name}");
                DataDictionaryRepository.Delete(dicParser.Name);

            }
        }

        Console.WriteLine($@"
Process started: {start}");
        Console.WriteLine($@"Process finished: {DateTime.Now}");
    }
}