#nullable enable

using System.Collections.Generic;
using JJMasterData.Core.DataDictionary.Models;

namespace JJMasterData.Core.DataManager.Models;

internal class FormContext(IDictionary<string, object?> values, IDictionary<string, string> errors, PageState pageState)
{
    public FormContext(IDictionary<string, object?> values, PageState pageState)
        : this(values, new Dictionary<string, string>(), pageState)
    {
    }

    public IDictionary<string, object?> Values { get; } = values;
    public IDictionary<string, string> Errors { get; } = errors;
    public PageState PageState { get; } = pageState;

    public void Deconstruct(out IDictionary<string, object?> values, out IDictionary<string, string> errors,
        out PageState pageState)
    {
        values = Values;
        errors = Errors;
        pageState = PageState;
    }
}