using System.Collections.Generic;
using System.IO;

namespace JJMasterData.Core.DataManager.IO;

internal interface IFileRepository
{
    public List<FileDetail> GetFiles();

    public FileDetail GetFile(string fileName);

    public void AddFile(string fileName, MemoryStream memoryStream);

    public void DeleteFile(string fileName);

    public void DeleteAll();

    public void RenameFile(string currentName, string newName);

    public int CountFiles();

    public void SaveMemoryFiles(string folderPath);
}
