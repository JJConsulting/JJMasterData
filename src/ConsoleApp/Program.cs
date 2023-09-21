using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Help;
using System.CommandLine.Parsing;
using JJMasterData.ConsoleApp.CommandLine;
using JJMasterData.ConsoleApp.Extensions;
using JJMasterData.ConsoleApp.Services;
using JJMasterData.ConsoleApp.Utils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services => services.AddJJMasterDataConsoleServices())
    .Build();


var rootCommand = new RootCommand("JJMasterData CLI. To learn more visit https://jjconsulting.tech/docs/JJMasterData/.")
{
    Commands.GetFormElementMigrationCommand(() =>
    {
        var service = host.Services.GetRequiredService<FormElementMigrationService>();
        service.Migrate();
    }),
    Commands.GetImportCommand(() =>
    {
        var service = host.Services.GetRequiredService<ImportService>();
        service.Import();
    }),
    Commands.GetJsonSchemaCommand((schemaName) =>
    {
        var service = host.Services.GetRequiredService<JsonSchemaService>();
        service.GenerateJsonSchema(schemaName);
    })
};


var parser = new CommandLineBuilder(rootCommand)
    .UseDefaults()
    .UseHelp(context =>
    {
        context.HelpBuilder.CustomizeLayout(
            _ =>
                HelpBuilder.Default
                    .GetLayout()
                    .Prepend(
                        _ => ConsoleHelper.WriteJJConsultingLogo()
                    ));
    })
    .Build();

await parser.InvokeAsync(args);