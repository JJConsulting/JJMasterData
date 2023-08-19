#nullable enable

using JJMasterData.Commons.Extensions;
using JJMasterData.Core.DataDictionary;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace JJMasterData.Core.DataManager;

public class FormStateData
{
    public required IDictionary<string, object>? UserValues { get; init; }

    public required IDictionary<string, object> FormValues { get; init; }

    public required PageState PageState { get; init; }
    
    [SetsRequiredMembers]
    public FormStateData(
        IDictionary<string, object> formValues, 
        IDictionary<string, object>? userValues,
        PageState pageState)
    {
        UserValues = userValues.DeepCopy();
        FormValues = formValues;
        PageState = pageState;
    }

    [SetsRequiredMembers]
    public FormStateData(
        IDictionary<string, object> formValues,
        PageState pageState)
    {
        FormValues = formValues;
        PageState = pageState;
    }

}