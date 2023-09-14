namespace JJMasterData.Core.Http.Abstractions;

public interface IFormValues
{
    public bool ContainsFormValues();
    string this[string key] { get; }
    
#if NETFRAMEWORK
    System.Web.HttpPostedFile GetFile(string file);
    string GetUnvalidated(string key);
#else
    Microsoft.AspNetCore.Http.IFormFile GetFile(string file);
#endif
}