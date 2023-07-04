#nullable enable

using System;
using System.Collections;
using System.Collections.Generic;

namespace JJMasterData.Core.DataManager;

public class FormLetter
{
    private IDictionary<string,dynamic>? _errors;

    public IDictionary<string,dynamic> Errors 
    {
        get => _errors ??= new Dictionary<string,dynamic>(StringComparer.InvariantCultureIgnoreCase);
        set => _errors = value;
    }

    public bool IsValid => _errors == null || _errors.Count == 0;

    public int NumberOfRowsAffected { get; set; }

    public string? UrlRedirect { get; set; }
    
    public FormLetter()
    {
            
    }

    public FormLetter(IDictionary<string,dynamic>errors)
    {
        _errors = errors;
    }
}

public class FormLetter<T> : FormLetter
{
    public T? Result { get; set; }
    
    public FormLetter() : base() { }
    public FormLetter(IDictionary<string,dynamic>errors) : base(errors) { }
    
}