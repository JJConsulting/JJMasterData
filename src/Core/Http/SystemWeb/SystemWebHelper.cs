#if NET48

using System.IO;
using System.Web;
using JJMasterData.Commons.Util;
using JJMasterData.Core.UI.Components;

namespace JJMasterData.Core.Http.SystemWeb;

public static class SystemWebHelper
{
    public static bool CanSendResult(ComponentResult result)
    {
        return result is not EmptyComponentResult && result is not RenderedComponentResult;
    }

    public static void SendResult(ComponentResult result)
    {
        var currentContext = System.Web.HttpContext.Current;

        if (result is RedirectComponentResult redirectComponentResult)
            currentContext.Response.Redirect(redirectComponentResult.Content);
        else if (result is FileComponentResult fileComponentResult)
        {
            var filePath = fileComponentResult.Content;
            var fileName = FileIO.GetFileNameFromPath(filePath);

            var file = new MemoryStream(File.ReadAllBytes(filePath));
            currentContext.Response.ClearHeaders();
            currentContext.Response.ClearContent();
            currentContext.Response.ContentType = MimeTypeUtil.GetMimeType(fileName);
            currentContext.Response.AddHeader("Content-Transfer-Encoding", "binary");
            currentContext.Response.AddHeader("Content-Description", "File Transfer");
            currentContext.Response.AddHeader("Content-Disposition",
                $"attachment; filename={HttpUtility.UrlEncode(fileName)}");
            currentContext.Response.AddHeader("Content-Length", file.Length.ToString());
            currentContext.Response.BinaryWrite(file.ToArray());
            currentContext.Response.End();
        }
        else
        {
            if (result is JsonComponentResult)
            {
                currentContext.Response.ContentType = "application/json";
            }

            currentContext.Response.Clear();
            currentContext.Response.StatusCode = result.StatusCode;
            currentContext.Response.Write(result.Content!);
            currentContext.Response.End();
        }
    }
}
#endif