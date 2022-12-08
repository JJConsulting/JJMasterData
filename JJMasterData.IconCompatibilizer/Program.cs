using JJMasterData.Commons.Extensions;
using JJMasterData.Commons.Options;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataDictionary.Action;
using JJMasterData.Core.DataDictionary.Repository;
using JJMasterData.Core.Extensions;
using JJMasterData.IconCompatibilizer.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

var configurationBuilder = new ConfigurationBuilder().AddJsonFile("appsettings.json", true, true);
IConfiguration configuration = configurationBuilder.Build();
IServiceCollection services = new ServiceCollection();
services.AddSingleton(configuration);
services.AddOptions<JJMasterDataOptions>().Bind(configuration.GetSection("JJMasterData"));
services.AddJJMasterDataCore();

var serviceProvider = services.BuildServiceProvider().UseJJMasterData();

MigrateIcons(serviceProvider);

Console.ReadLine();

static void MigrateIcons(IServiceProvider provider)
{
    Console.WriteLine($"\nProcess started: {DateTime.Now}");
    var dictionaryRepository = provider.GetRequiredService<IDictionaryRepository>();
    
    var metadataList = dictionaryRepository.GetMetadataList(null);

    foreach (var metadata in metadataList)
    {
        foreach (var action in metadata.UIOptions.ToolBarActions.GetAll())
        {
            MigrateIfLegacyIcon(action);
        }

        foreach (var action in metadata.UIOptions.GridActions.GetAll())
        {
            MigrateIfLegacyIcon(action);
        }
        dictionaryRepository.InsertOrReplace(metadata);
    }
    Console.WriteLine($"Process finished: {DateTime.Now}");
}
static void MigrateIfLegacyIcon(BasicAction action)
{
    if ((int)action.Icon < 60000)
    {
        var legacyIcon = (LegacyIconType)action.Icon;
        var newIcon = Enum.Parse<IconType>(Enum.GetNames<IconType>().ToList().First(i => i == legacyIcon.ToString()));
        action.Icon = newIcon;
    }
}