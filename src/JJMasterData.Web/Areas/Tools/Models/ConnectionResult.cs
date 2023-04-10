namespace JJMasterData.Web.Areas.Tools.Models;

public class ConnectionResult
{
    public bool? IsConnectionSuccessful { get; set; }
    public string? ErrorMessage { get; set; }

    public ConnectionResult()
    {
        
    }
    public ConnectionResult(bool isConnectionSuccessful, string errorMessage)
    {
        IsConnectionSuccessful = isConnectionSuccessful;
        ErrorMessage = errorMessage;
    }
    
}