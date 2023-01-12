using JJMasterData.Commons.Dao.Entity;
using JJMasterData.Commons.Dao.Entity.Abstractions;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataDictionary.Repository;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

var builder = new ConfigurationBuilder().AddJsonFile("appsettings.json", true, true);
IConfiguration configuration = builder.Build();

var path = configuration.GetValue<string>("DictionaryPath");

Console.WriteLine(AppContext.BaseDirectory);
Console.WriteLine("Starting Process...\n");

DateTime start = DateTime.Now;
IEntityRepository entityRepository = new Factory();
var dicDao = new DatabaseDataDictionaryRepository(entityRepository, "");
var databaseDictionaries = dicDao.GetMetadataList(false);
var folderDictionaries = new List<Metadata>();

if (path != null)
{
    foreach (string file in Directory.EnumerateFiles(path, "*.json"))
    {
        var json = new StreamReader(file).ReadToEnd();

        var dicParser = JsonConvert.DeserializeObject<Metadata>(json);

        if (dicParser != null)
        {
            Console.WriteLine($"SetDictionary: {dicParser.Table.Name}");
            dicDao.InsertOrReplace(dicParser);

            folderDictionaries.Add(dicParser);
        }
    }
}

foreach (var dicParser in databaseDictionaries)
{
    if (!folderDictionaries.Exists(dic => dic.Table.Name.Equals(dicParser.Table.Name)))
    {
        Console.WriteLine($"DelDictionary: {dicParser.Table.Name}");
        dicDao.Delete(dicParser.Table.Name);

    }
}

Console.WriteLine($"\nProcess started: {start}");
Console.WriteLine($"Process finished: {DateTime.Now}");
Console.ReadLine();