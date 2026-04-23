using JJMasterData.CommandLine.Commands;
using JJMasterData.CommandLine.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;
using Spectre.Console.Cli;

var services = new ServiceCollection()
    .AddMemoryCache()
    .AddSingleton(AnsiConsole.Console)
    .AddSingleton<ConsoleRunner>();

var registrar = new TypeRegistrar(services);
var app = new CommandApp(registrar);

app.Configure(config =>
{
    config.SetApplicationName("jjmasterdata");
    config.AddCommand<ImportCommand>("import")
        .WithDescription("Imports JSON dictionaries into the JJMasterData database.");
    config.AddCommand<ExportCommand>("export")
        .WithDescription("Exports JSON dictionaries from the JJMasterData database to a folder.");
    config.AddCommand<DiffCommand>("diff")
        .WithDescription("Compares folder dictionaries against the JJMasterData database.");
    config.ValidateExamples();
});

app.SetDefaultCommand<InteractiveCommand>();

return await app.RunAsync(args);