namespace JJMasterData.Core.Http.Abstractions;

public interface IHttpContext
{
    IHttpSession Session { get; }
    IHttpRequest Request { get; }
}