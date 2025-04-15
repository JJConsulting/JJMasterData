#nullable enable
namespace JJMasterData.Commons.Data.Entity.Models;

public class ScriptsResult
{
    public required string CreateTableScript { get; set; }
    public required bool UseWriteProcedure { get; init; }
    public required bool UseReadProcedure { get; init; }
    public bool ContainsAlterTableScript => AlterTableScript is not null;
    public required string? WriteProcedureScript { get; init; }
    public required string? ReadProcedureScript { get; init; }
    public required string? AlterTableScript { get; init; }
}