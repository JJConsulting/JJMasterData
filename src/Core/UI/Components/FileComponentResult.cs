#nullable enable
#if NET
using System.IO;
using System.Threading.Tasks;
using JJMasterData.Commons.Util;
using Microsoft.AspNetCore.Mvc;
#endif

namespace JJMasterData.Core.UI.Components;

public sealed class FileComponentResult(string filePath) : ComponentResult
#if NET
    ,IActionResult
#endif
{
    private string FilePath { get; } = filePath;
    public override string Content => FilePath;

#if NET
    public Task ExecuteResultAsync(Microsoft.AspNetCore.Mvc.ActionContext context)
    {
        var fileName = Path.GetFileName(FilePath);

        var fileContentResult = new PhysicalFileResult(FilePath,  MimeTypeUtil.GetMimeType(fileName))
        {
            FileDownloadName = fileName
        };

        return fileContentResult.ExecuteResultAsync(context);
    }
#endif
}