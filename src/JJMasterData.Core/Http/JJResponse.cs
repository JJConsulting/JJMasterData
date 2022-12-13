#if NETSTANDARD || NETCOREAPP
using Microsoft.AspNetCore.Http;
#endif

namespace JJMasterData.Core.Http;

public class JJResponse
{
    private static System.Web.HttpContext SystemWebCurrent => JJHttpContext.SystemWebCurrent;

#if NETCOREAPP || NETSTANDARD
    private static Microsoft.AspNetCore.Http.HttpContext AspNetCoreCurrent => JJHttpContext.AspNetCoreCurrent;
#endif

    public void SendFile(string path)
    {
#if NETFRAMEWORK
        SystemWebCurrent.Response.BufferOutput = false;
        SystemWebCurrent.Response.TransmitFile(path);
        SystemWebCurrent.Response.End();
#else
        AspNetCoreCurrent.Response.SendFileAsync(path).GetAwaiter().GetResult();
#endif

    }

    /// <summary>
    /// Ends the HttpResponse and sends the data to the client.
    /// </summary>
    /// <param name="data">Data to the client. Can be a HTML or JSON .</param>
    /// <param name="contentType">Optional. Usually application/json</param>
    public void SendResponse(string data, string contentType = null)
    {
        #if NETFRAMEWORK
        SystemWebCurrent.Response.ClearContent();
        
        if (contentType != null)
        {
            SystemWebCurrent.Response.ContentType = contentType;
        }

        SystemWebCurrent.Response.Write(data);
        #else
        AspNetCoreCurrent.Response.Clear();
        if (!AspNetCoreCurrent.Response.HasStarted && contentType != null)
        {
            AspNetCoreCurrent.Response.ContentType = contentType;
        }
        #endif
        
        SystemWebCurrent.Response.End();
    }

    public void ClearResponse()
    {
#if NETFRAMEWORK
        SystemWebCurrent.Response.Clear();
        SystemWebCurrent.Response.ClearHeaders();
        SystemWebCurrent.Response.ClearContent();
#else
        AspNetCoreCurrent.Response.Clear();
        AspNetCoreCurrent.Response.Headers.Clear();
#endif
    }

    public void AddResponseHeader(string key, string value)
    {
#if NETFRAMEWORK
        SystemWebCurrent.Response.Headers.Add(key, value);
#else
        if(!AspNetCoreCurrent.Response.HasStarted)
            AspNetCoreCurrent.Response.Headers.Add(key, value);
#endif
    }


#if NETFRAMEWORK
    public void ResponseRedirect(string url) => SystemWebCurrent.Response.Redirect(url);
#else
    public void ResponseRedirect(string url) => AspNetCoreCurrent.Response.Redirect(url);
#endif

}
