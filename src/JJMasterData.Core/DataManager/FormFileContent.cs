using System;
using System.IO;

namespace JJMasterData.Core.DataManager
{
    public class FormFileContent
    {
        public string FileName { get; set; }

        public MemoryStream FileStream { get; set; }

        public long SizeBytes { get; set; }

        public DateTime LastWriteTime { get; set; }
    }
}