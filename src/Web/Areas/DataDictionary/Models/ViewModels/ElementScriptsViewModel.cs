namespace JJMasterData.Web.Areas.DataDictionary.Models.ViewModels;

public class ElementScriptsViewModel : DataDictionaryViewModel
{
    public required string CreateTableScript { get; set; }

    public bool UseWriteProcedure => WriteProcedureScript is not null;
    public bool UseReadProcedure => ReadProcedureScript is not null;
    public required string? WriteProcedureScript { get; set; }
    public required string? ReadProcedureScript { get; set; }
    public required string? AlterTableScript { get; set; }
    public bool TableExists { get; set; }

    public ElementScriptsViewModel()
    {
        
    }
}