using System;

namespace JJMasterData.Core.Web.Http.Abstractions;

public interface IHttpResponse
{
    /// <summary>
    /// Ends the HttpResponse and sends the data to the client.
    /// </summary>
    /// <param name="data">Data to the client. Can be a HTML or JSON .</param>
    /// <param name="contentType">Optional. Usually application/json</param>
    /// TODO: Add a //#pragma directive for every SendResponse defeated.
    /// TODO: When removing all Response.End, add a #IFDEF here and REMOVE SystemWebAdapters :)
    [Obsolete("Response.End not supported by ASP.NET Core runtime without SystemWebAdapters.")]
    void SendResponse(string data, string contentType = null);

    void ClearResponse();
    void AddResponseHeader(string key, string value);
    void Redirect(string url);
}