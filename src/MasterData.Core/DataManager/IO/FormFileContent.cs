using System;
using System.IO;

namespace JJMasterData.Core.DataManager.IO;

public class FormFileContent
{
    public string FileName { get; set; }
    public Stream Stream { get; set; }
    public long Length { get; set;}
    public DateTime LastWriteTime { get; set; }
}
