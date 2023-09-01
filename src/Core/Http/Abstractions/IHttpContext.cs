namespace JJMasterData.Core.Web.Http.Abstractions;

public interface IHttpContext
{
    IHttpSession Session { get; }
    IHttpRequest Request { get; }
}