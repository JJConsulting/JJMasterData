#nullable enable

using System.Collections.Generic;
using System.Diagnostics;
using JJMasterData.Core.DataDictionary.Models;

namespace JJMasterData.Core.DataManager.Models;

[DebuggerStepThrough]
internal class FormContext(Dictionary<string, object?> values, Dictionary<string, string> errors, PageState pageState)
{
    public FormContext(Dictionary<string, object?> values, PageState pageState)
        : this(values, new Dictionary<string, string>(), pageState)
    {
    }

    public Dictionary<string, object?> Values { get; } = values;
    public Dictionary<string, string> Errors { get; } = errors;
    public PageState PageState { get; } = pageState;

    public void Deconstruct(out Dictionary<string, object?> values, out Dictionary<string, string> errors,
        out PageState pageState)
    {
        values = Values;
        errors = Errors;
        pageState = PageState;
    }
}