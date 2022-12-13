using System;
using System.IO;

namespace JJMasterData.Core.DataManager
{
    public class FormFileContent
    {
        public string FileName { get; set; }
        public byte[] Bytes { get; set; }
        public DateTime LastWriteTime { get; set; }
    }
}