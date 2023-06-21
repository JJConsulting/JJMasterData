namespace JJMasterData.Web.Areas.DataDictionary.Models.ViewModels;

public class ElementScriptsViewModel : DataDictionaryViewModel
{
    public required string CreateTableScript { get; set; }
    public required string WriteProcedureScript { get; set; }
    public required string ReadProcedureScript { get; set; }
    public required string? AlterTableScript { get; set; }

    public ElementScriptsViewModel()
    {
        
    }
}