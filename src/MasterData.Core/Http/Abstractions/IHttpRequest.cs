namespace JJMasterData.Core.Http.Abstractions;

public interface IHttpRequest
{
    string UserHostAddress { get; }
    string HttpMethod { get; }
    string UserAgent { get; }
    string AbsoluteUri { get; }
    string ApplicationPath { get; }
    string ApplicationUri { get; }
    string this[string key] { get; }
    public IQueryString QueryString { get; }
    IFormValues Form { get; } 
    string ContentType { get; }
    string Path { get; }
}