using JJMasterData.Commons.Dao;
using JJMasterData.Commons.Dao.Entity;
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
var dicDao = new DictionaryDao(entityRepository);
var databaseDictionaries = dicDao.GetListDictionary(false);
var folderDictionaries = new List<DataDictionary>();

foreach (string file in Directory.EnumerateFiles(path, "*.json"))
{
    var json = new StreamReader(file).ReadToEnd();

    var dicParser = JsonConvert.DeserializeObject<DataDictionary>(json);

    if(dicParser != null)
    {
        Console.WriteLine($"SetDictionary: {dicParser.Table.Name}");
        dicDao.SetDictionary(dicParser);

        folderDictionaries.Add(dicParser);
    }

}

foreach (var dicParser in databaseDictionaries)
{
    if (!folderDictionaries.Exists(dic => dic.Table.Name.Equals(dicParser.Table.Name)))
    {
        Console.WriteLine($"DelDictionary: {dicParser.Table.Name}");
        dicDao.DelDictionary(dicParser.Table.Name);

    }
}

Console.WriteLine($"\nProcess started: {start}");
Console.WriteLine($"Process finished: {DateTime.Now}");
Console.ReadLine();