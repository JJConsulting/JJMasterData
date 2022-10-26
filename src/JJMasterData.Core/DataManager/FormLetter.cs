#nullable enable

using System.Collections;

namespace JJMasterData.Core.DataManager;

public class FormLetter
{
    Hashtable? _errors;

    public Hashtable Errors 
    {
        get
        {
            if (_errors == null)
                _errors = new Hashtable();

            return _errors;
        }
        set
        {
            _errors = value;
        } 
    }

    public bool IsValid => _errors == null || _errors.Count == 0;

    public int Total { get; set; }

    public string? UrlRedirect { get; set; }
    
    public FormLetter()
    {
            
    }

    public FormLetter(Hashtable errors)
    {
        _errors = errors;
    }
}

public class DataDictionaryResult<T> : FormLetter
{
    public T? Result { get; set; }
}