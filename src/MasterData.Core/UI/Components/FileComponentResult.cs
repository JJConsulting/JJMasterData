#nullable enable
#if NET
using System.IO;
using System.Threading.Tasks;
using JJMasterData.Commons.Util;
using Microsoft.AspNetCore.Mvc;
#endif

namespace JJMasterData.Core.UI.Components;

public sealed class FileComponentResult(
    Stream stream,
    string fileName) : ComponentResult, IActionResult
{
    public override string Content => fileName;
    
    public Task ExecuteResultAsync(Microsoft.AspNetCore.Mvc.ActionContext context)
    {
        var fileContentResult = new FileStreamResult(stream, MimeTypeUtil.GetMimeType(fileName))
        {
            FileDownloadName = fileName
        };

        return fileContentResult.ExecuteResultAsync(context);
    }
}
