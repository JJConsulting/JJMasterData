#nullable enable

using System.Collections.Generic;

namespace JJMasterData.Core.DataManager.Models;

public class FormLetter(Dictionary<string, string> errors)
{
    public Dictionary<string, string> Errors { get; set; } = errors;

    public bool IsValid => Errors.Count == 0;

    public int NumberOfRowsAffected { get; set; }

    public string? UrlRedirect { get; set; }
}

public class FormLetter<T>(Dictionary<string, string> errors) : FormLetter(errors)
{
    public T? Result { get; set; }
}