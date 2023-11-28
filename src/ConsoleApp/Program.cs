using System.CommandLine;
using JJMasterData.Commons.Logging;
using JJMasterData.ConsoleApp.CommandLine;
using JJMasterData.ConsoleApp.Extensions;
using JJMasterData.ConsoleApp.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureHostConfiguration(c=>
    {
        c.AddJsonFile("appsettings.json");
        c.AddUserSecrets("f3a7ef0a-07c3-4eaa-95eb-7c578e67e286");
    })
    .ConfigureServices(services =>
    {
        services.AddJJMasterDataConsoleServices();
        services.AddLogging(opt =>
        {
            opt.AddFileLoggerProvider();
        });
    })
    .Build();


var rootCommand = new RootCommand("JJMasterData CLI. To learn more visit https://jjconsulting.tech/docs/JJMasterData/.")
{
    Commands.GetFormElementMigrationCommand(() =>
    {
        var service = host.Services.GetRequiredService<FormElementMigrationService>();
        service.Migrate();
    }),
    Commands.GetSqlExpressionsMigrationCommand(() =>
    {
        var service = host.Services.GetRequiredService<ExpressionsMigrationService>();
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
var service = host.Services.GetRequiredService<FormElementMigrationService>();
service.Migrate();

// host.Start();
//
// var parser = new CommandLineBuilder(rootCommand)
//     .UseDefaults()
//     .UseHelp(context =>
//     {
//         context.HelpBuilder.CustomizeLayout(
//             _ =>
//                 HelpBuilder.Default
//                     .GetLayout()
//                     .Prepend(
//                         _ => ConsoleHelper.WriteJJConsultingLogo()
//                     ));
//     })
//     .Build();

// await parser.InvokeAsync(args);