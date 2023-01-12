using System.IO;
using JJMasterData.Commons.Util;

namespace JJMasterData.Core.DataDictionary.Repository;

public class FileSystemDataDictionaryOptions
{
    public string FolderPath { get; set; } = Path.Combine(FileIO.GetApplicationPath(),"Metadata");
}