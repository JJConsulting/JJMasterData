using JJMasterData.CommandLine.Hosting;
using Spectre.Console.Cli;

namespace JJMasterData.CommandLine.Commands;

public sealed class ImportCommand(ImportRunner importRunner) : AsyncCommand<ImportCommandSettings>
{
    protected override async Task<int> ExecuteAsync(
        CommandContext context,
        ImportCommandSettings settings,
        CancellationToken cancellationToken)
    {
        await importRunner.ImportAsync(settings.DictionaryPath!, settings.Connection!, cancellationToken);
        return 0;
    }
}
