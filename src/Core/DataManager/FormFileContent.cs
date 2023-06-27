using System;

namespace JJMasterData.Core.DataManager;

public class FormFileContent
{
    public string FileName { get; set; }
    public byte[] Bytes { get; set; }
    public long Length { get; set;}
    public DateTime LastWriteTime { get; set; }
}