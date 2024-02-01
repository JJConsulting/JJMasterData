#nullable enable

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using JJMasterData.Commons.Util;
using JJMasterData.Core.DataDictionary.Models;

namespace JJMasterData.Core.DataManager.Models;

public class FormStateData
{
    public required IDictionary<string, object?> Values { get; init; }
    public required IDictionary<string, object?>? UserValues { get; init; }
    public required PageState PageState { get; init; }

    public FormStateData()
    {
        
    }
    
    [SetsRequiredMembers]
    public FormStateData(
        IDictionary<string, object?> values, 
        IDictionary<string, object?>? userValues,
        PageState pageState)
    {
        UserValues = ObjectCloner.DeepCopy(userValues);
        Values = values;
        PageState = pageState;
    }

    [SetsRequiredMembers]
    public FormStateData(
        IDictionary<string, object?> values,
        PageState pageState)
    {
        Values = values;
        PageState = pageState;
    }

    public void Deconstruct(
        out IDictionary<string, object?> values,
        out IDictionary<string, object?>? userValues,
        out PageState pageState)
    {
        values = Values;
        userValues = UserValues;
        pageState = PageState;
    }
}