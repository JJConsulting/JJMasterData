using System;

namespace JJMasterData.Core.DataDictionary.Repository;

public class DataDictionaryModel
{
    public string Name { get; set; }= null!;
    public char Type { get; set; }
    public DateTime Modified { get; set; } 
    public string Json { get; set; } = null!;
}