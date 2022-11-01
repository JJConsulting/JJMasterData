using JJMasterData.Commons.Language;
using JJMasterData.Core.Http;
using System;
using System.Collections.Generic;
using System.IO;

namespace JJMasterData.Core.DataManager.IO
{
    internal class FileRepositoryMemory : FileRepositoryBase, IFileRepository
    {
       

        public FileRepositoryMemory()
        {

        }

      
      

        public void DeleteAll()
        {
            MemoryFiles = null;
        }

        public void DeleteFile(string fileName)
        {
            var files = GetFiles();
            var file = files.Find(x => x.FileName.Equals(fileName));
            if (file != null)
            {
                if (!file.IsInMemory)
                    file.Deleted = true;
                else
                    files.Remove(file);
            }

            MemoryFiles = files;
        }


        public override List<FileDetail> GetFiles()
        {
            return MemoryFiles;
        }

        public void RenameFile(string currentName, string newName)
        {
            string err = ValidateRename(currentName, newName);
            if (!string.IsNullOrEmpty(err))
                throw new Exception(err);

            var files = GetFiles();
            if (files.Exists(x => x.FileName.Equals(newName)))
                throw new Exception(Translate.Key("A file with the name {0} already exists", newName));

            var file = files.Find(x => x.FileName.Equals(currentName));
            if (file == null)
                throw new Exception(Translate.Key("file {0} not found!", currentName));

            file.FileName = newName;
            if (file.FileStream == null & string.IsNullOrEmpty(file.OriginName))
                file.OriginName = currentName;

            MemoryFiles = files;
        }

        public void AddFile(string fileName, MemoryStream memoryStream)
        {
            var files = GetFiles();
            var currentFile = files.Find(x => x.FileName.Equals(fileName));
            if (currentFile == null)
            {
                var file = new FileDetail
                {
                    FileName = fileName,
                    FileStream = memoryStream,
                    LastWriteTime = DateTime.Now,
                    SizeBytes = memoryStream.Length
                };
                files.Add(file);
            }
            else
            {
                currentFile.Deleted = false;
                currentFile.FileStream = memoryStream;
                currentFile.LastWriteTime = DateTime.Now;
                currentFile.SizeBytes = memoryStream.Length;
            }

            MemoryFiles = files;
        }

    }
}
