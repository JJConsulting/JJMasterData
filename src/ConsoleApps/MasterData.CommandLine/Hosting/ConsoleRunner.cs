using JJMasterData.Commons.Data;
using JJMasterData.Core.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;

namespace JJMasterData.CommandLine.Hosting;

public sealed class ConsoleRunner(IAnsiConsole console)
{
    private const string SecretKey = "jjmasterdata-console-tool";

    public Task ImportAsync(string path, string connection, CancellationToken cancellationToken)
    {
        return ExecuteAsync<ImportService>(connection,
            (service, token) => service.ImportAsync(path, token), cancellationToken);
    }

    public Task ExportAsync(string path, string connection, CancellationToken cancellationToken)
    {
        return ExecuteAsync<ExportService>(connection,
            (service, token) => service.ExportAsync(path, token), cancellationToken);
    }

    public Task DiffAsync(string path, string connection, CancellationToken cancellationToken)
    {
        return ExecuteAsync<DiffService>(connection,
            (service, token) => service.DiffAsync(path, token), cancellationToken);
    }

    private async Task ExecuteAsync<TService>(string connection,
        Func<TService, CancellationToken, Task> action,
        CancellationToken cancellationToken)
        where TService : notnull
    {
        var services = new ServiceCollection();
        var configuration = BuildConfiguration(connection);

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

    private static IConfiguration BuildConfiguration(string connection)
    {
        var settings = new Dictionary<string, string?>
        {
            ["JJMasterData:ConnectionString"] = connection,
            ["JJMasterData:ConnectionProvider"] = nameof(DataAccessProvider.SqlServer),
            ["JJMasterData:SecretKey"] = SecretKey
        };

        return new ConfigurationBuilder()
            .AddInMemoryCollection(settings)
            .Build();
    }
}
