using JJMasterData.Commons.Extensions;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataDictionary.Repository.Abstractions;
using JJMasterData.Core.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

var services = new ServiceCollection();

var builder = new ConfigurationBuilder().SetBasePath(AppContext.BaseDirectory).AddJsonFile("appsettings.json", false, false);

IConfiguration configuration = builder.Build();

services.AddSingleton(configuration);

services.AddJJMasterDataCore(coreOptions =>
{
    coreOptions.DataDictionaryTableName = configuration.GetJJMasterData().GetValue<string>("DataDictionaryTableName")!;
}, commonsOptions =>
{
    commonsOptions.PrefixGetProc = configuration.GetJJMasterData().GetValue<string>("PrefixGetProc")!;
    commonsOptions.PrefixSetProc = configuration.GetJJMasterData().GetValue<string>("PrefixSetProc")!;
});

var path = configuration.GetValue<string>("DictionaryPath");

var serviceProvider = services.BuildServiceProvider().UseJJMasterData();

Console.WriteLine(AppContext.BaseDirectory);
Console.WriteLine(@"Starting Process...
");

var start = DateTime.Now;

var repository = serviceProvider.GetRequiredService<IDataDictionaryRepository>();
var databaseDictionaries = repository.GetMetadataList(false);
var folderDictionaries = new List<FormElement>();

if (path != null)
{
    foreach (string file in Directory.EnumerateFiles(path, "*.json"))
    {
        var json = new StreamReader(file).ReadToEnd();

        var dicParser = JsonConvert.DeserializeObject<FormElement>(json);

        if (dicParser != null)
        {
            Console.WriteLine($@"SetDictionary: {dicParser.Name}");
            repository.InsertOrReplace(dicParser);

            folderDictionaries.Add(dicParser);
        }
    }
}

foreach (var dicParser in databaseDictionaries)
{
    if (!folderDictionaries.Exists(dic => dic.Name.Equals(dicParser.Name)))
    {
        Console.WriteLine($@"DelDictionary: {dicParser.Name}");
        repository.Delete(dicParser.Name);

    }
}

Console.WriteLine($@"
Process started: {start}");
Console.WriteLine($@"Process finished: {DateTime.Now}");
Console.ReadLine();