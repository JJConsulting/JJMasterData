using System;
using System.IO;

namespace JJMasterData.Core.DataManager.Models;

public sealed class FormFileContent
{
    public string FileName { get; set; }
    public Stream Stream { get; init; }
    public long Length { get; init; }
    public DateTime LastWriteTime { get; init; }
}