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
    public async Task ExecuteResultAsync(Microsoft.AspNetCore.Mvc.ActionContext context)
    {
        var fileName = Path.GetFileName(FilePath);
        var fileBytes = await File.ReadAllBytesAsync(FilePath);
        var fileContentResult = new FileContentResult(fileBytes,  MimeTypeUtil.GetMimeType(fileName))
        {
            FileDownloadName = fileName
        };

        await fileContentResult.ExecuteResultAsync(context);
    }
#endif
}