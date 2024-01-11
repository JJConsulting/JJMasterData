#nullable enable
#if NET
using System.IO;
using System.Threading.Tasks;
using JJMasterData.Commons.Util;
using Microsoft.AspNetCore.Mvc;
#endif

namespace JJMasterData.Core.UI.Components;

public class FileComponentResult(string filePath) : ComponentResult
#if NET
    ,IActionResult
#endif
{
    private string FilePath { get; } = filePath;
    public override string Content => FilePath;

#if NET
    public async Task ExecuteResultAsync(Microsoft.AspNetCore.Mvc.ActionContext context)
    {
        var fileBytes = await File.ReadAllBytesAsync(FilePath);
        var fileContentResult = new FileContentResult(fileBytes, "application/octet-stream")
        {
            FileDownloadName = FileIO.GetFileNameFromPath(FilePath)
        };

        await fileContentResult.ExecuteResultAsync(context);
    }
#endif
}