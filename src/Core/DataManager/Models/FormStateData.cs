#nullable enable

using JJMasterData.Commons.Extensions;
using JJMasterData.Core.DataDictionary;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace JJMasterData.Core.DataManager;

public class FormStateData
{
    public required IDictionary<string, object?>? UserValues { get; init; }

    public required IDictionary<string, object?> Values { get; init; }
    
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
        UserValues = userValues.DeepCopy();
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

}