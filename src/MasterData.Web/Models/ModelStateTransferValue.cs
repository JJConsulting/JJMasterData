namespace JJMasterData.Web.Models;

public class ModelStateTransferValue
{
    public required string Key { get; init; }
    public required string? AttemptedValue { get; init; }
    public required object? RawValue { get; init; }
    public List<string> ErrorMessages { get; init; } = [];
}