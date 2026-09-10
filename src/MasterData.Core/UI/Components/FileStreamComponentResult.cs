using System.IO;
using System.Threading.Tasks;
using JJMasterData.Commons.Util;


namespace JJMasterData.Core.UI.Components;

public class FileStreamComponentResult(
    Stream stream,
    string fileName) : ComponentResult, IActionResult
{
    public override string Content => fileName;
    
    public Task ExecuteResultAsync(Microsoft.AspNetCore.Mvc.ActionContext context)
    {
        var fileStreamResult = new FileStreamResult(stream, MimeTypeUtil.GetMimeType(fileName))
        {
            FileDownloadName = fileName
        };

        return fileStreamResult.ExecuteResultAsync(context);
    }
}
