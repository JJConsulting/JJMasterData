using JJMasterData.Commons.Configuration;
using JJMasterData.Core.Configuration;
using JJMasterData.FormElementImportator;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();
var configurationBuilder = new ConfigurationBuilder();
configurationBuilder.AddJsonFile("appsettings.json");
services.AddSingleton<IConfiguration>(configurationBuilder.Build());
services.AddJJMasterDataCore();
services.AddTransient<ImportService>();
var serviceProvider = services.BuildServiceProvider();

var importService = serviceProvider.GetRequiredService<ImportService>();

await importService.Import();