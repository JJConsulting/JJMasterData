#nullable enable

using System.Collections;

namespace JJMasterData.Core.DataManager;

public class DataDictionaryResult
{
    public int Total { get; set; }
    public Hashtable? Errors { get; set; }
    public bool IsValid => Errors == null || Errors.Count == 0;
    public string? UrlRedirect { get; set; }
    
    public DataDictionaryResult()
    {
            
    }

    public DataDictionaryResult(Hashtable errors)
    {
        Errors = errors;
    }
}

public class DataDictionaryResult<T> : DataDictionaryResult
{
    public T? Result { get; set; }
}