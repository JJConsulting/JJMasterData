#nullable enable

namespace JJMasterData.Commons.Validations;

//todo: ao lançar c# 15, utilizar type union https://devblogs.microsoft.com/dotnet/csharp-15-union-types
public class ValidationResult
{
    public static readonly ValidationResult Success = new(isSuccess: true);
    
    public bool IsSuccess { get; }
    public string? ErrorMessage { get; } 
    
    private ValidationResult(bool isSuccess, string? errorMessage = null)
    {
        IsSuccess = isSuccess;
        ErrorMessage = errorMessage;
    }
    
    public static ValidationResult Error(string? message) => new(isSuccess: false, message);
}