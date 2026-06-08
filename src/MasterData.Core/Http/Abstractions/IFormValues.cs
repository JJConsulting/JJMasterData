namespace JJMasterData.Core.Http.Abstractions;

public interface IFormValues
{
    public bool ContainsFormValues();
    string this[string key] { get; }
    
    Microsoft.AspNetCore.Http.IFormFile GetFile(string file);
}
