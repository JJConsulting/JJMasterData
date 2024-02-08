#nullable enable

using System;
using System.Collections.Generic;

namespace JJMasterData.Core.DataManager.Models;

public class FormLetter(Dictionary<string, string>errors)
{
    private Dictionary<string, string>? _errors = errors;

    public Dictionary<string, string> Errors 
    {
        get => _errors ??= new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
        set => _errors = value;
    }

    public bool IsValid => _errors == null || _errors.Count == 0;

    public int NumberOfRowsAffected { get; set; }

    public string? UrlRedirect { get; set; }
}

public class FormLetter<T>(Dictionary<string, string> errors) : FormLetter(errors)
{
    public T? Result { get; set; }
}