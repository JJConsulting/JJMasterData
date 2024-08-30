#nullable enable
namespace JJMasterData.Commons.Data.Entity.Models;

public class ScriptsResult
{
    public required string CreateTableScript { get; set; }
    public bool UseWriteProcedure => WriteProcedureScript is not null;
    public bool UseReadProcedure => ReadProcedureScript is not null;
    public bool ContainsAlterTableScript => AlterTableScript is not null;
    public required string? WriteProcedureScript { get; init; }
    public required string? ReadProcedureScript { get; init; }
    public required string? AlterTableScript { get; init; }
}