#nullable enable

using System.Collections.Generic;
using JJMasterData.Core.DataDictionary.Models;

namespace JJMasterData.Core.DataManager.Models;

public sealed class FormStateData
{
    public Dictionary<string, object?> Values { get; }
    public Dictionary<string, object?>? UserValues { get; }
    public PageState PageState { get; }
    
    public FormStateData(PageState pageState) 
    {
        Values = new Dictionary<string, object?>();
        PageState = pageState;
    }
    
    public FormStateData(
        Dictionary<string, object?> values,
        Dictionary<string, object?>? userValues,
        PageState pageState) : this(values, pageState)
    {
        UserValues = userValues is null ? null : new Dictionary<string, object?>(userValues);
    }

    public FormStateData(
        Dictionary<string, object?> values,
        PageState pageState)
    {
        Values = new Dictionary<string, object?>(values);
        PageState = pageState;
    }

    public void Deconstruct(
        out Dictionary<string, object?> values,
        out Dictionary<string, object?>? userValues,
        out PageState pageState)
    {
        values = Values;
        userValues = UserValues;
        pageState = PageState;
    }

    public FormStateData DeepCopy()
    {
        return new FormStateData(Values, UserValues, PageState);
    }
}