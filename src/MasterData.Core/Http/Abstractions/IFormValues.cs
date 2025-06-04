namespace JJMasterData.Core.Http.Abstractions;

public interface IFormValues
{
    public bool ContainsFormValues();
    string this[string key] { get; }
    
#if NETFRAMEWORK
    System.Web.HttpPostedFile GetFile(string file);
    string GetUnvalidated(string key);
#elif NET
    Microsoft.AspNetCore.Http.IFormFile GetFile(string file);
#else 
     // .NET Standard workaround, this is never compiled.
     dynamic GetFile(string file);
#endif
}