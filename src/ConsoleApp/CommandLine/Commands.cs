using System.CommandLine;

namespace JJMasterData.ConsoleApp.CommandLine;

public static class Commands
{
    public static Command GetFormElementMigrationCommand(Action handler)
    {
        var command = new Command("migration", "Update a Saturn DataDictionary to Sun version.");
        command.SetHandler(handler);
        return command;
    }
    
    public static Command GetJsonSchemaCommand(Action<string> handler)
    {
        var command = new Command("schema", "Generate JJMasterData options JSON schema.");
        var schemaOption = new Option<string>(
            name: "--schemaName",
            description: "Schema to be generated",
            getDefaultValue: () => "JJMasterDataOptions");
        command.AddOption(schemaOption);
        command.SetHandler(handler, schemaOption);
        return command;
    }
    
    public static Command GetImportCommand(Action handler)
    {
        var command = new Command("import", "Normally used at CI pipelines, import data dictionaries from a folder and store at the database.");
        command.SetHandler(handler);
        return command;
    }
}