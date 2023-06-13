using JJMasterData.Commons.Data;
using JJMasterData.Commons.Extensions;
using JJMasterData.Core.DataDictionary.Repository.Abstractions;
using JJMasterData.Core.Extensions;
using JJMasterData.FormElementUpdater.Repository;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();

var builder = new ConfigurationBuilder().SetBasePath(AppContext.BaseDirectory).AddJsonFile("appsettings.json", false, false);

IConfiguration configuration = builder.Build();

services.AddSingleton(configuration);

var tableName = configuration.GetJJMasterData().GetValue<string>("DataDictionaryTableName")!;

services.AddJJMasterDataCore(coreOptions =>
{
    coreOptions.DataDictionaryTableName = tableName;
}, commonsOptions =>
{
    commonsOptions.PrefixGetProc = configuration.GetJJMasterData().GetValue<string>("PrefixGetProc")!;
    commonsOptions.PrefixSetProc = configuration.GetJJMasterData().GetValue<string>("PrefixSetProc")!;
});

services.AddTransient<MetadataRepository>();

var serviceProvider = services.BuildServiceProvider().UseJJMasterData();

Console.WriteLine(AppContext.BaseDirectory);
Console.WriteLine(@"Starting Process...");

var start = DateTime.Now;

var metadataRepository = serviceProvider.GetRequiredService<MetadataRepository>();
var formElementRepository = serviceProvider.GetRequiredService<IDataDictionaryRepository>();
var databaseDictionaries = metadataRepository.GetMetadataList();

foreach (var metadata in databaseDictionaries)
{
    var formElement = metadata.GetFormElement();
    formElementRepository.InsertOrReplace(formElement);
    Console.WriteLine(@"✅ " + formElement.Name);
}

new DataAccess().SetCommand($"delete from {tableName} where type <> 'F'");

Console.WriteLine($@"Process started: {start}");
Console.WriteLine($@"Process finished: {DateTime.Now}");
Console.ReadLine();