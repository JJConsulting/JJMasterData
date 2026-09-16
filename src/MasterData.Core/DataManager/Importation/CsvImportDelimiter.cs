using System;
using System.ComponentModel.DataAnnotations;

namespace JJMasterData.Core.DataManager.Importation;

public enum CsvImportDelimiter
{
    [Display(Name = "Semicolon (;)", ShortName = ";")]
    Semicolon,
    [Display(Name = "Comma (,)", ShortName = ",")]
    Comma,
    [Display(Name = "Pipe (|)", ShortName = "|")]
    Pipe,
    [Display(Name = "Tab", ShortName = "\\t")]
    Tab
}

public static class CsvImportDelimiterExtensions
{

    extension(CsvImportDelimiter)
    {
        public static CsvImportDelimiter From(char delimiter)
        {
            return delimiter switch
            {
                ';'  => CsvImportDelimiter.Semicolon,
                ','  => CsvImportDelimiter.Comma,
                '|'  => CsvImportDelimiter.Pipe,
                '\t' => CsvImportDelimiter.Tab,
                _ => throw new ArgumentOutOfRangeException(
                    nameof(delimiter),
                    delimiter,
                    "Invalid CSV delimiter.")
            };
        }
    }

}