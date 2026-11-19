#nullable disable warnings
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using JJMasterData.Commons.Storage;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.DataManager;

namespace JJMasterData.Core.Extensions;

public static class FileStoragePathExtensions
{
    private const string EncodedKeyPrefix = "~pk~";
    private static readonly char[] InvalidSegmentCharacters = ['/', '\\', ':', '*', '?', '"', '<', '>', '|'];

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

            var segment = IsPortableFolderName(pkValues)
                ? pkValues
                : EncodedKeyPrefix + Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(pkValues)));

            return FileStoragePath.CombineKey(field.DataFile.FolderPath, segment);
        }

        private static string CombineKey(string rootKey, string childKey)
        {
            return string.IsNullOrEmpty(childKey) ? rootKey : $"{rootKey.TrimEnd('/', '\\')}/{childKey}";
        }
    }

    private static bool IsPortableFolderName(string key)
    {
        if (string.IsNullOrEmpty(key) || key.EndsWith('.') || key.EndsWith(' ') ||
            key.StartsWith(EncodedKeyPrefix, StringComparison.OrdinalIgnoreCase) ||
            key.IndexOfAny(InvalidSegmentCharacters) >= 0 || key.Any(char.IsControl) ||
            Encoding.UTF8.GetByteCount(key) > 255)
            return false;

        var stem = key.Split('.', 2)[0].TrimEnd().ToUpperInvariant();
        if (stem is "CON" or "PRN" or "AUX" or "NUL" or "CONIN$" or "CONOUT$")
            return false;

        return stem.Length != 4 ||
               (!stem.StartsWith("COM", StringComparison.Ordinal) && !stem.StartsWith("LPT", StringComparison.Ordinal)) ||
               !"123456789¹²³".Contains(stem[3]);
    }
}
