using JJMasterData.Core.Configuration;
using JJMasterData.LegacyMetadataMigrator;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();
var configurationBuilder = new ConfigurationBuilder();
configurationBuilder.AddJsonFile("appsettings.json");
services.AddSingleton<IConfiguration>(configurationBuilder.Build());
services.AddJJMasterDataCore();
services.AddTransient<FormElementMigrationService>();
var serviceProvider = services.BuildServiceProvider();

var service = serviceProvider.GetRequiredService<FormElementMigrationService>();

service.Migrate();