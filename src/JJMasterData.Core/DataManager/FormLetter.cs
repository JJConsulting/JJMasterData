#nullable enable

using System.Collections;

namespace JJMasterData.Core.DataManager;

public class FormLetter
{
    Hashtable? _errors;

    public Hashtable Errors 
    {
        get => _errors ??= new Hashtable();
        set => _errors = value;
    }

    public bool IsValid => _errors == null || _errors.Count == 0;

    public int NumberOfRowsAffected { get; set; }

    public string? UrlRedirect { get; set; }
    
    public FormLetter()
    {
            
    }

    public FormLetter(Hashtable errors)
    {
        _errors = errors;
    }
}

public class FormLetter<T> : FormLetter
{
    public T? Result { get; set; }
    
    public FormLetter() : base() { }
    public FormLetter(Hashtable errors) : base(errors) { }
    
}