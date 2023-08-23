#nullable enable

using System.Collections.Generic;
using JJMasterData.Core.DataDictionary;

namespace JJMasterData.Core.DataManager;

internal class FormContext
{
    public FormContext(IDictionary<string, object?> values, PageState pageState) : this(values,
        new Dictionary<string, string>(), pageState)
    {
    }

    public FormContext(IDictionary<string, object?> values, IDictionary<string, string> errors, PageState pageState)
    {
        Values = values;
        Errors = errors;
        PageState = pageState;
    }

    public IDictionary<string, object?> Values { get; }
    public IDictionary<string, string> Errors { get; }
    public PageState PageState { get; }

    public void Deconstruct(out IDictionary<string, object?> values, out IDictionary<string, string> errors,
        out PageState pageState)
    {
        values = Values;
        errors = Errors;
        pageState = PageState;
    }
}