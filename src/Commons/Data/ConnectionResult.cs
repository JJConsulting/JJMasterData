#nullable enable

namespace JJMasterData.Commons.Data;

public sealed class ConnectionResult
{
    public bool IsSuccess { get; }
    public string? ErrorMessage { get; } 
    
    private ConnectionResult(bool isSuccess, string? errorMessage = null)
    {
        IsSuccess = isSuccess;
        ErrorMessage = errorMessage;
    }
    
    public static ConnectionResult Success => new(isSuccess: true);
    
    public static ConnectionResult Error(string? message) => new(isSuccess: false, message);
}