#nullable disable warnings
using System;
using System.Collections.Generic;
using JJMasterData.Commons.Storage;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.DataManager;

namespace JJMasterData.Core.Extensions;

public static class FileStoragePathExtensions
{
    extension(FileStoragePath)
    {
        public static string GetFolderPath(FormElement formElement, FormElementField field, Dictionary<string, object?> values)
        {
            if (field.DataFile == null)
                throw new ArgumentException(@$"{nameof(FormElementField.DataFile)} not defined.", field.Name);

            if (string.IsNullOrEmpty(field.DataFile.FolderPath))
                throw new ArgumentException(@$"{nameof(FormElementField.DataFile.FolderPath)} cannot be empty.",
                    field.Name);

            var pkValues = DataHelper.ParsePkValues(formElement, values, '_');

            var folderPath = FileStoragePath.ResolveFolderPath(field.DataFile.FolderPath);

            return FileStoragePath.CombineKey(folderPath, pkValues);
        }

        private static string CombineKey(string rootKey, string childKey)
        {
            return string.IsNullOrEmpty(childKey) ? rootKey : $"{rootKey.TrimEnd('/', '\\')}/{childKey}";
        }
    }
}