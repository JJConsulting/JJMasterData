#nullable enable
#if NET
using System.IO;
#endif

namespace JJMasterData.Core.UI.Components;

public sealed class FilePathComponentResult(string filePath) : FileStreamComponentResult(stream: CreateStream(filePath), Path.GetFileName(filePath))
{
    private static FileStream CreateStream(string filePath)
    {
        return new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 81920, true);
    }
}
