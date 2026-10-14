using Spectre.Console.Cli;

namespace JJMasterData.CommandLine.Commands;

public sealed class SchemaCommand : AsyncCommand<SchemaCommandSettings>
{
    public override async Task<int> ExecuteAsync(
        CommandContext context,
        SchemaCommandSettings settings,
        CancellationToken cancellationToken)
    {
        await FormElementSchemaService.WriteAsync(settings.Output!, cancellationToken);
        return 0;
    }
}
