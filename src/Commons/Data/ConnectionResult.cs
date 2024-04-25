#nullable enable

namespace JJMasterData.Commons.Data;

public class ConnectionResult(bool isConnectionSuccessful, string? errorMessage)
{
    public bool? IsConnectionSuccessful { get; } = isConnectionSuccessful;
    public string? ErrorMessage { get; } = errorMessage;
}