using System;
using System.IO;

namespace JJMasterData.Core.WebComponents;

public class FormUploadFile
{
    public string FileName { get; set; }

    public MemoryStream FileStream { get; set; }

    public long SizeBytes { get; set; }

    public DateTime LastWriteTime { get; set; }

    public bool Deleted { get; set; }

    public string OriginName { get; set; }

    public bool IsInMemory => FileStream != null && FileStream.Length > 0;
}
