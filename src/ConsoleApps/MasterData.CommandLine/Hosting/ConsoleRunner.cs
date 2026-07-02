using JJMasterData.CommandLine.Commands;
using JJMasterData.Commons.Data;
using JJMasterData.Core.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;

namespace JJMasterData.CommandLine.Hosting;

public sealed class ConsoleRunner(IAnsiConsole console)
{
    private const string SecretKey = "jjmasterdata-console-tool";

    public Task ImportAsync(MasterDataCommandSettings settings, CancellationToken cancellationToken)
    {
        return ExecuteAsync<ImportService>(
            settings,
            (service, token) => service.ImportAsync(settings.DictionaryPath!, token),
            cancellationToken);
    }

    public Task ExportAsync(MasterDataCommandSettings settings, CancellationToken cancellationToken)
    {
        return ExecuteAsync<ExportService>(
            settings,
            (service, token) => service.ExportAsync(settings.DictionaryPath!, token),
            cancellationToken);
    }

    public Task DiffAsync(MasterDataCommandSettings settings, CancellationToken cancellationToken)
    {
        return ExecuteAsync<DiffService>(
            settings,
            (service, token) => service.DiffAsync(settings.DictionaryPath!, token),
            cancellationToken);
    }

    private async Task ExecuteAsync<TService>(
        MasterDataCommandSettings settings,
        Func<TService, CancellationToken, Task> action,
        CancellationToken cancellationToken)
        where TService : notnull
    {
        var services = new ServiceCollection();
        var configuration = BuildConfiguration(settings);

        services.AddMemoryCache();
        services.AddLogging();
        services.AddSingleton(console);
        services.AddSingleton(configuration);
        services.AddJJMasterDataCore(configuration);
        services.AddTransient<ImportService>();
        services.AddTransient<ExportService>();
        services.AddTransient<DiffService>();

        await using var serviceProvider = services.BuildServiceProvider();
        using var scope = serviceProvider.CreateScope();

        var service = scope.ServiceProvider.GetRequiredService<TService>();
        await action(service, cancellationToken);
    }

    private static IConfiguration BuildConfiguration(MasterDataCommandSettings settings)
    {
        var (schema, table) = ParseTable(settings.Table);

        var values = new Dictionary<string, string?>
        {
            ["JJMasterData:ConnectionString"] = settings.Connection,
            ["JJMasterData:ConnectionProvider"] = nameof(DataAccessProvider.SqlServer),
            ["JJMasterData:SecretKey"] = SecretKey,
            ["JJMasterData:DataDictionaryTableSchema"] = schema,
            ["JJMasterData:DataDictionaryTableName"] = table
        };

        return new ConfigurationBuilder()
            .AddInMemoryCollection(values)
            .Build();
    }

    private static (string Schema, string Table) ParseTable(string? table)
    {
        if (string.IsNullOrWhiteSpace(table))
            return ("dbo", "tb_masterdata");

        var parts = table.Split('.', 2, StringSplitOptions.TrimEntries);

        return parts.Length == 2
            ? (parts[0], parts[1])
            : ("dbo", parts[0]);
    }
}