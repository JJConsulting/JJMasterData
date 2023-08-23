#nullable enable

using System;
using System.Collections;
using System.Collections.Generic;

namespace JJMasterData.Core.DataManager;

public class FormLetter
{
    private IDictionary<string, string>? _errors;

    public IDictionary<string, string> Errors 
    {
        get => _errors ??= new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
        set => _errors = value;
    }

    public bool IsValid => _errors == null || _errors.Count == 0;

    public int NumberOfRowsAffected { get; set; }

    public string? UrlRedirect { get; set; }

    public FormLetter(IDictionary<string, string>errors)
    {
        _errors = errors;
    }
}

public class FormLetter<T> : FormLetter
{
    public T? Result { get; set; }

    public FormLetter(IDictionary<string, string>errors) : base(errors) { }
    
}