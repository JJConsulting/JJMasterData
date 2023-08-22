using System;
using System.Collections;
using System.Collections.Generic;

namespace JJMasterData.Core.DataManager;

public class FormLetter
{
    private IDictionary<string, object>? _errors;

    public IDictionary<string, object> Errors 
    {
        get => _errors ??= new Dictionary<string, object>(StringComparer.InvariantCultureIgnoreCase);
        set => _errors = value;
    }

    public bool IsValid => _errors == null || _errors.Count == 0;

    public int NumberOfRowsAffected { get; set; }

    public string? UrlRedirect { get; set; }
    
    public FormLetter()
    {
            
    }

    public FormLetter(IDictionary<string, object>errors)
    {
        _errors = errors;
    }
}

public class FormLetter<T> : FormLetter
{
    public T? Result { get; set; }
    
    public FormLetter() : base() { }
    public FormLetter(IDictionary<string, object>errors) : base(errors) { }
    
}